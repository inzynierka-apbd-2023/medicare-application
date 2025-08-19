import { apiClient } from "./apiClient";

export interface DoctorPerformanceSummaryResponse {
  totalDoctors: number;
  averageAppointmentsPerDoctor: number;
  topRatedDoctor: string;
  doctorAverageRating: number;
  startDate?: string;
  endDate?: string;
  isStub: boolean;
}

export interface DoctorPerformanceFilters {
  startDate?: string;
  endDate?: string;
}

interface ApiResult<T> {
  success: boolean;
  data: T | null;
  error: string | null;
}

const buildQuery = (filters?: DoctorPerformanceFilters) => {
  if (!filters) return "";
  const p = new URLSearchParams();
  if (filters.startDate) p.append("startDate", filters.startDate);
  if (filters.endDate) p.append("endDate", filters.endDate);
  const qs = p.toString();
  return qs ? `?${qs}` : "";
};

export const doctorPerformanceApi = {
  async getSummary(filters?: DoctorPerformanceFilters): Promise<ApiResult<DoctorPerformanceSummaryResponse>> {
    try {
  const res = await apiClient.get(`/appointment/analytics/doctor-performance/summary${buildQuery(filters)}`);
      return { success: true, data: res.data, error: null };
    } catch (err: any) {
      let message = "Failed to load doctor performance summary";
      if (err.response?.data?.errors) {
        message = Array.isArray(err.response.data.errors) ? err.response.data.errors.join(", ") : err.response.data.errors;
      } else if (err.message) {
        message = err.message;
      }
      return { success: false, data: null, error: message };
    }
  }
};
