import type { Appointment } from "../appointments/types";

export type PaymentStatus = "paid" | "not_paid";

export type SubscriptionType = "Premium" | "Basic" | "Pro";

// Re-use the Appointment interface from appointments feature
export type WalletAppointment = Appointment;

export interface Subscription {
  type: SubscriptionType;
  active: boolean;
  renewalDate: string;
}

export interface WalletData {
  subscription: Subscription;
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
  onPayAppointment: (appointmentId: string) => Promise<void>;
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
