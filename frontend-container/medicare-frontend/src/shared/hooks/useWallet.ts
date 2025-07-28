import { useState, useEffect } from "react";
import { walletApi, type WalletAppointment } from "../services/walletApi";
import type { WalletData, Subscription } from "../../features/wallet/types";

interface UseWalletReturn {
  wallet: WalletData | null;
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
  payAppointment: (appointmentId: string) => Promise<boolean>;
  updateSubscription: (
    subscriptionData: Partial<Subscription>
  ) => Promise<boolean>;
}

export const useWallet = (): UseWalletReturn => {
  const [wallet, setWallet] = useState<WalletData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchWallet = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await walletApi.getWalletData();

      if (response.success) {
        setWallet(response.data);
      } else {
        setError(response.error || "Failed to fetch wallet data");
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "An error occurred");
    } finally {
      setLoading(false);
    }
  };

  const payAppointment = async (appointmentId: string): Promise<boolean> => {
    try {
      const response = await walletApi.payAppointment(appointmentId);

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

  const updateSubscription = async (
    subscriptionData: Partial<Subscription>
  ): Promise<boolean> => {
    try {
      const response = await walletApi.updateSubscription(subscriptionData);

      if (response.success) {
        setWallet((prev) =>
          prev
            ? {
                ...prev,
                subscription: response.data,
              }
            : prev
        );
        return true;
      } else {
        setError(response.error || "Failed to update subscription");
        return false;
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to update subscription"
      );
      return false;
    }
  };

  const refetch = async () => {
    await fetchWallet();
  };

  useEffect(() => {
    fetchWallet();
  }, []);

  return {
    wallet,
    loading,
    error,
    refetch,
    payAppointment,
    updateSubscription,
  };
};
