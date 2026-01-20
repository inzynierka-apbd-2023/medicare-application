export interface DoctorScheduleEvent {
  id: string;
  patientId: string;
  patientName: string;
  patientAge: number;
  patientPhone: string;
  patientEmail?: string;
  appointmentType: string;
  date: string;
  time: string;
  duration: number; // in minutes
  status: "scheduled" | "completed" | "no-show" | "cancelled" | "overdue";
  chiefComplaint?: string;
  notes?: string;
  medicalHistory?: string[];
  allergies?: string[];
  currentMedications?: string[];
  // Visit note related fields
  visitNoteDocumentId?: string;
  hasVisitNote?: boolean;
}

export interface DoctorScheduleModalProps {
  isOpen: boolean;
  onClose: () => void;
  appointment: DoctorScheduleEvent | null;
  onMarkCompleted?: (appointmentId: string) => Promise<boolean>;
  onMarkNoShow?: (appointmentId: string) => Promise<boolean>;
  onAddNotes?: (appointmentId: string, notes: string) => Promise<boolean>;
  onOpenVisitNote?: (appointment: DoctorScheduleEvent) => void;
}

export interface DoctorSchedulerProps {
  doctorId?: string;
  isReadOnly?: boolean;
}

export interface DoctorCalendarEvent {
  id: string;
  title: string;
  start: string;
  end: string;
  color: string;
  extendedProps: {
    appointment: DoctorScheduleEvent;
    timeStatus: "upcoming" | "current" | "overdue" | "completed" | "no-show";
  };
}
