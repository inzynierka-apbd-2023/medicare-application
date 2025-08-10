import { useEffect, useState } from "react";

import Header from "../../layout/Header";
import { ErrorDisplay, LoadingOverlay } from "../../shared/components";
import { useLoadingService } from "../../shared/hooks";
import {
  appointmentAnalyticsApi,
  type AppointmentMetric,
  type AppointmentsBySpecializationData,
  type DoctorPerformance,
  type DoctorProductivityData,
  type RevenueChartData,
} from "../../shared/services/dashboardApi";

import {
  AppointmentMetricsCard,
  DoctorPerformanceCard,
  InteractiveChartsCard,
} from "./components";

export default function AppointmentAnalytics() {
  // Analytics data state
  const [metrics, setMetrics] = useState<AppointmentMetric[]>([]);
  const [revenueData, setRevenueData] = useState<RevenueChartData[]>([]);
  const [specializationData, setSpecializationData] = useState<
    AppointmentsBySpecializationData[]
  >([]);
  const [productivityData, setProductivityData] = useState<
    DoctorProductivityData[]
  >([]);
  const [doctorPerformance, setDoctorPerformance] = useState<
    DoctorPerformance[]
  >([]);

  const { isLoading, error, clearError, executeInitialLoad } =
    useLoadingService();

  // Fetch analytics data on component mount
  useEffect(() => {
    const fetchAnalyticsData = async () => {
      try {
        const [
          metricsResponse,
          revenueResponse,
          specializationResponse,
          productivityResponse,
          performanceResponse,
        ] = await Promise.all([
          appointmentAnalyticsApi.getAppointmentMetrics(),
          appointmentAnalyticsApi.getRevenueChartData(),
          appointmentAnalyticsApi.getAppointmentsBySpecialization(),
          appointmentAnalyticsApi.getDoctorProductivity(),
          appointmentAnalyticsApi.getDoctorPerformance(),
        ]);

        if (metricsResponse.success) {
          setMetrics(metricsResponse.data);
        } else {
          throw new Error(
            metricsResponse.error || "Failed to fetch appointment metrics"
          );
        }

        if (revenueResponse.success) {
          setRevenueData(revenueResponse.data);
        } else {
          throw new Error(
            revenueResponse.error || "Failed to fetch revenue data"
          );
        }

        if (specializationResponse.success) {
          setSpecializationData(specializationResponse.data);
        } else {
          throw new Error(
            specializationResponse.error ||
              "Failed to fetch specialization data"
          );
        }

        if (productivityResponse.success) {
          setProductivityData(productivityResponse.data);
        } else {
          throw new Error(
            productivityResponse.error || "Failed to fetch productivity data"
          );
        }

        if (performanceResponse.success) {
          setDoctorPerformance(performanceResponse.data);
        } else {
          throw new Error(
            performanceResponse.error || "Failed to fetch doctor performance"
          );
        }
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to load analytics data";
        throw new Error(errorMessage);
      }
    };

    executeInitialLoad(fetchAnalyticsData);
  }, [executeInitialLoad]);

  return (
    <div className="min-h-screen bg-gray-100">
      <Header />
      <LoadingOverlay
        isLoading={isLoading}
        message="Loading appointment analytics..."
      >
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 pt-24">
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-gray-900">
              Appointment Analytics Dashboard
            </h1>
            <p className="mt-2 text-gray-600">
              Core business operations and appointment performance insights
            </p>
          </div>

          {error ? (
            <div className="flex items-center justify-center h-64">
              <ErrorDisplay
                message={error}
                onRetry={clearError}
                className="max-w-md"
              />
            </div>
          ) : (
            <div className="space-y-6">
              {/* Key Metrics - Full Width */}
              <AppointmentMetricsCard metrics={metrics} isLoading={isLoading} />

              {/* Interactive Charts - Full Width */}
              <InteractiveChartsCard
                revenueData={revenueData}
                specializationData={specializationData}
                productivityData={productivityData}
                isLoading={isLoading}
              />

              {/* Doctor Performance - Full Width */}
              <DoctorPerformanceCard
                doctors={doctorPerformance}
                isLoading={isLoading}
              />
            </div>
          )}
        </div>
      </LoadingOverlay>
    </div>
  );
}
