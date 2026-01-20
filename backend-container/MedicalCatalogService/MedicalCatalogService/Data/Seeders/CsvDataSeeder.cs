using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MedicalCatalogService.Models;

namespace MedicalCatalogService.Data.Seeders;

/// <summary>
/// Seeds the medical catalog database from CSV files in the seed-data folder.
/// This replaces the hardcoded MockDataSeeder with comprehensive real medical codes.
/// </summary>
public static class CsvDataSeeder
{
    private const string SeedDataFolderName = "seed-data";
    
    public static async Task SeedAsync(MedicalCatalogDbContext db, ILogger? logger = null)
    {
        var seedDataDir = GetSeedDataDirectory();
        if (seedDataDir == null)
        {
            logger?.LogWarning("Seed data directory not found. Skipping CSV seeding.");
            return;
        }

        logger?.LogInformation("Seeding medical catalog from CSV files in {Path}", seedDataDir);

        var icd10Count = await SeedIcd10Async(db, seedDataDir, logger);
        var loincCount = await SeedLoincAsync(db, seedDataDir, logger);
        var atcCount = await SeedAtcAsync(db, seedDataDir, logger);
        
        await SeedReleasesAsync(db);

        logger?.LogInformation("CSV Seeding complete. Added: ICD-10={Icd10}, LOINC={Loinc}, ATC={Atc}", 
            icd10Count, loincCount, atcCount);
    }

    private static string? GetSeedDataDirectory()
    {
        // Try multiple possible locations for the seed-data folder
        var possiblePaths = new[]
        {
            // In the output directory (copied via .csproj)
            Path.Combine(AppContext.BaseDirectory, SeedDataFolderName),
            // When running from project directory
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", SeedDataFolderName),
            // When running from bin/Debug/net9.0
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", SeedDataFolderName),
            // Relative to current directory
            Path.Combine(Directory.GetCurrentDirectory(), SeedDataFolderName),
            // Relative to project root (when running with dotnet run)
            Path.Combine(Directory.GetCurrentDirectory(), "..", SeedDataFolderName)
        };

        foreach (var path in possiblePaths)
        {
            var fullPath = Path.GetFullPath(path);
            if (Directory.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    private static async Task<int> SeedIcd10Async(MedicalCatalogDbContext db, string seedDataDir, ILogger? logger)
    {
        var filePath = Path.Combine(seedDataDir, "icd10-seed.csv");
        if (!File.Exists(filePath))
        {
            logger?.LogWarning("ICD-10 seed file not found: {Path}", filePath);
            return 0;
        }

        var existingCodes = await db.Icd10.Select(x => x.Code).ToHashSetAsync();
        var toInsert = new List<Icd10>();

        await foreach (var line in ReadCsvLinesAsync(filePath, skipHeader: true))
        {
            var parts = ParseCsvLine(line);
            if (parts.Length < 2) continue;

            var code = parts[0].Trim();
            var title = parts[1].Trim();
            
            if (string.IsNullOrEmpty(code) || existingCodes.Contains(code)) continue;

            DateTime? effectiveFrom = null;
            if (parts.Length > 2 && DateTime.TryParse(parts[2], CultureInfo.InvariantCulture, DateTimeStyles.None, out var ef))
                effectiveFrom = ef;

            var status = parts.Length > 3 ? parts[3].Trim() : "Active";

            toInsert.Add(new Icd10
            {
                Code = code,
                Title = title,
                EffectiveFrom = effectiveFrom,
                Status = status
            });
            existingCodes.Add(code);
        }

        if (toInsert.Count > 0)
        {
            db.Icd10.AddRange(toInsert);
            await db.SaveChangesAsync();
            logger?.LogInformation("Seeded {Count} ICD-10 codes", toInsert.Count);
        }

        return toInsert.Count;
    }

    private static async Task<int> SeedLoincAsync(MedicalCatalogDbContext db, string seedDataDir, ILogger? logger)
    {
        var filePath = Path.Combine(seedDataDir, "loinc-seed.csv");
        if (!File.Exists(filePath))
        {
            logger?.LogWarning("LOINC seed file not found: {Path}", filePath);
            return 0;
        }

        var existingCodes = await db.Loinc.Select(x => x.LoincNum).ToHashSetAsync();
        var toInsert = new List<LoincEntry>();

        // Read header to get column indices
        var headerLine = await ReadFirstLineAsync(filePath);
        if (headerLine == null) return 0;
        
        var headers = ParseCsvLine(headerLine);
        var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++)
        {
            columnMap[headers[i].Trim()] = i;
        }

        await foreach (var line in ReadCsvLinesAsync(filePath, skipHeader: true))
        {
            var parts = ParseCsvLine(line);
            if (parts.Length < 2) continue;

            var loincNum = GetColumn(parts, columnMap, "LOINC_NUM")?.Trim();
            if (string.IsNullOrEmpty(loincNum) || existingCodes.Contains(loincNum)) continue;

            toInsert.Add(new LoincEntry
            {
                LoincNum = loincNum,
                LongCommonName = GetColumn(parts, columnMap, "LONG_COMMON_NAME"),
                ShortName = GetColumn(parts, columnMap, "SHORTNAME"),
                Component = GetColumn(parts, columnMap, "COMPONENT"),
                Property = GetColumn(parts, columnMap, "PROPERTY"),
                TimeAspect = GetColumn(parts, columnMap, "TIME_ASPCT"),
                System = GetColumn(parts, columnMap, "SYSTEM"),
                ScaleType = GetColumn(parts, columnMap, "SCALE_TYP"),
                MethodType = GetColumn(parts, columnMap, "METHOD_TYP"),
                Class = GetColumn(parts, columnMap, "CLASS"),
                Status = GetColumn(parts, columnMap, "STATUS") ?? "ACTIVE"
            });
            existingCodes.Add(loincNum);
        }

        if (toInsert.Count > 0)
        {
            db.Loinc.AddRange(toInsert);
            await db.SaveChangesAsync();
            logger?.LogInformation("Seeded {Count} LOINC codes", toInsert.Count);
        }

        return toInsert.Count;
    }

    private static async Task<int> SeedAtcAsync(MedicalCatalogDbContext db, string seedDataDir, ILogger? logger)
    {
        var filePath = Path.Combine(seedDataDir, "atc-seed.csv");
        if (!File.Exists(filePath))
        {
            logger?.LogWarning("ATC seed file not found: {Path}", filePath);
            return 0;
        }

        var existingCodes = await db.Atc.Select(x => x.AtcCode).ToHashSetAsync();
        var toInsert = new List<AtcEntry>();

        // Read header to get column indices
        var headerLine = await ReadFirstLineAsync(filePath);
        if (headerLine == null) return 0;
        
        var headers = ParseCsvLine(headerLine);
        var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++)
        {
            columnMap[headers[i].Trim()] = i;
        }

        await foreach (var line in ReadCsvLinesAsync(filePath, skipHeader: true))
        {
            var parts = ParseCsvLine(line);
            if (parts.Length < 2) continue;

            var atcCode = GetColumn(parts, columnMap, "atc_code")?.Trim();
            var atcName = GetColumn(parts, columnMap, "atc_name")?.Trim();
            
            if (string.IsNullOrEmpty(atcCode) || string.IsNullOrEmpty(atcName)) continue;
            if (existingCodes.Contains(atcCode)) continue;

            decimal? ddd = null;
            var dddStr = GetColumn(parts, columnMap, "ddd");
            if (!string.IsNullOrEmpty(dddStr) && decimal.TryParse(dddStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var dddVal))
                ddd = dddVal;

            toInsert.Add(new AtcEntry
            {
                AtcCode = atcCode,
                AtcName = atcName,
                Ddd = ddd,
                Uom = GetColumn(parts, columnMap, "uom"),
                AdmR = GetColumn(parts, columnMap, "adm_r"),
                Note = GetColumn(parts, columnMap, "note")
            });
            existingCodes.Add(atcCode);
        }

        if (toInsert.Count > 0)
        {
            db.Atc.AddRange(toInsert);
            await db.SaveChangesAsync();
            logger?.LogInformation("Seeded {Count} ATC codes", toInsert.Count);
        }

        return toInsert.Count;
    }

    private static async Task SeedReleasesAsync(MedicalCatalogDbContext db)
    {
        var releases = new[]
        {
            ("icd10", "2025-seed", DateTime.UtcNow, "ICD-10-CM Seed Data"),
            ("loinc", "2.81-seed", DateTime.UtcNow, "LOINC Seed Data"),
            ("atc", "2024-seed", DateTime.UtcNow, "WHO ATC/DDD Seed Data")
        };

        var existingReleases = await db.Releases
            .Select(r => new { r.System, r.Version })
            .ToListAsync();
        var existingSet = existingReleases.Select(x => (x.System, x.Version)).ToHashSet();

        foreach (var (system, version, releasedOn, description) in releases)
        {
            if (!existingSet.Contains((system, version)))
            {
                db.Releases.Add(new CatalogRelease
                {
                    Id = Guid.NewGuid(),
                    System = system,
                    Version = version,
                    ReleasedOn = releasedOn,
                    Description = description
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static string? GetColumn(string[] parts, Dictionary<string, int> columnMap, string columnName)
    {
        if (columnMap.TryGetValue(columnName, out var index) && index < parts.Length)
        {
            var value = parts[index]?.Trim();
            return string.IsNullOrEmpty(value) ? null : value;
        }
        return null;
    }

    private static async Task<string?> ReadFirstLineAsync(string filePath)
    {
        await using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);
        return await reader.ReadLineAsync();
    }

    private static async IAsyncEnumerable<string> ReadCsvLinesAsync(string filePath, bool skipHeader = false)
    {
        await using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);

        if (skipHeader)
        {
            await reader.ReadLineAsync();
        }

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                yield return line;
            }
        }
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;
        int i = 0;

        while (i < line.Length)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i += 2; // Skip both quote characters
                    continue;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
            i++;
        }

        result.Add(current.ToString());
        return result.ToArray();
    }
}
