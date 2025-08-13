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
public class ImportController : ControllerBase
{
    private readonly MedicalCatalogDbContext _db;
    public ImportController(MedicalCatalogDbContext db) => _db = db;

    public class ImportResult { public int Inserted { get; set; } public int Updated { get; set; } public int Skipped { get; set; } public string? Version { get; set; } }

    [HttpPost("icd10")] 
    [Authorize] // protect in real environments
    [RequestSizeLimit(200_000_000)] // ~200MB
    public async Task<IActionResult> ImportIcd10([FromQuery] string version, IFormFile file)
    {
        if (string.IsNullOrWhiteSpace(version))
            return BadRequest("version is required, e.g., 2025-10");
        if (file == null || file.Length == 0)
            return BadRequest("file is required");

        var encoding = Encoding.UTF8;
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, encoding);

        // Load existing codes into memory
        var existing = await _db.Icd10.AsNoTracking().ToDictionaryAsync(x => x.Code, x => x);

        var result = new ImportResult { Version = version };
        var header = await reader.ReadLineAsync();
        if (header == null)
            return BadRequest("empty file");

        // Determine delimiter (TSV preferred); fall back to comma
        var delimiter = header.Contains('\t') ? '\t' : ',';
        var headerCols = header.Split(delimiter);
        int idxCode = Array.FindIndex(headerCols, h => string.Equals(h, "code", StringComparison.OrdinalIgnoreCase));
        int idxTitle = Array.FindIndex(headerCols, h => h.Equals("title", StringComparison.OrdinalIgnoreCase) || h.Equals("desc", StringComparison.OrdinalIgnoreCase) || h.Equals("description", StringComparison.OrdinalIgnoreCase));
        int idxFrom = Array.FindIndex(headerCols, h => h.Equals("effective_from", StringComparison.OrdinalIgnoreCase) || h.Equals("from", StringComparison.OrdinalIgnoreCase));
        int idxTo = Array.FindIndex(headerCols, h => h.Equals("effective_to", StringComparison.OrdinalIgnoreCase) || h.Equals("to", StringComparison.OrdinalIgnoreCase));
        int idxStatus = Array.FindIndex(headerCols, h => h.Equals("status", StringComparison.OrdinalIgnoreCase));
        if (idxCode < 0 || idxTitle < 0)
            return BadRequest("header must include at least 'code' and 'title' columns");

        var toInsert = new List<Icd10>(8192);
        var toUpdate = new List<Icd10>(8192);
        string? line;
        var lineNo = 1;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            lineNo++;
            if (string.IsNullOrWhiteSpace(line)) { result.Skipped++; continue; }
            var cols = line.Split(delimiter);
            if (cols.Length <= Math.Max(idxCode, idxTitle)) { result.Skipped++; continue; }
            var code = cols[idxCode].Trim();
            if (string.IsNullOrWhiteSpace(code)) { result.Skipped++; continue; }
            var title = cols[idxTitle].Trim();
            DateTime? effFrom = TryParseDate(idxFrom >= 0 && idxFrom < cols.Length ? cols[idxFrom] : null);
            DateTime? effTo = TryParseDate(idxTo >= 0 && idxTo < cols.Length ? cols[idxTo] : null);
            var status = idxStatus >= 0 && idxStatus < cols.Length ? cols[idxStatus].Trim() : null;

            if (existing.TryGetValue(code, out var ex))
            {
                if ((ex.Title ?? string.Empty) != title || ex.EffectiveFrom != effFrom || ex.EffectiveTo != effTo || (ex.Status ?? string.Empty) != (status ?? string.Empty))
                {
                    ex.Title = title;
                    ex.EffectiveFrom = effFrom;
                    ex.EffectiveTo = effTo;
                    ex.Status = status;
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
                    Code = code,
                    Title = title,
                    EffectiveFrom = effFrom,
                    EffectiveTo = effTo,
                    Status = status
                });
            }

            // Flush in batches to control memory
            if (toInsert.Count + toUpdate.Count >= 5000)
            {
                await FlushAsync();
            }
        }

        await FlushAsync();

        // Track the imported release
        if (!await _db.Releases.AnyAsync(r => r.System == "icd10" && r.Version == version))
        {
            _db.Releases.Add(new CatalogRelease { System = "icd10", Version = version, ReleasedOn = DateTime.UtcNow });
            await _db.SaveChangesAsync();
        }

        return Ok(result);

        async Task FlushAsync()
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

        static DateTime? TryParseDate(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)) return dt;
            if (DateTime.TryParseExact(s, new[] { "yyyy-MM-dd", "yyyyMMdd", "MM/dd/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dt)) return dt;
            return null;
        }
    }
}
