import React, { useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { Calendar, Clock, Filter, Users } from "lucide-react";

import Header from "../../layout/Header";
import { useAuth } from "../../shared/auth/AuthContext";
import { Card, ErrorDisplay, LoadingOverlay } from "../../shared/components";

import { DoctorScheduleCalendar } from "./components/DoctorScheduleCalendar";
import { DoctorScheduleModal } from "./components/DoctorScheduleModal";
import { useDoctorSchedule } from "./hooks/useDoctorSchedule";
import type {
  DoctorScheduleEvent,
  DoctorSchedulerProps,
} from "./types/doctorScheduler";

export const DoctorSchedulerPage: React.FC<DoctorSchedulerProps> = ({
  doctorId: propDoctorId,
  isReadOnly: _isReadOnly = true,
}) => {
  const { user } = useAuth();
  // Use prop if provided, otherwise fallback to current user's ID
  const doctorId =
    propDoctorId && propDoctorId !== "current-doctor-id"
      ? propDoctorId
      : user?.id;

  const [selectedDate, setSelectedDate] = useState<string | undefined>();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedAppointment, setSelectedAppointment] =
    useState<DoctorScheduleEvent | null>(null);

  const [searchParams] = useSearchParams();
  const patientIdFilter = searchParams.get("patientId");

  const {
    schedule,
    calendarEvents: rawCalendarEvents,
    todaysAppointments: rawTodaysAppointments,
    isLoading,
    error,
    refreshSchedule,
    markAppointmentCompleted,
    markAppointmentNoShow,
    addAppointmentNotes,
  } = useDoctorSchedule({
    ...(doctorId ? { doctorId } : {}),
    autoRefresh: true,
    refreshInterval: 60000, // Refresh every minute
  });

  // Filter events if patientId param is present
  const calendarEvents = useMemo(() => {
    if (!patientIdFilter) return rawCalendarEvents;
    return rawCalendarEvents.filter(
      (e) => e.extendedProps.appointment.patientId === patientIdFilter
    );
  }, [rawCalendarEvents, patientIdFilter]);

  const todaysAppointments = useMemo(() => {
    if (!patientIdFilter) return rawTodaysAppointments;
    return rawTodaysAppointments.filter(
      (appt) => appt.patientId === patientIdFilter
    );
  }, [rawTodaysAppointments, patientIdFilter]);

  const handleEventClick = (event: {
    extendedProps: { appointment: DoctorScheduleEvent };
  }) => {
    setSelectedAppointment(event.extendedProps.appointment);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedAppointment(null);
  };

  const handleMarkCompleted = async (
    appointmentId: string
  ): Promise<boolean> => {
    const success = await markAppointmentCompleted(appointmentId);
    if (success) {
      handleCloseModal();
    }
    return success;
  };

  const handleMarkNoShow = async (appointmentId: string): Promise<boolean> => {
    const success = await markAppointmentNoShow(appointmentId);
    if (success) {
      handleCloseModal();
    }
    return success;
  };

  const handleAddNotes = async (
    appointmentId: string,
    notes: string
  ): Promise<boolean> => {
    return await addAppointmentNotes(appointmentId, notes);
  };

  // Calculate statistics
  const todayStats = {
    total: todaysAppointments.length,
    completed: todaysAppointments.filter((apt) => apt.status === "completed")
      .length,
    upcoming: todaysAppointments.filter((apt) => apt.status === "scheduled")
      .length,
    noShow: todaysAppointments.filter((apt) => apt.status === "no-show").length,
  };

  const weekStats = {
    total: schedule.length,
    completed: schedule.filter((apt) => apt.status === "completed").length,
    scheduled: schedule.filter((apt) => apt.status === "scheduled").length,
  };

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100 pt-16">
        <Header />
        <div className="max-w-7xl mx-auto px-4 py-8">
          <ErrorDisplay message={error} onRetry={refreshSchedule} />
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 pt-16">
      <Header />

      <LoadingOverlay
        isLoading={isLoading && schedule.length === 0}
        message="Loading your schedule..."
      >
        <div className="max-w-7xl mx-auto px-4 py-8">
          {/* Page Header */}
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-blue-700 flex items-center">
              <Calendar className="w-8 h-8 mr-3" />
              Doctor Schedule
            </h1>
            <p className="text-gray-600 mt-2">
              View and manage your patient appointments
              {patientIdFilter && (
                <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800">
                  <Filter className="w-3 h-3 mr-1" />
                  Filtered by patient
                </span>
              )}
            </p>
          </div>

          {/* Statistics Cards */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
            <Card variant="medical" className="text-center">
              <div className="p-4">
                <div className="text-2xl font-bold text-blue-600">
                  {todayStats.total}
                </div>
                <div className="text-sm text-gray-600">
                  Today's Appointments
                </div>
              </div>
            </Card>

            <Card variant="medical" className="text-center">
              <div className="p-4">
                <div className="text-2xl font-bold text-green-600">
                  {todayStats.completed}
                </div>
                <div className="text-sm text-gray-600">Completed Today</div>
              </div>
            </Card>

            <Card variant="medical" className="text-center">
              <div className="p-4">
                <div className="text-2xl font-bold text-orange-600">
                  {todayStats.upcoming}
                </div>
                <div className="text-sm text-gray-600">Upcoming Today</div>
              </div>
            </Card>

            <Card variant="medical" className="text-center">
              <div className="p-4">
                <div className="text-2xl font-bold text-purple-600">
                  {weekStats.total}
                </div>
                <div className="text-sm text-gray-600">This Week</div>
              </div>
            </Card>
          </div>

          {/* Today's Schedule Quick View */}
          {todaysAppointments.length > 0 && (
            <Card variant="medical" className="mb-8">
              <div className="p-6">
                <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center">
                  <Clock className="w-5 h-5 mr-2" />
                  Today's Schedule
                </h2>
                <div className="space-y-3">
                  {todaysAppointments
                    .sort((a, b) => a.time.localeCompare(b.time))
                    .map((appointment) => {
                      const appointmentTime = new Date(
                        `${appointment.date}T${appointment.time}`
                      );
                      const now = new Date();
                      const isPast = now > appointmentTime;
                      const isCurrent =
                        now >= appointmentTime &&
                        now <=
                          new Date(
                            appointmentTime.getTime() +
                              appointment.duration * 60 * 1000
                          );

                      return (
                        <div
                          key={appointment.id}
                          onClick={() => {
                            setSelectedAppointment(appointment);
                            setIsModalOpen(true);
                          }}
                          className={`flex items-center justify-between p-4 rounded-lg border cursor-pointer transition-colors hover:bg-gray-50 ${
                            isCurrent
                              ? "border-orange-200 bg-orange-50"
                              : isPast && appointment.status === "scheduled"
                                ? "border-red-200 bg-red-50"
                                : appointment.status === "completed"
                                  ? "border-green-200 bg-green-50"
                                  : "border-gray-200"
                          }`}
                        >
                          <div className="flex items-center space-x-4">
                            <div className="text-sm font-medium text-gray-900">
                              {appointmentTime.toLocaleTimeString([], {
                                hour: "2-digit",
                                minute: "2-digit",
                              })}
                            </div>
                            <div>
                              <div className="font-medium text-gray-900">
                                {appointment.patientName}
                              </div>
                              <div className="text-sm text-gray-600">
                                {appointment.appointmentType} •{" "}
                                {appointment.duration} min
                              </div>
                            </div>
                          </div>
                          <div className="flex items-center space-x-2">
                            {isCurrent && (
                              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-orange-100 text-orange-800">
                                Current
                              </span>
                            )}
                            <span
                              className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                                appointment.status === "completed"
                                  ? "bg-green-100 text-green-800"
                                  : appointment.status === "no-show"
                                    ? "bg-red-100 text-red-800"
                                    : appointment.status === "cancelled"
                                      ? "bg-gray-100 text-gray-800"
                                      : "bg-blue-100 text-blue-800"
                              }`}
                            >
                              {appointment.status.charAt(0).toUpperCase() +
                                appointment.status.slice(1)}
                            </span>
                          </div>
                        </div>
                      );
                    })}
                </div>
              </div>
            </Card>
          )}

          {/* Calendar View */}
          <Card variant="medical">
            <div className="p-6">
              <div className="flex items-center justify-between mb-6">
                <h2 className="text-xl font-semibold text-gray-900 flex items-center">
                  <Users className="w-5 h-5 mr-2" />
                  Schedule Calendar
                </h2>
                <div className="flex items-center space-x-4 text-sm">
                  <div className="flex items-center space-x-2">
                    <div className="w-3 h-3 bg-blue-500 rounded"></div>
                    <span>Scheduled</span>
                  </div>
                  <div className="flex items-center space-x-2">
                    <div className="w-3 h-3 bg-green-500 rounded"></div>
                    <span>Completed</span>
                  </div>
                  <div className="flex items-center space-x-2">
                    <div className="w-3 h-3 bg-orange-500 rounded"></div>
                    <span>Current</span>
                  </div>
                  <div className="flex items-center space-x-2">
                    <div className="w-3 h-3 bg-red-500 rounded"></div>
                    <span>No-Show</span>
                  </div>
                </div>
              </div>

              <DoctorScheduleCalendar
                events={calendarEvents}
                onEventClick={handleEventClick}
                selectedDate={selectedDate}
                onDateSelect={setSelectedDate}
              />
            </div>
          </Card>

          {/* Appointment Details Modal */}
          <DoctorScheduleModal
            isOpen={isModalOpen}
            onClose={handleCloseModal}
            appointment={selectedAppointment}
            onMarkCompleted={handleMarkCompleted}
            onMarkNoShow={handleMarkNoShow}
            onAddNotes={handleAddNotes}
          />
        </div>
      </LoadingOverlay>
    </div>
  );
};
