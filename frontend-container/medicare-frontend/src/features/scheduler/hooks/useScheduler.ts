import { useCallback, useEffect, useState } from "react";

import SchedulerApiService from "../services/schedulerApiService";
import type {
  Appointment,
  CalendarEvent,
  CreateAppointmentRequest,
  Doctor,
  SchedulerFilters,
  SchedulerState,
  Service,
  UpdateAppointmentRequest,
} from "../types";

interface UseSchedulerProps {
  patientId: string;
  initialFilters?: SchedulerFilters;
}

interface UseSchedulerReturn extends SchedulerState {
  // Actions
  refreshAppointments: () => Promise<void>;
  createAppointment: (appointment: CreateAppointmentRequest) => Promise<void>;
  updateAppointment: (
    appointmentId: string,
    updates: UpdateAppointmentRequest
  ) => Promise<void>;
  cancelAppointment: (appointmentId: string) => Promise<void>;
  selectAppointment: (appointment: Appointment | null) => void;
  setSelectedDate: (date: string | null) => void;
  updateFilters: (filters: Partial<SchedulerFilters>) => void;
  loadAvailableTimeSlots: (
    doctorId: string,
    serviceId: string,
    startDate: string,
    endDate: string
  ) => Promise<void>;
  loadDoctorsBySpecialization: (specializationId: string) => Promise<void>;
  loadServicesBySpecialization: (specializationId: string) => Promise<void>;

  // Computed values
  calendarEvents: CalendarEvent[];
  filteredAppointments: Appointment[];
  availableDoctors: Doctor[];
  availableServices: Service[];
}

export const useScheduler = ({
  patientId,
  initialFilters = {},
}: UseSchedulerProps): UseSchedulerReturn => {
  const [state, setState] = useState<SchedulerState>({
    appointments: [],
    doctors: [],
    services: [],
    specializations: [],
    timeSlots: [],
    appointmentStatuses: [],
    isLoading: false,
    error: null,
    selectedDate: null,
    selectedAppointment: null,
    filters: initialFilters,
  });

  // Load initial data
  useEffect(() => {
    const loadInitialData = async () => {
      setState((prev) => ({ ...prev, isLoading: true, error: null }));

      try {
        const [
          appointments,
          doctors,
          services,
          specializations,
          appointmentStatuses,
        ] = await Promise.all([
          SchedulerApiService.getPatientAppointments(patientId),
          SchedulerApiService.getDoctors(),
          SchedulerApiService.getServices(),
          SchedulerApiService.getSpecializations(),
          SchedulerApiService.getAppointmentStatuses(),
        ]);

        setState((prev) => ({
          ...prev,
          appointments,
          doctors,
          services,
          specializations,
          appointmentStatuses,
          isLoading: false,
        }));
      } catch (error) {
        setState((prev) => ({
          ...prev,
          error:
            error instanceof Error
              ? error.message
              : "Failed to load scheduler data",
          isLoading: false,
        }));
      }
    };

    if (patientId) {
      loadInitialData();
    }
  }, [patientId]);

  // Refresh appointments
  const refreshAppointments = useCallback(async () => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }));

    try {
      const appointments =
        await SchedulerApiService.getPatientAppointments(patientId);
      setState((prev) => ({
        ...prev,
        appointments,
        isLoading: false,
      }));
    } catch (error) {
      setState((prev) => ({
        ...prev,
        error:
          error instanceof Error
            ? error.message
            : "Failed to refresh appointments",
        isLoading: false,
      }));
    }
  }, [patientId]);

  // Create appointment
  const createAppointment = useCallback(
    async (appointmentData: CreateAppointmentRequest) => {
      setState((prev) => ({ ...prev, isLoading: true, error: null }));

      try {
        const newAppointment = await SchedulerApiService.createAppointment(
          patientId,
          appointmentData
        );
        setState((prev) => ({
          ...prev,
          appointments: [...prev.appointments, newAppointment],
          isLoading: false,
        }));
      } catch (error) {
        setState((prev) => ({
          ...prev,
          error:
            error instanceof Error
              ? error.message
              : "Failed to create appointment",
          isLoading: false,
        }));
        throw error;
      }
    },
    [patientId]
  );

  // Update appointment
  const updateAppointment = useCallback(
    async (appointmentId: string, updates: UpdateAppointmentRequest) => {
      setState((prev) => ({ ...prev, isLoading: true, error: null }));

      try {
        const updatedAppointment = await SchedulerApiService.updateAppointment(
          appointmentId,
          updates
        );
        setState((prev) => ({
          ...prev,
          appointments: prev.appointments.map((apt) =>
            apt.id === appointmentId ? updatedAppointment : apt
          ),
          selectedAppointment:
            prev.selectedAppointment?.id === appointmentId
              ? updatedAppointment
              : prev.selectedAppointment,
          isLoading: false,
        }));
      } catch (error) {
        setState((prev) => ({
          ...prev,
          error:
            error instanceof Error
              ? error.message
              : "Failed to update appointment",
          isLoading: false,
        }));
        throw error;
      }
    },
    []
  );

  // Cancel appointment
  const cancelAppointment = useCallback(async (appointmentId: string) => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }));

    try {
      await SchedulerApiService.cancelAppointment(appointmentId);
      setState((prev) => ({
        ...prev,
        appointments: prev.appointments.filter(
          (apt) => apt.id !== appointmentId
        ),
        selectedAppointment:
          prev.selectedAppointment?.id === appointmentId
            ? null
            : prev.selectedAppointment,
        isLoading: false,
      }));
    } catch (error) {
      setState((prev) => ({
        ...prev,
        error:
          error instanceof Error
            ? error.message
            : "Failed to cancel appointment",
        isLoading: false,
      }));
      throw error;
    }
  }, []);

  // Select appointment
  const selectAppointment = useCallback((appointment: Appointment | null) => {
    setState((prev) => ({ ...prev, selectedAppointment: appointment }));
  }, []);

  // Set selected date
  const setSelectedDate = useCallback((date: string | null) => {
    setState((prev) => ({ ...prev, selectedDate: date }));
  }, []);

  // Update filters
  const updateFilters = useCallback((newFilters: Partial<SchedulerFilters>) => {
    setState((prev) => ({
      ...prev,
      filters: { ...prev.filters, ...newFilters },
    }));
  }, []);

  // Load available time slots
  const loadAvailableTimeSlots = useCallback(
    async (
      doctorId: string,
      serviceId: string,
      startDate: string,
      endDate: string
    ) => {
      setState((prev) => ({ ...prev, isLoading: true, error: null }));

      try {
        const timeSlots = await SchedulerApiService.getAvailableTimeSlots({
          doctorId,
          serviceId,
          startDate,
          endDate,
        });
        setState((prev) => ({
          ...prev,
          timeSlots,
          isLoading: false,
        }));
      } catch (error) {
        setState((prev) => ({
          ...prev,
          error:
            error instanceof Error
              ? error.message
              : "Failed to load available time slots",
          isLoading: false,
        }));
      }
    },
    []
  );

  // Load doctors by specialization
  const loadDoctorsBySpecialization = useCallback(
    async (specializationId: string) => {
      setState((prev) => ({ ...prev, isLoading: true, error: null }));

      try {
        const doctors =
          await SchedulerApiService.getDoctorsBySpecialization(
            specializationId
          );
        setState((prev) => ({
          ...prev,
          doctors,
          isLoading: false,
        }));
      } catch (error) {
        setState((prev) => ({
          ...prev,
          error:
            error instanceof Error ? error.message : "Failed to load doctors",
          isLoading: false,
        }));
      }
    },
    []
  );

  // Load services by specialization
  const loadServicesBySpecialization = useCallback(
    async (specializationId: string) => {
      setState((prev) => ({ ...prev, isLoading: true, error: null }));

      try {
        const services =
          await SchedulerApiService.getServicesBySpecialization(
            specializationId
          );
        setState((prev) => ({
          ...prev,
          services,
          isLoading: false,
        }));
      } catch (error) {
        setState((prev) => ({
          ...prev,
          error:
            error instanceof Error ? error.message : "Failed to load services",
          isLoading: false,
        }));
      }
    },
    []
  );

  // Compute calendar events from appointments
  const calendarEvents: CalendarEvent[] = state.appointments.map(
    (appointment) => {
      const doctor = state.doctors.find(
        (d) => d.id === appointment.doctorUserId
      );
      const status = state.appointmentStatuses.find(
        (s) => s.id === appointment.statusId
      );

      return {
        id: appointment.id,
        title: `${doctor?.firstName} ${doctor?.lastName} - ${appointment.description || "Appointment"}`,
        start: appointment.day,
        end: new Date(
          new Date(appointment.day).getTime() +
            appointment.durationMinutes * 60000
        ).toISOString(),
        backgroundColor: status?.colorCode || "#3b82f6",
        borderColor: status?.colorCode || "#3b82f6",
        textColor: "#ffffff",
        extendedProps: {
          appointment,
          doctorName:
            `${doctor?.firstName || ""} ${doctor?.lastName || ""}`.trim(),
          patientName: "", // Will be populated from patient data if needed
          appointmentType: appointment.appointmentType,
          status: status?.name || "Unknown",
          description: appointment.description || "",
        },
      };
    }
  );

  // Apply filters to appointments
  const filteredAppointments = state.appointments.filter((appointment) => {
    const { doctor, appointmentType, dateRange } = state.filters;

    // Filter by doctor
    if (doctor && appointment.doctorUserId !== doctor) {
      return false;
    }

    // Filter by appointment type
    if (
      appointmentType &&
      appointmentType !== "all" &&
      appointment.appointmentType !== appointmentType
    ) {
      return false;
    }

    // Filter by date range
    if (dateRange) {
      const appointmentDate = new Date(appointment.day);
      const startDate = new Date(dateRange.start);
      const endDate = new Date(dateRange.end);

      if (appointmentDate < startDate || appointmentDate > endDate) {
        return false;
      }
    }

    // Additional filters would require joining with doctor/service data
    // For now, these are basic filters

    return true;
  });

  // Get available doctors based on filters
  const availableDoctors = state.filters.specialization
    ? state.doctors.filter((doctor) =>
        doctor.specializations?.some(
          (spec) => spec.id === state.filters.specialization
        )
      )
    : state.doctors;

  // Get available services based on filters
  const availableServices = state.filters.specialization
    ? state.services.filter((service) =>
        state.specializations.some(
          (spec) =>
            spec.id === state.filters.specialization &&
            spec.serviceId === service.id
        )
      )
    : state.services;

  return {
    ...state,
    refreshAppointments,
    createAppointment,
    updateAppointment,
    cancelAppointment,
    selectAppointment,
    setSelectedDate,
    updateFilters,
    loadAvailableTimeSlots,
    loadDoctorsBySpecialization,
    loadServicesBySpecialization,
    calendarEvents,
    filteredAppointments,
    availableDoctors,
    availableServices,
  };
};

export default useScheduler;
