export type DocumentType =
  | "Prescription"
  | "Referral"
  | "Sick_Leave"
  | "VisitCard"
  | "Other";

export interface DocumentData {
  // Prescription specific data
  medication?: string;
  dosage?: string;
  frequency?: string;
  duration_days?: number;
  instructions?: string;

  // Referral specific data
  specialty?: string;
  referredTo?: string;
  validFrom?: string;
  validTo?: string;

  // Sick Leave specific data
  startDate?: string;
  endDate?: string;
  daysOff?: number;

  // Visit Card specific data
  symptoms?: string;
  findings?: string;
  diagnosis?: string;
  recommendations?: string;
}

export interface Document {
  id: string;
  appointmentId: string;
  patientId?: string; // Add patientId to documents
  type: DocumentType;
  createdAt: string;
  notes?: string;
  data: DocumentData;
}

export interface Appointment {
  id: string;
  date: string;
  doctor: string;
  specialization: string;
  patientId?: string; // Add patientId to appointments
  patientName?: string; // Add patient name for display
}

export interface DocumentListProps {
  documents: Document[];
  onDocumentClick: (document: Document) => void;
  searchTerm?: string;
  typeFilter?: DocumentType | "All";
  appointmentFilter?: string;
  emptyMessage?: string;
}

export interface DocumentCardProps {
  document: Document;
  onClick: (document: Document) => void;
}

export interface DocumentDetailsModalProps {
  document: Document | null;
  isOpen: boolean;
  onClose: () => void;
  onDownload?: (document: Document) => void;
}

export interface DocumentFilterProps {
  searchTerm: string;
  onSearchChange: (term: string) => void;
  typeFilter: DocumentType | "All";
  onTypeFilterChange: (type: DocumentType | "All") => void;
  appointmentFilter: string;
  onAppointmentFilterChange: (appointmentId: string) => void;
  appointments: Appointment[];
}

export interface DocumentsPageProps {
  initialAppointmentId?: string;
  initialPatientId?: string; // Add support for patient filtering
}
