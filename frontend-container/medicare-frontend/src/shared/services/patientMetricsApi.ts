import { api } from "./api";

export interface PatientMetricsResponse {
  totalActivePatients: number;
  newPatients: number;
  retentionRate: number; // percentage 0-100
  averageRating: number; // 0-5
  totalRatings: number;
  startDate?: string;
  endDate?: string;
  isStub: boolean;
}

export interface PatientMetricsFilters {
  startDate?: string; // yyyy-MM-dd
  endDate?: string; // yyyy-MM-dd
}

const buildQuery = (filters?: PatientMetricsFilters) => {
  if (!filters) return "";
  const p = new URLSearchParams();
  if (filters.startDate) p.append("startDate", filters.startDate);
  if (filters.endDate) p.append("endDate", filters.endDate);
  const qs = p.toString();
  return qs ? `?${qs}` : "";
};

export const patientMetricsApi = {
  getPatientMetrics: async (
    filters?: PatientMetricsFilters
  ): Promise<PatientMetricsResponse> => {
    return api.get<PatientMetricsResponse>(
      `/patient/metrics${buildQuery(filters)}`
    );
  },
};
