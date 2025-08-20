using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using ArchiveService.Data;
using ArchiveService.Models;
using ArchiveService.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connString = builder.Configuration.GetConnectionString("ArchiveDb") ?? "Data Source=archive.db";
builder.Services.AddDbContext<ArchiveDbContext>(o => o.UseSqlite(connString));

builder.Services.AddRabbit(builder.Configuration);

builder.Services.AddHostedService<DoctorArchiveConsumer>();

var app = builder.Build();

// DB init: apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ArchiveDbContext>();
    // Baseline handling: if tables exist (created previously via EnsureCreated) but
    // the __EFMigrationsHistory table does not, seed it with the initial migration
    // so EF Core Migrate() won't attempt to re-create existing tables.
    var connection = db.Database.GetDbConnection();
    await connection.OpenAsync();

    var baselineApplied = false;
    try
    {
        using var cmdHistory = connection.CreateCommand();
        cmdHistory.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE name='__EFMigrationsHistory' AND type='table'";
        var historyCount = Convert.ToInt32(await cmdHistory.ExecuteScalarAsync());

        // Always ensure critical columns exist (repair drifted schemas)
        static async Task<bool> TableExistsAsync(DbConnection conn, string table)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM sqlite_master WHERE name='{table}' AND type='table'";
            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        }

        static async Task EnsureColumnAsync(DbConnection conn, string table, string column, string ddl)
        {
            if (!await TableExistsAsync(conn, table)) return;
            using var pragma = conn.CreateCommand();
            pragma.CommandText = $"PRAGMA table_info('{table}')";
            using var reader = await pragma.ExecuteReaderAsync();
            var hasCol = false;
            while (await reader.ReadAsync())
            {
                var name = reader.GetString(1);
                if (string.Equals(name, column, StringComparison.OrdinalIgnoreCase)) { hasCol = true; break; }
            }
            if (!hasCol)
            {
                using var add = conn.CreateCommand();
                add.CommandText = ddl; // e.g., ALTER TABLE ... ADD COLUMN ...
                await add.ExecuteNonQueryAsync();
            }
        }

        // If legacy column 'SpecializationIds' exists (sometimes NOT NULL), and 'SpecializationIdsJson' doesn't, rebuild table
        async Task RepairArchivedDoctorsTableAsync(DbConnection conn)
        {
            using var pragma = conn.CreateCommand();
            pragma.CommandText = "PRAGMA table_info('ArchivedDoctors')";
            using var reader = await pragma.ExecuteReaderAsync();
            var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (await reader.ReadAsync())
            {
                cols.Add(reader.GetString(1));
            }
            var hasOld = cols.Contains("SpecializationIds");
            var hasNew = cols.Contains("SpecializationIdsJson");
            if (hasOld)
            {
                // Rebuild to the new schema
                using var tx = await conn.BeginTransactionAsync();
                async Task ExecAsync(string sql)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = sql;
                    await cmd.ExecuteNonQueryAsync();
                }

                // Create new table with desired schema
                await ExecAsync(@"CREATE TABLE IF NOT EXISTS ""ArchivedDoctors_new"" (
                    ""DoctorId"" TEXT NOT NULL CONSTRAINT ""PK_ArchivedDoctors"" PRIMARY KEY,
                    ""UserId"" TEXT NULL,
                    ""FullName"" TEXT NOT NULL,
                    ""Email"" TEXT NULL,
                    ""Phone"" TEXT NULL,
                    ""SpecializationIdsJson"" TEXT NULL,
                    ""ArchivedAtUtc"" TEXT NOT NULL,
                    ""SnapshotJson"" TEXT NULL
                )");

                // Copy data, mapping legacy SpecializationIds -> SpecializationIdsJson (as-is)
                // If legacy column is missing in some environments, COALESCE safety not needed because we gated by hasOld
                await ExecAsync(@"INSERT INTO ""ArchivedDoctors_new"" (
                        ""DoctorId"", ""UserId"", ""FullName"", ""Email"", ""Phone"", ""SpecializationIdsJson"", ""ArchivedAtUtc"", ""SnapshotJson""
                    )
                    SELECT ""DoctorId"", ""UserId"", ""FullName"", ""Email"", ""Phone"",
                           COALESCE(""SpecializationIdsJson"", ""SpecializationIds"", '') AS ""SpecializationIdsJson"",
                           ""ArchivedAtUtc"", ""SnapshotJson""
                    FROM ""ArchivedDoctors""
                ");

                // Drop old and rename
                await ExecAsync("DROP TABLE \"ArchivedDoctors\"");
                await ExecAsync("ALTER TABLE \"ArchivedDoctors_new\" RENAME TO \"ArchivedDoctors\"");

                await tx.CommitAsync();
            }
        }

        if (await TableExistsAsync(connection, "ArchivedDoctors"))
        {
            await RepairArchivedDoctorsTableAsync(connection);
        }

        await EnsureColumnAsync(connection, "ArchivedDoctors", "SpecializationIdsJson", "ALTER TABLE \"ArchivedDoctors\" ADD COLUMN \"SpecializationIdsJson\" TEXT NULL");

        if (historyCount == 0)
        {
            // Check for our known tables
            static async Task<int> CountTableAsync(DbConnection conn, string table)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM sqlite_master WHERE name='{table}' AND type='table'";
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }

            var hasDoctors = await CountTableAsync(connection, "ArchivedDoctors");
            var hasDocs = await CountTableAsync(connection, "ArchivedDocuments");

            if (hasDoctors > 0 || hasDocs > 0)
            {
                // Patch existing tables to include any missing columns expected by the current model
                // migrations history will be created/seeded below
                // Create migrations history table and insert the initial migration as applied
                using (var createHistory = connection.CreateCommand())
                {
                    createHistory.CommandText = "CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (\"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY, \"ProductVersion\" TEXT NOT NULL)";
                    await createHistory.ExecuteNonQueryAsync();
                }

                // Use the current EF Core product version; fallback to 8.0.0 if unavailable
                var productVersion = typeof(DbContext).Assembly.GetName().Version?.ToString() ?? "8.0.0";

                using (var insertInitial = connection.CreateCommand())
                {
                    insertInitial.CommandText = "INSERT OR IGNORE INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES (@id, @ver)";
                    var p1 = insertInitial.CreateParameter();
                    p1.ParameterName = "@id";
                    p1.Value = "20250820_InitialCreate";
                    insertInitial.Parameters.Add(p1);

                    var p2 = insertInitial.CreateParameter();
                    p2.ParameterName = "@ver";
                    p2.Value = productVersion;
                    insertInitial.Parameters.Add(p2);

                    await insertInitial.ExecuteNonQueryAsync();
                }
                baselineApplied = true;
            }
        }
    }
    finally
    {
        await connection.CloseAsync();
    }
    try
    {
        db.Database.Migrate();
    }
    catch (SqliteException ex) when (ex.SqliteErrorCode == 1 && ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
    {
        // If we reach here, the DB likely had schema created without migrations history.
        // We've attempted to baseline above; keep running instead of crashing.
        Console.WriteLine("[ArchiveService] SQLite schema already exists; skipping re-create.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Minimal controller-style endpoints
app.MapGet("/archive/doctors/{doctorId}", async (Guid doctorId, ArchiveDbContext db) =>
{
    var archived = await db.ArchivedDoctors.FindAsync(doctorId);
    return archived is null ? Results.NotFound() : Results.Ok(archived);
});

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();
