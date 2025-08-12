import React, { useState } from "react";
import { BarChart3, Clock, TrendingUp, Users } from "lucide-react";

import Header from "../../layout/Header";
import { Card } from "../../shared/components";

// Import our new analytics components
import AppointmentMetricsCard from "./components/AppointmentMetricsCard";
import AppointmentTrendsCard from "./components/AppointmentTrendsCard";
import DoctorPerformanceCard from "./components/DoctorPerformanceCard";
import SpecializationStatsCard from "./components/SpecializationStatsCard";
import TimeSlotAnalysisCard from "./components/TimeSlotAnalysisCard";
import { useAnalytics } from "./hooks";

const AppointmentAnalyticsPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState<
    "overview" | "doctors" | "specializations" | "timeslots"
  >("overview");

  const {
    metrics,
    trends,
    doctorPerformance,
    specializationStats,
    timeSlotData,
    weeklyData,
    isLoading,
    error,
  } = useAnalytics();

  const tabs = [
    { id: "overview", label: "Overview", icon: BarChart3 },
    { id: "doctors", label: "Doctor Performance", icon: Users },
    { id: "specializations", label: "Specializations", icon: TrendingUp },
    { id: "timeslots", label: "Time Analysis", icon: Clock },
  ];

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 pt-24">
          <div className="text-center">
            <h2 className="text-xl font-semibold text-red-600 mb-2">
              Error Loading Analytics
            </h2>
            <p className="text-gray-600">{error}</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      <Header />

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 pt-24">
        {/* Page Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">
            Appointment Analytics Dashboard
          </h1>
          <p className="mt-2 text-gray-600">
            Comprehensive analytics and insights for your clinic operations
          </p>
        </div>

        {/* Tab Navigation */}
        <Card variant="elevated" className="mb-8">
          <div className="">
            <nav className="flex space-x-8 px-6">
              {tabs.map((tab) => {
                const Icon = tab.icon;
                return (
                  <button
                    key={tab.id}
                    onClick={() => setActiveTab(tab.id as typeof activeTab)}
                    className={`group inline-flex items-center py-4 px-1 font-medium text-sm rounded-t-lg transition-colors ${
                      activeTab === tab.id
                        ? "bg-blue-50 text-blue-600 border-b-2 border-blue-500"
                        : "text-gray-500 hover:text-gray-700 hover:bg-gray-50"
                    }`}
                  >
                    <Icon
                      className={`mr-2 h-5 w-5 ${
                        activeTab === tab.id
                          ? "text-blue-500"
                          : "text-gray-400 group-hover:text-gray-500"
                      }`}
                    />
                    {tab.label}
                  </button>
                );
              })}
            </nav>
          </div>
        </Card>

        {/* Tab Content */}
        {isLoading ? (
          <div className="flex items-center justify-center py-12">
            <div className="text-center">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
              <p className="text-gray-600">Loading analytics data...</p>
            </div>
          </div>
        ) : (
          <div className="space-y-6">
            {activeTab === "overview" && (
              <div className="space-y-6">
                <AppointmentMetricsCard metrics={metrics} />
                <AppointmentTrendsCard data={trends} />
              </div>
            )}

            {activeTab === "doctors" && (
              <DoctorPerformanceCard data={doctorPerformance} />
            )}

            {activeTab === "specializations" && (
              <SpecializationStatsCard data={specializationStats} />
            )}

            {activeTab === "timeslots" && (
              <TimeSlotAnalysisCard
                data={timeSlotData}
                weeklyData={weeklyData}
              />
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default AppointmentAnalyticsPage;
