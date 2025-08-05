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
