import { useCallback, useEffect, useState } from "react";

import type { Appointment } from "../../features/appointments/types";
import { useAuth } from "../auth/AuthContext";
import { appointmentsApi } from "../services/appointmentsApi";

interface UseAppointmentsReturn {
  appointments: Appointment[];
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
  updatePayment: (
    id: string,
    paymentData: { paymentStatus: "paid" | "not_paid" }
  ) => Promise<boolean>;
  cancelAppointment: (id: string) => Promise<boolean>;
}

export const useAppointments = (): UseAppointmentsReturn => {
  const { user } = useAuth();
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchAppointments = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      if (!user?.id) {
        setAppointments([]);
        setLoading(false);
        return;
      }

      const response = await appointmentsApi.getAppointmentsForPatient(user.id);

      if (response.success) {
        setAppointments(response.data);
      } else {
        setError(response.error || "Failed to fetch appointments");
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "An unexpected error occurred"
      );
    } finally {
      setLoading(false);
    }
  }, [user?.id]);

  const updatePayment = async (
    id: string,
    paymentData: { paymentStatus: "paid" | "not_paid" }
  ): Promise<boolean> => {
    try {
      const response = await appointmentsApi.updatePaymentStatus(
        id,
        paymentData
      );

      if (response.success) {
        // Update local state
        setAppointments((prev: Appointment[]) =>
          prev.map((apt: Appointment) =>
            apt.id === id
              ? { ...apt, paymentStatus: paymentData.paymentStatus }
              : apt
          )
        );
        return true;
      } else {
        setError(response.error || "Failed to update payment");
        return false;
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update payment");
      return false;
    }
  };

  const cancelAppointment = async (id: string): Promise<boolean> => {
    try {
      const response = await appointmentsApi.cancelAppointment(id);

      if (response.success) {
        // Update local state
        setAppointments((prev: Appointment[]) =>
          prev.map((apt: Appointment) =>
            apt.id === id ? { ...apt, status: "cancelled" } : apt
          )
        );
        return true;
      } else {
        setError(response.error || "Failed to cancel appointment");
        return false;
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to cancel appointment"
      );
      return false;
    }
  };

  const refetch = async () => {
    await fetchAppointments();
  };

  useEffect(() => {
    fetchAppointments();
  }, [fetchAppointments]);

  return {
    appointments,
    loading,
    error,
    refetch,
    updatePayment,
    cancelAppointment,
  };
};
