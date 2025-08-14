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
    private const string SystemIcd10 = "icd10";
    private const string SystemLoinc = "loinc";
    private const string SystemAtc = "atc";

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
    [Authorize(Policy = "CatalogImport")]
    [RequestSizeLimit(200_000_000)] // ~200MB
    public async Task<IActionResult> ImportIcd10([FromQuery] string version, IFormFile file, [FromQuery] bool purge = false)
    {
        if (string.IsNullOrWhiteSpace(version)) return BadRequest("version is required, e.g., 2025-10");
        if (file == null || file.Length == 0) return BadRequest("file is required");

        if (!await _db.Releases.AnyAsync(r => r.System == SystemIcd10 && r.Version == version))
        {
            _db.Releases.Add(new CatalogRelease { System = SystemIcd10, Version = version, ReleasedOn = DateTime.UtcNow, Description = $"ICD-10 import: {file.FileName}" });
            await _db.SaveChangesAsync();
        }

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        var headerLine = await reader.ReadLineAsync();
        if (headerLine == null) return BadRequest("empty file");
        var header = GetHeaderInfo(headerLine);
        if (header == null) return BadRequest("header must include at least 'code' and 'title' columns");

        if (purge)
        {
            await PurgeTableAsync("[catalog].[icd10]");
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

    // release already ensured at start

        return Ok(result);
    }

    [HttpPost("loinc")]
    [Authorize(Policy = "CatalogImport")]
    [RequestSizeLimit(500_000_000)] // LOINC CSV can be large
    public async Task<IActionResult> ImportLoinc([FromQuery] string version, IFormFile file, [FromQuery] bool purge = false)
    {
        if (string.IsNullOrWhiteSpace(version)) return BadRequest("version is required, e.g., 2.81");
        if (file == null || file.Length == 0) return BadRequest("file is required");

        if (!await _db.Releases.AnyAsync(r => r.System == SystemLoinc && r.Version == version))
        {
            _db.Releases.Add(new CatalogRelease { System = SystemLoinc, Version = version, ReleasedOn = DateTime.UtcNow, Description = $"LOINC main: {file.FileName}" });
            await _db.SaveChangesAsync();
        }

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        var headerLine = await reader.ReadLineAsync();
        if (headerLine == null) return BadRequest("empty file");
        var header = GetLoincHeaderInfo(headerLine);
        if (header == null) return BadRequest("header must include LOINC_NUM and expected columns from the LOINC table");

        if (purge)
        {
            await PurgeTableAsync("[catalog].[loinc]");
        }

        var existing = await _db.Loinc.AsNoTracking().ToDictionaryAsync(x => x.LoincNum, x => x);
        var seenThisFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var toInsert = new List<LoincEntry>(8192);
        var toUpdate = new List<LoincEntry>(4096);
        var result = new ImportResult { Version = version };

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (!TryBuildLoincRow(line, header, out var row)) { result.Skipped++; continue; }
            if (!seenThisFile.Add(row.LoincNum)) { result.Skipped++; continue; }
            UpsertLoincDecision(row, existing, toInsert, toUpdate, result);
            if (toInsert.Count + toUpdate.Count >= 5000) await FlushLoincAsync(toInsert, toUpdate, result);
        }

        await FlushLoincAsync(toInsert, toUpdate, result);

    // release already ensured at start

        return Ok(result);
    }

    private sealed class LoincHeaderInfo
    {
        public required char Delimiter { get; init; }
        public required int IdxLoincNum { get; init; }
        public int IdxLongCommonName { get; init; } = -1;
        public int IdxShortName { get; init; } = -1;
        public int IdxComponent { get; init; } = -1;
        public int IdxProperty { get; init; } = -1;
        public int IdxTimeAspct { get; init; } = -1; // TIME_ASPCT
        public int IdxSystem { get; init; } = -1;
        public int IdxScaleTyp { get; init; } = -1; // SCALE_TYP
        public int IdxMethodTyp { get; init; } = -1; // METHOD_TYP
        public int IdxClass { get; init; } = -1;
        public int IdxStatus { get; init; } = -1;
        public int IdxVersionLastChanged { get; init; } = -1;
        public int IdxDefinitionDescription { get; init; } = -1;
        public int IdxExampleUnits { get; init; } = -1; // EXAMPLE_UCUM_UNITS
        public int IdxExternalCopyrightNotice { get; init; } = -1;
        public int IdxPanelType { get; init; } = -1;
        public int IdxEquation { get; init; } = -1;
    }

    private LoincHeaderInfo? GetLoincHeaderInfo(string header)
    {
        // LOINC official CSVs are comma-delimited, but allow TSV for testing
        var delimiter = header.Contains('\t') ? '\t' : ',';
        var cols = delimiter == ',' ? SplitCsvLine(header) : header.Split(delimiter);

        int find(params string[] names) => Array.FindIndex(cols, h => names.Any(n => h.Equals(n, StringComparison.OrdinalIgnoreCase)));

        int idxNum = find("LOINC_NUM", "LoincNum", "Loinc", "LOINC");
        if (idxNum < 0) return null;
        int idxLong = find("LONG_COMMON_NAME", "LongCommonName", "LCN");
        int idxShort = find("SHORTNAME", "ShortName");
        int idxComponent = find("COMPONENT", "Component");
        int idxProperty = find("PROPERTY", "Property");
        int idxTime = find("TIME_ASPCT", "TimeAspct", "TIMEASPECT");
        int idxSystem = find("SYSTEM", "System");
        int idxScale = find("SCALE_TYP", "ScaleTyp", "SCALETYP");
        int idxMethod = find("METHOD_TYP", "MethodTyp", "METHODTYP");
        int idxClass = find("CLASS");
        int idxStatus = find("STATUS");
        int idxVersionLastChanged = find("VERSION_LAST_CHANGED", "VersionLastChanged");
        int idxDefinitionDescription = find("DEFINITION_DESCRIPTION", "DefinitionDescription");
        int idxExample = find("EXAMPLE_UCUM_UNITS", "ExampleUnits");
        int idxCopy = find("EXTERNAL_COPYRIGHT_NOTICE", "ExternalCopyrightNotice");
        int idxPanelType = find("PANELTYPE", "PanelType");
        int idxEquation = find("EQUATION", "Equation");

        return new LoincHeaderInfo
        {
            Delimiter = delimiter,
            IdxLoincNum = idxNum,
            IdxLongCommonName = idxLong,
            IdxShortName = idxShort,
            IdxComponent = idxComponent,
            IdxProperty = idxProperty,
            IdxTimeAspct = idxTime,
            IdxSystem = idxSystem,
            IdxScaleTyp = idxScale,
            IdxMethodTyp = idxMethod,
            IdxClass = idxClass,
            IdxStatus = idxStatus,
            IdxVersionLastChanged = idxVersionLastChanged,
            IdxDefinitionDescription = idxDefinitionDescription,
            IdxExampleUnits = idxExample,
            IdxExternalCopyrightNotice = idxCopy,
            IdxPanelType = idxPanelType,
            IdxEquation = idxEquation
        };
    }

    private sealed record LoincRow(
        string LoincNum,
        string? LongCommonName,
        string? ShortName,
        string? Component,
        string? Property,
        string? TimeAspect,
        string? System,
        string? ScaleType,
        string? MethodType,
        string? Class,
        string? Status,
        string? VersionLastChanged,
        string? DefinitionDescription,
        string? ExampleUnits,
        string? ExternalCopyrightNotice,
        string? PanelType,
        string? Equation
    );

    private static bool TryBuildLoincRow(string line, LoincHeaderInfo h, out LoincRow row)
    {
        row = new LoincRow("", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
        if (string.IsNullOrWhiteSpace(line)) return false;
        var cols = h.Delimiter == ',' ? SplitCsvLine(line) : line.Split(h.Delimiter);
        if (h.IdxLoincNum < 0 || h.IdxLoincNum >= cols.Length) return false;
        var num = TrimQuotes(cols[h.IdxLoincNum].Trim());
        if (string.IsNullOrWhiteSpace(num)) return false;

        string? pick(int idx) => (idx >= 0 && idx < cols.Length) ? TrimQuotes(cols[idx].Trim()) : null;

        row = new LoincRow(
            num,
            pick(h.IdxLongCommonName),
            pick(h.IdxShortName),
            pick(h.IdxComponent),
            pick(h.IdxProperty),
            pick(h.IdxTimeAspct),
            pick(h.IdxSystem),
            pick(h.IdxScaleTyp),
            pick(h.IdxMethodTyp),
            pick(h.IdxClass),
            pick(h.IdxStatus),
            pick(h.IdxVersionLastChanged),
            pick(h.IdxDefinitionDescription),
            pick(h.IdxExampleUnits),
            pick(h.IdxExternalCopyrightNotice),
            pick(h.IdxPanelType),
            pick(h.IdxEquation)
        );
        return true;
    }

    private static void UpsertLoincDecision(LoincRow row, Dictionary<string, LoincEntry> existing, List<LoincEntry> toInsert, List<LoincEntry> toUpdate, ImportResult result)
    {
        if (existing.TryGetValue(row.LoincNum, out var ex))
        {
            if ((ex.LongCommonName ?? "") != (row.LongCommonName ?? "") ||
                (ex.ShortName ?? "") != (row.ShortName ?? "") ||
                (ex.Component ?? "") != (row.Component ?? "") ||
                (ex.Property ?? "") != (row.Property ?? "") ||
                (ex.TimeAspect ?? "") != (row.TimeAspect ?? "") ||
                (ex.System ?? "") != (row.System ?? "") ||
                (ex.ScaleType ?? "") != (row.ScaleType ?? "") ||
                (ex.MethodType ?? "") != (row.MethodType ?? "") ||
                (ex.Class ?? "") != (row.Class ?? "") ||
                (ex.Status ?? "") != (row.Status ?? "") ||
                (ex.VersionLastChanged ?? "") != (row.VersionLastChanged ?? "") ||
                (ex.DefinitionDescription ?? "") != (row.DefinitionDescription ?? "") ||
                (ex.ExampleUnits ?? "") != (row.ExampleUnits ?? "") ||
                (ex.ExternalCopyrightNotice ?? "") != (row.ExternalCopyrightNotice ?? "") ||
                (ex.PanelType ?? "") != (row.PanelType ?? "") ||
                (ex.Equation ?? "") != (row.Equation ?? ""))
            {
                ex.LongCommonName = row.LongCommonName;
                ex.ShortName = row.ShortName;
                ex.Component = row.Component;
                ex.Property = row.Property;
                ex.TimeAspect = row.TimeAspect;
                ex.System = row.System;
                ex.ScaleType = row.ScaleType;
                ex.MethodType = row.MethodType;
                ex.Class = row.Class;
                ex.Status = row.Status;
                ex.VersionLastChanged = row.VersionLastChanged;
                ex.DefinitionDescription = row.DefinitionDescription;
                ex.ExampleUnits = row.ExampleUnits;
                ex.ExternalCopyrightNotice = row.ExternalCopyrightNotice;
                ex.PanelType = row.PanelType;
                ex.Equation = row.Equation;
                toUpdate.Add(ex);
            }
            else
            {
                result.Skipped++;
            }
        }
        else
        {
            toInsert.Add(new LoincEntry
            {
                LoincNum = row.LoincNum,
                LongCommonName = row.LongCommonName,
                ShortName = row.ShortName,
                Component = row.Component,
                Property = row.Property,
                TimeAspect = row.TimeAspect,
                System = row.System,
                ScaleType = row.ScaleType,
                MethodType = row.MethodType,
                Class = row.Class,
                Status = row.Status,
                VersionLastChanged = row.VersionLastChanged,
                DefinitionDescription = row.DefinitionDescription,
                ExampleUnits = row.ExampleUnits,
                ExternalCopyrightNotice = row.ExternalCopyrightNotice,
                PanelType = row.PanelType,
                Equation = row.Equation
            });
        }
    }

    private async Task FlushLoincAsync(List<LoincEntry> toInsert, List<LoincEntry> toUpdate, ImportResult result)
    {
        if (toInsert.Count > 0)
        {
            _db.Loinc.AddRange(toInsert);
            result.Inserted += toInsert.Count;
            toInsert.Clear();
            await _db.SaveChangesAsync();
        }
        if (toUpdate.Count > 0)
        {
            _db.Loinc.UpdateRange(toUpdate);
            result.Updated += toUpdate.Count;
            toUpdate.Clear();
            await _db.SaveChangesAsync();
        }
    }

    [HttpPost("loinc-mapto")]
    [Authorize(Policy = "CatalogImport")]
    [RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> ImportLoincMapTo([FromQuery] string version, IFormFile file, [FromQuery] bool purge = false)
    {
        if (string.IsNullOrWhiteSpace(version)) return BadRequest("version is required, e.g., 2.81");
        if (file == null || file.Length == 0) return BadRequest("file is required");

        if (!await _db.Releases.AnyAsync(r => r.System == SystemLoinc && r.Version == version))
        {
            _db.Releases.Add(new CatalogRelease { System = SystemLoinc, Version = version, ReleasedOn = DateTime.UtcNow, Description = $"LOINC MapTo: {file.FileName}" });
            await _db.SaveChangesAsync();
        }

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var headerLine = await reader.ReadLineAsync();
        if (headerLine == null) return BadRequest("empty file");
    var (delimiter, idxSrc, idxDst, idxMapType, idxComment) = GetMapToHeader(headerLine);
    if (idxSrc < 0 || idxDst < 0) return BadRequest("header must include From and To LOINC columns");

        if (purge)
        {
            await PurgeTableAsync("[catalog].[loinc_map_to]");
        }

        // preload existing loinc->loinc mappings to avoid duplicates across runs
        var existingPairs = await _db.LoincMapTo.AsNoTracking()
            .Select(m => new { SourceCode = m.FromLoinc, TargetCode = m.ToLoinc })
            .ToListAsync();
        string key2(string a, string b) => $"{a?.Trim().ToUpperInvariant()}||{b?.Trim().ToUpperInvariant()}";
        var existingSet = new HashSet<string>(existingPairs.Select(x => key2(x.SourceCode, x.TargetCode)));

    var toAdd = new List<LoincMapTo>(8192);
        var resultObj = new ImportResult { Version = version };
        string? line;
    var seen = new HashSet<(string,string)>();
    var pending = new HashSet<string>();
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) { resultObj.Skipped++; continue; }
            var cols = delimiter == ',' ? SplitCsvLine(line) : line.Split(delimiter);
            if (cols.Length <= Math.Max(idxSrc, idxDst)) { resultObj.Skipped++; continue; }
            var src = TrimQuotes(cols[idxSrc].Trim());
            var dst = TrimQuotes(cols[idxDst].Trim());
            if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(dst)) { resultObj.Skipped++; continue; }
            if (!seen.Add((src, dst))) { resultObj.Skipped++; continue; }
            var k = key2(src, dst);
            if (existingSet.Contains(k) || !pending.Add(k)) { resultObj.Skipped++; continue; }
            string? pick(int i) => i >= 0 && i < cols.Length ? TrimQuotes(cols[i].Trim()) : null;
            // collect optional columns
            toAdd.Add(new LoincMapTo { FromLoinc = src, ToLoinc = dst, MapType = pick(idxMapType), Comment = pick(idxComment) });
            if (toAdd.Count >= 5000) { _db.LoincMapTo.AddRange(toAdd); await _db.SaveChangesAsync(); resultObj.Inserted += toAdd.Count; toAdd.Clear(); }
        }
        if (toAdd.Count > 0) { _db.LoincMapTo.AddRange(toAdd); await _db.SaveChangesAsync(); resultObj.Inserted += toAdd.Count; toAdd.Clear(); }

    // release already ensured at start

        return Ok(resultObj);
    }

    private static (char delimiter, int idxSrc, int idxDst, int idxMapType, int idxComment) GetMapToHeader(string header)
    {
        var delimiter = header.Contains('\t') ? '\t' : ',';
        var cols = delimiter == ',' ? SplitCsvLine(header) : header.Split(delimiter);
        static string N(string s) => new string((s ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        var norm = cols.Select(N).ToArray();
        int find(params string[] keys)
        {
            for (int i = 0; i < norm.Length; i++)
            {
                if (keys.Contains(norm[i]))
                {
                    return i;
                }
            }
            return -1;
        }
        int idxSrc = find("FROMLOINC", "LOINCNUM", "LOINC", "SOURCE", "MAPTOSOURCE");
        int idxDst = find("TOLOINC", "MAPTO", "MAPTOCODE", "TARGET", "MAPTOTARGET");
        int idxMapType = find("MAPTYPE", "TYPE");
        int idxComment = find("COMMENT", "COMMENTS");
        return (delimiter, idxSrc, idxDst, idxMapType, idxComment);
    }

    [HttpPost("loinc-answers")]
    [Authorize(Policy = "CatalogImport")]
    [RequestSizeLimit(500_000_000)]
    public async Task<IActionResult> ImportLoincAnswers([FromQuery] string version, IFormFile answerList, IFormFile listLink, [FromQuery] bool purge = false)
    {
        if (string.IsNullOrWhiteSpace(version)) return BadRequest("version is required, e.g., 2.81");
        if (answerList == null || listLink == null) return BadRequest("answerList and listLink files are required");

        if (!await _db.Releases.AnyAsync(r => r.System == SystemLoinc && r.Version == version))
        {
            _db.Releases.Add(new CatalogRelease { System = SystemLoinc, Version = version, ReleasedOn = DateTime.UtcNow, Description = $"LOINC Answers: {answerList.FileName}; {listLink.FileName}" });
            await _db.SaveChangesAsync();
        }

        if (purge)
        {
            await PurgeTableAsync("[catalog].[loinc_answer_link]");
            await PurgeTableAsync("[catalog].[loinc_answer_list]");
        }

        // Load AnswerList rows
        var answerEntries = new List<LoincAnswerList>();
        using (var s = answerList.OpenReadStream())
        using (var r = new StreamReader(s, Encoding.UTF8))
        {
            var h = await r.ReadLineAsync();
            if (h == null) return BadRequest("empty answerList");
            var (delim, idxListId, idxAnswerStringId, idxDisplayName, idxDescription) = GetAnswerListHeader(h);
            if (idxListId < 0) return BadRequest("answerList header must include AnswerListId");
            string? line;
            while ((line = await r.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = delim == ',' ? SplitCsvLine(line) : line.Split(delim);
                if (cols.Length <= idxListId) continue;
                var listId = TrimQuotes(cols[idxListId].Trim());
                if (string.IsNullOrWhiteSpace(listId)) continue;
                string? pick(int i) => i >= 0 && i < cols.Length ? TrimQuotes(cols[i].Trim()) : null;
                var answerStringId = pick(idxAnswerStringId);
                var displayName = pick(idxDisplayName);
                var description = pick(idxDescription);
                answerEntries.Add(new LoincAnswerList {
                    AnswerListId = listId,
                    AnswerStringId = answerStringId,
                    DisplayName = displayName,
                    Description = description
                });
            }
        }

        // Load LOINC -> AnswerList links
        var loincToList = new List<LoincAnswerLink>();
        using (var s = listLink.OpenReadStream())
        using (var r = new StreamReader(s, Encoding.UTF8))
        {
            var h = await r.ReadLineAsync();
            if (h == null) return BadRequest("empty listLink");
        var (delim, idxLoinc, idxList, idxLinkType) = GetAnswerListLinkHeader(h);
        if (idxLoinc < 0 || idxList < 0) return BadRequest("listLink header must include LOINC_NUM and ANSWERLIST_ID");
            string? line;
            while ((line = await r.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = delim == ',' ? SplitCsvLine(line) : line.Split(delim);
                if (cols.Length <= Math.Max(idxLoinc, idxList)) continue;
                var loinc = TrimQuotes(cols[idxLoinc].Trim());
                var listId = TrimQuotes(cols[idxList].Trim());
                if (!string.IsNullOrWhiteSpace(loinc) && !string.IsNullOrWhiteSpace(listId))
                {
            string? linkType = (idxLinkType >= 0 && idxLinkType < cols.Length) ? TrimQuotes(cols[idxLinkType].Trim()) : null;
            loincToList.Add(new LoincAnswerLink { LoincNum = loinc, AnswerListId = listId, LinkType = linkType });
                }
            }
        }

        // Insert into dedicated tables with DB-level dedupe
        var toAddLists = new List<LoincAnswerList>(8192);
        var toAddLinks = new List<LoincAnswerLink>(8192);
        int inserted = 0;
        string keyAL(LoincAnswerList x) => $"{x.AnswerListId?.Trim().ToUpperInvariant()}||{x.AnswerStringId?.Trim().ToUpperInvariant()}||{x.DisplayName?.Trim().ToUpperInvariant()}||{x.Description?.Trim().ToUpperInvariant()}";
        var existingALSet = new HashSet<string>((await _db.LoincAnswerList.AsNoTracking().Select(x => new { x.AnswerListId, x.AnswerStringId, x.DisplayName, x.Description }).ToListAsync())
            .Select(x => $"{x.AnswerListId?.Trim().ToUpperInvariant()}||{x.AnswerStringId?.Trim().ToUpperInvariant()}||{x.DisplayName?.Trim().ToUpperInvariant()}||{x.Description?.Trim().ToUpperInvariant()}"));
        var pendingAL = new HashSet<string>();
        foreach (var item in answerEntries)
        {
            var k = keyAL(item);
            if (existingALSet.Contains(k) || !pendingAL.Add(k)) continue;
            toAddLists.Add(item);
            if (toAddLists.Count >= 5000) { _db.LoincAnswerList.AddRange(toAddLists); await _db.SaveChangesAsync(); inserted += toAddLists.Count; toAddLists.Clear(); }
        }
        var existingLinks = new HashSet<string>((await _db.LoincAnswerLink.AsNoTracking().Select(x => new { x.LoincNum, x.AnswerListId }).ToListAsync())
            .Select(x => $"{x.LoincNum?.Trim().ToUpperInvariant()}||{x.AnswerListId?.Trim().ToUpperInvariant()}"));
        var pendingLinks = new HashSet<string>();
        foreach (var link in loincToList)
        {
            var k = $"{link.LoincNum?.Trim().ToUpperInvariant()}||{link.AnswerListId?.Trim().ToUpperInvariant()}";
            if (existingLinks.Contains(k) || !pendingLinks.Add(k)) continue;
            toAddLinks.Add(link);
            if (toAddLinks.Count >= 5000) { _db.LoincAnswerLink.AddRange(toAddLinks); await _db.SaveChangesAsync(); inserted += toAddLinks.Count; toAddLinks.Clear(); }
        }
        if (toAddLists.Count > 0) { _db.LoincAnswerList.AddRange(toAddLists); await _db.SaveChangesAsync(); inserted += toAddLists.Count; toAddLists.Clear(); }
        if (toAddLinks.Count > 0) { _db.LoincAnswerLink.AddRange(toAddLinks); await _db.SaveChangesAsync(); inserted += toAddLinks.Count; toAddLinks.Clear(); }

    return Ok(new ImportResult { Version = version, Inserted = inserted, Updated = 0, Skipped = 0 });
    }

    private static (char delim, int idxListId, int idxAnswerStringId, int idxDisplayName, int idxDescription) GetAnswerListHeader(string header)
    {
        var delim = header.Contains('\t') ? '\t' : ',';
        var cols = delim == ',' ? SplitCsvLine(header) : header.Split(delim);
        static string N(string s) => new string((s ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        var norm = cols.Select(N).ToArray();
        int find(params string[] keys)
        {
            for (int i = 0; i < norm.Length; i++)
            {
                if (keys.Contains(norm[i]))
                {
                    return i;
                }
            }
            return -1;
        }
        int idxListId = find("ANSWERLISTID", "LIST", "LISTID");
        int idxAnswerStringId = find("ANSWERLISTNAMEID", "ANSWERSTRINGID", "ANSWERSTRING");
        int idxDisplayName = find("ANSWERLISTNAME", "DISPLAYNAME", "DISPLAY", "DISPLAYTEXT");
        int idxDescription = find("DESCRIPTION", "DESC");
        return (delim, idxListId, idxAnswerStringId, idxDisplayName, idxDescription);
    }

    private static (char delim, int idxLoinc, int idxList, int idxLinkType) GetAnswerListLinkHeader(string header)
    {
        var delim = header.Contains('\t') ? '\t' : ',';
        var cols = delim == ',' ? SplitCsvLine(header) : header.Split(delim);
        static string N(string s) => new string((s ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        var norm = cols.Select(N).ToArray();
        int find(params string[] keys)
        {
            for (int i = 0; i < norm.Length; i++)
            {
                if (keys.Contains(norm[i]))
                {
                    return i;
                }
            }
            return -1;
        }
        int idxLoinc = find("LOINCNUM", "LOINC", "LOINCNUMBER");
        int idxList = find("ANSWERLISTID", "LIST", "LISTID");
        int idxLinkType = find("LINKTYPE", "TYPE");
        return (delim, idxLoinc, idxList, idxLinkType);
    }

    // =============== Panels ===============

    private static (char delim, int idxPanel) GetPanelHeader(string header)
    {
        var delim = header.Contains('\t') ? '\t' : ',';
        var cols = delim == ',' ? SplitCsvLine(header) : header.Split(delim);
        static string N(string s) => new string((s ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        var norm = cols.Select(N).ToArray();
        int find(params string[] keys)
        {
            for (int i = 0; i < norm.Length; i++) if (keys.Contains(norm[i])) return i;
            return -1;
        }
        int idxPanel = find("PANELLOINCNUM", "PANEL", "LOINCNUM", "LOINC", "PARENT", "PARENTLOINC");
        return (delim, idxPanel);
    }

    private static (char delim, int idxPanel, int idxItem, int idxOrdinal, int idxOptional) GetPanelItemHeader(string header)
    {
        var delim = header.Contains('\t') ? '\t' : ',';
        var cols = delim == ',' ? SplitCsvLine(header) : header.Split(delim);
        static string N(string s) => new string((s ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        var norm = cols.Select(N).ToArray();
        int find(params string[] keys)
        {
            for (int i = 0; i < norm.Length; i++) if (keys.Contains(norm[i])) return i;
            return -1;
        }
        int idxPanel = find("PANELLOINCNUM", "PANEL", "PARENTLOINC", "PARENT");
        int idxItem = find("ITEMLOINCNUM", "ITEM", "CHILDLOINC", "CHILD", "LOINC");
        int idxOrdinal = find("SEQUENCE", "ORDINAL", "SEQ");
        int idxOptional = find("OPTIONALITY", "REQUIRED", "OPT", "OBSERVATIONREQUIREDINPANEL");
        return (delim, idxPanel, idxItem, idxOrdinal, idxOptional);
    }

    [HttpPost("loinc-panels")]
    [Authorize(Policy = "CatalogImport")]
    [RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> ImportLoincPanels([FromQuery] string version, IFormFile file, [FromQuery] bool purge = false)
    {
        if (string.IsNullOrWhiteSpace(version)) return BadRequest("version is required");
        if (file == null || file.Length == 0) return BadRequest("file is required");

        if (!await _db.Releases.AnyAsync(r => r.System == SystemLoinc && r.Version == version))
        {
            _db.Releases.Add(new CatalogRelease { System = SystemLoinc, Version = version, ReleasedOn = DateTime.UtcNow, Description = $"LOINC Panels: {file.FileName}" });
            await _db.SaveChangesAsync();
        }

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var headerLine = await reader.ReadLineAsync();
        if (headerLine == null) return BadRequest("empty file");
    var (delim, idxPanel) = GetPanelHeader(headerLine);
    if (idxPanel < 0) return BadRequest("header must include Panel LOINC column");

        if (purge)
        {
            await PurgeTableAsync("[catalog].[loinc_panel_item]");
            await PurgeTableAsync("[catalog].[loinc_panel]");
        }

        var existing = await _db.LoincPanel.AsNoTracking().ToDictionaryAsync(x => x.PanelLoincNum, x => x);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var toInsert = new List<LoincPanel>(4096);
        var res = new ImportResult { Version = version };

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) { res.Skipped++; continue; }
            var cols = delim == ',' ? SplitCsvLine(line) : line.Split(delim);
            if (cols.Length <= idxPanel) { res.Skipped++; continue; }
            var panel = TrimQuotes(cols[idxPanel].Trim());
            if (string.IsNullOrWhiteSpace(panel) || !seen.Add(panel)) { res.Skipped++; continue; }
            if (!existing.ContainsKey(panel))
            {
                toInsert.Add(new LoincPanel { PanelLoincNum = panel });
            }
            if (toInsert.Count >= 5000)
            {
                if (toInsert.Count > 0) { _db.LoincPanel.AddRange(toInsert); res.Inserted += toInsert.Count; toInsert.Clear(); await _db.SaveChangesAsync(); }
            }
        }
        if (toInsert.Count > 0) { _db.LoincPanel.AddRange(toInsert); res.Inserted += toInsert.Count; toInsert.Clear(); await _db.SaveChangesAsync(); }

        return Ok(res);
    }

    [HttpPost("loinc-panel-items")]
    [Authorize(Policy = "CatalogImport")]
    [RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> ImportLoincPanelItems([FromQuery] string version, IFormFile file, [FromQuery] bool purge = false)
    {
        if (string.IsNullOrWhiteSpace(version)) return BadRequest("version is required");
        if (file == null || file.Length == 0) return BadRequest("file is required");

        if (!await _db.Releases.AnyAsync(r => r.System == SystemLoinc && r.Version == version))
        {
            _db.Releases.Add(new CatalogRelease { System = SystemLoinc, Version = version, ReleasedOn = DateTime.UtcNow, Description = $"LOINC Panel Items: {file.FileName}" });
            await _db.SaveChangesAsync();
        }

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var headerLine = await reader.ReadLineAsync();
        if (headerLine == null) return BadRequest("empty file");
    var (delim, idxPanel, idxItem, idxOrdinal, idxOptional) = GetPanelItemHeader(headerLine);
    if (idxPanel < 0 || idxItem < 0) return BadRequest("header must include Panel and Item LOINC columns");

    if (purge) await PurgeTableAsync("[catalog].[loinc_panel_item]");

        var existing = new HashSet<string>((await _db.LoincPanelItem.AsNoTracking().Select(x => new { x.PanelLoincNum, x.ItemLoincNum }).ToListAsync())
            .Select(x => $"{x.PanelLoincNum.ToUpperInvariant()}||{x.ItemLoincNum.ToUpperInvariant()}"));
        var pending = new HashSet<string>();
        var toAdd = new List<LoincPanelItem>(8192);
        var res = new ImportResult { Version = version };
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) { res.Skipped++; continue; }
            var cols = delim == ',' ? SplitCsvLine(line) : line.Split(delim);
            if (cols.Length <= Math.Max(idxPanel, idxItem)) { res.Skipped++; continue; }
            var panel = TrimQuotes(cols[idxPanel].Trim());
            var item = TrimQuotes(cols[idxItem].Trim());
            if (string.IsNullOrWhiteSpace(panel) || string.IsNullOrWhiteSpace(item)) { res.Skipped++; continue; }
            var key = $"{panel.ToUpperInvariant()}||{item.ToUpperInvariant()}";
            if (existing.Contains(key) || !pending.Add(key)) { res.Skipped++; continue; }
            int? ord = null;
            if (idxOrdinal >= 0 && idxOrdinal < cols.Length)
            {
                var vo = TrimQuotes(cols[idxOrdinal].Trim());
                if (int.TryParse(vo, out var o)) ord = o;
            }
            string? opt = (idxOptional >= 0 && idxOptional < cols.Length) ? TrimQuotes(cols[idxOptional].Trim()) : null;
            toAdd.Add(new LoincPanelItem { PanelLoincNum = panel, ItemLoincNum = item, Ordinal = ord, Optionality = opt });
            if (toAdd.Count >= 5000) { _db.LoincPanelItem.AddRange(toAdd); await _db.SaveChangesAsync(); res.Inserted += toAdd.Count; toAdd.Clear(); }
        }
        if (toAdd.Count > 0) { _db.LoincPanelItem.AddRange(toAdd); await _db.SaveChangesAsync(); res.Inserted += toAdd.Count; toAdd.Clear(); }

        return Ok(res);
    }

    [HttpPost("loinc-panels-and-forms")]
    [Authorize(Policy = "CatalogImport")]
    [RequestSizeLimit(500_000_000)]
    public async Task<IActionResult> ImportLoincPanelsAndForms([FromQuery] string version, IFormFile file, [FromQuery] bool purge = false)
    {
        if (string.IsNullOrWhiteSpace(version)) return BadRequest("version is required");
        if (file == null || file.Length == 0) return BadRequest("file is required");

        if (!await _db.Releases.AnyAsync(r => r.System == SystemLoinc && r.Version == version))
        {
            _db.Releases.Add(new CatalogRelease { System = SystemLoinc, Version = version, ReleasedOn = DateTime.UtcNow, Description = $"LOINC PanelsAndForms: {file.FileName}" });
            await _db.SaveChangesAsync();
        }

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var headerLine = await reader.ReadLineAsync();
        if (headerLine == null) return BadRequest("empty file");
        // Reuse a flexible header matcher
    var (delim, idxPanel, idxItem, idxOrdinal, idxOptional) = GetPanelItemHeader(headerLine);
        // Try to find a name column for panel (optional)
    if (idxPanel < 0 || idxItem < 0) return BadRequest("header must include panel and item LOINC columns");

        if (purge)
        {
            await PurgeTableAsync("[catalog].[loinc_panel_item]");
            await PurgeTableAsync("[catalog].[loinc_panel]");
        }

        var existingPanels = await _db.LoincPanel.AsNoTracking().ToDictionaryAsync(x => x.PanelLoincNum, x => x);
        var existingItems = new HashSet<string>((await _db.LoincPanelItem.AsNoTracking().Select(x => new { x.PanelLoincNum, x.ItemLoincNum }).ToListAsync())
            .Select(x => $"{x.PanelLoincNum.ToUpperInvariant()}||{x.ItemLoincNum.ToUpperInvariant()}"));

        var seenPanels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pendingItems = new HashSet<string>();
        var toInsertPanels = new List<LoincPanel>(4096);
        var toInsertItems = new List<LoincPanelItem>(8192);
        var res = new ImportResult { Version = version };

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) { res.Skipped++; continue; }
            var cols = delim == ',' ? SplitCsvLine(line) : line.Split(delim);
            if (cols.Length <= Math.Max(idxPanel, idxItem)) { res.Skipped++; continue; }
            var panel = TrimQuotes(cols[idxPanel].Trim());
            var item = TrimQuotes(cols[idxItem].Trim());
            if (string.IsNullOrWhiteSpace(panel) || string.IsNullOrWhiteSpace(item)) { res.Skipped++; continue; }

            // Panels: capture once
            if (seenPanels.Add(panel))
            {
                if (!existingPanels.ContainsKey(panel))
                {
                    toInsertPanels.Add(new LoincPanel { PanelLoincNum = panel });
                }
            }

            // Panel items
            var key = $"{panel.ToUpperInvariant()}||{item.ToUpperInvariant()}";
            if (!existingItems.Contains(key) && pendingItems.Add(key))
            {
                int? ord = null;
                if (idxOrdinal >= 0 && idxOrdinal < cols.Length)
                {
                    var vo = TrimQuotes(cols[idxOrdinal].Trim());
                    if (int.TryParse(vo, out var o)) ord = o;
                }
                string? opt = (idxOptional >= 0 && idxOptional < cols.Length) ? TrimQuotes(cols[idxOptional].Trim()) : null;
                toInsertItems.Add(new LoincPanelItem { PanelLoincNum = panel, ItemLoincNum = item, Ordinal = ord, Optionality = opt });
            }

            if (toInsertPanels.Count + toInsertItems.Count >= 5000)
            {
                if (toInsertPanels.Count > 0) { _db.LoincPanel.AddRange(toInsertPanels); res.Inserted += toInsertPanels.Count; toInsertPanels.Clear(); await _db.SaveChangesAsync(); }
                if (toInsertItems.Count > 0) { _db.LoincPanelItem.AddRange(toInsertItems); res.Inserted += toInsertItems.Count; toInsertItems.Clear(); await _db.SaveChangesAsync(); }
            }
        }

        if (toInsertPanels.Count > 0) { _db.LoincPanel.AddRange(toInsertPanels); res.Inserted += toInsertPanels.Count; toInsertPanels.Clear(); await _db.SaveChangesAsync(); }
        if (toInsertItems.Count > 0) { _db.LoincPanelItem.AddRange(toInsertItems); res.Inserted += toInsertItems.Count; toInsertItems.Clear(); await _db.SaveChangesAsync(); }

        return Ok(res);
    }

    [HttpPost("loinc-consumer-names")]
    [Authorize(Policy = "CatalogImport")]
    [RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> ImportLoincConsumerNames([FromQuery] string version, IFormFile file, [FromQuery] bool purge = false)
    {
        if (string.IsNullOrWhiteSpace(version)) return BadRequest("version is required");
        if (file == null || file.Length == 0) return BadRequest("file is required");

        if (!await _db.Releases.AnyAsync(r => r.System == SystemLoinc && r.Version == version))
        {
            _db.Releases.Add(new CatalogRelease { System = SystemLoinc, Version = version, ReleasedOn = DateTime.UtcNow, Description = $"LOINC ConsumerName: {file.FileName}" });
            await _db.SaveChangesAsync();
        }

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var headerLine = await reader.ReadLineAsync();
        if (headerLine == null) return BadRequest("empty file");
        var (delim, idxLoinc, idxName, idxLang) = GetConsumerNameHeader(headerLine);
        if (idxLoinc < 0 || idxName < 0) return BadRequest("header must include LOINC_NUM and CONSUMER_NAME");

    if (purge) await PurgeTableAsync("[catalog].[loinc_consumer_name]");

        var existing = new HashSet<string>((await _db.LoincConsumerName.AsNoTracking().Select(x => new { x.LoincNum, x.ConsumerName, x.Language }).ToListAsync())
            .Select(x => $"{x.LoincNum.ToUpperInvariant()}||{x.ConsumerName.ToUpperInvariant()}||{(x.Language ?? string.Empty).ToUpperInvariant()}"));
        var pending = new HashSet<string>();
        var toAdd = new List<LoincConsumerName>(8192);
        var res = new ImportResult { Version = version };

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) { res.Skipped++; continue; }
            var cols = delim == ',' ? SplitCsvLine(line) : line.Split(delim);
            if (cols.Length <= Math.Max(idxLoinc, idxName)) { res.Skipped++; continue; }
            var loinc = TrimQuotes(cols[idxLoinc].Trim());
            var name = TrimQuotes(cols[idxName].Trim());
            var lang = (idxLang >= 0 && idxLang < cols.Length) ? TrimQuotes(cols[idxLang].Trim()) : null;
            if (string.IsNullOrWhiteSpace(loinc) || string.IsNullOrWhiteSpace(name)) { res.Skipped++; continue; }
            var key = $"{loinc.ToUpperInvariant()}||{name.ToUpperInvariant()}||{(lang ?? string.Empty).ToUpperInvariant()}";
            if (existing.Contains(key) || !pending.Add(key)) { res.Skipped++; continue; }
            toAdd.Add(new LoincConsumerName { LoincNum = loinc, ConsumerName = name, Language = string.IsNullOrWhiteSpace(lang) ? null : lang });
            if (toAdd.Count >= 5000) { _db.LoincConsumerName.AddRange(toAdd); await _db.SaveChangesAsync(); res.Inserted += toAdd.Count; toAdd.Clear(); }
        }
        if (toAdd.Count > 0) { _db.LoincConsumerName.AddRange(toAdd); await _db.SaveChangesAsync(); res.Inserted += toAdd.Count; toAdd.Clear(); }

        if (!await _db.Releases.AnyAsync(r => r.System == SystemLoinc && r.Version == version))
        {
            _db.Releases.Add(new CatalogRelease { System = SystemLoinc, Version = version, ReleasedOn = DateTime.UtcNow });
            await _db.SaveChangesAsync();
        }
        return Ok(res);
    }

    private static (char delim, int idxLoinc, int idxName, int idxLang) GetConsumerNameHeader(string header)
    {
        var delim = header.Contains('\t') ? '\t' : ',';
        var cols = delim == ',' ? SplitCsvLine(header) : header.Split(delim);
        static string N(string s) => new string((s ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        var norm = cols.Select(N).ToArray();
        int find(params string[] keys)
        {
            for (int i = 0; i < norm.Length; i++) if (keys.Contains(norm[i])) return i;
            return -1;
        }
        int idxLoinc = find("LOINCNUM", "LOINC", "LOINCNUMBER", "LOINC_NUM");
        int idxName = find("CONSUMERNAME", "CONSUMER_NAME", "NAME");
        int idxLang = find("LANG", "LANGUAGE");
        return (delim, idxLoinc, idxName, idxLang);
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

    private async Task PurgeTableAsync(string qualifiedTable)
    {
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "[catalog].[icd10]",
            "[catalog].[loinc]",
            "[catalog].[loinc_map_to]",
            "[catalog].[loinc_answer_list]",
            "[catalog].[loinc_answer_link]",
            "[catalog].[loinc_panel]",
            "[catalog].[loinc_panel_item]",
            "[catalog].[loinc_consumer_name]",
            "[catalog].[atc]"
        };
        if (!allowed.Contains(qualifiedTable)) throw new InvalidOperationException("Invalid table for purge.");
        try
        {
            await _db.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {qualifiedTable}");
            return;
        }
        catch
        {
            // Fallback to batched delete
        }
        while (true)
        {
            var rows = await _db.Database.ExecuteSqlRawAsync($"DELETE TOP (100000) FROM {qualifiedTable}");
            if (rows <= 0) break;
        }
    }

    // =============== ATC / DDD ===============

    private sealed class AtcHeaderInfo
    {
        public required char Delimiter { get; init; }
        public required int IdxCode { get; init; }
        public required int IdxName { get; init; }
        public int IdxDdd { get; init; } = -1;
        public int IdxUom { get; init; } = -1;
        public int IdxAdmR { get; init; } = -1;
        public int IdxNote { get; init; } = -1;
    }

    private static AtcHeaderInfo? GetAtcHeader(string header)
    {
        var delim = header.Contains('\t') ? '\t' : ',';
        var cols = delim == ',' ? SplitCsvLine(header) : header.Split(delim);
        static string N(string s) => new string((s ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        var norm = cols.Select(N).ToArray();
        int find(params string[] keys) { for (int i = 0; i < norm.Length; i++) if (keys.Contains(norm[i])) return i; return -1; }
        int idxCode = find("ATCCODE", "CODE", "ATC");
        int idxName = find("ATCNAME", "NAME", "DESCRIPTION");
        if (idxCode < 0 || idxName < 0) return null;
        int idxDdd = find("DDD", "DEFINEDDAILYDOSE");
        int idxUom = find("UOM", "UNIT", "UNITOFMEASURE");
        int idxAdmR = find("ADMR", "ADMINISTRATIONROUTE", "ROUTE");
        int idxNote = find("NOTE", "NOTES");
        return new AtcHeaderInfo { Delimiter = delim, IdxCode = idxCode, IdxName = idxName, IdxDdd = idxDdd, IdxUom = idxUom, IdxAdmR = idxAdmR, IdxNote = idxNote };
    }

    private sealed record AtcRow(string Code, string Name, decimal? Ddd, string? Uom, string? AdmR, string? Note);

    private static bool TryBuildAtcRow(string line, AtcHeaderInfo h, out AtcRow row)
    {
        row = new AtcRow("", "", null, null, null, null);
        if (string.IsNullOrWhiteSpace(line)) return false;
        var cols = h.Delimiter == ',' ? SplitCsvLine(line) : line.Split(h.Delimiter);
        if (cols.Length <= Math.Max(h.IdxCode, h.IdxName)) return false;
        var code = TrimQuotes(cols[h.IdxCode].Trim());
        var name = TrimQuotes(cols[h.IdxName].Trim());
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)) return false;
        decimal? ddd = null;
        if (h.IdxDdd >= 0 && h.IdxDdd < cols.Length)
        {
            var s = TrimQuotes(cols[h.IdxDdd].Trim());
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var val)) ddd = val;
        }
        string? pick(int i) => i >= 0 && i < cols.Length ? TrimQuotes(cols[i].Trim()) : null;
        row = new AtcRow(code, name, ddd, pick(h.IdxUom), pick(h.IdxAdmR), pick(h.IdxNote));
        return true;
    }

    [HttpPost("atc")]
    [Authorize(Policy = "CatalogImport")]
    [RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> ImportAtc([FromQuery] string version, IFormFile file, [FromQuery] bool purge = false)
    {
        if (string.IsNullOrWhiteSpace(version)) return BadRequest("version is required, e.g., 2024-07-31");
        if (file == null || file.Length == 0) return BadRequest("file is required");

        if (!await _db.Releases.AnyAsync(r => r.System == SystemAtc && r.Version == version))
        {
            _db.Releases.Add(new CatalogRelease { System = SystemAtc, Version = version, ReleasedOn = DateTime.UtcNow, Description = $"WHO ATC/DDD: {file.FileName}" });
            await _db.SaveChangesAsync();
        }

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var headerLine = await reader.ReadLineAsync();
        if (headerLine == null) return BadRequest("empty file");
        var header = GetAtcHeader(headerLine);
        if (header == null) return BadRequest("header must include atc_code and atc_name");

        if (purge) await PurgeTableAsync("[catalog].[atc]");

        var existing = await _db.Atc.AsNoTracking().ToDictionaryAsync(x => x.AtcCode, x => x);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var toInsert = new List<AtcEntry>(4096);
        var toUpdate = new List<AtcEntry>(1024);
        var res = new ImportResult { Version = version };

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (!TryBuildAtcRow(line, header, out var row)) { res.Skipped++; continue; }
            if (!seen.Add(row.Code)) { res.Skipped++; continue; }
            if (existing.TryGetValue(row.Code, out var ex))
            {
                if (ex.AtcName != row.Name || ex.Ddd != row.Ddd || ex.Uom != row.Uom || ex.AdmR != row.AdmR || ex.Note != row.Note)
                {
                    ex.AtcName = row.Name; ex.Ddd = row.Ddd; ex.Uom = row.Uom; ex.AdmR = row.AdmR; ex.Note = row.Note; toUpdate.Add(ex);
                }
                else { res.Skipped++; }
            }
            else
            {
                toInsert.Add(new AtcEntry { AtcCode = row.Code, AtcName = row.Name, Ddd = row.Ddd, Uom = row.Uom, AdmR = row.AdmR, Note = row.Note });
            }
            if (toInsert.Count + toUpdate.Count >= 5000) await FlushAtcAsync(toInsert, toUpdate, res);
        }
        await FlushAtcAsync(toInsert, toUpdate, res);
        return Ok(res);
    }

    private async Task FlushAtcAsync(List<AtcEntry> toInsert, List<AtcEntry> toUpdate, ImportResult res)
    {
        if (toInsert.Count > 0)
        {
            _db.Atc.AddRange(toInsert); res.Inserted += toInsert.Count; toInsert.Clear(); await _db.SaveChangesAsync();
        }
        if (toUpdate.Count > 0)
        {
            _db.Atc.UpdateRange(toUpdate); res.Updated += toUpdate.Count; toUpdate.Clear(); await _db.SaveChangesAsync();
        }
    }
}
