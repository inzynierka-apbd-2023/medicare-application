# Mock Authentication for Testing

## Overview

The application now includes a mock authentication system for easy testing of role-based routes and functionality without needing a backend server.

## How to Use

### 1. Automatic Mock Mode

The mock authentication is automatically enabled when `USE_MOCK_AUTH = true` in `AuthContext.tsx`.

### 2. Quick Login Buttons

On the login page, you'll see colored buttons for quick login:

- **Green "Patient"** - Logs in as a patient
- **Blue "Doctor"** - Logs in as a doctor
- **Purple "Owner"** - Logs in as an owner/admin
- **Orange "Receptionist"** - Logs in as a receptionist

### 3. Manual Login

You can also manually enter credentials:

#### Patient Login:

- **Username:** `patient`
- **Password:** `test`

#### Doctor Login:

- **Username:** `doctor`
- **Password:** `test`

#### Owner Login:

- **Username:** `owner`
- **Password:** `test`

#### Receptionist Login:

- **Username:** `receptionist`
- **Password:** `test`

## Mock User Data

Each role comes with realistic user data:

### Patient (John Doe)

- ID: 1
- Email: patient@test.com
- Role: Patient
- Access: Patient dashboard, appointments, documents, lab results, wallet

### Doctor (Dr. Jane Smith)

- ID: 2
- Email: doctor@test.com
- Role: Doctor
- Access: Doctor dashboard, patient list, medical records, prescriptions

### Owner (Admin User)

- ID: 3
- Email: owner@test.com
- Role: Owner
- Access: Owner dashboard, analytics, staff management

### Receptionist (Mary Johnson)

- ID: 4
- Email: receptionist@test.com
- Role: Receptionist
- Access: Receptionist dashboard, scheduler, patient registry

## Testing Role-Based Routes

1. **Login** with any role using the quick buttons
2. **Navigate** to different routes - you'll be automatically redirected if you don't have access
3. **Check Header** - navigation items change based on your role
4. **Switch Roles** - logout and login as a different role to see different features

## Route Access Examples

### Patient Routes

- `/patient-dashboard` ✅
- `/my-appointments` ✅
- `/staff-management` ❌ (redirected to patient dashboard)

### Doctor Routes

- `/doctor-dashboard` ✅
- `/patient-list` ✅
- `/my-wallet` ❌ (redirected to doctor dashboard)

### Shared Routes

- `/messages` ✅ (Patient, Doctor, Receptionist)
- `/my-profile` ✅ (All roles)

## Switching Back to Real Authentication

To use real authentication instead of mock:

1. Open `src/shared/auth/AuthContext.tsx`
2. Change `USE_MOCK_AUTH = true` to `USE_MOCK_AUTH = false`
3. Restart the development server

## Files Modified for Mock Auth

- `src/shared/services/mockAuthService.ts` - Mock authentication service
- `src/shared/auth/AuthContext.tsx` - Added mock auth toggle
- `src/features/signon/Login.tsx` - Added quick login buttons
- `docs/MOCK_AUTH.md` - This documentation

## Benefits

✅ **No Backend Required** - Test frontend role-based features immediately  
✅ **Quick Role Switching** - One-click login for any role  
✅ **Realistic Data** - Each user has appropriate profile information  
✅ **Easy Testing** - Test all role-based routes and navigation  
✅ **Development Speed** - No need to set up authentication server

## Next Steps

1. **Test all roles** - Login as each role and explore available features
2. **Verify route protection** - Try accessing unauthorized routes
3. **Check navigation** - Ensure header shows appropriate menu items
4. **Test logout** - Verify logout clears session and redirects to login

Happy testing! 🧪
