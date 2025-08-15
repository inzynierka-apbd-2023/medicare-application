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
  appointmentType: "in-person" | "virtual" | "phone";
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
  extendedProps?: {
    appointment: Appointment;
  };
}

// API request/response types
export interface CreateAppointmentRequest {
  doctorUserId: string;
  serviceId: string;
  timeSlotId: string;
  appointmentType: "in-person" | "virtual" | "phone";
  appointmentCategory?: string;
  description?: string;
}

export interface UpdateAppointmentRequest {
  appointmentType?: "in-person" | "virtual" | "phone";
  appointmentCategory?: string;
  description?: string;
  timeSlotId?: string;
}

export interface AvailableSlotsRequest {
  doctorId: string;
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
  appointmentType: "all" | "in-person" | "virtual" | "phone";
  dateRange?: {
    start: string;
    end: string;
  };
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
