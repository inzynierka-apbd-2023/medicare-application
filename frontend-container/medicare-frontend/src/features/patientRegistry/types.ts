// Patient Registry Types
export interface PatientRegistryInfo {
  // Basic Information
  id?: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  gender?: "male" | "female" | "other" | "prefer-not-to-say";

  // Address Information
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;

  // Medical Information
  medicalRecordNumber?: string;
  bloodType?: string;
  height?: number; // in cm
  weight?: number; // in kg
  generalDoctorId?: string;

  // Insurance Information
  insurance?: PatientInsurance[];

  // Emergency Contacts
  emergencyContacts?: EmergencyContact[];

  // Medical Conditions
  medicalConditions?: PatientMedicalCondition[];

  // System fields
  isActive?: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface PatientInsurance {
  id?: string;
  providerName: string;
  policyNumber: string;
  groupNumber?: string;
  validFrom: string;
  validTo?: string;
  isPrimary: boolean;
  isActive: boolean;
}

export interface EmergencyContact {
  id?: string;
  name: string;
  phone: string;
  relationship: string;
  isPrimary: boolean;
}

export interface MedicalCondition {
  id: string;
  code: string;
  name: string;
  description?: string;
  category?: string;
  isChronic: boolean;
}

export interface PatientMedicalCondition {
  id?: string;
  medicalConditionId: string;
  medicalCondition?: MedicalCondition;
  diagnosedDate?: string;
  status: "active" | "inactive" | "resolved";
  severity?: "mild" | "moderate" | "severe";
  notes?: string;
}

export interface PatientRegistryFilters {
  searchTerm?: string;
  doctorId?: string;
  bloodType?: string;
  isActive?: boolean;
  registrationDateFrom?: string;
  registrationDateTo?: string;
  ageRange?: {
    min: number;
    max: number;
  };
}

export interface PatientRegistryData {
  patients: PatientRegistryInfo[];
  totalCount: number;
  currentPage: number;
  totalPages: number;
}

export interface PatientRegistryPageProps {
  className?: string;
}

// API Response types
export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

// Form data for patient registration
export interface PatientRegistrationFormData {
  // Personal Information
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  gender: "male" | "female" | "other" | "prefer-not-to-say";

  // Address
  addressLine1: string;
  addressLine2: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;

  // Medical Information
  bloodType: string;
  height: string; // string for form input, converted to number later
  weight: string; // string for form input, converted to number later
  generalDoctorId: string;

  // Insurance
  insuranceProvider: string;
  policyNumber: string;
  groupNumber: string;
  validFrom: string;
  validTo: string;

  // Emergency Contact
  emergencyContactName: string;
  emergencyContactPhone: string;
  emergencyContactRelationship: string;

  // Account Setup
  password: string;
  confirmPassword: string;
}

// Doctor interface for dropdown
export interface Doctor {
  id: string;
  firstName: string;
  lastName: string;
  specialization: string;
  email: string;
  phone: string;
}

// Create patient request for API
export interface CreatePatientRequest {
  personalInfo: {
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
    dateOfBirth: string;
    gender: "male" | "female" | "other" | "prefer-not-to-say";
  };
  address: {
    addressLine1: string;
    addressLine2?: string;
    city: string;
    state: string;
    zipCode: string;
    country: string;
  };
  medicalInfo: {
    bloodType?: string;
    height?: number;
    weight?: number;
    generalDoctorId?: string;
  };
  insurance?: {
    providerName: string;
    policyNumber: string;
    groupNumber?: string;
    validFrom: string;
    validTo?: string;
    isPrimary: boolean;
  };
  emergencyContact: {
    name: string;
    phone: string;
    relationship: string;
  };
  accountSetup: {
    password: string;
  };
}
