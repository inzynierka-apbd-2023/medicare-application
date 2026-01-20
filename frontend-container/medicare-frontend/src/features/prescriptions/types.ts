export interface Prescription {
  id: string;
  patientId: string;
  doctorId: string;
  appointmentId?: string;
  medications: Medication[];
  diagnosis: string;
  notes?: string;
  status: PrescriptionStatus;
  createdAt: Date;
  updatedAt: Date;
  validUntil: Date;
  issuedAt: Date;
  pharmacyId?: string;
  dispensedAt?: Date;
}

export interface Medication {
  id: string;
  name: string;
  atcCode?: string;
  genericName?: string;
  dosage: string;
  frequency: string;
  duration: string;
  instructions: string;
  quantity: number;
  unit: string;
  refills: number;
  isGenericAllowed: boolean;
}

export interface Patient {
  id: string;
  name: string;
  email: string;
  phone: string;
  dateOfBirth: Date;
  allergies: string[];
  medicalHistory: string[];
}

export interface Doctor {
  id: string;
  name: string;
  specialization: string;
  licenseNumber: string;
  email: string;
  phone: string;
}

export interface Pharmacy {
  id: string;
  name: string;
  address: string;
  phone: string;
  email: string;
}

export type PrescriptionStatus =
  | "draft"
  | "active"
  | "partially_dispensed"
  | "fully_dispensed"
  | "expired"
  | "cancelled";

export interface PrescriptionFormData {
  patientId: string;
  diagnosis: string;
  notes?: string;
  medications: Omit<Medication, "id">[];
  validUntil: Date;
}

export interface PrescriptionFilter {
  status?: PrescriptionStatus;
  patientId?: string;
  doctorId?: string;
  dateFrom?: Date;
  dateTo?: Date;
  searchTerm?: string;
}

// Component Props
export interface PrescriptionListProps {
  prescriptions: Prescription[];
  onPrescriptionSelect: (prescription: Prescription) => void;
  onPrescriptionEdit: (prescription: Prescription) => void;
  onPrescriptionDelete: (prescriptionId: string) => void;
  isLoading?: boolean;
}

export interface PrescriptionCardProps {
  prescription: Prescription;
  onSelect: (prescription: Prescription) => void;
  onEdit: (prescription: Prescription) => void;
  onDelete: (prescriptionId: string) => void;
}

export interface PrescriptionFormProps {
  prescription?: Prescription | undefined;
  patients: Patient[];
  preSelectedPatientId?: string | null;
  onSubmit: (data: PrescriptionFormData) => void;
  onCancel: () => void;
  isLoading?: boolean;
}

export interface PrescriptionDetailsProps {
  prescription: Prescription;
  patient: Patient;
  doctor: Doctor;
  onEdit: (prescription: Prescription) => void;
  onPrint: (prescription: Prescription) => void;
  onClose: () => void;
}

export interface PrescriptionFiltersProps {
  filters: PrescriptionFilter;
  patients: Patient[];
  onFiltersChange: (filters: PrescriptionFilter) => void;
  onClearFilters: () => void;
}

export interface MedicationInputProps {
  medication: Omit<Medication, "id">;
  onMedicationChange: (medication: Omit<Medication, "id">) => void;
  onRemove: () => void;
  index: number;
}
