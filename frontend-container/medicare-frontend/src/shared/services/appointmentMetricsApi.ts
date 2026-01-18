import { api } from "./api";

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
  endDate?: string; // yyyy-MM-dd
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
  getAppointmentMetrics: async (
    filters?: AppointmentMetricsFilters
  ): Promise<AppointmentMetricsResponse> => {
    return api.get<AppointmentMetricsResponse>(
      `/appointment/metrics${buildQuery(filters)}`
    );
  },
};
