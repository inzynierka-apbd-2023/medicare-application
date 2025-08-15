import React, { useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { Calendar, Plus, RefreshCw, Users } from "lucide-react";

import Header from "../../../layout/Header";
import {
  Button,
  ErrorDisplay,
  LoadingOverlay,
} from "../../../shared/components";
import { DashboardLayout } from "../shared/components";

import { ReadOnlyScheduler } from "./components/ReadOnlyScheduler";
import { useReceptionistDashboard } from "./hooks/useReceptionistDashboard";
import {
  DoctorAvailabilityList,
  StatsCards,
  TodayAppointments,
} from "./components";
import type {
  DoctorAvailability,
  QuickAppointment,
  ReceptionistDashboardPageProps,
} from "./types";

export const ReceptionistDashboard: React.FC<
  ReceptionistDashboardPageProps
> = ({ className = "" }) => {
  const navigate = useNavigate();

  const { dashboardData, isLoading, error, refreshData } =
    useReceptionistDashboard({
      autoRefresh: true,
      refreshInterval: 30000, // 30 seconds
    });

  const handleAppointmentClick = useCallback(
    (appointment: QuickAppointment) => {
      // Navigate to appointment details or scheduler
      navigate(`/receptionist-scheduler?appointmentId=${appointment.id}`);
    },
    [navigate]
  );

  const handleDoctorClick = useCallback(
    (doctor: DoctorAvailability) => {
      // Navigate to doctor's schedule or details
      navigate(`/receptionist-scheduler?doctorId=${doctor.id}`);
    },
    [navigate]
  );

  const handleQuickActions = useCallback(
    (action: string) => {
      switch (action) {
        case "new-appointment":
          navigate("/receptionist-scheduler?openBooking=true");
          break;
        case "new-patient":
          navigate("/patient-registry?action=new");
          break;
        case "view-schedule":
          navigate("/receptionist-scheduler");
          break;
        default:
          break;
      }
    },
    [navigate]
  );

  const handleRefresh = useCallback(async () => {
    await refreshData();
  }, [refreshData]);

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <DashboardLayout title="Receptionist Dashboard">
          <ErrorDisplay message={error} onRetry={handleRefresh} />
        </DashboardLayout>
      </div>
    );
  }

  return (
    <div className={`min-h-screen bg-gray-100 ${className}`}>
      <Header />
      <DashboardLayout title="Receptionist Dashboard">
        {/* Loading overlay for the entire dashboard */}
        {isLoading && !dashboardData && (
          <LoadingOverlay isLoading={true}>
            <div />
          </LoadingOverlay>
        )}

        {/* Header with quick actions */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8">
          <div>
            <h2 className="text-lg text-gray-600">
              Welcome back! Here's what's happening today.
            </h2>
          </div>
          <div className="flex flex-wrap gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={handleRefresh}
              disabled={isLoading}
            >
              <RefreshCw
                className={`h-4 w-4 mr-2 ${isLoading ? "animate-spin" : ""}`}
              />
              Refresh
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => handleQuickActions("new-patient")}
            >
              <Users className="h-4 w-4 mr-2" />
              New Patient
            </Button>
            <Button
              variant="primary"
              size="sm"
              onClick={() => handleQuickActions("new-appointment")}
            >
              <Plus className="h-4 w-4 mr-2" />
              New Appointment
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => handleQuickActions("view-schedule")}
            >
              <Calendar className="h-4 w-4 mr-2" />
              View Schedule
            </Button>
          </div>
        </div>

        {dashboardData && (
          <>
            {/* Stats Cards */}
            <StatsCards
              stats={dashboardData.stats}
              isLoading={isLoading && !dashboardData}
            />

            {/* Top Row - Appointments and Doctor Availability */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
              {/* Today's Appointments */}
              <TodayAppointments
                appointments={dashboardData.todayAppointments}
                isLoading={isLoading && !dashboardData}
                onAppointmentClick={handleAppointmentClick}
              />

              {/* Doctor Availability */}
              <DoctorAvailabilityList
                doctors={dashboardData.doctorAvailability}
                isLoading={isLoading && !dashboardData}
                onDoctorClick={handleDoctorClick}
              />
            </div>

            {/* Schedule Overview - Full Width */}
            <div className="mb-8">
              <ReadOnlyScheduler />
            </div>
          </>
        )}

        {/* Empty state when no data */}
        {!isLoading && !dashboardData && (
          <div className="text-center py-12">
            <div className="mx-auto h-24 w-24 text-gray-300 mb-4">
              <Calendar className="h-full w-full" />
            </div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              Dashboard Loading
            </h3>
            <p className="text-gray-500 mb-6">
              Please wait while we load your dashboard data.
            </p>
            <Button onClick={handleRefresh}>Try Again</Button>
          </div>
        )}
      </DashboardLayout>
    </div>
  );
};

export default ReceptionistDashboard;
