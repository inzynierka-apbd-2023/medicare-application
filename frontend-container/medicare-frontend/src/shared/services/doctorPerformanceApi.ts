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
  async getSummary(
    filters?: DoctorPerformanceFilters
  ): Promise<ApiResult<DoctorPerformanceSummaryResponse>> {
    try {
      const res = await apiClient.get(
        `/practitioner/doctor-performance/summary${buildQuery(filters)}`
      );
      return { success: true, data: res.data, error: null };
    } catch (err: unknown) {
      let message = "Failed to load doctor performance summary";
      if (err && typeof err === "object" && "response" in err) {
        const response = (
          err as { response?: { data?: { errors?: string | string[] } } }
        ).response;
        if (response?.data?.errors) {
          message = Array.isArray(response.data.errors)
            ? response.data.errors.join(", ")
            : response.data.errors;
        }
      } else if (err instanceof Error) {
        message = err.message;
      }
      return { success: false, data: null, error: message };
    }
  },
};
