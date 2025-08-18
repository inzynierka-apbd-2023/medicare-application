import React, { useCallback, useMemo, useState } from "react";
import { Calendar, Edit3, Plus, Trash2 } from "lucide-react";

import Header from "../../layout/Header";
import {
  Button,
  Card,
  ErrorDisplay,
  LoadingOverlay,
} from "../../shared/components";
import { useAuth } from "../../shared/auth/AuthContext";

import AppointmentModal from "./components/AppointmentModal";
import CalendarView from "./components/CalendarView";
import SchedulerFiltersComponent from "./components/SchedulerFilters";
import useScheduler from "./hooks/useScheduler";
import type {
  CalendarEvent,
  CreateAppointmentRequest,
  SchedulerPageProps,
  UpdateAppointmentRequest,
} from "./types";

export const SchedulerPage: React.FC<SchedulerPageProps> = ({ patientId }) => {
  const { user } = useAuth();
  const [modalState, setModalState] = useState({
    isOpen: false,
    mode: "create" as "create" | "edit" | "view",
  });

  // Resolve patient id: prefer prop, else currently logged-in user id
  const currentPatientId = useMemo(() => patientId || user?.id || "", [patientId, user]);

  const {
    appointments,
    doctors,
    services,
    specializations,
    selectedAppointment,
    isLoading,
    error,
    filters,
    calendarEvents,
    updateFilters,
    createAppointment,
    updateAppointment,
    cancelAppointment,
    selectAppointment,
    setSelectedDate,
    refreshAppointments,
  } = useScheduler({
    patientId: currentPatientId,
    initialFilters: {
      appointmentType: "all",
    },
  });

  const handleEventClick = useCallback(
    (event: CalendarEvent) => {
      selectAppointment(event.extendedProps.appointment);
      setModalState({
        isOpen: true,
        mode: "view",
      });
    },
    [selectAppointment]
  );

  const handleDateSelect = useCallback(
    (date: string) => {
      setSelectedDate(date);
      // Optionally open modal for creating appointment on selected date
    },
    [setSelectedDate]
  );

  const handleDateRangeChange = useCallback(
    (start: string, end: string) => {
      updateFilters({
        dateRange: { start, end },
      });
    },
    [updateFilters]
  );

  const handleCreateAppointment = useCallback(() => {
    selectAppointment(null);
    setModalState({
      isOpen: true,
      mode: "create",
    });
  }, [selectAppointment]);

  const handleModalSave = useCallback(
    async (data: CreateAppointmentRequest | UpdateAppointmentRequest) => {
      try {
        if (modalState.mode === "create") {
          await createAppointment(data as CreateAppointmentRequest);
        } else if (modalState.mode === "edit" && selectedAppointment) {
          await updateAppointment(
            selectedAppointment.id,
            data as UpdateAppointmentRequest
          );
        }
        setModalState({ isOpen: false, mode: "create" });
      } catch (error) {
        console.error("Failed to save appointment:", error);
        throw error;
      }
    },
    [modalState.mode, selectedAppointment, createAppointment, updateAppointment]
  );

  const handleModalClose = useCallback(() => {
    setModalState({ isOpen: false, mode: "create" });
    selectAppointment(null);
  }, [selectAppointment]);

  if (!currentPatientId) {
    return (
      <div className="min-h-screen bg-gray-100 pt-16">
        <Header />
        <div className="max-w-7xl mx-auto px-4 py-8">
          <LoadingOverlay isLoading message="Loading your profile..." />
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100 pt-16">
        <Header />
        <div className="max-w-7xl mx-auto px-4 py-8">
          <ErrorDisplay message={error} onRetry={refreshAppointments} />
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 pt-16">
      <Header />

      <LoadingOverlay
        isLoading={isLoading && appointments.length === 0}
        message="Loading your appointments..."
      >
        <div className="max-w-7xl mx-auto px-4 py-8">
          {/* Page Header */}
          <div className="mb-8">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-3xl font-bold text-blue-700 flex items-center">
                  <Calendar className="w-8 h-8 mr-3" />
                  Appointment Scheduler
                </h1>
                <p className="text-gray-600 mt-2">
                  Book and manage your medical appointments
                </p>
              </div>
              <Button
                onClick={handleCreateAppointment}
                className="flex items-center"
              >
                <Plus className="w-5 h-5 mr-2" />
                Book Appointment
              </Button>
            </div>
          </div>

          {/* Quick Stats */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
            <Card variant="medical" className="text-center">
              <div className="p-4">
                <div className="text-2xl font-bold text-blue-600">
                  {appointments.length}
                </div>
                <div className="text-sm text-gray-600">Total Appointments</div>
              </div>
            </Card>

            <Card variant="medical" className="text-center">
              <div className="p-4">
                <div className="text-2xl font-bold text-green-600">
                  {
                    appointments.filter((apt) => {
                      const status = apt.status?.name?.toLowerCase();
                      return status === "scheduled" || status === "confirmed";
                    }).length
                  }
                </div>
                <div className="text-sm text-gray-600">Upcoming</div>
              </div>
            </Card>

            <Card variant="medical" className="text-center">
              <div className="p-4">
                <div className="text-2xl font-bold text-orange-600">
                  {
                    appointments.filter(
                      (apt) =>
                        new Date(apt.day).toDateString() ===
                        new Date().toDateString()
                    ).length
                  }
                </div>
                <div className="text-sm text-gray-600">Today</div>
              </div>
            </Card>

            <Card variant="medical" className="text-center">
              <div className="p-4">
                <div className="text-2xl font-bold text-purple-600">
                  {doctors.length}
                </div>
                <div className="text-sm text-gray-600">Available Doctors</div>
              </div>
            </Card>
          </div>

          {/* Filters */}
          <div className="mb-6">
            <SchedulerFiltersComponent
              filters={filters}
              onFiltersChange={updateFilters}
              specializations={specializations}
              services={services}
              doctors={doctors}
              isLoading={isLoading}
            />
          </div>

          {/* Calendar */}
          <Card variant="medical">
            <div className="p-6">
              <CalendarView
                events={calendarEvents}
                onEventClick={handleEventClick}
                onDateSelect={handleDateSelect}
                onDateRangeChange={handleDateRangeChange}
                isLoading={isLoading}
              />
            </div>
          </Card>

          {/* Recent Appointments */}
          <div className="mt-8">
            <Card variant="medical">
              <div className="p-6">
                <h3 className="text-lg font-semibold text-blue-600 mb-4">
                  Recent Appointments
                </h3>

                {appointments.length === 0 ? (
                  <div className="text-center py-8">
                    <Calendar className="w-12 h-12 text-gray-400 mx-auto mb-4" />
                    <h4 className="text-lg font-medium text-gray-600 mb-2">
                      No appointments scheduled
                    </h4>
                    <p className="text-gray-500 mb-4">
                      Book your first appointment to get started
                    </p>
                    <Button onClick={handleCreateAppointment}>
                      <Plus className="w-4 h-4 mr-2" />
                      Book Appointment
                    </Button>
                  </div>
                ) : (
                  <div className="space-y-4">
                    {appointments.slice(0, 5).map((appointment) => {
                      const doctor = doctors.find(
                        (d) => d.id === appointment.doctorUserId
                      );

                      return (
                        <div
                          key={appointment.id}
                          className="flex items-center justify-between p-4 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
                        >
                          <div
                            className="flex items-center space-x-4 flex-1 cursor-pointer"
                            onClick={() =>
                              handleEventClick({
                                id: appointment.id,
                                title: "",
                                start: appointment.day,
                                end: appointment.day,
                                extendedProps: {
                                  appointment,
                                  doctorName:
                                    `${doctor?.firstName || ""} ${doctor?.lastName || ""}`.trim(),
                                  patientName: "",
                                  appointmentType: appointment.appointmentType,
                                  status: appointment.status?.name || "Unknown",
                                  description: appointment.description || "",
                                },
                              })
                            }
                          >
                            <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center">
                              <Calendar className="w-6 h-6 text-blue-600" />
                            </div>
                            <div>
                              <div className="font-medium text-gray-900">
                                Dr. {doctor?.firstName} {doctor?.lastName}
                              </div>
                              <div className="text-sm text-gray-600">
                                {new Date(appointment.day).toLocaleDateString()}{" "}
                                at{" "}
                                {new Date(appointment.day).toLocaleTimeString(
                                  [],
                                  {
                                    hour: "2-digit",
                                    minute: "2-digit",
                                  }
                                )}
                              </div>
                              <div className="text-xs text-gray-500">
                                {appointment.appointmentType} •{" "}
                                {appointment.durationMinutes} min
                              </div>
                            </div>
                          </div>

                          {/* Action buttons */}
                          <div className="flex items-center space-x-2">
                            <button
                              onClick={(e) => {
                                e.stopPropagation();
                                selectAppointment(appointment);
                                setModalState({
                                  isOpen: true,
                                  mode: "edit",
                                });
                              }}
                              className="p-2 text-blue-600 hover:text-blue-700 hover:bg-blue-50 rounded-lg transition-colors"
                              title="Edit appointment"
                            >
                              <Edit3 className="w-4 h-4" />
                            </button>
                            <button
                              onClick={async (e) => {
                                e.stopPropagation();
                                if (
                                  window.confirm(
                                    "Are you sure you want to cancel this appointment?"
                                  )
                                ) {
                                  try {
                                    await cancelAppointment(appointment.id);
                                  } catch (error) {
                                    console.error(
                                      "Failed to cancel appointment:",
                                      error
                                    );
                                  }
                                }
                              }}
                              className="p-2 text-red-600 hover:text-red-700 hover:bg-red-50 rounded-lg transition-colors"
                              title="Cancel appointment"
                            >
                              <Trash2 className="w-4 h-4" />
                            </button>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            </Card>
          </div>
        </div>
      </LoadingOverlay>

      {/* Appointment Modal */}
      <AppointmentModal
        isOpen={modalState.isOpen}
        onClose={handleModalClose}
        appointment={selectedAppointment}
        onSave={handleModalSave}
        mode={modalState.mode}
      />
    </div>
  );
};

export default SchedulerPage;
