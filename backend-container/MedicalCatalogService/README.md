# MedicalCatalogService

Reference data service for clinical conditions and lab test types. Owns schema `catalog` in shared Azure SQL DB with independent EF Core migrations (history table `catalog.__EFMigrationsHistory`).

Endpoints:

- GET /api/catalog/conditions?q=
- GET /api/catalog/lab-tests?q=
- POST /api/catalog/conditions (JWT)
- POST /api/catalog/lab-tests (JWT)
- GET /api/catalog/diag/migrations
- GET /api/catalog/diag/schema

Docker: listens on 8083.
