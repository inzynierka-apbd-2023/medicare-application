# Changelog

All notable changes to this project will be documented in this file.

## 2025-08-14

- feat(catalog): add MedicalCatalogService with independent EF Core migrations and schema `catalog`
  - Entities: Icd10, SnomedConcept, LoincEntry, CptCode, HcpcsCode, CodeMapping, CatalogRelease, plus app-level Medical_Condition and Lab_Test_Type.
  - Endpoints for browsing/searching and diagnostics; integrated into solution, compose, and reverse proxy at /api/catalog/.
- feat(catalog/import): robust ICD-10 importer with CSV/TSV autodetect, quoted-field parsing, in-file dedupe, and batched upsert
  - POST /api/catalog/import/icd10?version=YYYY-MM-DD[&purge=true] now requires JWT; records catalog.release on success.
  - Skips empty rows and duplicates; upserts only changed rows; optional purge clears catalog.icd10 before import.
- chore(catalog): add scripts/enrich-icd10-csv.ps1 and scripts/convert-icd10-to-csv.ps1; add testdata for importer edge cases
  - Enrichment script adds effective_from/effective_to/status columns; test CSV/TSV cover quotes, commas, Unicode, and duplicates.
  - .gitignore excludes large ICD-10 source folders and enriched artifacts.

## 2025-08-13

- feat(catalog): adopt exact LOINC schema (2.81) with full?text index and complete import suite
  - New exact tables under schema `catalog`: loinc, loinc_map_to, loinc_answer_list, loinc_answer_link, loinc_panel, loinc_panel_item, loinc_consumer_name; full?text index on LongCommonName/Component/ShortName.
  - Importers for LOINC main, MapTo, Answers (AnswerList + Link), Panels & Forms (with Ordinal and Optionality), and ConsumerName.
  - Robust purge via TRUNCATE with batched DELETE fallback; 5k batch inserts; release row logged first.
  - Diagnostics expanded: migrations, schema presence, loinc-stats, mapTo by code, answers by code, panel by code.
- feat(catalog): widen LOINC columns for full dataset
  - DefinitionDescription, ExternalCopyrightNotice, Equation ? NVARCHAR(MAX); System ? NVARCHAR(512).
- fix(catalog): add Description column to catalog.release to unblock diagnostics
  - Updates loinc-stats to include recent releases without query failures.

- feat(practitioner): add PractitionerService (ASP.NET Core 8 Web API) sharing Azure SQL DB with independent EF Core migrations
  - New service under `backend-container/PractitionerService` with schema `practitioner` and migration history table `practitioner.__EFMigrationsHistory`.
  - Domain: Doctor, Receptionist, Service, Specialization, Doctor_Specialization, Doctor_Schedule; projection view `practitioner.DoctorDirectory` joining `[user].[User_Profile]`.
  - JWT auth, Swagger, health checks, and CORS aligned with UserService; added diagnostics endpoints to list migrations and verify schema.
  - Non-prod startup auto-migrates and seeds catalogs plus rich test data (idempotent).
- refactor(userservice): move EF Core tables to schema `[user]` and update PractitionerService view reference
  - Mapped Role, User, and User_Profile to schema `[user]`; created migration to ensure schema and transfer existing tables from `dbo`.
  - Added a safe pre-migration SQL guard in UserService startup to CREATE SCHEMA and ALTER SCHEMA transfer if needed before EF queries.
- fix(cors): correct wildcard + credentials usage across services
  - Use AllowAnyOrigin without credentials for wildcard; require explicit origins when using credentials.
- docs(practitioner): add comprehensive README with architecture, setup, endpoints, migrations, and validation steps.

- feat(patient): add PatientService (ASP.NET Core 8) with independent EF Core migrations and patient schema
  - Domain: Patient, Emergency_Contact, Insurance, Patient_Status; projection view patient.PatientOverview joining [user].[User_Profile].
  - Endpoints for registration, status changes, emergency contacts, and insurance management; diagnostics and health.
  - Uses shared Azure SQL DB with separate history table patient.__EFMigrationsHistory; wired into compose, Nginx, and solution.

## 2025-08-11

- build(frontend): add production Dockerfile, Nginx config, and .dockerignore for SPA hosting
  - Creates containerized build for Vite React app and serves via Nginx with SPA routing.
- build(devops): update docker-compose to include frontend and db-seed services; switch DB image; align ports
  - Adds `frontend` service (Nginx static server) and `db-seed` one-shot job to apply SQL scripts.
  - Switches database image to `azure-sql-edge` for local development.
  - Maps UserService to port 8080 and frontend to 5173:80.
- fix(docker): install curl and create non-root user in UserService image
  - Ensures HEALTHCHECK works (uses curl) and runs the app as an unprivileged user.
- chore(solution): add Visual Studio solution file for UserService
  - Adds `medicare-application.sln` referencing the UserService project.

- feat(userservice): replug to Azure SQL using Azure AD Default; add env configs and resiliency
  - Test/Production appsettings use `Authentication=Active Directory Default` and point to `medicare-db-dev` and `medicare-db`.
  - Program.cs reads `ConnectionStrings:DefaultConnection` or env overrides and enables `EnableRetryOnFailure`.
  - Auto-migrate enabled in non-production only.
- build(db): create initial EF Core migration and apply to dev DB
  - Generated `InitSchema` and updated Azure dev DB using AAD auth.
- chore(azure): verify server/DBs, add firewall, set AAD principal
  - Confirmed `medicareapp-dbserver`/`medicare-db-dev`; created `medicare-db` (prod).
  - Added firewall rules (client IP and 0.0.0.0 for Azure).
  - Created dev AAD user `s25366@pjwstk.edu.pl` with reader/writer/ddladmin.
  

> Notes

> - Compose now seeds the `medicare_dev` database on startup via the `db-seed` service.
> - Frontend is served at <http://localhost:5173> and proxies can be configured later if needed.
