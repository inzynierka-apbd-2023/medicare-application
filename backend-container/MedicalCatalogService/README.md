# MedicalCatalogService

Reference data service for clinical conditions and lab test types. Owns schema `catalog` in shared Azure SQL DB with independent EF Core migrations (history table `catalog.__EFMigrationsHistory`).

Endpoints:

- GET /api/catalog/conditions?q=
- GET /api/catalog/lab-tests?q=
- POST /api/catalog/conditions (JWT)
- POST /api/catalog/lab-tests (JWT)
- GET /api/catalog/icd10?q=
- POST /api/catalog/import/icd10?version=YYYY-MM-DD[&purge=true] (JWT)
- GET /api/catalog/diag/migrations
- GET /api/catalog/diag/schema

Docker: listens on 8083.

ICD-10 Import

- Auth: POST /api/catalog/import/icd10 requires a valid JWT. Temporary AllowAnonymous used during bootstrap has been removed.
- Format: CSV or TSV with headers including at least code and title (or desc/description). Optional columns: effective_from, effective_to, status.
- Behavior:
	- Auto-detects delimiter (tab or comma) and parses quoted values correctly.
	- Skips empty rows and duplicate codes within the same upload.
	- Upserts existing codes only when values change; use purge=true to clear catalog.icd10 before import.
	- Records a release row in catalog.release for the provided version.

Scripts

- scripts/enrich-icd10-csv.ps1: enriches an input CSV to include columns effective_from/effective_to/status. Usage:
	- powershell -File scripts/enrich-icd10-csv.ps1 -InputPath <in.csv> -OutputPath <out.csv> -EffectiveFrom 2025-10-01 -Status active
- scripts/convert-icd10-to-csv.ps1: converts a space-delimited text list (code title...) into a CSV with code,title columns.

Test data

- testdata/test1.csv and testdata/test2.tsv include edge cases (quotes, commas, Unicode, duplicates) used to validate the importer.
