import { useCallback, useEffect, useState } from "react";

import {
  analyticsApi,
  type AnalyticsFilters,
  type AppointmentMetric,
  type DayData,
  type DoctorPerformance,
  type SpecializationStats,
  type TimeSlotData,
  type TrendData,
} from "../../../shared/services/analyticsApi";

interface UseAnalyticsReturn {
  // Data
  metrics: AppointmentMetric[];
  trends: TrendData[];
  doctorPerformance: DoctorPerformance[];
  specializationStats: SpecializationStats[];
  timeSlotData: TimeSlotData[];
  weeklyData: DayData[];

  // Loading states
  isLoading: boolean;
  isLoadingMetrics: boolean;
  isLoadingTrends: boolean;
  isLoadingDoctors: boolean;
  isLoadingSpecializations: boolean;
  isLoadingTimeSlots: boolean;

  // Error handling
  error: string | null;

  // Actions
  refetchData: () => Promise<void>;
  refetchMetrics: () => Promise<void>;
  refetchTrends: () => Promise<void>;
  refetchDoctorPerformance: () => Promise<void>;
  refetchSpecializationStats: () => Promise<void>;
  refetchTimeSlotAnalysis: () => Promise<void>;
  clearError: () => void;

  // Filters
  filters: AnalyticsFilters;
  setFilters: (filters: AnalyticsFilters) => void;
  updateFilters: (partialFilters: Partial<AnalyticsFilters>) => void;
}

export const useAnalytics = (
  initialFilters?: AnalyticsFilters
): UseAnalyticsReturn => {
  // State for all analytics data
  const [metrics, setMetrics] = useState<AppointmentMetric[]>([]);
  const [trends, setTrends] = useState<TrendData[]>([]);
  const [doctorPerformance, setDoctorPerformance] = useState<
    DoctorPerformance[]
  >([]);
  const [specializationStats, setSpecializationStats] = useState<
    SpecializationStats[]
  >([]);
  const [timeSlotData, setTimeSlotData] = useState<TimeSlotData[]>([]);
  const [weeklyData, setWeeklyData] = useState<DayData[]>([]);

  // Loading states
  const [isLoadingMetrics, setIsLoadingMetrics] = useState(false);
  const [isLoadingTrends, setIsLoadingTrends] = useState(false);
  const [isLoadingDoctors, setIsLoadingDoctors] = useState(false);
  const [isLoadingSpecializations, setIsLoadingSpecializations] =
    useState(false);
  const [isLoadingTimeSlots, setIsLoadingTimeSlots] = useState(false);

  // Error handling
  const [error, setError] = useState<string | null>(null);

  // Filters
  const [filters, setFiltersState] = useState<AnalyticsFilters>(
    initialFilters || {}
  );

  // Computed loading state
  const isLoading =
    isLoadingMetrics ||
    isLoadingTrends ||
    isLoadingDoctors ||
    isLoadingSpecializations ||
    isLoadingTimeSlots;

  // Fetch functions
  const refetchMetrics = useCallback(async () => {
    setIsLoadingMetrics(true);
    setError(null);

    try {
      const data = await analyticsApi.getAppointmentMetrics(filters);
      setMetrics(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch metrics");
    } finally {
      setIsLoadingMetrics(false);
    }
  }, [filters]);

  const refetchTrends = useCallback(async () => {
    setIsLoadingTrends(true);
    setError(null);

    try {
      const data = await analyticsApi.getAppointmentTrends(filters);
      setTrends(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch trends");
    } finally {
      setIsLoadingTrends(false);
    }
  }, [filters]);

  const refetchDoctorPerformance = useCallback(async () => {
    setIsLoadingDoctors(true);
    setError(null);

    try {
      const data = await analyticsApi.getDoctorPerformance(filters);
      setDoctorPerformance(data);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Failed to fetch doctor performance"
      );
    } finally {
      setIsLoadingDoctors(false);
    }
  }, [filters]);

  const refetchSpecializationStats = useCallback(async () => {
    setIsLoadingSpecializations(true);
    setError(null);

    try {
      const data = await analyticsApi.getSpecializationStats(filters);
      setSpecializationStats(data);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Failed to fetch specialization stats"
      );
    } finally {
      setIsLoadingSpecializations(false);
    }
  }, [filters]);

  const refetchTimeSlotAnalysis = useCallback(async () => {
    setIsLoadingTimeSlots(true);
    setError(null);

    try {
      const data = await analyticsApi.getTimeSlotAnalysis(filters);

      setTimeSlotData(data.timeSlots);
      setWeeklyData(data.weeklyData);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Failed to fetch time slot analysis"
      );
    } finally {
      setIsLoadingTimeSlots(false);
    }
  }, [filters]);

  const refetchData = useCallback(async () => {
    await Promise.all([
      refetchMetrics(),
      refetchTrends(),
      refetchDoctorPerformance(),
      refetchSpecializationStats(),
      refetchTimeSlotAnalysis(),
    ]);
  }, [
    refetchMetrics,
    refetchTrends,
    refetchDoctorPerformance,
    refetchSpecializationStats,
    refetchTimeSlotAnalysis,
  ]);

  // Filter management
  const setFilters = useCallback((newFilters: AnalyticsFilters) => {
    setFiltersState(newFilters);
  }, []);

  const updateFilters = useCallback(
    (partialFilters: Partial<AnalyticsFilters>) => {
      setFiltersState((prev: AnalyticsFilters) => ({
        ...prev,
        ...partialFilters,
      }));
    },
    []
  );

  // Error clearing
  const clearError = useCallback(() => {
    setError(null);
  }, []);

  // Initial data fetch
  useEffect(() => {
    refetchData();
  }, [refetchData]);

  return {
    // Data
    metrics,
    trends,
    doctorPerformance,
    specializationStats,
    timeSlotData,
    weeklyData,

    // Loading states
    isLoading,
    isLoadingMetrics,
    isLoadingTrends,
    isLoadingDoctors,
    isLoadingSpecializations,
    isLoadingTimeSlots,

    // Error handling
    error,

    // Actions
    refetchData,
    refetchMetrics,
    refetchTrends,
    refetchDoctorPerformance,
    refetchSpecializationStats,
    refetchTimeSlotAnalysis,
    clearError,

    // Filters
    filters,
    setFilters,
    updateFilters,
  };
};
