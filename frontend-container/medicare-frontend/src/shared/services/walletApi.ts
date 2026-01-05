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
  /**
   * Process payment for an appointment (Mock)
   */
  payAppointment: async (
    appointmentId: string,
    _patientId: string,
    _amountCents: number
  ): Promise<ApiResponse<boolean>> => {
    try {
      await apiClient.post("/billing/payment/mock", {
        appointmentId,
        paymentMethod: "BLIK", // Default or passed? For now defaulting or we can add param
      });
      return { success: true, data: true };
    } catch (_error) {
      return createErrorResponse("Payment processing failed");
    }
  },

  mockPayAppointment: async (
    appointmentId: string,
    patientId: string,
    method: "BLIK" | "Card"
  ): Promise<ApiResponse<boolean>> => {
    try {
      await apiClient.post(
        `/appointment/appointments/${appointmentId}/mock-payment`,
        {
          patientId,
          paymentMethod: method,
        }
      );
      return { success: true, data: true };
    } catch (_error) {
      return createErrorResponse("Payment processing failed");
    }
  },
};
