# Test Credentials for Local Development

> **⚠️ DEVELOPMENT ONLY** - These credentials are seeded automatically when running in development mode.
> Never use these credentials in production!

---

## Quick Reference

**All passwords:** `P@ssw0rd!`

---

## Doctors

| Username | Email | Name | Specialization | Schedule | User ID |
|----------|-------|------|----------------|----------|---------|
| `doctor1` | `doctor1@medicare.local` | Dr. John Carter | Cardiologist, General Practice | Mon 9-13, Wed 14-18, Fri 9-12 | `bbbbbbbb-0002-0002-0002-000000000001` |
| `doctor2` | `doctor2@medicare.local` | Dr. Sarah Chen | General Practice | Mon 8-16, Tue 8-16, Thu 8-16 | `bbbbbbbb-0002-0002-0002-000000000002` |

### Doctor Details

#### Dr. John Carter (doctor1@medicare.local)
- **Phone:** 555-0101
- **Address:** 100 Medical Plaza, Suite 200, Chicago, IL 60601
- **Bio:** Cardiology specialist with 15 years of experience in interventional procedures
- **Services:** General Consultation, Cardiology Services
- **Schedule:**
  - Monday: 9:00 AM - 1:00 PM
  - Wednesday: 2:00 PM - 6:00 PM
  - Friday: 9:00 AM - 12:00 PM

#### Dr. Sarah Chen (doctor2@medicare.local)
- **Phone:** 555-0102
- **Address:** 100 Medical Plaza, Suite 300, Chicago, IL 60601
- **Bio:** General practitioner focused on preventive care and family medicine
- **Services:** General Consultation
- **Schedule:**
  - Monday: 8:00 AM - 4:00 PM
  - Tuesday: 8:00 AM - 4:00 PM
  - Thursday: 8:00 AM - 4:00 PM

---

## Patients

| Username | Email | Name | Date of Birth | Gender | User ID |
|----------|-------|------|---------------|--------|---------|
| `patient1` | `patient1@medicare.local` | Alice Johnson | May 10, 1990 | Female | `aaaaaaaa-0001-0001-0001-000000000001` |
| `patient2` | `patient2@medicare.local` | Bob Smith | Nov 25, 1985 | Male | `aaaaaaaa-0001-0001-0001-000000000002` |

### Patient Details

#### Alice Johnson (patient1@medicare.local)
- **Phone:** 555-0201
- **Address:** 456 Patient Ave, Chicago, IL 60602
- **Pre-seeded Appointment:** With Dr. John Carter, tomorrow at 10:00 AM

#### Bob Smith (patient2@medicare.local)
- **Phone:** 555-0202
- **Address:** 789 Health St, Chicago, IL 60603
- **Pre-seeded Appointment:** With Dr. Sarah Chen, day after tomorrow at 2:00 PM

---

## Staff

| Username | Email | Name | Role | User ID |
|----------|-------|------|------|---------|
| `receptionist` | `receptionist@medicare.local` | Mary Williams | Receptionist | `cccccccc-0003-0003-0003-000000000001` |
| `admin` | `admin@medicare.local` | System Administrator | Admin | `dddddddd-0004-0004-0004-000000000001` |

---

## Cross-Service ID Reference

The following GUIDs are shared across all services for consistency:

```
# Doctors (Doctor.Id = Doctor.UserId = User.Id)
Doctor1 (John Carter):    bbbbbbbb-0002-0002-0002-000000000001
Doctor2 (Sarah Chen):     bbbbbbbb-0002-0002-0002-000000000002

# Patients
Patient1 (Alice Johnson): aaaaaaaa-0001-0001-0001-000000000001
Patient2 (Bob Smith):     aaaaaaaa-0001-0001-0001-000000000002

# Staff
Receptionist (Mary):      cccccccc-0003-0003-0003-000000000001
Admin:                    dddddddd-0004-0004-0004-000000000001

# Specializations
Cardiologist:             33333333-3333-3333-3333-000000000001
General Practitioner:     33333333-3333-3333-3333-000000000002

# Medical Services
General Consultation:     44444444-4444-4444-4444-000000000001
Cardiology Services:      44444444-4444-4444-4444-000000000002

# Appointment Categories
Annual Checkup:           66666666-6666-6666-6666-000000000001
General Consultation:     66666666-6666-6666-6666-000000000002

# Pre-seeded Appointments
Appointment1 (Alice+John): 55555555-5555-5555-5555-000000000001
Appointment2 (Bob+Sarah):  55555555-5555-5555-5555-000000000002
```

---

## Re-seeding Data

To reset and re-seed all data:

1. Stop Aspire
2. Delete all service databases (or drop Docker volumes):
   ```powershell
   docker volume rm medicare-application_sqldata
   ```
3. Restart Aspire:
   ```powershell
   cd backend-container/Medicare.AppHost
   dotnet run
   ```

The seeders will automatically run and create all test data.

---

## Seeder Source Files

- **UserService:** `backend-container/UserService/UserService/Data/MockDataSeeder.cs`
- **PractitionerService:** `backend-container/PractitionerService/PractitionerService/Data/MockDataSeeder.cs`
- **AppointmentService:** `backend-container/AppointmentService/AppointmentService/Data/MockDataSeeder.cs`
