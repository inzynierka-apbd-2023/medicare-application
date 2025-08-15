import { useCallback, useEffect, useState } from "react";

import { ReceptionistSchedulerApiService } from "../services/receptionistSchedulerApiService";
import type {
  AppointmentFilters,
  CalendarEvent,
  CreateAppointmentRequest,
  ReceptionistAppointment,
  UpdateAppointmentRequest,
} from "../types";

interface UseReceptionistSchedulerOptions {
  autoRefresh?: boolean;
  refreshInterval?: number;
}

export const useReceptionistScheduler = (
  options: UseReceptionistSchedulerOptions = {}
) => {
  const { autoRefresh = false, refreshInterval = 30000 } = options;

  const [appointments, setAppointments] = useState<ReceptionistAppointment[]>(
    []
  );
  const [filters, setFilters] = useState<AppointmentFilters>({});
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Convert appointments to calendar events
  const calendarEvents: CalendarEvent[] = appointments.map((appointment) => {
    const startDateTime = new Date(`${appointment.day}T${appointment.time}`);
    const endDateTime = new Date(
      startDateTime.getTime() + appointment.duration * 60 * 1000
    );

    const patientName = appointment.patient
      ? `${appointment.patient.firstName} ${appointment.patient.lastName}`
      : "Unknown Patient";

    const doctorName = appointment.doctor
      ? `Dr. ${appointment.doctor.firstName} ${appointment.doctor.lastName}`
      : "Unknown Doctor";

    return {
      id: appointment.id,
      title: `${patientName} - ${appointment.appointmentType}`,
      start: startDateTime.toISOString(),
      end: endDateTime.toISOString(),
      backgroundColor: appointment.status?.colorCode || "#3B82F6",
      borderColor: appointment.status?.colorCode || "#3B82F6",
      extendedProps: {
        appointment,
        patientName,
        doctorName,
        appointmentType: appointment.appointmentType,
        status: appointment.status?.name || "Unknown",
        ...(appointment.room && { room: appointment.room }),
      },
    };
  });

  const loadAppointments = useCallback(
    async (newFilters?: AppointmentFilters) => {
      try {
        setIsLoading(true);
        setError(null);

        const filtersToUse = newFilters || filters;
        const appointmentsData =
          await ReceptionistSchedulerApiService.getAppointments(filtersToUse);
        setAppointments(appointmentsData);
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "An unexpected error occurred"
        );
      } finally {
        setIsLoading(false);
      }
    },
    [filters]
  );

  const updateFilters = useCallback(
    (newFilters: Partial<AppointmentFilters>) => {
      const updatedFilters = { ...filters, ...newFilters };
      setFilters(updatedFilters);
      loadAppointments(updatedFilters);
    },
    [filters, loadAppointments]
  );

  const createAppointment = useCallback(
    async (
      appointmentData: CreateAppointmentRequest
    ): Promise<ReceptionistAppointment> => {
      try {
        setIsLoading(true);
        setError(null);

        const newAppointment =
          await ReceptionistSchedulerApiService.createAppointment(
            appointmentData
          );

        // Refresh appointments to get the updated list
        await loadAppointments();

        return newAppointment;
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to create appointment";
        setError(errorMessage);
        throw new Error(errorMessage);
      } finally {
        setIsLoading(false);
      }
    },
    [loadAppointments]
  );

  const updateAppointment = useCallback(
    async (
      appointmentData: UpdateAppointmentRequest
    ): Promise<ReceptionistAppointment> => {
      try {
        setIsLoading(true);
        setError(null);

        const updatedAppointment =
          await ReceptionistSchedulerApiService.updateAppointment(
            appointmentData
          );

        // Refresh appointments to get the updated list
        await loadAppointments();

        return updatedAppointment;
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to update appointment";
        setError(errorMessage);
        throw new Error(errorMessage);
      } finally {
        setIsLoading(false);
      }
    },
    [loadAppointments]
  );

  const cancelAppointment = useCallback(
    async (appointmentId: string): Promise<void> => {
      try {
        setIsLoading(true);
        setError(null);

        await ReceptionistSchedulerApiService.cancelAppointment(appointmentId);

        // Refresh appointments to get the updated list
        await loadAppointments();
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to cancel appointment";
        setError(errorMessage);
        throw new Error(errorMessage);
      } finally {
        setIsLoading(false);
      }
    },
    [loadAppointments]
  );

  // Initial load
  useEffect(() => {
    loadAppointments();
  }, [loadAppointments]);

  // Auto-refresh functionality
  useEffect(() => {
    if (autoRefresh) {
      const interval = setInterval(loadAppointments, refreshInterval);
      return () => clearInterval(interval);
    }
    return undefined;
  }, [autoRefresh, refreshInterval, loadAppointments]);

  const clearFilters = useCallback(() => {
    const emptyFilters = {};
    setFilters(emptyFilters);
    loadAppointments(emptyFilters);
  }, [loadAppointments]);

  const refreshAppointments = useCallback(async () => {
    await loadAppointments();
  }, [loadAppointments]);

  return {
    appointments,
    calendarEvents,
    filters,
    isLoading,
    error,
    loadAppointments,
    updateFilters,
    clearFilters,
    refreshAppointments,
    createAppointment,
    updateAppointment,
    cancelAppointment,
  };
};
