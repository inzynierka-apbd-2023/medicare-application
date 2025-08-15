export interface ReceptionistDashboardStats {
  totalAppointments: number;
  todayAppointments: number;
  totalDoctors: number;
  availableDoctors: number;
}

export interface QuickAppointment {
  id: string;
  patientName: string;
  doctorName: string;
  time: string;
  type: "in-person" | "video-call" | "phone";
  status: "waiting" | "in-progress" | "completed" | "cancelled";
  room?: string;
}

export interface DoctorAvailability {
  id: string;
  name: string;
  specialization: string;
  status: "available" | "busy" | "off-duty";
  currentPatient?: string;
  nextAvailable?: string;
  totalAppointments: number;
  completedToday: number;
}

export interface ReceptionistDashboardData {
  stats: ReceptionistDashboardStats;
  todayAppointments: QuickAppointment[];
  doctorAvailability: DoctorAvailability[];
}

export interface ReceptionistDashboardPageProps {
  className?: string;
}

// Scheduler-related types
export interface Patient {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
}

export interface Doctor {
  id: string;
  firstName: string;
  lastName: string;
  specialization: string;
  email: string;
  phone: string;
}

export interface CalendarEvent {
  id: string;
  title: string;
  start: string;
  end: string;
  color?: string;
  backgroundColor?: string;
  borderColor?: string;
  extendedProps?: {
    patientId: string;
    doctorId: string;
    type: "in-person" | "video-call" | "phone";
    status: "scheduled" | "completed" | "cancelled";
    room?: string;
  };
}
