import type { Appointment } from "../../features/appointments/types";
import { toastMessages } from "../toast/toastMessages";

import { api } from "./api";

export type PaymentStatus = "paid" | "not_paid";
export type WalletAppointment = Appointment;

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

export interface PaymentIntentResponse {
  id: string;
  kind: string;
  amountCents: number;
  currency: string;
  status: string;
  clientSecret: string;
}

export const walletApi = {
  renewSubscription: async (
    contractId: string
  ): Promise<PaymentIntentResponse> => {
    return api.post<PaymentIntentResponse>(
      `/payments/subscriptions/${contractId}/renewals`,
      undefined,
      undefined,
      {
        showToastOnSuccess: true,
        successMessage: toastMessages.wallet.renewSubscriptionSuccess,
      }
    );
  },

  mockPayAppointment: async (
    appointmentId: string,
    patientId: string,
    method: "BLIK" | "Card"
  ): Promise<boolean> => {
    await api.post(
      `/appointment/appointments/${appointmentId}/mock-payment`,
      {
        patientId,
        paymentMethod: method,
      },
      undefined,
      {
        showToastOnSuccess: true,
        successMessage: toastMessages.wallet.paymentSuccess,
      }
    );
    return true;
  },
};
