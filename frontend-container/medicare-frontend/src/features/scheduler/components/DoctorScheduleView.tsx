/**
 * DoctorScheduleView - Refactored modular doctor schedule interface
 *
 * Features:
 * - Modular calendar component
 * - Appointment details sidebar
 * - Statistics overview
 * - Patient management integration
 * - Readonly video call information
 */

import React, { useCallback, useEffect, useState } from "react";
import { SchedulerApiService } from "@features/scheduler/services/schedulerApiService";
import type { Appointment } from "@features/scheduler/types";
import Header from "@layout/Header";
import { Button, Card, LoadingOverlay } from "@shared/components";
import { Calendar } from "lucide-react";

import AppointmentDetails from "./AppointmentDetails";
import AppointmentStats from "./AppointmentStats";
import DoctorCalendar from "./DoctorCalendar";

interface DoctorScheduleViewProps {
  doctorId?: string;
}

export const DoctorScheduleView: React.FC<DoctorScheduleViewProps> = ({
  doctorId = "current-doctor-id", // Replace with actual doctor ID from auth context
}) => {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [selectedAppointment, setSelectedAppointment] =
    useState<Appointment | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [currentView, setCurrentView] = useState<
    "dayGridMonth" | "timeGridWeek" | "timeGridDay"
  >("timeGridDay");

  // Load doctor's appointments
  const loadDoctorAppointments = useCallback(async () => {
    setIsLoading(true);
    try {
      const doctorAppointments =
        await SchedulerApiService.getDoctorAppointments(doctorId);
      setAppointments(doctorAppointments);
    } catch (error) {
      console.error("Failed to load doctor appointments:", error);
    } finally {
      setIsLoading(false);
    }
  }, [doctorId]);

  useEffect(() => {
    loadDoctorAppointments();
  }, [loadDoctorAppointments]);

  // Handle view change
  const handleViewChange = useCallback(
    (view: "dayGridMonth" | "timeGridWeek" | "timeGridDay") => {
      setCurrentView(view);
    },
    []
  );

  // Handle appointment selection
  const handleAppointmentSelect = useCallback((appointment: Appointment) => {
    setSelectedAppointment(appointment);
  }, []);

  // Handle appointment status updates
  const handleMarkAsCompleted = useCallback(
    async (appointmentId: string) => {
      try {
        setIsLoading(true);
        const completedStatusId = "status-3"; // "completed" status
        await SchedulerApiService.updateAppointmentStatus(
          appointmentId,
          completedStatusId
        );
        await loadDoctorAppointments(); // Refresh appointments
        setSelectedAppointment(null); // Clear selection
      } catch (error) {
        console.error("Failed to mark appointment as completed:", error);
      } finally {
        setIsLoading(false);
      }
    },
    [loadDoctorAppointments]
  );

  const handleMarkAsNoShow = useCallback(
    async (appointmentId: string) => {
      try {
        setIsLoading(true);
        const noShowStatusId = "status-5"; // "no-show" status
        await SchedulerApiService.updateAppointmentStatus(
          appointmentId,
          noShowStatusId
        );
        await loadDoctorAppointments(); // Refresh appointments
        setSelectedAppointment(null); // Clear selection
      } catch (error) {
        console.error("Failed to mark appointment as no show:", error);
      } finally {
        setIsLoading(false);
      }
    },
    [loadDoctorAppointments]
  );

  // Handle virtual consultation (readonly)
  const handleStartVideoCall = useCallback((appointmentId: string) => {
    const videoCallInfo =
      SchedulerApiService.getVirtualConsultationInfo(appointmentId);
    alert(videoCallInfo); // In real app, show a proper modal or notification
  }, []);

  // Get today's statistics
  const todayStats = SchedulerApiService.getAppointmentStats(
    appointments,
    new Date().toISOString()
  );

  return (
    <div className="min-h-screen bg-gray-100 pt-16">
      <Header />

      <LoadingOverlay isLoading={isLoading}>
        <div className="max-w-7xl mx-auto px-4 py-8">
          {/* Header */}
          <div className="flex flex-col md:flex-row md:justify-between md:items-center mb-6">
            <div>
              <h1 className="text-2xl font-bold text-gray-900 flex items-center">
                <Calendar className="mr-2" />
                My Schedule - Doctor Timeline
              </h1>
              <p className="text-gray-600 mt-1">
                View and manage your patient appointments
              </p>
            </div>

            {/* View Controls */}
            <div className="flex gap-2 mt-4 md:mt-0">
              <Button
                variant={currentView === "timeGridDay" ? "primary" : "outline"}
                size="sm"
                onClick={() => handleViewChange("timeGridDay")}
              >
                Day
              </Button>
              <Button
                variant={currentView === "timeGridWeek" ? "primary" : "outline"}
                size="sm"
                onClick={() => handleViewChange("timeGridWeek")}
              >
                Week
              </Button>
              <Button
                variant={currentView === "dayGridMonth" ? "primary" : "outline"}
                size="sm"
                onClick={() => handleViewChange("dayGridMonth")}
              >
                Month
              </Button>
            </div>
          </div>

          {/* Today's Stats */}
          <AppointmentStats stats={todayStats} title="Today's Overview" />

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Calendar */}
            <div className="lg:col-span-2">
              <Card>
                <div className="p-4">
                  <DoctorCalendar
                    appointments={appointments}
                    currentView={currentView}
                    onEventClick={handleAppointmentSelect}
                    isLoading={isLoading}
                  />
                </div>
              </Card>
            </div>

            {/* Appointment Details Sidebar */}
            <div className="lg:col-span-1">
              <Card>
                <div className="p-4">
                  <h3 className="text-lg font-semibold mb-4">
                    {selectedAppointment
                      ? "Appointment Details"
                      : "Select an Appointment"}
                  </h3>

                  <AppointmentDetails
                    appointment={selectedAppointment}
                    onMarkAsCompleted={handleMarkAsCompleted}
                    onMarkAsNoShow={handleMarkAsNoShow}
                    onStartVideoCall={handleStartVideoCall}
                    isLoading={isLoading}
                  />
                </div>
              </Card>
            </div>
          </div>
        </div>
      </LoadingOverlay>
    </div>
  );
};

export default DoctorScheduleView;
