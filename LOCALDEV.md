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

## Configuration

### Frontend API Proxying

The frontend uses Vite's proxy feature to route API calls to the correct backend services.
Configuration is in `.env.development` - update ports after Aspire starts:

1. Start Aspire and open the dashboard (http://localhost:18888)
2. Note the ports for each service from the "Endpoints" column
3. Update `.env.development` with the correct ports:

```env
VITE_USER_SERVICE_URL=http://localhost:9184
VITE_APPOINTMENT_SERVICE_URL=http://localhost:8284
VITE_NOTIFICATION_SERVICE_URL=http://localhost:8884
VITE_PATIENT_SERVICE_URL=http://localhost:9084
VITE_PRACTITIONER_SERVICE_URL=http://localhost:8384
VITE_DOCUMENTS_SERVICE_URL=http://localhost:8184
VITE_BILLING_SERVICE_URL=http://localhost:8584
VITE_MEDICAL_RECORDS_SERVICE_URL=http://localhost:8684
VITE_MESSAGING_SERVICE_URL=http://localhost:8984
VITE_LAB_SERVICE_URL=http://localhost:8784
VITE_CATALOG_SERVICE_URL=http://localhost:8484
```

4. Restart Vite: `npm run dev`

### API Endpoint Routing

| Endpoint Prefix | Service | Examples |
|-----------------|---------|----------|
| `/api/auth/*` | UserService | Login, Register, Refresh |
| `/api/users/*` | UserService | Profile management |
| `/api/appointment/*` | AppointmentService | Appointments |
| `/api/patient/*` | PatientService | Patient data |
| `/api/notifications/*` | NotificationService | Notifications |
| `/api/doctors/*` | PractitionerService | Doctor directory |
| `/api/documents/*` | DocumentsService | Documents |
| `/api/billing/*` | BillingService | Payments |
| `/api/messages/*` | MessagingService | Messages |
| `/api/lab/*` | LabService | Lab orders |

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

## Connecting to SQL Server Database

Connect to the database using **Azure Data Studio**, **SSMS**, or any SQL client.

### Step 1: Get Connection Info

Run these PowerShell commands:

```powershell
# Get the port
docker ps --format "{{.Ports}}" | Select-String "1433"

# Get the password
docker exec $(docker ps --filter "name=sql" --format "{{.Names}}") printenv MSSQL_SA_PASSWORD
```

### Step 2: Build Connection String

```
Server=127.0.0.1,{PORT};User Id=sa;Password={PASSWORD};TrustServerCertificate=True;Encrypt=True
```

Replace `{PORT}` and `{PASSWORD}` with values from Step 1.

### Step 3: Connect in Azure Data Studio

1. Click **New Connection**
2. Select **Connection String** input type
3. Paste your connection string
4. Click **Connect**

### Available Databases

- `UserServiceDb` - Users & authentication
- `PatientServiceDb` - Patient profiles
- `PractitionerServiceDb` - Doctor profiles
- `AppointmentDb` - Appointments
- `BillingDb` - Payments
- `DocumentsDb` - Documents
- `MedicalCatalogDb` - Medical codes (LOINC, ICD-10)
- `MedicalRecordsDb` - Medical records
- `LabDb` - Lab orders
- `NotificationDb` - Notifications
- `MessagingDb` - Messages

---

## Stopping the Application

1. Press `Ctrl+C` in the Aspire terminal
2. This gracefully stops all services and containers
3. Containers are ephemeral - data is lost when stopped

---

## Test Users (Development Only)

These users are automatically seeded on first startup in development mode:

| Username | Password | Role |
|----------|----------|------|
| `patient_a_20250818` | `P@ssw0rd!` | Patient |
| `doctor_a_20250818` | `P@ssw0rd!` | Doctor |
| `reception_a_20250818` | `P@ssw0rd!` | Receptionist |
| `admin_a_20250818` | `P@ssw0rd!` | Admin |
| `owner@test.local` | `P@ssw0rd!` | Owner |

> **Note:** To re-seed users, delete the `UserServiceDb` database and restart Aspire.
