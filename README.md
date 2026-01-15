# Medicare Application User Guide

## Accessing the Application

The application is hosted and available at:
**[https://frontend.happyforest-7b2f676b.westeurope.azurecontainerapps.io/](https://frontend.happyforest-7b2f676b.westeurope.azurecontainerapps.io/)**

---

## About the Application

Medicare is a comprehensive clinic management system designed to streamline healthcare operations. It orchestrates complex workflows between doctors, patients, and administrative staff, covering everything from appointment scheduling and medical record management to billing and real-time notifications.

---

## Architecture Overview

The application is built using a modern microservices architecture orchestrated by **.NET Aspire**.

### 🎨 Frontend
*   **Framework**: React 19
*   **Build Tool**: Vite
*   **Styling**: TailwindCSS
*   **Language**: TypeScript

### ⚙️ Backend Services
The backend consists of **13 specialized microservices** built with **.NET 9**, communicating via HTTP and RabbitMQ:

1.  **UserService**: Identity and profile management.
2.  **PractitionerService**: Doctor management and schedules.
3.  **PatientService**: Patient demographics and history.
4.  **AppointmentService**: Booking and scheduling logic.
5.  **MedicalRecordsService**: Clinical records and history.
6.  **MedicalCatalogService**: Management of medical procedures and drugs.
7.  **BillingService**: Invoicing and payments.
8.  **DocumentsService**: File management and document generation.
9.  **LabService**: Laboratory order and result processing.
10. **NotificationService**: Email and real-time alerts.
11. **MessagingService**: Internal communication.
12. **ArchiveService**: Data archiving and retention.
13. **PdfService**: PDF generation for reports and prescriptions.

### 🏗️ Infrastructure
*   **Orchestrator**: .NET Aspire
*   **Database**: SQL Server (Schema per service)
*   **Message Broker**: RabbitMQ
*   **Container Platform**: Azure Container Apps

---

## Login Credentials

Use the following credentials to log in and explore the application features.

### 🩺 Doctors
Doctors can manage appointments, view patient medical records, and issue prescriptions.

| Username | Email | Password | Role / Specialization |
|----------|-------|----------|-----------------------|
| `doctor1` | `doctor1@medicare.local` | `P@ssw0rd!` | Cardiologist, General Practice |
| `doctor2` | `doctor2@medicare.local` | `P@ssw0rd!` | General Practice |

### 🤒 Patients
Patients can book appointments, view their medical history, and check lab results.

| Username | Email | Password | Name |
|----------|-------|----------|------|
| `patient1` | `patient1@medicare.local` | `P@ssw0rd!` | Alice Johnson |
| `patient2` | `patient2@medicare.local` | `P@ssw0rd!` | Bob Smith |

### 🏥 Staff
Staff members manage the clinic operations.

| Username | Email | Password | Role Description |
|----------|-------|----------|------------------|
| `receptionist` | `receptionist@medicare.local` | `P@ssw0rd!` | **Receptionist**: Manages schedules and checks in patients. |
| `admin` | `admin@medicare.local` | `P@ssw0rd!` | **Admin**: System administration and configuration. |
| `owner` | `owner@medicare.local` | `P@ssw0rd!` | **Owner**: Full access and business insights. |

---

## Management

This README provides all the necessary information to access and manage the Medicare application as a user. For technical development details or local setup, please refer to internal documentation.