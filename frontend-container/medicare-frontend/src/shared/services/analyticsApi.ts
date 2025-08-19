// Analytics API service aligned with Medicare database schema
// Based on actual tables: Schedule_Appointment, Appointment_Payment, Rate, Doctor_Specialization, etc.

import { apiClient } from "./apiClient";

// Base API Response interface
interface ApiResponse<T> {
  success: boolean;
  data: T;
  error: string | null;
}

// Analytics interfaces based on actual database schema
export interface AppointmentMetric {
  id: string;
  title: string;
  value: number;
  change: number;
  period: string;
  icon: "calendar" | "trending" | "users" | "clock" | "dollar" | "star";
}

export interface TrendData {
  date: string; // From Schedule_Appointment.Day
  appointments: number; // COUNT(*) from Schedule_Appointment
  completed: number; // COUNT(*) WHERE Schedule_Appointment_Status = 'completed'
  cancelled: number; // COUNT(*) WHERE Schedule_Appointment_Status = 'cancelled'
  noShow: number; // COUNT(*) WHERE Schedule_Appointment_Status = 'no-show'
  revenue: number; // SUM(Amount) from Appointment_Payment WHERE Status = 'Paid'
}

export interface DoctorPerformance {
  id: string; // Doctor.Id
  name: string; // User_Profile.FirstName + LastName
  specialization: string; // From Doctor_Specialization -> Specialization.Name (primary)
  totalAppointments: number; // COUNT(*) from Schedule_Appointment WHERE Doctor_User_Id
  completedAppointments: number; // COUNT(*) WHERE status = 'completed'
  cancelledAppointments: number; // COUNT(*) WHERE status = 'cancelled'
  noShowAppointments: number; // COUNT(*) WHERE status = 'no-show'
  averageRating: number; // AVG(Rate_Value) from Rate WHERE Doctor_User_Id
  totalRatings: number; // COUNT(*) from Rate WHERE Doctor_User_Id
  revenue: number; // SUM(Amount) from Appointment_Payment for this doctor's appointments
  utilizationRate: number; // (completed / total) * 100
}

export interface SpecializationStats {
  specialization: string; // Specialization.Name
  totalAppointments: number; // COUNT(*) through Doctor_Specialization JOIN
  totalPatients: number; // COUNT(DISTINCT Patient_User_Id)
  totalDoctors: number; // COUNT(DISTINCT Doctor_Id) from Doctor_Specialization
  averageAppointmentDuration: number; // AVG(Duration_Minutes) from Schedule_Appointment
  revenue: number; // SUM(Amount) from Appointment_Payment for this specialization
  completionRate: number; // (completed / total) * 100
  averageRating: number; // AVG(Rate_Value) for doctors in this specialization
}

export interface TimeSlotData {
  hour: number; // HOUR(Schedule_Appointment.Day)
  timeSlot: string; // Formatted time range
  monday: number; // COUNT(*) WHERE DATEPART(weekday, Day) = 2
  tuesday: number; // COUNT(*) WHERE DATEPART(weekday, Day) = 3
  wednesday: number; // COUNT(*) WHERE DATEPART(weekday, Day) = 4
  thursday: number; // COUNT(*) WHERE DATEPART(weekday, Day) = 5
  friday: number; // COUNT(*) WHERE DATEPART(weekday, Day) = 6
  saturday: number; // COUNT(*) WHERE DATEPART(weekday, Day) = 7
  sunday: number; // COUNT(*) WHERE DATEPART(weekday, Day) = 1
  totalAppointments: number; // Total for this hour
  averageRevenue: number; // AVG revenue for this time slot
  completionRate: number; // (completed / total) * 100 for this hour
}

export interface DayData {
  day: string; // Day name
  totalAppointments: number; // COUNT(*) for this day of week
  peakHour: string; // Hour with most appointments
  revenue: number; // SUM(revenue) for this day of week
  utilizationRate: number; // Based on available vs booked slots
}

export interface AnalyticsFilters {
  startDate?: string;
  endDate?: string;
  specialization?: string;
  doctorId?: string;
  status?: string;
}

// Helper function to build query parameters
const buildQueryParams = (filters?: AnalyticsFilters): string => {
  if (!filters) return "";
  
  const params = new URLSearchParams();
  
  if (filters.startDate) params.append("startDate", filters.startDate);
  if (filters.endDate) params.append("endDate", filters.endDate);
  if (filters.doctorId) params.append("doctorId", filters.doctorId);
  if (filters.specialization) params.append("specialization", filters.specialization);
  if (filters.status) params.append("status", filters.status);
  
  const queryString = params.toString();
  return queryString ? `?${queryString}` : "";
};

// Helper function to handle API responses
const handleApiResponse = <T>(response: any): ApiResponse<T> => {
  return {
    success: true,
    data: response.data,
    error: null,
  };
};

// Helper function to handle API errors
const handleApiError = <T>(error: any): ApiResponse<T> => {
  console.error("Analytics API Error:", error);
  
  let errorMessage = "An unexpected error occurred";
  
  if (error.response?.data?.message) {
    errorMessage = error.response.data.message;
  } else if (error.response?.data?.errors) {
    errorMessage = Array.isArray(error.response.data.errors) 
      ? error.response.data.errors.join(", ")
      : error.response.data.errors;
  } else if (error.message) {
    errorMessage = error.message;
  } else if (error.response?.status === 401) {
    errorMessage = "Unauthorized. Please check your permissions.";
  } else if (error.response?.status === 403) {
    errorMessage = "Forbidden. You don't have access to this data.";
  } else if (error.response?.status === 404) {
    errorMessage = "Analytics endpoint not found.";
  } else if (error.response?.status >= 500) {
    errorMessage = "Server error. Please try again later.";
  }

  return {
    success: false,
    data: [] as any,
    error: errorMessage,
  };
};

// API service implementation
const analyticsApi = {
  // Get appointment metrics
  getAppointmentMetrics: async (
    filters?: AnalyticsFilters
  ): Promise<ApiResponse<AppointmentMetric[]>> => {
    try {
      const queryParams = buildQueryParams(filters);
      const response = await apiClient.get(`/appointment/analytics/metrics${queryParams}`);
      return handleApiResponse<AppointmentMetric[]>(response);
    } catch (error) {
      return handleApiError<AppointmentMetric[]>(error);
    }
  },

  // Get appointment trends
  getAppointmentTrends: async (
    filters?: AnalyticsFilters
  ): Promise<ApiResponse<TrendData[]>> => {
    try {
      const queryParams = buildQueryParams(filters);
      const response = await apiClient.get(`/appointment/analytics/trends${queryParams}`);
      return handleApiResponse<TrendData[]>(response);
    } catch (error) {
      return handleApiError<TrendData[]>(error);
    }
  },

  // Get doctor performance data
  getDoctorPerformance: async (
    filters?: AnalyticsFilters
  ): Promise<ApiResponse<DoctorPerformance[]>> => {
    try {
      const queryParams = buildQueryParams(filters);
      const response = await apiClient.get(`/appointment/analytics/doctor-performance${queryParams}`);
      return handleApiResponse<DoctorPerformance[]>(response);
    } catch (error) {
      return handleApiError<DoctorPerformance[]>(error);
    }
  },

  // Get specialization statistics
  getSpecializationStats: async (
    filters?: AnalyticsFilters
  ): Promise<ApiResponse<SpecializationStats[]>> => {
    try {
      const queryParams = buildQueryParams(filters);
      const response = await apiClient.get(`/appointment/analytics/specialization-stats${queryParams}`);
      return handleApiResponse<SpecializationStats[]>(response);
    } catch (error) {
      return handleApiError<SpecializationStats[]>(error);
    }
  },

  // Get time slot analysis
  getTimeSlotAnalysis: async (
    filters?: AnalyticsFilters
  ): Promise<
    ApiResponse<{
      timeSlots: TimeSlotData[];
      weeklyData: DayData[];
    }>
  > => {
    try {
      const queryParams = buildQueryParams(filters);
      const response = await apiClient.get(`/appointment/analytics/time-slot-analysis${queryParams}`);
      return handleApiResponse<{ timeSlots: TimeSlotData[]; weeklyData: DayData[] }>(response);
    } catch (error) {
      return handleApiError<{ timeSlots: TimeSlotData[]; weeklyData: DayData[] }>(error);
    }
  },

  // Get comprehensive analytics dashboard data
  getDashboardData: async (
    filters?: AnalyticsFilters
  ): Promise<
    ApiResponse<{
      metrics: AppointmentMetric[];
      trends: TrendData[];
      doctorPerformance: DoctorPerformance[];
      specializationStats: SpecializationStats[];
      timeAnalysis: {
        timeSlots: TimeSlotData[];
        weeklyData: DayData[];
      };
    }>
  > => {
    try {
      const queryParams = buildQueryParams(filters);
      const response = await apiClient.get(`/appointment/analytics/dashboard${queryParams}`);
      return handleApiResponse<{
        metrics: AppointmentMetric[];
        trends: TrendData[];
        doctorPerformance: DoctorPerformance[];
        specializationStats: SpecializationStats[];
        timeAnalysis: {
          timeSlots: TimeSlotData[];
          weeklyData: DayData[];
        };
      }>(response);
    } catch (error) {
      return handleApiError<{
        metrics: AppointmentMetric[];
        trends: TrendData[];
        doctorPerformance: DoctorPerformance[];
        specializationStats: SpecializationStats[];
        timeAnalysis: {
          timeSlots: TimeSlotData[];
          weeklyData: DayData[];
        };
      }>(error);
    }
  },
};

// Export the analytics API and types
export { analyticsApi };
