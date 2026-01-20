import type { Appointment } from "@features/appointments/types";

export type PaymentStatus = "paid" | "not_paid";

// Re-use the Appointment interface from appointments feature
export type WalletAppointment = Appointment;

// Subscription interface matching the backend BillingService response
export interface Subscription {
  id: string;
  type: string;
  active: boolean;
  renewalDate: string;
  periodStart: string;
  periodEnd: string;
}

export interface WalletData {
  subscription: Subscription | null;
  unpaidAppointments: WalletAppointment[];
}

export interface Plan {
  id: string;
  name: string;
  description: string;
  price: number;
  currency: string;
  best?: boolean;
}

export interface WalletProps {
  wallet: WalletData | null;
  onPayAppointment: (appointmentId: string) => void | Promise<void>;
  onNavigateToSubscription: () => void;
  payingAppointmentId?: string | null;
}

export interface SubscriptionViewProps {
  subscription: Subscription | null;
  onBuySubscription: () => void;
}

export interface AppointmentCardProps {
  appointment: WalletAppointment;
  onPay: (appointmentId: string) => void;
  isPaying?: boolean;
}

export interface BuySubscriptionModalProps {
  isOpen: boolean;
  onClose: () => void;
  onPaymentSuccess: (plan: Plan) => void;
  plans?: Plan[];
}
