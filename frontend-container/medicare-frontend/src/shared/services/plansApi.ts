import { toastMessages } from "@shared/toast/toastMessages";

import { api } from "./api";

export interface PlanDto {
  code: string;
  name: string;
  description: string | null;
  priceCents: number;
  currency: string;
  billingPeriod: "monthly" | "yearly";
  freeVisitsPerMonth: number;
  hasMessaging: boolean;
  hasPrescriptions: boolean;
  hasDocuments: boolean;
}

export interface PatientPlanResponse {
  plan: PlanDto | null;
  subscription: {
    id: string;
    periodStart: string;
    periodEnd: string;
    status: string;
  } | null;
}

export interface UpdateSubscriptionResponse {
  success: boolean;
  errorMessage?: string;
  newPlanCode?: string;
  newPlanName?: string;
}

export const plansApi = {
  async getPlans(): Promise<PlanDto[]> {
    return await api.get<PlanDto[]>("/billing/plans", undefined, {
      showToastOnSuccess: false,
    });
  },

  async getPlan(code: string): Promise<PlanDto> {
    return await api.get<PlanDto>(`/billing/plans/${code}`, undefined, {
      showToastOnSuccess: false,
    });
  },

  async getPatientPlan(patientId: string): Promise<PatientPlanResponse> {
    return await api.get<PatientPlanResponse>(
      `/billing/plans/patient/${patientId}`,
      undefined,
      {
        showToastOnSuccess: false,
      }
    );
  },

  async updateSubscription(
    patientId: string,
    newPlanCode: string
  ): Promise<UpdateSubscriptionResponse> {
    return await api.put<UpdateSubscriptionResponse>(
      `/billing/plans/patient/${patientId}/subscription`,
      { newPlanCode },
      undefined,
      {
        showToastOnSuccess: true,
        successMessage: toastMessages.plans.updateSubscriptionSuccess,
      }
    );
  },
};
