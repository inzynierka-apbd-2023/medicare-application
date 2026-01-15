import React, { useCallback, useState } from "react";
import dayGridPlugin from "@fullcalendar/daygrid";
import FullCalendar from "@fullcalendar/react";
import timeGridPlugin from "@fullcalendar/timegrid";
import {
  Calendar,
  Clock,
  MapPin,
  Phone,
  Plus,
  RefreshCcw,
  User,
  Users,
  Video,
} from "lucide-react";

import Header from "../../layout/Header";
import { Button, Card, LoadingOverlay } from "../../shared/components";

import AppointmentModal from "./components/AppointmentModal";
import SchedulerFiltersComponent from "./components/SchedulerFilters";
import { useScheduler } from "./hooks/useScheduler";
import {
  Appointment,
  CreateAppointmentRequest,
  SchedulerPageProps,
  UpdateAppointmentRequest,
} from "./types";

// Custom event formatting
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const formatEventContent = (eventInfo: any) => {
  const extendedProps = eventInfo.event.extendedProps;
  // Fallback if type is missing or simplified
  const appointmentType = extendedProps.appointmentType || "in-person";

  const getTypeIcon = () => {
    switch (appointmentType) {
      case "video":
      case "video-call":
        return <Video size={10} className="inline mr-1" />;
      case "phone":
        return <Phone size={10} className="inline mr-1" />;
      case "in-person":
        return <MapPin size={10} className="inline mr-1" />;
      default:
        return <User size={10} className="inline mr-1" />;
    }
  };

  return (
    <div className="text-xs p-1 overflow-hidden" title={eventInfo.event.title}>
      <div className="font-medium whitespace-nowrap overflow-hidden text-ellipsis flex items-center">
        {getTypeIcon()}
        {eventInfo.timeText} {eventInfo.event.title}
      </div>
    </div>
  );
};

import { useAuth } from "../../shared/auth/AuthContext";

// ... (keep formatEventContent as is)

const SchedulerPage: React.FC<SchedulerPageProps> = ({ patientId }) => {
  const { user } = useAuth();

  // If patientId prop is provided, use it.
  // Otherwise, if user is logged in and has role 'Patient', use their ID.
  // Assumes user.id corresponds to patientId
  const isPatientRole = user?.role?.toLowerCase() === "patient";
  const effectivePatientId =
    patientId || (isPatientRole ? user?.id : undefined);

  // Show header if it's the main view (no patientId prop) OR if it's a patient viewing their own schedule
  const showHeader = !patientId || isPatientRole;

  const {
    appointments,
    calendarEvents,
    doctors,
    services,
    specializations,
    isLoading,
    error,
    filters,
    refreshAppointments,
    createAppointment,
    updateAppointment,
    cancelAppointment,
    // selectAppointment: setGlobalSelectedAppointment,
    updateFilters,
    stats,
  } = useScheduler(effectivePatientId ? { patientId: effectivePatientId } : {});

  const [modalState, setModalState] = useState<{
    isOpen: boolean;
    mode: "create" | "edit" | "view";
    appointment: Appointment | null;
  }>({
    isOpen: false,
    mode: "view",
    appointment: null,
  });

  const handleEventClick = useCallback(
    (info: {
      event: { id: string; extendedProps: Record<string, unknown> };
    }) => {
      const appointmentId = info.event.id;
      const appointment = appointments.find((a) => a.id === appointmentId);
      if (appointment) {
        setModalState({
          isOpen: true,
          mode: "view",
          appointment,
        });
      }
    },
    [appointments]
  );

  const handleCreateClick = () => {
    setModalState({
      isOpen: true,
      mode: "create",
      appointment: null,
    });
  };

  const handleEditClick = () => {
    if (modalState.appointment) {
      setModalState((prev) => ({ ...prev, mode: "edit" }));
    }
  };

  const handleCancelClick = async () => {
    if (
      modalState.appointment &&
      window.confirm("Are you sure you want to cancel this appointment?")
    ) {
      try {
        await cancelAppointment(modalState.appointment.id);
        setModalState((prev) => ({ ...prev, isOpen: false }));
        await refreshAppointments();
      } catch (e) {
        console.error("Cancel failed", e);
        // Might want to show error to user but for now console error
      }
    }
  };

  const handleModalClose = () => {
    setModalState((prev) => ({ ...prev, isOpen: false }));
  };

  const handleModalSave = async (
    data: CreateAppointmentRequest | UpdateAppointmentRequest
  ) => {
    try {
      if (modalState.mode === "create") {
        // Pass patientId from context if available, otherwise it comes from data (Step 0) which is handled inside hook now?
        // Actually hook's createAppointment signature: (data, patientIdOverride?)
        // If data has patientId (from Modal Step 0), pass it.
        const requestData = data as CreateAppointmentRequest;
        await createAppointment(
          requestData,
          requestData.patientId || effectivePatientId
        );
      } else if (modalState.mode === "edit" && modalState.appointment) {
        await updateAppointment(
          modalState.appointment.id,
          data as UpdateAppointmentRequest
        );
      }
      await refreshAppointments();
    } catch (error) {
      console.error("Failed to save appointment:", error);
      throw error;
    }
  };

  return (
    <>
      {showHeader && <Header />}
      <div className={showHeader ? "pt-16 min-h-screen bg-gray-50" : ""}>
        <div className="container mx-auto px-4 py-8">
          {/* Page Header */}
          <div className="flex items-center justify-between mb-8">
            <div className="flex items-center">
              <Calendar size={32} className="mr-3 text-blue-600" />
              <div>
                <h1 className="text-2xl font-bold text-gray-900">
                  Schedule Management
                </h1>
                <p className="mt-1 text-sm text-gray-500">
                  {effectivePatientId
                    ? "Manage your appointments"
                    : "Manage clinic schedule"}
                </p>
              </div>
            </div>
            <div className="flex space-x-2">
              <Button
                variant="outline"
                onClick={() => refreshAppointments()}
                disabled={isLoading}
              >
                <RefreshCcw
                  className={`w-4 h-4 mr-2 ${isLoading ? "animate-spin" : ""}`}
                />
                Refresh
              </Button>
              <Button onClick={handleCreateClick} className="flex items-center">
                <Plus className="w-4 h-4 mr-2" />
                New Appointment
              </Button>
            </div>
          </div>

          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-6">
              {error}
            </div>
          )}

          {/* Appointment Stats - Moved above filters and styled like Receptionist view */}
          {!effectivePatientId && stats && (
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
                      {stats.totalAppointments}
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
                      {stats.todaysAppointments}
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
                    <p className="text-sm font-medium text-gray-600">
                      Confirmed
                    </p>
                    <p className="text-2xl font-bold text-gray-900">
                      {stats.confirmedAppointments}
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
                    <p className="text-sm font-medium text-gray-600">
                      Cancelled
                    </p>
                    <p className="text-2xl font-bold text-gray-900">
                      {stats.cancelledAppointments}
                    </p>
                  </div>
                </div>
              </Card>
            </div>
          )}

          <div className="mb-6">
            <SchedulerFiltersComponent
              filters={filters}
              onFiltersChange={updateFilters}
              specializations={specializations}
              services={services}
              doctors={doctors}
              // cast to any if Type definition mismatch (isLoading optional vs required) for now
              isLoading={isLoading}
            />
          </div>

          <LoadingOverlay isLoading={isLoading}>
            <Card>
              <div className="p-4 scheduler-calendar-container">
                <FullCalendar
                  plugins={[dayGridPlugin, timeGridPlugin]}
                  initialView="timeGridWeek"
                  headerToolbar={{
                    left: "prev,next today",
                    center: "title",
                    right: "dayGridMonth,timeGridWeek,timeGridDay",
                  }}
                  events={calendarEvents}
                  eventClick={handleEventClick}
                  height="auto"
                  eventContent={formatEventContent}
                  dayMaxEvents={3}
                  eventTimeFormat={{
                    hour: "2-digit",
                    minute: "2-digit",
                    meridiem: false,
                  }}
                />
              </div>
            </Card>
          </LoadingOverlay>

          {/* Appointment Modal */}
          <AppointmentModal
            isOpen={modalState.isOpen}
            onClose={handleModalClose}
            appointment={modalState.appointment}
            onSave={handleModalSave}
            mode={modalState.mode}
            patientId={effectivePatientId}
            onEdit={handleEditClick}
            onCancel={handleCancelClick}
          />
        </div>
      </div>
    </>
  );
};

export default SchedulerPage;
