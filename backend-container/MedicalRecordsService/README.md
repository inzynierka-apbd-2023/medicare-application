# MedicalRecordsService

This service manages medical records, prescriptions, diagnoses, and vital signs for the Medicare application.

## Features

- Medical record management
- Prescription tracking
- Diagnosis management
- Vital signs recording
- Complete medical history

## API Endpoints

### Medical Records
- `POST /api/medical/medicalrecords` - Create medical record
- `GET /api/medical/medicalrecords/{id}` - Get medical record by ID
- `GET /api/medical/medicalrecords/patient/{patientId}` - Get patient medical records
- `GET /api/medical/medicalrecords/{id}/complete` - Get complete record with diagnoses, prescriptions, vitals

### Prescriptions
- `POST /api/medical/prescriptions` - Create prescription
- `GET /api/medical/prescriptions/{id}` - Get prescription by ID
- `GET /api/medical/prescriptions/patient/{patientId}` - Get patient prescriptions
- `GET /api/medical/prescriptions/patient/{patientId}/active` - Get active prescriptions
- `PUT /api/medical/prescriptions/{id}/status` - Update prescription status

## Database Schema

- `medical.Medical_Record` - Main medical records
- `medical.Prescription` - Prescription records
- `medical.Diagnosis` - Diagnosis records with ICD-10 codes
- `medical.Vital_Signs` - Vital signs measurements

## Port

- Development: 8088
