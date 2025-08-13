using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalCatalogService.Data;
using MedicalCatalogService.Models;

namespace MedicalCatalogService.Controllers;

[ApiController]
[Route("api/catalog/import")]
public sealed class ImportController : ControllerBase
{
    private readonly MedicalCatalogDbContext _db;
    public ImportController(MedicalCatalogDbContext db) => _db = db;

    public sealed class ImportResult { public int Inserted { get; set; } public int Updated { get; set; } public int Skipped { get; set; } public string? Version { get; set; } }

    private sealed class HeaderInfo
    {
        public required char Delimiter { get; init; }
        public required int IdxCode { get; init; }
        public required int IdxTitle { get; init; }
        public int IdxFrom { get; init; } = -1;
        public int IdxTo { get; init; } = -1;
        public int IdxStatus { get; init; } = -1;
    }

    [HttpPost("icd10")]
    [Authorize]
    [RequestSizeLimit(200_000_000)] // ~200MB
    public async Task<IActionResult> ImportIcd10([FromQuery] string version, IFormFile file, [FromQuery] bool purge = false)
    {
        if (string.IsNullOrWhiteSpace(version)) return BadRequest("version is required, e.g., 2025-10");
        if (file == null || file.Length == 0) return BadRequest("file is required");

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        var headerLine = await reader.ReadLineAsync();
        if (headerLine == null) return BadRequest("empty file");
        var header = GetHeaderInfo(headerLine);
        if (header == null) return BadRequest("header must include at least 'code' and 'title' columns");

        if (purge)
        {
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM [catalog].[icd10]");
        }

    var existing = await _db.Icd10.AsNoTracking().ToDictionaryAsync(x => x.Code, x => x);
    var seenThisFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var toInsert = new List<Icd10>(8192);
        var toUpdate = new List<Icd10>(4096);
        var result = new ImportResult { Version = version };

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (!TryBuildRow(line, header, out var row)) { result.Skipped++; continue; }
            if (!seenThisFile.Add(row.Code)) { result.Skipped++; continue; }
            UpsertDecision(row, existing, toInsert, toUpdate, result);
            if (toInsert.Count + toUpdate.Count >= 5000) await FlushAsync(toInsert, toUpdate, result);
        }

        await FlushAsync(toInsert, toUpdate, result);

        if (!await _db.Releases.AnyAsync(r => r.System == "icd10" && r.Version == version))
        {
            _db.Releases.Add(new CatalogRelease { System = "icd10", Version = version, ReleasedOn = DateTime.UtcNow });
            await _db.SaveChangesAsync();
        }

        return Ok(result);
    }

    private HeaderInfo? GetHeaderInfo(string header)
    {
        var delimiter = header.Contains('\t') ? '\t' : ',';
        var cols = delimiter == ',' ? SplitCsvLine(header) : header.Split(delimiter);
        int idxCode = Array.FindIndex(cols, h => string.Equals(h, "code", StringComparison.OrdinalIgnoreCase));
        int idxTitle = Array.FindIndex(cols, h => h.Equals("title", StringComparison.OrdinalIgnoreCase) || h.Equals("desc", StringComparison.OrdinalIgnoreCase) || h.Equals("description", StringComparison.OrdinalIgnoreCase));
        if (idxCode < 0 || idxTitle < 0) return null;
        int idxFrom = Array.FindIndex(cols, h => h.Equals("effective_from", StringComparison.OrdinalIgnoreCase) || h.Equals("from", StringComparison.OrdinalIgnoreCase));
        int idxTo = Array.FindIndex(cols, h => h.Equals("effective_to", StringComparison.OrdinalIgnoreCase) || h.Equals("to", StringComparison.OrdinalIgnoreCase));
        int idxStatus = Array.FindIndex(cols, h => h.Equals("status", StringComparison.OrdinalIgnoreCase));
        return new HeaderInfo { Delimiter = delimiter, IdxCode = idxCode, IdxTitle = idxTitle, IdxFrom = idxFrom, IdxTo = idxTo, IdxStatus = idxStatus };
    }

    private sealed record Row(string Code, string Title, DateTime? EffectiveFrom, DateTime? EffectiveTo, string? Status);

    private static bool TryBuildRow(string line, HeaderInfo h, out Row row)
    {
        row = new Row("", "", null, null, null);
        if (string.IsNullOrWhiteSpace(line)) return false;
        var cols = h.Delimiter == ',' ? SplitCsvLine(line) : line.Split(h.Delimiter);
        if (cols.Length <= Math.Max(h.IdxCode, h.IdxTitle)) return false;
        var code = TrimQuotes(cols[h.IdxCode].Trim());
        if (string.IsNullOrWhiteSpace(code)) return false;
        var title = TrimQuotes(cols[h.IdxTitle].Trim());
        var effFrom = TryParseDate(h.IdxFrom >= 0 && h.IdxFrom < cols.Length ? cols[h.IdxFrom] : null);
        var effTo = TryParseDate(h.IdxTo >= 0 && h.IdxTo < cols.Length ? cols[h.IdxTo] : null);
        var status = h.IdxStatus >= 0 && h.IdxStatus < cols.Length ? cols[h.IdxStatus].Trim() : null;
        row = new Row(code, title, effFrom, effTo, status);
        return true;
    }

    private static void UpsertDecision(Row row, Dictionary<string, Icd10> existing, List<Icd10> toInsert, List<Icd10> toUpdate, ImportResult result)
    {
        if (existing.TryGetValue(row.Code, out var ex))
        {
            if ((ex.Title ?? string.Empty) != row.Title || ex.EffectiveFrom != row.EffectiveFrom || ex.EffectiveTo != row.EffectiveTo || (ex.Status ?? string.Empty) != (row.Status ?? string.Empty))
            {
                ex.Title = row.Title;
                ex.EffectiveFrom = row.EffectiveFrom;
                ex.EffectiveTo = row.EffectiveTo;
                ex.Status = row.Status;
                toUpdate.Add(ex);
            }
            else
            {
                result.Skipped++;
            }
        }
        else
        {
            toInsert.Add(new Icd10
            {
                Code = row.Code,
                Title = row.Title,
                EffectiveFrom = row.EffectiveFrom,
                EffectiveTo = row.EffectiveTo,
                Status = row.Status
            });
        }
    }

    private async Task FlushAsync(List<Icd10> toInsert, List<Icd10> toUpdate, ImportResult result)
    {
        if (toInsert.Count > 0)
        {
            _db.Icd10.AddRange(toInsert);
            result.Inserted += toInsert.Count;
            toInsert.Clear();
            await _db.SaveChangesAsync();
        }
        if (toUpdate.Count > 0)
        {
            _db.Icd10.UpdateRange(toUpdate);
            result.Updated += toUpdate.Count;
            toUpdate.Clear();
            await _db.SaveChangesAsync();
        }
    }

    private static DateTime? TryParseDate(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)) return dt;
        if (DateTime.TryParseExact(s, new[] { "yyyy-MM-dd", "yyyyMMdd", "MM/dd/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dt)) return dt;
        return null;
    }

    private static string TrimQuotes(string value)
    {
        if (value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
        {
            value = value.Substring(1, value.Length - 2).Replace("\"\"", "\"");
        }
        return value;
    }

    private static string[] SplitCsvLine(string line)
    {
        var list = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;
        int i = 0;
        while (i < line.Length)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i += 2; // skip escaped quote
                    continue;
                }
                inQuotes = !inQuotes;
            }
            else if (ch == ',' && !inQuotes)
            {
                list.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(ch);
            }
            i++;
        }
        list.Add(sb.ToString());
        return list.ToArray();
    }
}
