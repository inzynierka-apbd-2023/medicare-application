# ArchiveService

Stores archived doctors for historical lookups when a doctor is removed.

- Consumes: `practitioner.events` topic `doctor.remove.requested` ? writes archive ? emits `doctor.archived`.
- API: `GET /archive/doctors/{doctorId}` returns `{ doctorId, fullName, ... }`.
- Config:
  - ConnectionStrings__ArchiveDb: e.g., `Data Source=/data/archive.db`
  - RABBITMQ__HOST/USERNAME/PASSWORD
