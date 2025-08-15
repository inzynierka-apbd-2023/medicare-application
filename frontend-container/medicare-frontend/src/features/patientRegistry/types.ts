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
