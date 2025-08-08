export interface MedicalCondition {
  id: string;
  code: string;
  name: string;
  diagnosedDate: string;
  status: "Active" | "Resolved" | "Monitoring" | "Chronic";
  severity: "Mild" | "Moderate" | "Severe" | "Critical";
  notes?: string;
}

// Removed Allergy interface - not supported by database schema

// Simplified medication interface to match prescription documents
export interface Medication {
  id: string;
  name: string;
  dosage: string;
  frequency: string;
  prescribedDate: string;
  prescribedBy: string;
  duration?: string; // duration_days from prescription table
  instructions?: string;
  status: "Active" | "Discontinued" | "Completed";
}

// Simplified vital signs - stored as part of visit documents
export interface VitalSigns {
  bloodPressureSystolic?: number;
  bloodPressureDiastolic?: number;
  heartRate?: number;
  temperature?: number;
  weight?: number;
  height?: number;
}

export interface MedicalVisit {
  id: string;
  date: string;
  doctorName: string;
  specialty: string;
  chiefComplaint: string; // symptoms from visit_document
  diagnosis: string;
  treatment: string; // recommendations from visit_document
  notes?: string;
  followUpDate?: string;
  vitalSigns?: VitalSigns; // embedded vital signs
}

export interface EmergencyContact {
  id: string;
  name: string;
  relationship: string;
  phone: string;
  isPrimary: boolean;
}

export interface InsuranceInfo {
  id: string;
  provider: string; // provider_name from database
  policyNumber: string;
  groupNumber?: string;
  validFrom: string; // valid_from from database
  validTo?: string; // valid_to from database
  isPrimary: boolean;
}

export interface PatientMedicalRecord {
  // Basic Patient Info - matches User_Profile and Patient tables
  id: string;
  patientId: string;
  name: string; // firstName + lastName from User_Profile
  dateOfBirth: string;
  gender: "Male" | "Female" | "Other";
  bloodType?: string;
  medicalRecordNumber: string;

  // Contact Information - from User_Profile
  phone: string;
  email: string;
  address: string;
  emergencyContacts: EmergencyContact[];

  // Insurance - from Insurance table
  insurance: InsuranceInfo[];

  // Medical History - from Patient_Medical_Condition
  medicalConditions: MedicalCondition[];

  // Medications - from Prescription documents only
  currentMedications: Medication[];

  // Visit History - from Visit_Document
  visits: MedicalVisit[];

  // Metadata
  lastUpdated: string;
  createdDate: string;
}

export interface MedicalRecordsPageProps {
  patientId?: string;
}

export interface MedicalRecordsSummaryProps {
  record: PatientMedicalRecord;
  onViewDetails: (section: MedicalRecordSection) => void;
}

export interface MedicalRecordDetailModalProps {
  isOpen: boolean;
  onClose: () => void;
  record: PatientMedicalRecord;
  section: MedicalRecordSection;
}

export type MedicalRecordSection =
  | "overview"
  | "conditions"
  | "medications"
  | "visits"
  | "contacts"
  | "insurance";

export interface MedicalRecordsFilters {
  searchTerm: string;
  conditionStatus: "All" | "Active" | "Resolved" | "Monitoring" | "Chronic";
  medicationStatus: "All" | "Active" | "Discontinued" | "Completed";
  dateRange: {
    start?: string;
    end?: string;
  };
}
