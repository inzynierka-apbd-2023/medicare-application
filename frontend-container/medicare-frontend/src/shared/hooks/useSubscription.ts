import { useEffect, useState } from "react";
import { useAuth } from "@shared/auth/AuthContext";
import { PlanDto, plansApi } from "@shared/services/plansApi";

export interface SubscriptionState {
  plan: PlanDto | null;
  isLoading: boolean;
  error: string | null;
  features: {
    hasMessaging: boolean;
    hasPrescriptions: boolean;
    hasDocuments: boolean;
  };
}

export const useSubscription = () => {
  const { user } = useAuth();
  const [state, setState] = useState<SubscriptionState>({
    plan: null,
    isLoading: true,
    error: null,
    features: {
      hasMessaging: false,
      hasPrescriptions: false,
      hasDocuments: false,
    },
  });

  useEffect(() => {
    const fetchPlan = async () => {
      if (!user?.id) {
        setState((prev) => ({ ...prev, isLoading: false }));
        return;
      }

      // Non-patients (Doctors, Staff) act as having all features enabled by default
      if (user.role !== "Patient") {
        setState({
          plan: null,
          isLoading: false,
          error: null,
          features: {
            hasMessaging: true,
            hasPrescriptions: true,
            hasDocuments: true,
          },
        });
        return;
      }
      try {
        const response = await plansApi.getPatientPlan(user.id);
        const plan = response.plan;

        setState({
          plan,
          isLoading: false,
          error: null,
          features: {
            hasMessaging: plan?.hasMessaging ?? false,
            hasPrescriptions: plan?.hasPrescriptions ?? false,
            hasDocuments: plan?.hasDocuments ?? false,
          },
        });
      } catch (err) {
        console.error("Failed to fetch subscription plan:", err);
        setState((prev) => ({
          ...prev,
          isLoading: false,
          error: "Failed to load subscription details",
        }));
      }
    };

    fetchPlan();
  }, [user?.id, user?.role]);

  return state;
};
