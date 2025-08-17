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

## Messaging (User Registered ? Patient auto-create)

- Consumes from RabbitMQ exchange `user.events` with routing key `user.created`.
- Queue: `patient.user-registered` (durable), bound to `user.events`.
- Dead-lettering: DLX `patient.dlx` routes to DLQ `patient.user-registered.dlq`.
- Bounded retry: on handler errors, message is republished with `x-retry` header up to 3 times, then dead-lettered.

Idempotency

- Patient is upserted by unique `UserId`.
- `patient.Patient_Status` has column `IdempotencyKey` with a unique filtered index; the consumer uses the RabbitMQ `MessageId` (outbox id) or a fallback `UserId:OccurredAt` to avoid duplicates on redelivery.

Environment

- `RABBITMQ__HOST` (default `rabbitmq`)
- `RABBITMQ__USERNAME` (default `guest`)
- `RABBITMQ__PASSWORD` (default `guest`)

Quick validation

1) Bring up `rabbitmq`, `user-service`, and `patient-service` via docker-compose.
2) Register a user in UserService; PatientService should create one Patient and one Patient_Status (Active).
3) Republish the same event (same MessageId) and confirm Patient_Status isn’t duplicated.
4) Publish an invalid message to `user.events`/`user.created` to observe retries and final routing to the DLQ.
