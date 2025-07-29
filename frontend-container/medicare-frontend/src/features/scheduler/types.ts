export interface Service {
  id: string;
  name: string;
  specializationId: string;
  doctorIds: string[];
  duration?: number; // in minutes
  description?: string;
}

export interface Specialization {
  id: string;
  name: string;
  serviceIds: string[];
  doctorIds: string[];
  description?: string;
}

export interface Doctor {
  id: string;
  name: string;
  specializationIds?: string[];
  email?: string;
  phone?: string;
}

export interface TimeSlot {
  id: string;
  start: Date;
  end: Date;
  isAvailable: boolean;
  doctorId: string;
}

export interface CalendarEvent {
  id: string;
  title: string;
  start: Date;
  end: Date;
  doctorId?: string;
  patientId?: string;
  serviceId?: string;
  status: "scheduled" | "confirmed" | "cancelled" | "completed";
  description?: string;
}

export interface AppointmentBooking {
  serviceId: string;
  specializationId: string;
  doctorId: string;
  timeSlot: TimeSlot;
  notes?: string;
}

export interface SchedulerState {
  services: Service[];
  specializations: Specialization[];
  doctors: Doctor[];
  selectedService: string;
  selectedSpecialization: string;
  selectedDoctor: string;
  availableTimeSlots: TimeSlot[];
  events: CalendarEvent[];
  isLoading: boolean;
  error: string | null;
}

// Props interfaces
export interface SchedulerProps {
  onAppointmentBook?: (booking: AppointmentBooking) => void;
  onEventSelect?: (event: CalendarEvent) => void;
}

export interface ServiceSelectorProps {
  services: Service[];
  selectedService: string;
  onServiceChange: (serviceId: string) => void;
  disabled?: boolean;
}

export interface SpecializationSelectorProps {
  specializations: Specialization[];
  selectedSpecialization: string;
  onSpecializationChange: (specializationId: string) => void;
  disabled?: boolean;
}

export interface DoctorSelectorProps {
  doctors: Doctor[];
  selectedDoctor: string;
  onDoctorChange: (doctorId: string) => void;
  disabled?: boolean;
}

export interface CalendarViewProps {
  events: CalendarEvent[];
  timeSlots: TimeSlot[];
  onTimeSlotSelect: (timeSlot: TimeSlot) => void;
  onEventSelect?: (event: CalendarEvent) => void;
  selectedDoctor?: string;
}

export interface ScheduleFiltersProps {
  services: Service[];
  specializations: Specialization[];
  doctors: Doctor[];
  selectedService: string;
  selectedSpecialization: string;
  selectedDoctor: string;
  onServiceChange: (serviceId: string) => void;
  onSpecializationChange: (specializationId: string) => void;
  onDoctorChange: (doctorId: string) => void;
}
