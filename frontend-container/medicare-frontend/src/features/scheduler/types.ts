export * from "./types/doctorScheduler";

// Original types from the existing scheduler
export interface CalendarEvent {
  id: string;
  title: string;
  start: string;
  end: string;
  color?: string;
  extendedProps?: {
    appointment: unknown;
  };
}

export interface CreateAppointmentRequest {
  patientId: string;
  doctorId: string;
  date: string;
  time: string;
  duration: number;
  appointmentType: string;
  description?: string;
}

export interface UpdateAppointmentRequest {
  id: string;
  date?: string;
  time?: string;
  duration?: number;
  appointmentType?: string;
  description?: string;
  status?: string;
}

export interface SchedulerPageProps {
  patientId?: string;
}
