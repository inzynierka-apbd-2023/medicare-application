# PractitionerService

Domain microservice for managing doctors, receptionists, medical services and specializations. Built with ASP.NET Core 8, EF Core 8, and SQL Server (shared Azure SQL database) using a dedicated schema and independent migrations.

## URLs (local/dev)

- Swagger UI: <http://localhost:8081/swagger>
- Health: <http://localhost:8081/health>
- API base path: /api/practitioner

## What’s included

- Separate schema: practitioner (tables) and a projection view joining the user service data
- Independent EF Core migrations with history table practitioner.__EFMigrationsHistory
- Azure AD token-based SQL authentication support (DefaultAzureCredential)
- JWT Bearer auth, CORS policy, Swagger/OpenAPI, and health checks
- Rich, idempotent seed data for catalogs and sample practitioners in non-production
- Diagnostics endpoints to validate migrations and schema presence

## Architecture overview

- API: ASP.NET Core minimal hosting with controllers
- Data: EF Core DbContext `PractitionerDbContext`
	- Tables (schema: practitioner):
		- Doctor, Receptionist, Service, Specialization, Doctor_Specialization (join), Doctor_Schedule
	- View: practitioner.DoctorDirectory (keyless) joining [user].[User_Profile] for display/search
- Security: JWT Bearer; public GETs for search/catalog; mutating endpoints require Authorization
- DB: Shared Azure SQL DB with UserService; PractitionerService uses its own schema and migration history

## Configuration

Environment variables (container or local):

- ConnectionStrings__DefaultConnection: SQL Server connection string to the shared DB
- USE_AZURE_DEFAULT_CREDENTIAL: true to use Azure AD token auth; removes user/password from the connection string
- AZURE_TENANT_ID / AZURE_CLIENT_ID / AZURE_CLIENT_SECRET: optional (workload identity/managed identity also supported)
- Jwt__SecretKey: JWT signing key (dev default is in appsettings, change in prod)
- Jwt__Issuer, Jwt__Audience: JWT validation parameters
- Cors__AllowedOrigins: array of allowed origins; ["*"] allows all (no credentials)

See `PractitionerService/Program.cs` for exact reading order and defaults.

## Running locally

- Docker Compose (recommended): brings up UserService, PractitionerService, and Frontend
	- Exposes PractitionerService on port 8081
	- Uses shared DB connection string via env var: USER_SERVICE_CONNECTION
- Direct run: `dotnet run` inside `backend-container/PractitionerService/PractitionerService` with ASPNETCORE_URLS=http://+:8081 and the connection/env vars above

## Endpoints

- Catalog
	- GET /api/practitioner/catalog/services
	- GET /api/practitioner/catalog/specializations
- Doctors
	- POST /api/practitioner/doctors { userId, bio? } (auth required)
	- GET /api/practitioner/doctors?specializationId=&serviceId=&q= (search via DoctorDirectory)
	- PUT /api/practitioner/doctors/{id}/specializations { specializationIds: [] } (auth required)
	- PUT /api/practitioner/doctors/{id}/availability [ { dayOfWeek, start, end } ] (auth required)
- Receptionists
	- POST /api/practitioner/receptionists { userId } (auth required)
- Diagnostics (non-sensitive; intended for non-prod)
	- GET /api/practitioner/diag/migrations -> lists all/applied/pending EF migrations, history table location
	- GET /api/practitioner/diag/schema -> checks presence of practitioner tables and DoctorDirectory view
- Infra
	- GET /health
	- Swagger at /swagger

## Database and migrations

- PractitionerService uses EF Core migrations with history table practitioner.__EFMigrationsHistory and leaves UserService history untouched
- Migrations included:
	- 20250813120000_InitPractitioner: creates practitioner tables and initial DoctorDirectory view
	- 20250813162000_UpdateDoctorDirectoryView_UserSchema: updates view to reference [user].[User_Profile]
	- 20250813163000_SeedPractitionerTestData: adds idempotent inserts for richer sample data and refreshes view
- Startup applies migrations automatically in non-production and then seeds data

Schemas across services:

- UserService maps its tables to schema [user]
- PractitionerService maps to schema [practitioner] and references [user].[User_Profile] in its view

## Seeding strategy

- On first run (non-prod), default catalog (services & specializations) is inserted if empty
- Additional idempotent test data is inserted for Doctors, mappings, schedules, and a sample Receptionist
- View practitioner.DoctorDirectory is refreshed after seeding

## Validation and troubleshooting

- Validate migrations and schema:
	- GET /api/practitioner/diag/migrations
	- GET /api/practitioner/diag/schema
- Common checks:
	- Ensure the connection string points to the same DB as UserService
	- If using AAD token auth, set USE_AZURE_DEFAULT_CREDENTIAL=true and mount ~/.azure in the container
	- For CORS, avoid wildcard with credentials; use explicit origins for cookies/credentials

## Notes

- This service intentionally shares the database with UserService but uses separate schemas and migration history for isolation
- Authentication and Swagger configuration mirror the UserService for consistency
