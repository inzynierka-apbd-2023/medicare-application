export * from "./types/doctorScheduler";

// Core data models
export interface Specialization {
  id: string;
  name: string;
  description?: string;
  serviceId: string;
  service: Service;
  isActive: boolean;
}

export interface Service {
  id: string;
  name: string;
  description?: string;
  durationMinutes: number;
  isActive: boolean;
}

export interface Doctor {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  specializationId: string;
  specialization: Specialization;
  // Optional array used across UI for filtering and labels
  specializations?: Specialization[];
  isAvailable: boolean;
  workingHours: {
    start: string;
    end: string;
  };
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

export interface Patient {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
}

export interface AppointmentStatus {
  id: string;
  name: string;
  description?: string;
  colorCode?: string;
}

export interface Appointment {
  id: string;
  patientId: string;
  patient: Patient;
  doctorUserId: string;
  doctor: Doctor;
  serviceId: string;
  service: Service;
  timeSlotId: string;
  timeSlot: TimeSlot;
  day: string;
  durationMinutes: number;
  appointmentType: AppointmentType;
  appointmentCategory?: string;
  description?: string;
  statusId: string;
  status: AppointmentStatus;
  createdAt: string;
  updatedAt: string;
}

// Calendar integration
export interface CalendarEvent {
  id: string;
  title: string;
  start: string;
  end: string;
  color?: string;
  backgroundColor?: string;
  borderColor?: string;
  textColor?: string;
  extendedProps?: {
    appointment: Appointment;
  };
}

// API request/response types
export interface CreateAppointmentRequest {
  doctorUserId: string;
  serviceId: string;
  timeSlotId: string;
  appointmentType: AppointmentType;
  appointmentCategory?: string;
  description?: string;
}

export interface UpdateAppointmentRequest {
  appointmentType?: AppointmentType;
  appointmentCategory?: string;
  description?: string;
  timeSlotId?: string;
}

export interface AvailableSlotsRequest {
  doctorId: string;
  serviceId?: string;
  startDate: string;
  endDate: string;
}

export interface DoctorSchedule {
  doctorId: string;
  date: string;
  timeSlots: TimeSlot[];
}

// Filter types
export interface AppointmentFilters {
  appointmentType: "all" | AppointmentType;
  dateRange?: {
    start: string;
    end: string;
  };
}

// Scheduler filters used by patient/receptionist schedulers
export interface SchedulerFilters {
  specialization?: string;
  service?: string;
  doctor?: string;
  appointmentType?: "all" | AppointmentType;
  dateRange?: {
    start: string;
    end: string;
  };
}

export type AppointmentType = "in-person" | "virtual" | "phone";

// Internal scheduler hook state
export interface SchedulerState {
  appointments: Appointment[];
  doctors: Doctor[];
  services: Service[];
  specializations: Specialization[];
  timeSlots: TimeSlot[];
  appointmentStatuses: AppointmentStatus[];
  isLoading: boolean;
  error: string | null;
  selectedDate: string | null;
  selectedAppointment: Appointment | null;
  filters: SchedulerFilters;
}

// Component props
export interface SchedulerPageProps {
  patientId?: string;
}

export interface AppointmentModalProps {
  isOpen: boolean;
  onClose: () => void;
  appointment: Appointment | null;
  onSave: (
    data: CreateAppointmentRequest | UpdateAppointmentRequest
  ) => Promise<void>;
  mode: "create" | "edit" | "view";
}
