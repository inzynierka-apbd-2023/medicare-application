import { apiClient } from "./apiClient";

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

export const plansApi = {
  /**
   * Get all available plans
   */
  async getPlans(): Promise<PlanDto[]> {
    const response = await apiClient.get<PlanDto[]>("/billing/plans");
    return response.data;
  },

  /**
   * Get a specific plan by code
   */
  async getPlan(code: string): Promise<PlanDto> {
    const response = await apiClient.get<PlanDto>(`/billing/plans/${code}`);
    return response.data;
  },

  /**
   * Get the current patient's plan
   */
  async getPatientPlan(patientId: string): Promise<PatientPlanResponse> {
    const response = await apiClient.get<PatientPlanResponse>(
      `/billing/plans/patient/${patientId}`
    );
    return response.data;
  },
};
