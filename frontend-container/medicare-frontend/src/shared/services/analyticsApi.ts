import { api } from "./api";

export interface AppointmentMetric {
  id: string;
  title: string;
  value: number;
  change: number;
  period: string;
  icon: "calendar" | "trending" | "users" | "clock" | "dollar" | "star";
}

export interface TrendData {
  date: string;
  appointments: number;
  completed: number;
  cancelled: number;
  noShow: number;
  revenue: number;
}

export interface DoctorPerformance {
  id: string;
  name: string;
  specialization: string;
  totalAppointments: number;
  completedAppointments: number;
  cancelledAppointments: number;
  noShowAppointments: number;
  averageRating: number;
  totalRatings: number;
  revenue: number;
  utilizationRate: number;
}

export interface SpecializationStats {
  specialization: string;
  totalAppointments: number;
  totalPatients: number;
  totalDoctors: number;
  averageAppointmentDuration: number;
  revenue: number;
  completionRate: number;
  averageRating: number;
}

export interface TimeSlotData {
  hour: number;
  timeSlot: string;
  monday: number;
  tuesday: number;
  wednesday: number;
  thursday: number;
  friday: number;
  saturday: number;
  sunday: number;
  totalAppointments: number;
  averageRevenue: number;
  completionRate: number;
}

export interface DayData {
  day: string;
  totalAppointments: number;
  peakHour: string;
  revenue: number;
  utilizationRate: number;
}

export interface AnalyticsFilters {
  startDate?: string;
  endDate?: string;
  specialization?: string;
  doctorId?: string;
  status?: string;
}

const buildQueryParams = (filters?: AnalyticsFilters): string => {
  if (!filters) return "";

  const params = new URLSearchParams();

  if (filters.startDate) params.append("startDate", filters.startDate);
  if (filters.endDate) params.append("endDate", filters.endDate);
  if (filters.doctorId) params.append("doctorId", filters.doctorId);
  if (filters.specialization)
    params.append("specialization", filters.specialization);
  if (filters.status) params.append("status", filters.status);

  const queryString = params.toString();
  return queryString ? `?${queryString}` : "";
};

const analyticsApi = {
  getAppointmentMetrics: async (
    filters?: AnalyticsFilters
  ): Promise<AppointmentMetric[]> => {
    const queryParams = buildQueryParams(filters);
    return api.get<AppointmentMetric[]>(
      `/appointment/analytics/metrics${queryParams}`
    );
  },

  getAppointmentTrends: async (
    filters?: AnalyticsFilters
  ): Promise<TrendData[]> => {
    const queryParams = buildQueryParams(filters);
    return api.get<TrendData[]>(`/appointment/analytics/trends${queryParams}`);
  },

  getDoctorPerformance: async (
    filters?: AnalyticsFilters
  ): Promise<DoctorPerformance[]> => {
    const queryParams = buildQueryParams(filters);
    return api.get<DoctorPerformance[]>(
      `/appointment/analytics/doctor-performance${queryParams}`
    );
  },

  getSpecializationStats: async (
    filters?: AnalyticsFilters
  ): Promise<SpecializationStats[]> => {
    const queryParams = buildQueryParams(filters);
    return api.get<SpecializationStats[]>(
      `/appointment/analytics/specialization-stats${queryParams}`
    );
  },

  getTimeSlotAnalysis: async (
    filters?: AnalyticsFilters
  ): Promise<{
    timeSlots: TimeSlotData[];
    weeklyData: DayData[];
  }> => {
    const queryParams = buildQueryParams(filters);
    return api.get<{ timeSlots: TimeSlotData[]; weeklyData: DayData[] }>(
      `/appointment/analytics/time-slot-analysis${queryParams}`
    );
  },

  getDashboardData: async (
    filters?: AnalyticsFilters
  ): Promise<{
    metrics: AppointmentMetric[];
    trends: TrendData[];
    doctorPerformance: DoctorPerformance[];
    specializationStats: SpecializationStats[];
    timeAnalysis: {
      timeSlots: TimeSlotData[];
      weeklyData: DayData[];
    };
  }> => {
    const queryParams = buildQueryParams(filters);
    return api.get<{
      metrics: AppointmentMetric[];
      trends: TrendData[];
      doctorPerformance: DoctorPerformance[];
      specializationStats: SpecializationStats[];
      timeAnalysis: {
        timeSlots: TimeSlotData[];
        weeklyData: DayData[];
      };
    }>(`/appointment/analytics/dashboard${queryParams}`);
  },
};

export { analyticsApi };
