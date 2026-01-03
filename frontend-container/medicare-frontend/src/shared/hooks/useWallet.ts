import { useEffect, useState } from "react";

import type { Appointment } from "../../features/appointments/types";
import { useAuth } from "../auth/AuthContext";
import { appointmentsApi } from "../services/appointmentsApi";
import { type PatientPlanResponse, plansApi } from "../services/plansApi";
import {
  type Subscription,
  walletApi,
  type WalletData,
} from "../services/walletApi";

interface UseWalletReturn {
  wallet: WalletData | null;
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
  payAppointment: (
    appointmentId: string,
    amountCents?: number
  ) => Promise<boolean>;
  renewSubscription: () => Promise<boolean>;
}

export const useWallet = (): UseWalletReturn => {
  const { user } = useAuth();
  const [wallet, setWallet] = useState<WalletData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchWallet = async () => {
    if (!user?.id) {
      setLoading(false);
      setError("User not authenticated");
      return;
    }

    try {
      setLoading(true);
      setError(null);

      // Fetch appointments for the current user
      const appointmentsResponse =
        await appointmentsApi.getAppointmentsForPatient(user.id);

      // Fetch subscription/plan info from BillingService
      let subscriptionData: Subscription | null = null;
      try {
        const planResponse: PatientPlanResponse = await plansApi.getPatientPlan(
          user.id
        );
        if (planResponse.subscription) {
          subscriptionData = {
            id: planResponse.subscription.id,
            type: planResponse.plan?.name || "Unknown",
            active: planResponse.subscription.status === "Active",
            renewalDate: planResponse.subscription.periodEnd,
            periodStart: planResponse.subscription.periodStart,
            periodEnd: planResponse.subscription.periodEnd,
          };
        }
      } catch {
        // Subscription fetch may fail if patient has no subscription
        console.log("No active subscription found for patient");
      }

      if (appointmentsResponse.success) {
        // Filter for unpaid appointments only
        const unpaidAppointments = appointmentsResponse.data.filter(
          (appointment: Appointment) => appointment.paymentStatus === "not_paid"
        );

        setWallet({
          subscription: subscriptionData,
          unpaidAppointments,
        });
      } else {
        setError(appointmentsResponse.error || "Failed to fetch wallet data");
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "An error occurred");
    } finally {
      setLoading(false);
    }
  };

  const payAppointment = async (
    appointmentId: string,
    amountCents: number = 10000
  ): Promise<boolean> => {
    if (!user?.id) {
      setError("User not authenticated");
      return false;
    }

    try {
      const response = await walletApi.payAppointment(
        appointmentId,
        user.id,
        amountCents
      );

      if (response.success) {
        // Remove the paid appointment from unpaid appointments list
        setWallet((prev) =>
          prev
            ? {
                ...prev,
                unpaidAppointments: prev.unpaidAppointments.filter(
                  (apt) => apt.id !== appointmentId
                ),
              }
            : prev
        );
        return true;
      } else {
        setError(response.error || "Payment failed");
        return false;
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Payment failed");
      return false;
    }
  };

  const renewSubscription = async (): Promise<boolean> => {
    if (!wallet?.subscription?.id) {
      setError("No subscription to renew");
      return false;
    }

    try {
      const response = await walletApi.renewSubscription(
        wallet.subscription.id
      );

      if (response.success) {
        // Refetch wallet data to get updated subscription status
        await fetchWallet();
        return true;
      } else {
        setError(response.error || "Failed to renew subscription");
        return false;
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to renew subscription"
      );
      return false;
    }
  };

  const refetch = async () => {
    await fetchWallet();
  };

  useEffect(() => {
    fetchWallet();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [user?.id]);

  return {
    wallet,
    loading,
    error,
    refetch,
    payAppointment,
    renewSubscription,
  };
};
