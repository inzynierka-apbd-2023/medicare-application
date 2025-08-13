# PatientService

Owns the Patient domain: Patient, Emergency_Contact, Insurance, Patient_Status. Provides registration, status changes, emergency contact management, and insurance updates. Uses Azure SQL (shared DB) with a separate `patient` schema and independent EF Core migrations (history at `patient.__EFMigrationsHistory`).

URLs (local):

- Swagger: <http://localhost:8082/swagger>
- Health: <http://localhost:8082/health>
- API base: /api/patient

Key endpoints:

- POST /api/patient/patients { userId, primaryDoctorId? }
- PUT /api/patient/patients/{id}/status { status }
- PUT /api/patient/patients/{id}/emergency-contacts [ { name, relation?, phone? } ]
- PUT /api/patient/patients/{id}/insurance { provider?, policyNumber?, validFrom?, validTo? }
- GET /api/patient/diag/migrations; GET /api/patient/diag/schema

Projection view `patient.PatientOverview` joins [user].[User_Profile].
