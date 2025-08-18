# AppointmentService

This service manages appointments, scheduling, and appointment-related analytics for the Medicare application.

## Features

- Create and manage appointments
- Doctor scheduling
- Available time slots
- Appointment categories
- Analytics and reporting

## API Endpoints

### Appointments
- `POST /api/appointment/appointments` - Create appointment
- `GET /api/appointment/appointments/{id}` - Get appointment by ID
- `GET /api/appointment/appointments/patient/{patientId}` - Get patient appointments
- `GET /api/appointment/appointments/doctor/{doctorId}` - Get doctor appointments
- `PUT /api/appointment/appointments/{id}/status` - Update appointment status
- `GET /api/appointment/appointments/analytics/today` - Get today's analytics

### Schedules
- `POST /api/appointment/schedules` - Create doctor schedule
- `GET /api/appointment/schedules/doctor/{doctorId}` - Get doctor schedules
- `GET /api/appointment/schedules/slots/{doctorId}` - Get available slots

## Database Schema

- `appointment.Appointment` - Main appointment records
- `appointment.Appointment_Slot` - Available time slots
- `appointment.Schedule` - Doctor working schedules
- `appointment.Appointment_Category` - Appointment types

## Port

- Development: 8087
