import { api } from "./api";

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

const buildQuery = (filters?: DoctorPerformanceFilters) => {
  if (!filters) return "";
  const p = new URLSearchParams();
  if (filters.startDate) p.append("startDate", filters.startDate);
  if (filters.endDate) p.append("endDate", filters.endDate);
  const qs = p.toString();
  return qs ? `?${qs}` : "";
};

export const doctorPerformanceApi = {
  getSummary: async (
    filters?: DoctorPerformanceFilters
  ): Promise<DoctorPerformanceSummaryResponse> => {
    return api.get<DoctorPerformanceSummaryResponse>(
      `/practitioner/doctor-performance/summary${buildQuery(filters)}`
    );
  },
};
