using Microsoft.EntityFrameworkCore;
using MedicalCatalogService.Models;

namespace MedicalCatalogService.Data;

public static class MockDataSeeder
{
    public static async Task SeedAsync(MedicalCatalogDbContext db)
    {
        int created = 0;

        // Seed ICD-10 codes (common diagnosis codes)
        var icd10Codes = new[]
        {
            ("E11.9", "Type 2 diabetes mellitus without complications"),
            ("I10", "Essential (primary) hypertension"),
            ("J06.9", "Acute upper respiratory infection, unspecified"),
            ("M54.5", "Low back pain"),
            ("G43.909", "Migraine, unspecified, not intractable, without status migrainosus"),
            ("K21.0", "Gastro-esophageal reflux disease with esophagitis"),
            ("F32.9", "Major depressive disorder, single episode, unspecified")
        };

        var existingIcd10 = await db.Icd10.Select(c => c.Code).ToHashSetAsync();
        foreach (var (code, title) in icd10Codes)
        {
            if (!existingIcd10.Contains(code))
            {
                db.Icd10.Add(new Icd10
                {
                    Code = code,
                    Title = title,
                    EffectiveFrom = new DateTime(2024, 1, 1),
                    Status = "Active"
                });
                created++;
            }
        }

        // Seed LOINC entries (common lab test codes)
        var loincEntries = new (string loincNum, string longName, string shortName, string component, string property)[]
        {
            ("718-7", "Hemoglobin [Mass/volume] in Blood", "Hemoglobin", "Hemoglobin", "MCnc"),
            ("4544-3", "Hematocrit [Volume Fraction] of Blood by Automated count", "Hematocrit", "Hematocrit", "VFr"),
            ("789-8", "Erythrocytes [#/volume] in Blood", "RBC", "Erythrocytes", "NCnc"),
            ("2160-0", "Creatinine [Mass/volume] in Serum or Plasma", "Creatinine", "Creatinine", "MCnc"),
            ("2345-7", "Glucose [Mass/volume] in Serum or Plasma", "Glucose", "Glucose", "MCnc"),
            ("2951-2", "Sodium [Moles/volume] in Serum or Plasma", "Sodium", "Sodium", "SCnc"),
            ("2823-3", "Potassium [Moles/volume] in Serum or Plasma", "Potassium", "Potassium", "SCnc")
        };

        var existingLoinc = await db.Loinc.Select(l => l.LoincNum).ToHashSetAsync();
        foreach (var (loincNum, longName, shortName, component, property) in loincEntries)
        {
            if (!existingLoinc.Contains(loincNum))
            {
                db.Loinc.Add(new LoincEntry
                {
                    LoincNum = loincNum,
                    LongCommonName = longName,
                    ShortName = shortName,
                    Component = component,
                    Property = property,
                    TimeAspect = "Pt",
                    System = "Bld",
                    ScaleType = "Qn",
                    Class = "CHEM",
                    Status = "Active"
                });
                created++;
            }
        }

        // Seed ATC entries (common medication codes)
        var atcEntries = new (string atcCode, string atcName, decimal? ddd, string? uom, string? admR)[]
        {
            ("A10BA02", "Metformin", 2000m, "mg", "O"),
            ("C09AA01", "Captopril", 50m, "mg", "O"),
            ("C10AA01", "Simvastatin", 30m, "mg", "O"),
            ("M01AE01", "Ibuprofen", 1200m, "mg", "O"),
            ("N02BE01", "Paracetamol", 3000m, "mg", "O"),
            ("J01CA04", "Amoxicillin", 1500m, "mg", "O"),
            ("R06AX13", "Loratadine", 10m, "mg", "O")
        };

        var existingAtc = await db.Atc.Select(a => a.AtcCode).ToHashSetAsync();
        foreach (var (atcCode, atcName, ddd, uom, admR) in atcEntries)
        {
            if (!existingAtc.Contains(atcCode))
            {
                db.Atc.Add(new AtcEntry
                {
                    AtcCode = atcCode,
                    AtcName = atcName,
                    Ddd = ddd,
                    Uom = uom,
                    AdmR = admR
                });
                created++;
            }
        }

        // Seed Catalog Releases
        var releases = new[]
        {
            ("icd10", "2024-10", new DateTime(2024, 10, 1), "ICD-10-CM 2024 October Update"),
            ("loinc", "2.77", new DateTime(2024, 6, 1), "LOINC Version 2.77"),
            ("atc", "2024", new DateTime(2024, 1, 1), "WHO ATC/DDD 2024")
        };

        var existingReleases = await db.Releases
            .Select(r => new { r.System, r.Version })
            .ToListAsync();
        var existingReleaseSet = existingReleases.Select(x => (x.System, x.Version)).ToHashSet();

        foreach (var (system, version, releasedOn, description) in releases)
        {
            if (!existingReleaseSet.Contains((system, version)))
            {
                db.Releases.Add(new CatalogRelease
                {
                    Id = Guid.NewGuid(),
                    System = system,
                    Version = version,
                    ReleasedOn = releasedOn,
                    Description = description
                });
                created++;
            }
        }

        // Seed LOINC Panels (common lab panels)
        var panels = new[]
        {
            "24357-6", // CBC panel
            "24323-8", // Comprehensive metabolic panel
            "57698-3"  // Lipid panel in Serum or Plasma
        };

        var existingPanels = await db.LoincPanel.Select(p => p.PanelLoincNum).ToHashSetAsync();
        foreach (var panelLoinc in panels)
        {
            if (!existingPanels.Contains(panelLoinc))
            {
                db.LoincPanel.Add(new LoincPanel
                {
                    Id = Guid.NewGuid(),
                    PanelLoincNum = panelLoinc
                });
                created++;
            }
        }

        // Seed LOINC Panel Items (link tests to panels)
        var panelItems = new (string panelLoinc, string itemLoinc, int ordinal)[]
        {
            ("24357-6", "718-7", 1),
            ("24357-6", "4544-3", 2),
            ("24357-6", "789-8", 3),
            ("24323-8", "2160-0", 1),
            ("24323-8", "2345-7", 2),
            ("24323-8", "2951-2", 3),
            ("24323-8", "2823-3", 4)
        };

        var existingPanelItems = await db.LoincPanelItem
            .Select(pi => new { pi.PanelLoincNum, pi.ItemLoincNum })
            .ToListAsync();
        var existingPanelItemSet = existingPanelItems.Select(x => (x.PanelLoincNum, x.ItemLoincNum)).ToHashSet();

        foreach (var (panelLoinc, itemLoinc, ordinal) in panelItems)
        {
            if (!existingPanelItemSet.Contains((panelLoinc, itemLoinc)))
            {
                db.LoincPanelItem.Add(new LoincPanelItem
                {
                    Id = Guid.NewGuid(),
                    PanelLoincNum = panelLoinc,
                    ItemLoincNum = itemLoinc,
                    Ordinal = ordinal,
                    Optionality = "R" // Required
                });
                created++;
            }
        }

        // Seed LOINC Consumer Names (patient-friendly test names)
        var consumerNames = new (string loincNum, string consumerName)[]
        {
            ("718-7", "Hemoglobin Blood Test"),
            ("4544-3", "Hematocrit Blood Test"),
            ("789-8", "Red Blood Cell Count"),
            ("2160-0", "Kidney Function - Creatinine"),
            ("2345-7", "Blood Sugar Level"),
            ("2951-2", "Sodium Blood Level"),
            ("2823-3", "Potassium Blood Level")
        };

        var existingConsumerNames = await db.LoincConsumerName
            .Select(cn => cn.LoincNum)
            .ToHashSetAsync();

        foreach (var (loincNum, consumerName) in consumerNames)
        {
            if (!existingConsumerNames.Contains(loincNum))
            {
                db.LoincConsumerName.Add(new LoincConsumerName
                {
                    Id = Guid.NewGuid(),
                    LoincNum = loincNum,
                    ConsumerName = consumerName,
                    Language = "en"
                });
                created++;
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
        }
    }
}

