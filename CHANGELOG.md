# Changelog

All notable changes to this project will be documented in this file.

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
