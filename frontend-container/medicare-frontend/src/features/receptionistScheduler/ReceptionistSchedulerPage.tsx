import React, { useCallback, useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import Header from "@layout/Header";
import { Button, Card, ErrorDisplay, LoadingOverlay } from "@shared/components";
import { Calendar, Clock, Plus, Users } from "lucide-react";

import { AppointmentModal } from "./components/AppointmentModal";
import { ReceptionistCalendarView } from "./components/ReceptionistCalendarView";
import { SchedulerFilters } from "./components/SchedulerFilters";
import { useReceptionistScheduler } from "./hooks/useReceptionistScheduler";
import type {
  CreateAppointmentRequest,
  ReceptionistAppointment,
  ReceptionistSchedulerPageProps,
  UpdateAppointmentRequest,
} from "./types";

export const ReceptionistSchedulerPage: React.FC<
  ReceptionistSchedulerPageProps
> = ({ className = "", autoOpenBooking = false, isEmbedded = false }) => {
  const [searchParams, setSearchParams] = useSearchParams();
  const shouldOpenBooking =
    searchParams.get("openBooking") === "true" || autoOpenBooking;
  const initialPatientId = searchParams.get("patientId") || undefined;

  const [modalState, setModalState] = useState<{
    isOpen: boolean;
    mode: "create" | "edit" | "view";
    appointment: ReceptionistAppointment | null;
    patientId?: string;
  }>({
    isOpen: shouldOpenBooking,
    mode: "create",
    appointment: null,
    patientId: initialPatientId,
  });

  // Clear the URL parameter after opening the modal
  useEffect(() => {
    if (searchParams.get("openBooking") === "true") {
      setSearchParams((params) => {
        params.delete("openBooking");
        params.delete("patientId");
        return params;
      });
    }
  }, [searchParams, setSearchParams]);

  const {
    appointments,
    calendarEvents,
    filters,
    isLoading,
    error,
    updateFilters,
    clearFilters,
    createAppointment,
    updateAppointment,
    cancelAppointment,
    refreshAppointments,
  } = useReceptionistScheduler();

  const [selectedAppointment, setSelectedAppointment] =
    useState<ReceptionistAppointment | null>(null);
  const [selectedDate, setSelectedDate] = useState<string>("");

  const handleEventClick = useCallback(
    (appointment: ReceptionistAppointment) => {
      setSelectedAppointment(appointment);
      setModalState({
        isOpen: true,
        mode: "view",
        appointment,
      });
    },
    []
  );

  const handleDateSelect = useCallback((date: string) => {
    setSelectedDate(date);
    setSelectedAppointment(null);
    setModalState({
      isOpen: true,
      mode: "create",
      appointment: null,
    });
  }, []);

  const handleCreateAppointment = useCallback(() => {
    setSelectedAppointment(null);
    setSelectedDate("");
    setModalState({
      isOpen: true,
      mode: "create",
      appointment: null,
    });
  }, []);

  const handleEditAppointment = useCallback(() => {
    if (selectedAppointment) {
      setModalState({
        isOpen: true,
        mode: "edit",
        appointment: selectedAppointment,
      });
    }
  }, [selectedAppointment]);

  const handleCloseModal = useCallback(() => {
    setModalState({
      isOpen: false,
      mode: "create",
      appointment: null,
      patientId: undefined,
    });
    setSelectedAppointment(null);
    setSelectedDate("");
  }, []);

  const handleCreateSubmit = useCallback(
    async (data: CreateAppointmentRequest) => {
      try {
        await createAppointment(data);
        handleCloseModal();
        await refreshAppointments();
      } catch (error) {
        console.error("Failed to create appointment:", error);
      }
    },
    [createAppointment, handleCloseModal, refreshAppointments]
  );

  const handleUpdateSubmit = useCallback(
    async (data: UpdateAppointmentRequest) => {
      if (!selectedAppointment) return;

      try {
        const updateData = {
          ...data,
          id: selectedAppointment.id,
        };
        await updateAppointment(updateData);
        handleCloseModal();
        await refreshAppointments();
      } catch (error) {
        console.error("Failed to update appointment:", error);
      }
    },
    [
      selectedAppointment,
      updateAppointment,
      handleCloseModal,
      refreshAppointments,
    ]
  );

  const handleCancelAppointment = useCallback(
    async (appointmentId: string) => {
      try {
        await cancelAppointment(appointmentId);
        handleCloseModal();
        await refreshAppointments();
      } catch (error) {
        console.error("Failed to cancel appointment:", error);
      }
    },
    [cancelAppointment, handleCloseModal, refreshAppointments]
  );

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-100 pt-16">
        <Header />
        <LoadingOverlay
          isLoading={true}
          message="Loading appointment scheduler..."
        >
          <div className="min-h-screen" />
        </LoadingOverlay>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100 pt-16">
        <Header />
        <div className="max-w-7xl mx-auto px-4 py-8">
          <h1 className="text-3xl font-bold text-blue-700 mb-6">
            Appointment Scheduler
          </h1>
          <ErrorDisplay message={error} onRetry={refreshAppointments} />
        </div>
      </div>
    );
  }

  return (
    <div
      className={`${isEmbedded ? "" : "min-h-screen bg-gray-100 pt-16"} ${className}`}
    >
      {!isEmbedded && <Header />}
      <div className={`${isEmbedded ? "p-6" : "max-w-7xl mx-auto px-4 py-8"}`}>
        {/* Page Header */}
        <div className="flex items-center justify-between mb-8">
          <div className="flex items-center">
            <Calendar size={32} className="mr-3 text-blue-600" />
            <div>
              <h1 className="text-3xl font-bold text-blue-700">
                Appointment Scheduler
              </h1>
              <p className="text-gray-600 mt-1">
                Manage patient appointments and schedules
              </p>
            </div>
          </div>
          <Button
            onClick={handleCreateAppointment}
            className="flex items-center"
          >
            <Plus size={20} className="mr-2" />
            Book Appointment
          </Button>
        </div>

        {/* Statistics Cards */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <Card className="p-6">
            <div className="flex items-center">
              <div className="p-3 bg-blue-100 rounded-lg mr-4">
                <Calendar size={24} className="text-blue-600" />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-600">
                  Total Appointments
                </p>
                <p className="text-2xl font-bold text-gray-900">
                  {appointments.length}
                </p>
              </div>
            </div>
          </Card>

          <Card className="p-6">
            <div className="flex items-center">
              <div className="p-3 bg-green-100 rounded-lg mr-4">
                <Clock size={24} className="text-green-600" />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-600">
                  Today's Appointments
                </p>
                <p className="text-2xl font-bold text-gray-900">
                  {
                    appointments.filter(
                      (apt) =>
                        apt.day === new Date().toISOString().split("T")[0]
                    ).length
                  }
                </p>
              </div>
            </div>
          </Card>

          <Card className="p-6">
            <div className="flex items-center">
              <div className="p-3 bg-yellow-100 rounded-lg mr-4">
                <Users size={24} className="text-yellow-600" />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-600">Confirmed</p>
                <p className="text-2xl font-bold text-gray-900">
                  {
                    appointments.filter((apt) => apt.statusId === "status-2")
                      .length
                  }
                </p>
              </div>
            </div>
          </Card>

          <Card className="p-6">
            <div className="flex items-center">
              <div className="p-3 bg-red-100 rounded-lg mr-4">
                <Clock size={24} className="text-red-600" />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-600">Cancelled</p>
                <p className="text-2xl font-bold text-gray-900">
                  {
                    appointments.filter((apt) => apt.statusId === "status-4")
                      .length
                  }
                </p>
              </div>
            </div>
          </Card>
        </div>

        {/* Filters */}
        <SchedulerFilters
          filters={filters}
          onFiltersChange={updateFilters}
          onClearFilters={clearFilters}
        />

        {/* Calendar View */}
        <Card className="p-6">
          <ReceptionistCalendarView
            events={calendarEvents}
            onEventClick={handleEventClick}
            onDateSelect={handleDateSelect}
          />
        </Card>

        {/* Appointment Modal */}
        <AppointmentModal
          isOpen={modalState.isOpen}
          mode={modalState.mode}
          appointment={selectedAppointment}
          selectedDate={selectedDate}
          onClose={handleCloseModal}
          onCreateSubmit={handleCreateSubmit}
          onUpdateSubmit={handleUpdateSubmit}
          onCancelAppointment={handleCancelAppointment}
          onEdit={handleEditAppointment}
          patientId={modalState.patientId}
        />
      </div>
    </div>
  );
};
