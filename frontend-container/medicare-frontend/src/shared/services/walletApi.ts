import {
  createMockResponse,
  createErrorResponse,
  type ApiResponse,
} from "./api";
import type { Appointment } from "../../features/appointments/types";

export type PaymentStatus = "paid" | "not_paid";

// Re-export the Appointment interface as WalletAppointment for consistency
export type WalletAppointment = Appointment;

export interface Subscription {
  type: "Premium" | "Basic" | "Pro";
  active: boolean;
  renewalDate: string;
}

export interface WalletData {
  subscription: Subscription;
  unpaidAppointments: WalletAppointment[];
}

// Import the same mock appointments data from appointmentsApi
// This ensures consistency between appointments and wallet data
import { appointmentsApi } from "./appointmentsApi";

// Mock subscription data
const mockSubscription: Subscription = {
  type: "Premium",
  active: false,
  renewalDate: "2025-07-01",
};

export const walletApi = {
  /**
   * Fetch wallet data including subscription and unpaid appointments
   */
  getWalletData: async (): Promise<ApiResponse<WalletData>> => {
    try {
      // Get all appointments from the appointments API
      const appointmentsResponse = await appointmentsApi.getAppointments();

      if (!appointmentsResponse.success) {
        return createErrorResponse("Failed to fetch appointments data");
      }

      // Filter for unpaid appointments only
      const unpaidAppointments = appointmentsResponse.data.filter(
        (appointment) => appointment.paymentStatus === "not_paid"
      );

      const walletData: WalletData = {
        subscription: mockSubscription,
        unpaidAppointments,
      };

      // Simulate API delay
      return await createMockResponse(walletData, 800);
    } catch (error) {
      return createErrorResponse("Failed to fetch wallet data");
    }
  },

  /**
   * Process payment for an appointment
   */
  payAppointment: async (
    appointmentId: string
  ): Promise<ApiResponse<WalletAppointment>> => {
    try {
      // Update the appointment payment status in the appointments API
      const response = await appointmentsApi.updatePaymentStatus(
        appointmentId,
        {
          paymentStatus: "paid",
        }
      );

      if (!response.success) {
        return createErrorResponse("Payment processing failed");
      }

      return await createMockResponse(response.data, 1500);
    } catch (error) {
      return createErrorResponse("Payment processing failed");
    }
  },

  /**
   * Update subscription status
   */
  updateSubscription: async (
    subscriptionData: Partial<Subscription>
  ): Promise<ApiResponse<Subscription>> => {
    try {
      // Update the mock subscription data
      Object.assign(mockSubscription, subscriptionData);

      return await createMockResponse(mockSubscription, 500);
    } catch (error) {
      return createErrorResponse("Failed to update subscription");
    }
  },
};
