import { apiClient } from "./apiClient";

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
  endDate?: string;   // yyyy-MM-dd
}

interface ApiResult<T> {
  success: boolean;
  data: T | null;
  error: string | null;
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
  async getPatientMetrics(filters?: PatientMetricsFilters): Promise<ApiResult<PatientMetricsResponse>> {
    try {
      const res = await apiClient.get(`/patient/metrics${buildQuery(filters)}`);
      return { success: true, data: res.data, error: null };
    } catch (err: any) {
      let message = "Failed to load patient metrics";
      if (err.response?.data?.errors) {
        message = Array.isArray(err.response.data.errors) ? err.response.data.errors.join(", ") : err.response.data.errors;
      } else if (err.message) {
        message = err.message;
      }
      return { success: false, data: null, error: message };
    }
  }
};
