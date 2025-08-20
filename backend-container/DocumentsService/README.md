# DocumentsService

Clinical documents microservice for Medicare Application.

- Schema under `documents` schema with independent migrations history.
- Entities: Document, Document_Type, Visit_Document, Prescription, Referral, Sick_Leave, Lab_Results, Lab_Test_Result, Documents_Assigned.
- APIs:
  - POST /api/documents — create document shell
  - POST /api/documents/{id}/visit-note — attach visit note
  - POST /api/documents/{id}/prescription — attach prescription
  - POST /api/documents/{id}/referral — attach referral
  - POST /api/documents/{id}/sick-leave — attach sick leave
  - POST /api/documents/{id}/lab-results — attach lab results
  - POST /api/documents/{id}/assign — assign to appointment
  - GET  /api/documents/{id} — fetch full aggregate

Environment:
- ConnectionStrings__DefaultConnection (Azure SQL)
- USE_AZURE_DEFAULT_CREDENTIAL=true to use DefaultAzureCredential
- ASPNETCORE_URLS=http://+:8084

Fallbacks:

- If a referenced doctor was deleted from PractitionerService, query ArchiveService at <http://archive-service:8091/archive/doctors/{doctorId}> to hydrate basic identity for rendering historical documents.
