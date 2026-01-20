import Header from "@layout/Header";
import { ErrorDisplay, LoadingOverlay } from "@shared/components";

import {
  AppointmentMetricsCard,
  AppointmentTrendsCard,
  DoctorPerformanceCard,
  SpecializationStatsCard,
} from "./components";
import { useAnalytics } from "./hooks";

export default function AppointmentAnalytics() {
  const {
    metrics,
    trends,
    doctorPerformance,
    specializationStats,
    isLoading,
    error,
    clearError,
    refetchData,
  } = useAnalytics();

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
              Comprehensive analytics and insights for your clinic operations
            </p>
          </div>

          {error ? (
            <div className="flex items-center justify-center h-64">
              <ErrorDisplay
                message={error}
                onRetry={() => {
                  clearError();
                  refetchData();
                }}
                className="max-w-md"
              />
            </div>
          ) : (
            <div className="space-y-8">
              {/* Key Metrics */}
              <AppointmentMetricsCard metrics={metrics} />

              {/* Appointment Trends */}
              <AppointmentTrendsCard
                data={trends}
                title="Appointment Trends Over Time"
              />

              {/* Doctor Performance */}
              <DoctorPerformanceCard data={doctorPerformance} />

              {/* Specialization Statistics */}
              <SpecializationStatsCard data={specializationStats} />
            </div>
          )}
        </div>
      </LoadingOverlay>
    </div>
  );
}
