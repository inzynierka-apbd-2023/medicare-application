export type AppointmentStatus = "upcoming" | "past" | "cancelled";

export type PaymentStatus = "paid" | "not_paid";

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
}

export interface AppointmentListProps {
  appointments: Appointment[];
  onDetails: (appointment: Appointment) => void;
  onPayment?: (appointmentId: string) => void;
  onCancel?: (appointmentId: string) => void;
}

export interface AppointmentCardProps {
  appointment: Appointment;
  onDetails: (appointment: Appointment) => void;
  onPayment?: (appointmentId: string) => void;
  onCancel?: (appointmentId: string) => void;
  isUpcoming?: boolean;
}

export interface AppointmentSectionProps {
  title: string;
  appointments: Appointment[];
  onDetails: (appointment: Appointment) => void;
  onPayment?: (appointmentId: string) => void;
  onCancel?: (appointmentId: string) => void;
  isUpcoming?: boolean;
  emptyMessage: string;
}
