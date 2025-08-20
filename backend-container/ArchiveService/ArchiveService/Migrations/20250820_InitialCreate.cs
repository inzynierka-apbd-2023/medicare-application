using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ArchiveService.Data;

namespace ArchiveService.Migrations;

[DbContext(typeof(ArchiveDbContext))]
[Migration("20250820_InitialCreate")]
public partial class _20250820_InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ArchivedDoctors",
            columns: table => new
            {
                DoctorId = table.Column<Guid>(type: "TEXT", nullable: false),
                UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                FullName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                Phone = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                SpecializationIdsJson = table.Column<string>(type: "TEXT", nullable: true),
                ArchivedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                SnapshotJson = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_ArchivedDoctors", x => x.DoctorId); }
        );

        migrationBuilder.CreateTable(
            name: "ArchivedDocuments",
            columns: table => new
            {
                DocumentId = table.Column<Guid>(type: "TEXT", nullable: false),
                DoctorId = table.Column<Guid>(type: "TEXT", nullable: false),
                PatientId = table.Column<Guid>(type: "TEXT", nullable: true),
                Type = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                SnapshotJson = table.Column<string>(type: "TEXT", nullable: true),
                ArchivedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_ArchivedDocuments", x => x.DocumentId); }
        );
        migrationBuilder.CreateIndex(
            name: "IX_ArchivedDocuments_DoctorId",
            table: "ArchivedDocuments",
            column: "DoctorId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ArchivedDocuments");
        migrationBuilder.DropTable(name: "ArchivedDoctors");
    }
}
