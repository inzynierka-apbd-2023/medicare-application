// Analytics API service aligned with Medicare database schema
// Based on actual tables: Schedule_Appointment, Appointment_Payment, Rate, Doctor_Specialization, etc.

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

// Mock data generators based on realistic database scenarios
const generateMockMetrics = (): AppointmentMetric[] => [
  {
    id: "1",
    title: "Total Appointments",
    value: 847, // Realistic number for a clinic
    change: 8.2,
    period: "vs last month",
    icon: "calendar",
  },
  {
    id: "2",
    title: "Completed",
    value: 756, // ~89% completion rate
    change: 12.1,
    period: "vs last month",
    icon: "trending",
  },
  {
    id: "3",
    title: "Active Patients",
    value: 324, // Unique patients
    change: 6.5,
    period: "vs last month",
    icon: "users",
  },
  {
    id: "4",
    title: "Avg Duration",
    value: 32, // Average from Duration_Minutes (realistic for clinic)
    change: -1.2,
    period: "minutes",
    icon: "clock",
  },
  {
    id: "5",
    title: "Total Revenue",
    value: 42680, // From Appointment_Payment
    change: 15.8,
    period: "vs last month",
    icon: "dollar",
  },
  {
    id: "6",
    title: "Avg Rating",
    value: 4.7, // From Rate table
    change: 2.1,
    period: "vs last month",
    icon: "star",
  },
];

const generateMockTrends = (days: number = 30): TrendData[] => {
  const trends: TrendData[] = [];
  const baseDate = new Date();

  for (let i = days - 1; i >= 0; i--) {
    const date = new Date(baseDate);
    date.setDate(date.getDate() - i);

    // More realistic numbers for a clinic
    const appointments = Math.floor(Math.random() * 40) + 15; // 15-55 appointments per day
    const completed = Math.floor(appointments * (0.85 + Math.random() * 0.1)); // 85-95% completion
    const cancelled = Math.floor(
      (appointments - completed) * (0.6 + Math.random() * 0.3)
    );
    const noShow = appointments - completed - cancelled;

    trends.push({
      date: date.toISOString().split("T")[0],
      appointments,
      completed,
      cancelled,
      noShow,
      revenue: completed * (40 + Math.random() * 80), // $40-120 per appointment
    });
  }

  return trends;
};

const generateMockDoctorPerformance = (): DoctorPerformance[] => [
  {
    id: "doc_1",
    name: "Dr. Sarah Johnson",
    specialization: "Cardiology", // From your Specialization table
    totalAppointments: 156,
    completedAppointments: 139,
    cancelledAppointments: 12,
    noShowAppointments: 5,
    averageRating: 4.8,
    totalRatings: 67,
    revenue: 8340, // Realistic revenue
    utilizationRate: 89.1,
  },
  {
    id: "doc_2",
    name: "Dr. Michael Chen",
    specialization: "Internal Medicine",
    totalAppointments: 134,
    completedAppointments: 121,
    cancelledAppointments: 9,
    noShowAppointments: 4,
    averageRating: 4.6,
    totalRatings: 52,
    revenue: 7260,
    utilizationRate: 90.3,
  },
  {
    id: "doc_3",
    name: "Dr. Emily Rodriguez",
    specialization: "Pediatrics",
    totalAppointments: 189,
    completedAppointments: 174,
    cancelledAppointments: 11,
    noShowAppointments: 4,
    averageRating: 4.9,
    totalRatings: 89,
    revenue: 8700,
    utilizationRate: 92.1,
  },
  {
    id: "doc_4",
    name: "Dr. David Park",
    specialization: "Dermatology",
    totalAppointments: 143,
    completedAppointments: 128,
    cancelledAppointments: 10,
    noShowAppointments: 5,
    averageRating: 4.5,
    totalRatings: 61,
    revenue: 7680,
    utilizationRate: 89.5,
  },
  {
    id: "doc_5",
    name: "Dr. Lisa Wang",
    specialization: "Family Medicine",
    totalAppointments: 167,
    completedAppointments: 152,
    cancelledAppointments: 11,
    noShowAppointments: 4,
    averageRating: 4.7,
    totalRatings: 74,
    revenue: 7600,
    utilizationRate: 91.0,
  },
];

const generateMockSpecializationStats = (): SpecializationStats[] => [
  {
    specialization: "Cardiology",
    totalAppointments: 156, // Realistic for one cardiologist
    totalPatients: 89,
    totalDoctors: 1,
    averageAppointmentDuration: 35, // 35 minutes for cardiology
    revenue: 8340,
    completionRate: 89.1,
    averageRating: 4.8,
  },
  {
    specialization: "Internal Medicine",
    totalAppointments: 134,
    totalPatients: 78,
    totalDoctors: 1,
    averageAppointmentDuration: 30,
    revenue: 7260,
    completionRate: 90.3,
    averageRating: 4.6,
  },
  {
    specialization: "Pediatrics",
    totalAppointments: 189,
    totalPatients: 112,
    totalDoctors: 1,
    averageAppointmentDuration: 25, // Shorter for kids
    revenue: 8700,
    completionRate: 92.1,
    averageRating: 4.9,
  },
  {
    specialization: "Dermatology",
    totalAppointments: 143,
    totalPatients: 98,
    totalDoctors: 1,
    averageAppointmentDuration: 20, // Quick dermatology visits
    revenue: 7680,
    completionRate: 89.5,
    averageRating: 4.5,
  },
  {
    specialization: "Family Medicine",
    totalAppointments: 167,
    totalPatients: 95,
    totalDoctors: 1,
    averageAppointmentDuration: 25,
    revenue: 7600,
    completionRate: 91.0,
    averageRating: 4.7,
  },
];

const generateMockTimeSlotData = (): TimeSlotData[] => [
  {
    hour: 9,
    timeSlot: "09:00-10:00",
    monday: 12,
    tuesday: 15,
    wednesday: 14,
    thursday: 16,
    friday: 13,
    saturday: 8,
    sunday: 0,
    totalAppointments: 78,
    averageRevenue: 52,
    completionRate: 89.7,
  },
  {
    hour: 10,
    timeSlot: "10:00-11:00",
    monday: 18,
    tuesday: 19,
    wednesday: 17,
    thursday: 20,
    friday: 16,
    saturday: 12,
    sunday: 0,
    totalAppointments: 102,
    averageRevenue: 48,
    completionRate: 92.1,
  },
  {
    hour: 11,
    timeSlot: "11:00-12:00",
    monday: 16,
    tuesday: 17,
    wednesday: 18,
    thursday: 19,
    friday: 15,
    saturday: 10,
    sunday: 0,
    totalAppointments: 95,
    averageRevenue: 51,
    completionRate: 88.4,
  },
  {
    hour: 12,
    timeSlot: "12:00-13:00",
    monday: 8,
    tuesday: 9,
    wednesday: 10,
    thursday: 11,
    friday: 8,
    saturday: 5,
    sunday: 0,
    totalAppointments: 51,
    averageRevenue: 45,
    completionRate: 85.3,
  },
  {
    hour: 14,
    timeSlot: "14:00-15:00",
    monday: 14,
    tuesday: 16,
    wednesday: 15,
    thursday: 18,
    friday: 14,
    saturday: 9,
    sunday: 0,
    totalAppointments: 86,
    averageRevenue: 49,
    completionRate: 87.2,
  },
  {
    hour: 15,
    timeSlot: "15:00-16:00",
    monday: 13,
    tuesday: 14,
    wednesday: 16,
    thursday: 15,
    friday: 12,
    saturday: 8,
    sunday: 0,
    totalAppointments: 78,
    averageRevenue: 53,
    completionRate: 90.3,
  },
  {
    hour: 16,
    timeSlot: "16:00-17:00",
    monday: 11,
    tuesday: 12,
    wednesday: 13,
    thursday: 14,
    friday: 10,
    saturday: 6,
    sunday: 0,
    totalAppointments: 66,
    averageRevenue: 47,
    completionRate: 88.6,
  },
];

const generateMockWeeklyData = (): DayData[] => [
  {
    day: "Monday",
    totalAppointments: 69,
    peakHour: "10:00-11:00",
    revenue: 3450,
    utilizationRate: 91.2,
  },
  {
    day: "Tuesday",
    totalAppointments: 77,
    peakHour: "10:00-11:00",
    revenue: 3850,
    utilizationRate: 93.1,
  },
  {
    day: "Wednesday",
    totalAppointments: 78,
    peakHour: "15:00-16:00",
    revenue: 3900,
    utilizationRate: 94.5,
  },
  {
    day: "Thursday",
    totalAppointments: 85,
    peakHour: "10:00-11:00",
    revenue: 4250,
    utilizationRate: 95.7,
  },
  {
    day: "Friday",
    totalAppointments: 63,
    peakHour: "09:00-10:00",
    revenue: 3150,
    utilizationRate: 87.5,
  },
  {
    day: "Saturday",
    totalAppointments: 39,
    peakHour: "10:00-11:00",
    revenue: 1950,
    utilizationRate: 78.3,
  },
  {
    day: "Sunday",
    totalAppointments: 0,
    peakHour: "-",
    revenue: 0,
    utilizationRate: 0,
  },
];

// API service with mock data simulation
const analyticsApi = {
  // Get appointment metrics
  getAppointmentMetrics: async (
    _filters?: AnalyticsFilters
  ): Promise<ApiResponse<AppointmentMetric[]>> => {
    // Simulate API delay
    await new Promise((resolve) => setTimeout(resolve, 500));

    try {
      const metrics = generateMockMetrics();

      return {
        success: true,
        data: metrics,
        error: null,
      };
    } catch (error) {
      return {
        success: false,
        data: [],
        error:
          error instanceof Error ? error.message : "Failed to fetch metrics",
      };
    }
  },

  // Get appointment trends
  getAppointmentTrends: async (
    _filters?: AnalyticsFilters
  ): Promise<ApiResponse<TrendData[]>> => {
    await new Promise((resolve) => setTimeout(resolve, 600));

    try {
      const trends = generateMockTrends(30);

      return {
        success: true,
        data: trends,
        error: null,
      };
    } catch (error) {
      return {
        success: false,
        data: [],
        error:
          error instanceof Error ? error.message : "Failed to fetch trends",
      };
    }
  },

  // Get doctor performance data
  getDoctorPerformance: async (
    filters?: AnalyticsFilters
  ): Promise<ApiResponse<DoctorPerformance[]>> => {
    await new Promise((resolve) => setTimeout(resolve, 700));

    try {
      let performance = generateMockDoctorPerformance();

      // Apply filters if provided
      if (filters?.specialization) {
        performance = performance.filter(
          (doc) => doc.specialization === filters.specialization
        );
      }

      return {
        success: true,
        data: performance,
        error: null,
      };
    } catch (error) {
      return {
        success: false,
        data: [],
        error:
          error instanceof Error
            ? error.message
            : "Failed to fetch doctor performance",
      };
    }
  },

  // Get specialization statistics
  getSpecializationStats: async (
    _filters?: AnalyticsFilters
  ): Promise<ApiResponse<SpecializationStats[]>> => {
    await new Promise((resolve) => setTimeout(resolve, 500));

    try {
      const stats = generateMockSpecializationStats();

      return {
        success: true,
        data: stats,
        error: null,
      };
    } catch (error) {
      return {
        success: false,
        data: [],
        error:
          error instanceof Error
            ? error.message
            : "Failed to fetch specialization stats",
      };
    }
  },

  // Get time slot analysis
  getTimeSlotAnalysis: async (
    _filters?: AnalyticsFilters
  ): Promise<
    ApiResponse<{
      timeSlots: TimeSlotData[];
      weeklyData: DayData[];
    }>
  > => {
    await new Promise((resolve) => setTimeout(resolve, 800));

    try {
      const timeSlots = generateMockTimeSlotData();
      const weeklyData = generateMockWeeklyData();

      return {
        success: true,
        data: { timeSlots, weeklyData },
        error: null,
      };
    } catch (error) {
      return {
        success: false,
        data: { timeSlots: [], weeklyData: [] },
        error:
          error instanceof Error
            ? error.message
            : "Failed to fetch time slot analysis",
      };
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
    await new Promise((resolve) => setTimeout(resolve, 1000));

    try {
      const [metricsRes, trendsRes, performanceRes, statsRes, timeRes] =
        await Promise.all([
          analyticsApi.getAppointmentMetrics(filters),
          analyticsApi.getAppointmentTrends(filters),
          analyticsApi.getDoctorPerformance(filters),
          analyticsApi.getSpecializationStats(filters),
          analyticsApi.getTimeSlotAnalysis(filters),
        ]);

      if (
        !metricsRes.success ||
        !trendsRes.success ||
        !performanceRes.success ||
        !statsRes.success ||
        !timeRes.success
      ) {
        throw new Error("Failed to fetch all analytics data");
      }

      return {
        success: true,
        data: {
          metrics: metricsRes.data,
          trends: trendsRes.data,
          doctorPerformance: performanceRes.data,
          specializationStats: statsRes.data,
          timeAnalysis: timeRes.data,
        },
        error: null,
      };
    } catch (error) {
      return {
        success: false,
        data: {
          metrics: [],
          trends: [],
          doctorPerformance: [],
          specializationStats: [],
          timeAnalysis: { timeSlots: [], weeklyData: [] },
        },
        error:
          error instanceof Error
            ? error.message
            : "Failed to fetch dashboard data",
      };
    }
  },
};

// Export the analytics API and types
export { analyticsApi };
