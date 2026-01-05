export type AppointmentStatus = "upcoming" | "past" | "cancelled";

export type PaymentStatus = "paid" | "not_paid";

export interface DoctorRating {
  rating: number; // 1-5 stars
  comment?: string;
  ratedAt: string;
}

export interface Appointment {
  id: string;
  date: string;
  time: string;
  doctor: string;
  specialization?: string;
  serviceName?: string;
  description?: string;
  status: AppointmentStatus;
  paymentStatus: PaymentStatus;
  total: number;
  doctorRating?: DoctorRating;
}

export interface AppointmentListProps {
  appointments: Appointment[];
  onDetails: (appointment: Appointment) => void;
  onPayment?: (appointmentId: string) => void;
  onCancel?: (appointmentId: string) => void;
  onRateDoctor?: (
    appointmentId: string,
    rating: number,
    comment?: string
  ) => void;
}

export interface AppointmentCardProps {
  appointment: Appointment;
  onDetails: (appointment: Appointment) => void;
  onPayment?: (appointmentId: string) => void;
  onCancel?: (appointmentId: string) => void;
  onRateDoctor?: (
    appointmentId: string,
    rating: number,
    comment?: string
  ) => void;
  isUpcoming?: boolean;
}

export interface AppointmentSectionProps {
  title: string;
  appointments: Appointment[];
  onDetails: (appointment: Appointment) => void;
  onPayment?: (appointmentId: string) => void;
  onCancel?: (appointmentId: string) => void;
  onRateDoctor?: (
    appointmentId: string,
    rating: number,
    comment?: string
  ) => void;
  isUpcoming?: boolean;
  emptyMessage: string;
}

// Today's appointments specific types
export type TodayAppointmentStatus =
  | "scheduled"
  | "completed"
  | "no-show"
  | "cancelled";

export type TimeStatus =
  | "upcoming"
  | "current"
  | "overdue"
  | "completed"
  | "no-show";

export interface PatientInfo {
  id: string;
  name: string;
  age: number;
  phone: string;
  email?: string;
  medicalHistory?: string[];
  allergies?: string[];
  currentMedications?: string[];
}

export interface TodayAppointment {
  id: string;
  date: string;
  time: string;
  duration: number; // in minutes
  patient: PatientInfo;
  appointmentType: string;
  description?: string;
  status: TodayAppointmentStatus;
  notes?: string;
  chiefComplaint?: string;
}

export interface TodayAppointmentCardProps {
  appointment: TodayAppointment;
  timeStatus: TimeStatus;
  onDetails: (appointment: TodayAppointment) => void;
  onMarkCompleted?: (appointmentId: string) => void;
  onMarkNoShow?: (appointmentId: string) => void;
  onContactPatient?: (patientId: string) => void;
  showCompletionActions?: boolean;
}
