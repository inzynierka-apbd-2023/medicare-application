# Medicare Application - Local Development Guide

This guide explains how to run the Medicare application locally using **.NET Aspire** for orchestration.

---

## Prerequisites

1. **Docker Desktop** - Running and healthy
2. **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
3. **Node.js 20+** & **npm** - [Download](https://nodejs.org/)
4. **Visual Studio 2022** or **VS Code** with C# Dev Kit (optional)

Verify your setup:
```powershell
docker --version    # Should show Docker version
dotnet --version    # Should show 9.x.x
node --version      # Should show v20.x.x or higher
```

---

## Quick Start

### Step 1: Start Backend Services with Aspire

```powershell
cd backend-container/Medicare.AppHost
dotnet run
```

**What happens automatically:**
- 🐳 SQL Server container starts
- 📦 11 databases are created (one per service)
- 🐰 RabbitMQ container starts with management UI
- 🚀 All 13 microservices start
- 🔄 Database migrations run automatically on each service

### Step 2: Open Aspire Dashboard

The dashboard opens at: **http://localhost:15015**

Here you can:
- Monitor all services (green = healthy)
- View logs for each service
- **Find service URLs** (click on a service to see its endpoint)

### Step 3: Start Frontend

```powershell
cd frontend-container/medicare-frontend
npm install
npm run dev
```

Frontend runs at: **http://localhost:5173**

---

## Service Architecture

| Service | Database | Purpose |
|---------|----------|---------|
| UserService | UserServiceDb | Authentication & user management |
| PractitionerService | PractitionerServiceDb | Doctor profiles & schedules |
| PatientService | PatientServiceDb | Patient profiles |
| MedicalCatalogService | MedicalCatalogDb | LOINC codes & medical catalog |
| BillingService | BillingDb | Invoices & payments |
| DocumentsService | DocumentsDb | Medical documents |
| AppointmentService | AppointmentDb | Appointment scheduling |
| MedicalRecordsService | MedicalRecordsDb | Patient medical records |
| LabService | LabDb | Lab orders & results |
| NotificationService | NotificationDb | Email/SMS notifications |
| MessagingService | MessagingDb | In-app messaging |
| ArchiveService | (none) | Document archival via RabbitMQ |
| PdfService | (none) | PDF generation via RabbitMQ |

---

## Configuration

### Frontend API URLs (Optional)

If Aspire assigns different ports than the defaults, create `.env.local` in the frontend directory:

```env
VITE_API_URL=http://localhost:XXXXX
VITE_NOTIFICATION_URL=http://localhost:XXXXX
VITE_APPOINTMENT_URL=http://localhost:XXXXX
```

Find the actual URLs in the Aspire Dashboard.

### JWT Secret

The JWT secret for local development is pre-configured in:
`backend-container/Medicare.AppHost/appsettings.json`

⚠️ **Never use this secret in production!**

---

## Useful URLs

| Resource | URL |
|----------|-----|
| Aspire Dashboard | http://localhost:15015 |
| Frontend | http://localhost:5173 |
| RabbitMQ Management | Check Aspire Dashboard |
| Swagger (per service) | `{service-url}/swagger` |
| Health Check (per service) | `{service-url}/health` |

---

## Troubleshooting

### Docker not running
```
Error: Cannot connect to Docker daemon
```
**Solution:** Start Docker Desktop and wait until it shows "Docker is running"

### Port conflicts
```
Error: Address already in use
```
**Solution:** Stop other services using those ports or restart Docker

### Database connection errors
```
Error: Cannot open database
```
**Solution:** Wait 30-60 seconds for SQL Server container to fully initialize, then restart the AppHost

### Migrations failed
Check service logs in Aspire Dashboard for specific error messages. Each service retries migrations up to 10 times with 5-second delays.

### Frontend can't connect to backend
1. Check Aspire Dashboard - ensure services are green
2. Verify the correct URLs in `.env.local`
3. Check browser console for CORS errors

---

## Stopping the Application

1. Press `Ctrl+C` in the Aspire terminal
2. This gracefully stops all services and containers
3. Containers are ephemeral - data is lost when stopped

---

## Test Users

After first startup, the following user is seeded:
- **Username:** `admin`
- **Password:** Shown in UserService logs (temporary, auto-generated)

Check the Aspire Dashboard logs for UserService to find the password.
