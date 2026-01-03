export interface Patient {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  medicalRecordNumber?: string;
  bloodType?: string;
}

export interface Doctor {
  id: string;
  userId?: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  licenseNumber?: string;
  yearsExperience?: number;
  specializations: Specialization[];
}

export interface Specialization {
  id: string;
  name: string;
  description?: string;
}

export interface AppointmentStatus {
  id: string;
  name: string;
  description?: string;
  colorCode?: string;
}

export interface ReceptionistAppointment {
  id: string;
  patientId: string;
  doctorId: string;
  day: string; // YYYY-MM-DD
  time: string; // HH:mm
  duration: number; // minutes
  appointmentType: "in-person" | "video-call" | "phone";
  appointmentCategory:
    | "consultation"
    | "emergency"
    | "follow-up"
    | "procedure"
    | "surgery"
    | "check-up"
    | "vaccination";
  statusId: string;
  room?: string;
  description?: string;
  totalCost?: number;
  patient?: Patient;
  doctor?: Doctor;
  status?: AppointmentStatus;
  createdAt: string;
  updatedAt: string;
}

export interface TimeSlot {
  id: string;
  doctorId: string;
  startDateTime: string;
  endDateTime: string;
  isAvailable: boolean;
  durationMinutes: number;
  slotType: string;
}

export interface CreateAppointmentRequest {
  patientId: string;
  doctorId: string;
  day: string;
  time: string;
  duration: number;
  appointmentType: "in-person" | "video-call" | "phone";
  appointmentCategory:
    | "consultation"
    | "emergency"
    | "follow-up"
    | "procedure"
    | "surgery"
    | "check-up"
    | "vaccination";
  room?: string;
  description?: string;
}

export interface UpdateAppointmentRequest {
  id: string;
  day?: string;
  time?: string;
  duration?: number;
  appointmentType?: "in-person" | "video-call" | "phone";
  appointmentCategory?:
    | "consultation"
    | "emergency"
    | "follow-up"
    | "procedure"
    | "surgery"
    | "check-up"
    | "vaccination";
  room?: string;
  description?: string;
  statusId?: string;
}

export interface AppointmentFilters {
  patientName?: string;
  doctorId?: string;
  status?: string;
  appointmentType?: string;
  appointmentCategory?: string;
  dateRange?: {
    start: string;
    end: string;
  };
}

export interface CalendarEvent {
  id: string;
  title: string;
  start: string;
  end: string;
  backgroundColor?: string;
  borderColor?: string;
  extendedProps: {
    appointment: ReceptionistAppointment;
    patientName: string;
    doctorName: string;
    appointmentType: string;
    status: string;
    room?: string;
  };
}

export interface ReceptionistSchedulerPageProps {
  className?: string;
  autoOpenBooking?: boolean;
  isEmbedded?: boolean;
}

export interface AppointmentModalProps {
  isOpen: boolean;
  mode: "create" | "edit" | "view";
  appointment?: ReceptionistAppointment | null;
  selectedDate?: string;
  onClose: () => void;
  onCreateSubmit: (data: CreateAppointmentRequest) => Promise<void>;
  onUpdateSubmit: (data: UpdateAppointmentRequest) => Promise<void>;
  onCancelAppointment: (appointmentId: string) => Promise<void>;
  onEdit: () => void;
}

export interface DailyScheduleProps {
  appointments: ReceptionistAppointment[];
  selectedDate: Date;
  onAppointmentSelect: (appointment: ReceptionistAppointment) => void;
  onCreateAppointment: (timeSlot: string) => void;
}

export interface PatientSearchProps {
  onPatientSelect: (patient: Patient) => void;
  selectedPatient?: Patient | null;
}

export interface DoctorAvailabilityProps {
  doctorId: string;
  selectedDate: string;
  onTimeSlotSelect: (timeSlot: TimeSlot) => void;
}
