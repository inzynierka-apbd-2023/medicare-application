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

---

## Deployment to Azure

To deploy the application to Azure:

1. Open a terminal in the solution root.
2. Run `azd up`
3. Follow the prompts (login, select subscription, location).

Once deployed, the application URL will be displayed in the terminal.
To find it later, run:
```powershell
azd show
```
Look for the **Ingress** URL in the output.