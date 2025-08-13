# Medicare Application

Monorepo containing:

* UserService (ASP.NET Core 8 Web API + EF Core + Azure AD token-based SQL auth)
* PractitionerService (ASP.NET Core 8 Web API + EF Core, isolated practitioner schema and migrations)
* PatientService (ASP.NET Core 8 Web API + EF Core, isolated patient schema and migrations)
* Frontend (Vite + React + TypeScript, served via Nginx container)

## Quick Start (Docker)

```bash
docker compose build
# Provide required env in .env (see .env.example) including USER_SERVICE_CONNECTION or AAD vars
USE_AZURE_DEFAULT_CREDENTIAL=true AZURE_TENANT_ID=... AZURE_CLIENT_ID=... AZURE_CLIENT_SECRET=... docker compose up -d
```

Frontend: <http://localhost:5173>  
UserService Swagger (dev only): <http://localhost:8080/swagger>
PractitionerService Swagger (dev only): <http://localhost:8081/swagger>
PatientService Swagger (dev only): <http://localhost:8082/swagger>

## Azure AD Token Auth & Logging Inside Docker

The UserService can authenticate to Azure SQL using a Service Principal (recommended for AAD-only databases) via `DefaultAzureCredential` and environment variables.

### Required Environment Variables

| Variable | Description |
|----------|-------------|
| USE_AZURE_DEFAULT_CREDENTIAL | Set `true` to enable token mode (otherwise connection string auth is used). |
| AZURE_TENANT_ID | Directory (tenant) ID of your Azure AD. |
| AZURE_CLIENT_ID | Application (client) ID of the Service Principal. |
| AZURE_CLIENT_SECRET | Client secret for the Service Principal. |
| AZURE_SQL_CONNECTIONSTRING (optional) | Raw connection string without user/password when using AAD. If omitted, falls back to `ConnectionStrings__DefaultConnection` or appsettings. |

Connection string example for AAD token mode (note: no User ID / Password / Authentication clause):

```ini
Server=tcp:<your-server>.database.windows.net,1433;Database=<your-db>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;MultipleActiveResultSets=False
```

### SQL Database Setup (once)

1. Create Service Principal:


   ```bash
   az ad sp create-for-rbac --name medicare-sql-sp --role Reader --scopes /subscriptions/${SUBSCRIPTION_ID}
   ```

   Capture `appId`, `tenant`, `password`.
2. In Azure SQL (master or target DB), create user mapped to the SP object ID:


   ```sql
   CREATE USER [medicare-sql-sp] FROM EXTERNAL PROVIDER;
   ALTER ROLE db_datareader ADD MEMBER [medicare-sql-sp];
   ALTER ROLE db_datawriter ADD MEMBER [medicare-sql-sp];
   ```

   If migrations need to run, temporarily grant `db_ddladmin` during deployment, then remove it.

### Enabling Structured Logging

Currently console logging is enabled by default (writes to Docker logs). To enrich logs:

1. Add Serilog (example):


   ```xml
   <PackageReference Include="Serilog.AspNetCore" Version="8.*" />
   <PackageReference Include="Serilog.Sinks.Console" Version="5.*" />
   ```
   
2. In `Program.cs` (before `builder.Build()`):


   ```csharp
   using Serilog;
   Log.Logger = new LoggerConfiguration()
      .Enrich.FromLogContext()
      .WriteTo.Console()
      .CreateLogger();
   builder.Host.UseSerilog();
   ```

3. (Optional) Send logs to Azure Monitor / Log Analytics:
   * Add `Serilog.Sinks.AzureAnalytics` or use Azure Diagnostic Settings on the container host / AKS.

### Viewing Logs

Container logs:

```bash
docker compose logs -f user-service
```

Look for `[Startup]` lines: they show connection source & normalization actions. No secrets (passwords) are logged.

### Azure AD Authentication Flow Summary

1. `USE_AZURE_DEFAULT_CREDENTIAL=true` triggers connection string normalization (credentials + Authentication removed).
2. `DefaultAzureCredential` acquires token using SP env vars inside the container.
3. EF Core uses a `SqlConnection` with `AccessToken` set; no password ever traverses the wire.

### Local Developer Setup (Windows / Docker Desktop)

1. Copy `.env.example` to `.env` and fill SP vars.
2. Ensure local IP/firewall allowed in Azure SQL or use Private Endpoint.
3. Run `docker compose up -d`.
4. Inspect logs to confirm: `Normalized connection string for AAD token` & migrations applied.

### Rotating Secrets

Rotate `AZURE_CLIENT_SECRET` in Azure Portal / CLI, update `.env`, and recreate container:

```bash
docker compose up -d --force-recreate --no-deps user-service
```

## Frontend Auth Integration

* Register & Login forms call `/api/auth/register` and `/api/auth/login` via Nginx proxy (`/api/*` -> user-service).
* JWT token stored in `localStorage` as `authToken`.
* Protected routes enforce presence of token.

## Development Tips

| Action | Command |
|--------|---------|
| Rebuild all | `docker compose build --parallel` |
| Tail logs | `docker compose logs -f user-service` |
| Run only frontend | `docker compose up -d frontend` |
| Apply migrations manually | Inside container: `dotnet ef database update` (if tools added) |

## Security Notes

* Do not commit `.env` or Azure CLI tokens.
* Limit SP to least-privilege roles; drop `db_ddladmin` after schema stabilized.
* Consider Azure Key Vault for secret injection (Managed Identity) in future.

---

For further database seed scripts see `docs/sql-seed/`.

## Practitioner Service

`PractitionerService` (under `backend-container/PractitionerService`) owns doctor & receptionist domain data in its own schema `practitioner` inside the shared database. It exposes:

* Doctors catalog & search: `GET /api/doctors?specializationId=&serviceId=&q=`
* Doctor registration: `POST /api/doctors`
* Manage doctor specializations: `PUT /api/doctors/{id}/specializations`
* Manage recurring availability: `PUT /api/doctors/{id}/availability` & fetch via `GET /api/doctors/{id}/availability`
* Receptionist registry: `POST /api/receptionists`
* Services & Specializations catalog: `GET /api/catalog/services`, `GET /api/catalog/specializations?serviceId=`

Events (stored in Outbox for future dispatch): `DoctorRegistered`, `DoctorSpecializationUpdated`, `DoctorAvailabilityChanged`, `ReceptionistRegistered`.

The initial migration also creates a simple `practitioner.DoctorDirectory` view (placeholder for a richer projection joining user profile data once cross-service integration is implemented). Each service uses isolated migrations (different schema) to avoid conflicts while sharing the physical Azure SQL database.
