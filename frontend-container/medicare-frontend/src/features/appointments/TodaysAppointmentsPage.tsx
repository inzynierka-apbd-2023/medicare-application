import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Calendar, Clock, FileText, User } from "lucide-react";

import Header from "../../layout/Header";
import {
  Card,
  EmptyState,
  ErrorDisplay,
  Loading,
} from "../../shared/components";
import { DashboardLayout } from "../dashboard/shared/components";

import {
  TodayAppointmentCard,
  TodayAppointmentDetailsModal,
} from "./components";
import { useTodaysAppointments } from "./hooks";
import type { TodayAppointment } from "./types";

const TodaysAppointmentsPage: React.FC = () => {
  const navigate = useNavigate();
  const [selectedAppointment, setSelectedAppointment] =
    useState<TodayAppointment | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const {
    appointments,
    loading,
    error,
    refetch,
    markAsCompleted,
    markAsNoShow,
  } = useTodaysAppointments();

  const handleAppointmentDetails = (appointment: TodayAppointment) => {
    setSelectedAppointment(appointment);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedAppointment(null);
  };

  const handleMarkCompleted = async (appointmentId: string) => {
    const success = await markAsCompleted(appointmentId);
    if (success) {
      // Could add toast notification here
      console.log("Appointment marked as completed");
    }
  };

  const handleMarkNoShow = async (appointmentId: string) => {
    const success = await markAsNoShow(appointmentId);
    if (success) {
      // Could add toast notification here
      console.log("Appointment marked as no-show");
    }
  };

  const handleContactPatient = (patientId: string) => {
    navigate(`/messages?patientId=${patientId}`);
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <DashboardLayout title="Today's Appointments">
          <Loading text="Loading today's appointments..." />
        </DashboardLayout>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <DashboardLayout title="Today's Appointments">
          <ErrorDisplay
            message={error}
            onRetry={refetch}
            retryText="Try Again"
          />
        </DashboardLayout>
      </div>
    );
  }

  const upcomingAppointments = appointments.filter(
    (apt: TodayAppointment) => apt.status === "scheduled"
  );
  const completedAppointments = appointments.filter(
    (apt: TodayAppointment) => apt.status === "completed"
  );
  const noShowAppointments = appointments.filter(
    (apt: TodayAppointment) => apt.status === "no-show"
  );

  const currentTime = new Date();
  const currentHour = currentTime.getHours();
  const currentMinutes = currentTime.getMinutes();

  const getTimeStatus = (time: string) => {
    const [hours, minutes] = time.split(":").map(Number);
    const appointmentTime = hours * 60 + minutes;
    const currentTimeMinutes = currentHour * 60 + currentMinutes;

    if (appointmentTime < currentTimeMinutes - 15) return "overdue";
    if (appointmentTime <= currentTimeMinutes + 15) return "current";
    return "upcoming";
  };

  return (
    <div className="min-h-screen bg-gray-100">
      <Header />
      <DashboardLayout title="Today's Appointments">
        <div className="space-y-6">
          {/* Summary Stats */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <Card variant="medical" padding="md">
              <div className="text-center">
                <div className="text-2xl font-bold text-blue-600">
                  {appointments.length}
                </div>
                <div className="text-sm text-gray-600">Total Today</div>
              </div>
            </Card>
            <Card variant="medical" padding="md">
              <div className="text-center">
                <div className="text-2xl font-bold text-green-600">
                  {completedAppointments.length}
                </div>
                <div className="text-sm text-gray-600">Completed</div>
              </div>
            </Card>
            <Card variant="medical" padding="md">
              <div className="text-center">
                <div className="text-2xl font-bold text-yellow-600">
                  {upcomingAppointments.length}
                </div>
                <div className="text-sm text-gray-600">Remaining</div>
              </div>
            </Card>
            <Card variant="medical" padding="md">
              <div className="text-center">
                <div className="text-2xl font-bold text-red-600">
                  {noShowAppointments.length}
                </div>
                <div className="text-sm text-gray-600">No Shows</div>
              </div>
            </Card>
          </div>

          {/* Current Time Display */}
          <Card variant="medical" padding="md">
            <div className="flex items-center justify-center space-x-2">
              <Clock className="h-5 w-5 text-blue-600" />
              <span className="text-lg font-medium text-gray-700">
                Current Time:{" "}
                {currentTime.toLocaleTimeString([], {
                  hour: "2-digit",
                  minute: "2-digit",
                })}
              </span>
            </div>
          </Card>

          {appointments.length === 0 ? (
            <EmptyState
              icon={<Calendar className="h-12 w-12 text-gray-400" />}
              title="No appointments today"
              description="You have no appointments scheduled for today. Enjoy your free time!"
            />
          ) : (
            <div className="space-y-6">
              {/* Upcoming Appointments */}
              {upcomingAppointments.length > 0 && (
                <div>
                  <div className="flex items-center space-x-2 mb-4">
                    <Calendar className="h-5 w-5 text-blue-600" />
                    <h2 className="text-xl font-semibold text-gray-800">
                      Upcoming Appointments ({upcomingAppointments.length})
                    </h2>
                  </div>
                  <div className="grid gap-4">
                    {upcomingAppointments
                      .sort((a: TodayAppointment, b: TodayAppointment) =>
                        a.time.localeCompare(b.time)
                      )
                      .map((appointment: TodayAppointment) => (
                        <TodayAppointmentCard
                          key={appointment.id}
                          appointment={appointment}
                          timeStatus={getTimeStatus(appointment.time)}
                          onDetails={handleAppointmentDetails}
                          onMarkCompleted={handleMarkCompleted}
                          onMarkNoShow={handleMarkNoShow}
                          onContactPatient={handleContactPatient}
                        />
                      ))}
                  </div>
                </div>
              )}

              {/* Completed Appointments */}
              {completedAppointments.length > 0 && (
                <div>
                  <div className="flex items-center space-x-2 mb-4">
                    <FileText className="h-5 w-5 text-green-600" />
                    <h2 className="text-xl font-semibold text-gray-800">
                      Completed Appointments ({completedAppointments.length})
                    </h2>
                  </div>
                  <div className="grid gap-4">
                    {completedAppointments
                      .sort((a: TodayAppointment, b: TodayAppointment) =>
                        a.time.localeCompare(b.time)
                      )
                      .map((appointment: TodayAppointment) => (
                        <TodayAppointmentCard
                          key={appointment.id}
                          appointment={appointment}
                          timeStatus="completed"
                          onDetails={handleAppointmentDetails}
                          onContactPatient={handleContactPatient}
                          showCompletionActions={false}
                        />
                      ))}
                  </div>
                </div>
              )}

              {/* No Show Appointments */}
              {noShowAppointments.length > 0 && (
                <div>
                  <div className="flex items-center space-x-2 mb-4">
                    <User className="h-5 w-5 text-red-600" />
                    <h2 className="text-xl font-semibold text-gray-800">
                      No Show Appointments ({noShowAppointments.length})
                    </h2>
                  </div>
                  <div className="grid gap-4">
                    {noShowAppointments
                      .sort((a: TodayAppointment, b: TodayAppointment) =>
                        a.time.localeCompare(b.time)
                      )
                      .map((appointment: TodayAppointment) => (
                        <TodayAppointmentCard
                          key={appointment.id}
                          appointment={appointment}
                          timeStatus="no-show"
                          onDetails={handleAppointmentDetails}
                          onContactPatient={handleContactPatient}
                          showCompletionActions={false}
                        />
                      ))}
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      </DashboardLayout>

      {/* Appointment Details Modal */}
      {selectedAppointment && (
        <TodayAppointmentDetailsModal
          isOpen={isModalOpen}
          onClose={handleCloseModal}
          appointment={selectedAppointment}
        />
      )}
    </div>
  );
};

export default TodaysAppointmentsPage;
