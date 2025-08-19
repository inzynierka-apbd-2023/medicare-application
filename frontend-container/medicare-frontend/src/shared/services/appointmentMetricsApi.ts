import { apiClient } from "./apiClient";

export interface AppointmentMetricsResponse {
  totalAppointments: number;
  appointmentsThisMonth: number;
  completedAppointments: number;
  cancelledAppointments: number;
  noShowAppointments: number;
  completionRate: number; // percentage 0-100
  startDate?: string;
  endDate?: string;
  isStub: boolean;
}

export interface AppointmentMetricsFilters {
  startDate?: string; // yyyy-MM-dd
  endDate?: string;   // yyyy-MM-dd
}

interface ApiResult<T> {
  success: boolean;
  data: T | null;
  error: string | null;
}

const buildQuery = (filters?: AppointmentMetricsFilters) => {
  if (!filters) return "";
  const p = new URLSearchParams();
  if (filters.startDate) p.append("startDate", filters.startDate);
  if (filters.endDate) p.append("endDate", filters.endDate);
  const qs = p.toString();
  return qs ? `?${qs}` : "";
};

export const appointmentMetricsApi = {
  async getAppointmentMetrics(filters?: AppointmentMetricsFilters): Promise<ApiResult<AppointmentMetricsResponse>> {
    try {
      const res = await apiClient.get(`/appointment/metrics${buildQuery(filters)}`);
      return { success: true, data: res.data, error: null };
    } catch (err: any) {
      let message = "Failed to load appointment metrics";
      if (err.response?.data?.errors) {
        message = Array.isArray(err.response.data.errors) ? err.response.data.errors.join(", ") : err.response.data.errors;
      } else if (err.message) {
        message = err.message;
      }
      return { success: false, data: null, error: message };
    }
  }
};
