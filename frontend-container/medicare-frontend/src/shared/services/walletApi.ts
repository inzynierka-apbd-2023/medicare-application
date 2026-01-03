import type { Appointment } from "../../features/appointments/types";

import { type ApiResponse, createErrorResponse } from "./api";
import { apiClient } from "./apiClient";

export type PaymentStatus = "paid" | "not_paid";

// Re-export the Appointment interface as WalletAppointment for consistency
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
  /**
   * Renew subscription for a patient
   */
  renewSubscription: async (
    contractId: string
  ): Promise<ApiResponse<PaymentIntentResponse>> => {
    try {
      const response = await apiClient.post<PaymentIntentResponse>(
        `/payments/subscriptions/${contractId}/renewals`
      );
      return { success: true, data: response.data };
    } catch (_error) {
      return createErrorResponse("Failed to renew subscription");
    }
  },

  /**
   * Process payment for an appointment
   */
  payAppointment: async (
    appointmentId: string,
    patientId: string,
    amountCents: number
  ): Promise<ApiResponse<PaymentIntentResponse>> => {
    try {
      // Create a payment intent for the appointment
      const response = await apiClient.post<PaymentIntentResponse>(
        "/payments/intents",
        {
          kind: "Appointment",
          subjectId: appointmentId,
          patientId: patientId,
          provider: "mock",
          amountCents: amountCents,
          currency: "PLN",
        }
      );
      return { success: true, data: response.data };
    } catch (_error) {
      return createErrorResponse("Payment processing failed");
    }
  },
};
