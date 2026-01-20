import { useCallback, useEffect, useState } from "react";
import { ReceptionistDashboardApiService } from "@features/dashboard/receptionist/services/receptionistDashboardApiService";
import type { ReceptionistDashboardData } from "@features/dashboard/receptionist/types";

interface UseReceptionistDashboardOptions {
  autoRefresh?: boolean;
  refreshInterval?: number;
}

export const useReceptionistDashboard = (
  options: UseReceptionistDashboardOptions = {}
) => {
  const { autoRefresh = true, refreshInterval = 30000 } = options;

  const [dashboardData, setDashboardData] =
    useState<ReceptionistDashboardData | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadDashboardData = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);

      const data = await ReceptionistDashboardApiService.getDashboardData();
      setDashboardData(data);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "An unexpected error occurred"
      );
    } finally {
      setIsLoading(false);
    }
  }, []);

  const refreshData = useCallback(async () => {
    await loadDashboardData();
  }, [loadDashboardData]);

  // Initial load
  useEffect(() => {
    loadDashboardData();
  }, [loadDashboardData]);

  // Auto-refresh functionality
  useEffect(() => {
    if (autoRefresh) {
      const interval = setInterval(loadDashboardData, refreshInterval);
      return () => clearInterval(interval);
    }
    return undefined;
  }, [autoRefresh, refreshInterval, loadDashboardData]);

  return {
    dashboardData,
    isLoading,
    error,
    refreshData,
  };
};
