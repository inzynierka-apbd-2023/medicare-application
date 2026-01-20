import { useCallback, useEffect, useState } from "react";
import SchedulerApiService from "@features/scheduler/services/schedulerApiService";
import type {
  Appointment,
  CalendarEvent,
  CreateAppointmentRequest,
  Doctor,
  SchedulerFilters,
  SchedulerState,
  Service,
  UpdateAppointmentRequest,
} from "@features/scheduler/types";
import { getStatusColors } from "@features/scheduler/utils/statusColors";

interface UseSchedulerProps {
  patientId?: string | undefined; // Optional for receptionist view
  initialFilters?: SchedulerFilters;
}

interface UseSchedulerReturn extends SchedulerState {
  // Actions
  refreshAppointments: () => Promise<void>;
  createAppointment: (
    appointment: CreateAppointmentRequest,
    patientIdOverride?: string
  ) => Promise<void>;
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
  loadDoctorsByService: (serviceId: string) => Promise<void>;
  loadSpecializationsByService: (serviceId: string) => Promise<void>;

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
    stats: null,
  });

  // Load initial data
  useEffect(() => {
    const loadInitialData = async () => {
      setState((prev: SchedulerState) => ({
        ...prev,
        isLoading: true,
        error: null,
      }));

      try {
        const [doctors, services, specializations, appointmentStatuses, stats] =
          await Promise.all([
            SchedulerApiService.getDoctors(),
            SchedulerApiService.getServices(),
            SchedulerApiService.getSpecializations(),
            SchedulerApiService.getAppointmentStatuses(),
            SchedulerApiService.getSchedulerStats(
              patientId ? { patientId } : {}
            ),
          ]);

        let appointments: Appointment[] = [];
        if (patientId) {
          appointments =
            await SchedulerApiService.getPatientAppointments(patientId);
        } else {
          appointments = await SchedulerApiService.getAllAppointments();
        }

        let joinedAppointments = appointments.map((apt) => {
          const doc = doctors.find(
            (d) => d.id === apt.doctorUserId || d.userId === apt.doctorUserId
          );
          return {
            ...apt,
            doctor: doc ?? apt.doctor,
          };
        });

        const missing = joinedAppointments.filter(
          (a) => !a.doctor && a.doctorUserId
        );
        if (missing.length > 0) {
          // Unique doctor IDs to fetch
          const doctorIds = Array.from(
            new Set(missing.map((a) => a.doctorUserId))
          );

          const fetchedPairs = await Promise.all(
            doctorIds.map(async (id) => {
              try {
                const d = await SchedulerApiService.getDoctorById(id);
                return { id, doctor: d };
              } catch {
                return {
                  id,
                  doctor: undefined as Doctor | undefined,
                };
              }
            })
          );

          const fetchedMap = new Map<string, Doctor | undefined>(
            fetchedPairs.map((p) => [p.id, p.doctor])
          );
          joinedAppointments = joinedAppointments.map((a) =>
            a.doctor
              ? a
              : {
                  ...a,
                  doctor: fetchedMap.get(a.doctorUserId) ?? a.doctor,
                }
          );
        }

        setState((prev: SchedulerState) => ({
          ...prev,
          appointments: joinedAppointments,
          doctors,
          services,
          specializations,
          appointmentStatuses,
          stats,
          isLoading: false,
        }));
      } catch (error) {
        setState((prev: SchedulerState) => ({
          ...prev,
          error:
            error instanceof Error
              ? error.message
              : "Failed to load scheduler data",
          isLoading: false,
        }));
      }
    };

    loadInitialData();
  }, [patientId]);

  // Refresh appointments & stats
  const refreshAppointments = useCallback(async () => {
    setState((prev: SchedulerState) => ({
      ...prev,
      isLoading: true,
      error: null,
    }));

    try {
      const statsPromise = SchedulerApiService.getSchedulerStats(
        patientId ? { patientId } : {}
      );
      let appointmentsPromise: Promise<Appointment[]>;

      if (patientId) {
        appointmentsPromise =
          SchedulerApiService.getPatientAppointments(patientId);
      } else {
        appointmentsPromise = SchedulerApiService.getAllAppointments();
      }

      const [stats, appointments] = await Promise.all([
        statsPromise,
        appointmentsPromise,
      ]);

      let joinedAppointments = appointments.map((apt) => {
        const doc = state.doctors.find(
          (d: Doctor) =>
            d.id === apt.doctorUserId || d.userId === apt.doctorUserId
        );
        return {
          ...apt,
          doctor: doc ?? apt.doctor,
        };
      });

      const missing = joinedAppointments.filter(
        (a) => !a.doctor && a.doctorUserId
      );
      if (missing.length > 0) {
        const doctorIds = Array.from(
          new Set(missing.map((a) => a.doctorUserId))
        );
        const fetchedPairs = await Promise.all(
          doctorIds.map(async (id) => {
            try {
              const d = await SchedulerApiService.getDoctorById(id);
              return { id, doctor: d };
            } catch {
              return {
                id,
                doctor: undefined as Doctor | undefined,
              };
            }
          })
        );
        const fetchedMap = new Map<string, Doctor | undefined>(
          fetchedPairs.map((p) => [p.id, p.doctor])
        );
        joinedAppointments = joinedAppointments.map((a) =>
          a.doctor
            ? a
            : {
                ...a,
                doctor: fetchedMap.get(a.doctorUserId) ?? a.doctor,
              }
        );
      }
      setState((prev: SchedulerState) => ({
        ...prev,
        appointments: joinedAppointments,
        stats,
        isLoading: false,
      }));
    } catch (error) {
      setState((prev: SchedulerState) => ({
        ...prev,
        error:
          error instanceof Error
            ? error.message
            : "Failed to refresh appointments",
        isLoading: false,
      }));
    }
  }, [patientId, state.doctors]);

  // Create appointment
  const createAppointment = useCallback(
    async (
      appointmentData: CreateAppointmentRequest,
      patientIdOverride?: string
    ) => {
      const targetPatientId = patientIdOverride || patientId;

      if (!targetPatientId) {
        setState((prev) => ({
          ...prev,
          error: "Patient ID is required to create an appointment",
        }));
        return;
      }

      setState((prev: SchedulerState) => ({
        ...prev,
        isLoading: true,
        error: null,
      }));

      try {
        await SchedulerApiService.createAppointment(
          targetPatientId,
          appointmentData
        );
        // Full refresh to ensure stats and list are in sync
        await refreshAppointments();
      } catch (error) {
        setState((prev: SchedulerState) => ({
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
    [patientId, refreshAppointments]
  );

  // Update appointment
  const updateAppointment = useCallback(
    async (appointmentId: string, updates: UpdateAppointmentRequest) => {
      setState((prev: SchedulerState) => ({
        ...prev,
        isLoading: true,
        error: null,
      }));

      try {
        await SchedulerApiService.updateAppointment(appointmentId, updates);
        await refreshAppointments();
      } catch (error) {
        setState((prev: SchedulerState) => ({
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
    [refreshAppointments]
  );

  // Cancel appointment
  const cancelAppointment = useCallback(
    async (appointmentId: string) => {
      setState((prev: SchedulerState) => ({
        ...prev,
        isLoading: true,
        error: null,
      }));

      try {
        await SchedulerApiService.cancelAppointment(appointmentId);
        await refreshAppointments();
      } catch (error) {
        setState((prev: SchedulerState) => ({
          ...prev,
          error:
            error instanceof Error
              ? error.message
              : "Failed to cancel appointment",
          isLoading: false,
        }));
        throw error;
      }
    },
    [refreshAppointments]
  );

  // Select appointment
  const selectAppointment = useCallback((appointment: Appointment | null) => {
    setState((prev: SchedulerState) => ({
      ...prev,
      selectedAppointment: appointment,
    }));
  }, []);

  // Set selected date
  const setSelectedDate = useCallback((date: string | null) => {
    setState((prev: SchedulerState) => ({ ...prev, selectedDate: date }));
  }, []);

  // Helpers to narrow/reload catalogs while preserving compatible selections
  const applyNarrowBySpecialization = useCallback(
    async (specId: string) => {
      try {
        const [srv, docs] = await Promise.all([
          SchedulerApiService.getServicesBySpecialization(specId),
          SchedulerApiService.getDoctorsFiltered({
            specializationId: specId,
            ...(state.filters.service
              ? { serviceId: state.filters.service }
              : {}),
          }),
        ]);
        setState((prev: SchedulerState) => {
          const selSvc = prev.filters?.service;
          const nextService =
            selSvc && srv.some((s) => s.id === selSvc) ? selSvc : undefined;
          const selDoc = prev.filters?.doctor;
          const nextDoctor =
            selDoc && docs.some((d) => d.id === selDoc) ? selDoc : undefined;
          return {
            ...prev,
            services: srv,
            doctors: docs,
            filters: {
              ...prev.filters,
              service: nextService,
              doctor: nextDoctor,
            },
          } as SchedulerState;
        });
      } catch (e) {
        console.error("Failed to narrow by specialization", e);
      }
    },
    [state.filters.service]
  );

  const handleClearedSpecialization = useCallback(
    async (serviceId?: string) => {
      try {
        if (serviceId) {
          const [specs, docs] = await Promise.all([
            SchedulerApiService.getSpecializationsByService(serviceId),
            SchedulerApiService.getDoctorsByService(serviceId),
          ]);
          setState((prev: SchedulerState) => ({
            ...prev,
            specializations: specs,
            doctors: docs,
          }));
        } else {
          const [srv, specs, docs] = await Promise.all([
            SchedulerApiService.getServices(),
            SchedulerApiService.getSpecializations(),
            SchedulerApiService.getDoctors(),
          ]);
          setState((prev: SchedulerState) => ({
            ...prev,
            services: srv,
            specializations: specs,
            doctors: docs,
          }));
        }
      } catch (err) {
        console.error(
          "Failed to reload catalogs after clearing specialization",
          err
        );
      }
    },
    []
  );

  const applyNarrowByService = useCallback(
    async (serviceId: string) => {
      try {
        const [specs, docs] = await Promise.all([
          SchedulerApiService.getSpecializationsByService(serviceId),
          SchedulerApiService.getDoctorsFiltered({
            serviceId,
            ...(state.filters.specialization
              ? { specializationId: state.filters.specialization }
              : {}),
          }),
        ]);
        setState((prev: SchedulerState) => {
          const selSpec = prev.filters?.specialization;
          const nextSpec =
            selSpec && specs.some((sp) => sp.id === selSpec)
              ? selSpec
              : undefined;
          const selDoc = prev.filters?.doctor;
          const nextDoctor =
            selDoc && docs.some((d) => d.id === selDoc) ? selDoc : undefined;
          return {
            ...prev,
            specializations: specs,
            doctors: docs,
            filters: {
              ...prev.filters,
              specialization: nextSpec,
              doctor: nextDoctor,
            },
          } as SchedulerState;
        });
      } catch (err) {
        console.error("Failed to narrow by service", err);
      }
    },
    [state.filters.specialization]
  );

  const handleClearedService = useCallback(
    async (specializationId?: string) => {
      try {
        if (specializationId) {
          const [srv, docs] = await Promise.all([
            SchedulerApiService.getServicesBySpecialization(specializationId),
            SchedulerApiService.getDoctorsBySpecialization(specializationId),
          ]);
          setState((prev: SchedulerState) => ({
            ...prev,
            services: srv,
            doctors: docs,
          }));
        } else {
          const [srv, specs, docs] = await Promise.all([
            SchedulerApiService.getServices(),
            SchedulerApiService.getSpecializations(),
            SchedulerApiService.getDoctors(),
          ]);
          setState((prev: SchedulerState) => ({
            ...prev,
            services: srv,
            specializations: specs,
            doctors: docs,
          }));
        }
      } catch (err) {
        console.error("Failed to reload catalogs after clearing service", err);
      }
    },
    []
  );

  const applyDoctorAutofill = useCallback(
    (doctorId: string, current: SchedulerFilters) => {
      const picked = state.doctors.find((d: Doctor) => d.id === doctorId);
      const docSpecId = picked?.specializations?.[0]?.id;
      if (docSpecId && current.specialization !== docSpecId) {
        setState((prev: SchedulerState) => ({
          ...prev,
          filters: {
            ...prev.filters,
            specialization: docSpecId,
            doctor: doctorId,
          } as SchedulerFilters,
        }));
        applyNarrowBySpecialization(docSpecId);
      }
    },
    [state.doctors, applyNarrowBySpecialization]
  );

  // Update filters
  const updateFilters = useCallback(
    (newFilters: Partial<SchedulerFilters>) => {
      // Compute next filters to decide dependent loads
      const nextFilters: SchedulerFilters = {
        ...state.filters,
        ...newFilters,
      } as SchedulerFilters;
      setState((prev: SchedulerState) => ({ ...prev, filters: nextFilters }));
      // Specialization change
      if ("specialization" in newFilters) {
        const sid = nextFilters.specialization;
        if (sid) {
          applyNarrowBySpecialization(sid);
        } else {
          handleClearedSpecialization(nextFilters.service);
        }
      }

      // Service change
      if ("service" in newFilters) {
        const serviceId = nextFilters.service;
        if (serviceId) {
          applyNarrowByService(serviceId);
        } else if (nextFilters.specialization) {
          handleClearedService(nextFilters.specialization);
        } else {
          handleClearedService(undefined);
        }
      }

      // Doctor change: auto-fill specialization/service based on the picked doctor
      if ("doctor" in newFilters) {
        const doctorId = nextFilters.doctor;
        if (doctorId) applyDoctorAutofill(doctorId, nextFilters);
      }
    },
    [
      state.filters,
      applyNarrowByService,
      handleClearedService,
      handleClearedSpecialization,
      applyNarrowBySpecialization,
      applyDoctorAutofill,
    ]
  );

  // Load available time slots
  const loadAvailableTimeSlots = useCallback(
    async (
      doctorId: string,
      serviceId: string,
      startDate: string,
      endDate: string
    ) => {
      setState((prev: SchedulerState) => ({
        ...prev,
        isLoading: true,
        error: null,
      }));

      try {
        const timeSlots = await SchedulerApiService.getAvailableTimeSlots({
          doctorId,
          serviceId,
          startDate,
          endDate,
        });
        setState((prev: SchedulerState) => ({
          ...prev,
          timeSlots,
          isLoading: false,
        }));
      } catch (error) {
        setState((prev: SchedulerState) => ({
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
      setState((prev: SchedulerState) => ({
        ...prev,
        isLoading: true,
        error: null,
      }));

      try {
        const doctors =
          await SchedulerApiService.getDoctorsBySpecialization(
            specializationId
          );
        setState((prev: SchedulerState) => ({
          ...prev,
          doctors,
          isLoading: false,
        }));
      } catch (error) {
        setState((prev: SchedulerState) => ({
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
      setState((prev: SchedulerState) => ({
        ...prev,
        isLoading: true,
        error: null,
      }));

      try {
        const services =
          await SchedulerApiService.getServicesBySpecialization(
            specializationId
          );
        setState((prev: SchedulerState) => ({
          ...prev,
          services,
          isLoading: false,
        }));
      } catch (error) {
        setState((prev: SchedulerState) => ({
          ...prev,
          error:
            error instanceof Error ? error.message : "Failed to load services",
          isLoading: false,
        }));
      }
    },
    []
  );

  // Load doctors by service
  const loadDoctorsByService = useCallback(async (serviceId: string) => {
    setState((prev: SchedulerState) => ({
      ...prev,
      isLoading: true,
      error: null,
    }));
    try {
      const doctors = await SchedulerApiService.getDoctorsByService(serviceId);
      setState((prev: SchedulerState) => ({
        ...prev,
        doctors,
        isLoading: false,
      }));
    } catch (error) {
      setState((prev: SchedulerState) => ({
        ...prev,
        error:
          error instanceof Error ? error.message : "Failed to load doctors",
        isLoading: false,
      }));
    }
  }, []);

  // Load specializations by service
  const loadSpecializationsByService = useCallback(
    async (serviceId: string) => {
      setState((prev: SchedulerState) => ({
        ...prev,
        isLoading: true,
        error: null,
      }));
      try {
        const specs =
          await SchedulerApiService.getSpecializationsByService(serviceId);
        setState((prev: SchedulerState) => ({
          ...prev,
          specializations: specs,
          isLoading: false,
        }));
      } catch (error) {
        setState((prev: SchedulerState) => ({
          ...prev,
          error:
            error instanceof Error
              ? error.message
              : "Failed to load specializations",
          isLoading: false,
        }));
      }
    },
    []
  );

  // Apply filters to appointments
  const filteredAppointments = state.appointments.filter(
    (appointment: Appointment) => {
      const { doctor, appointmentType, dateRange, specialization, service } =
        state.filters;

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

      // Filter by date range - only compare valid boundaries
      if (dateRange) {
        const appointmentDate = new Date(appointment.day);

        if (dateRange.start) {
          const startDate = new Date(dateRange.start);
          if (!isNaN(startDate.getTime()) && appointmentDate < startDate) {
            return false;
          }
        }

        if (dateRange.end) {
          const endDate = new Date(dateRange.end);
          // Add 1 day to end date to include the entire end day
          endDate.setDate(endDate.getDate() + 1);
          if (!isNaN(endDate.getTime()) && appointmentDate >= endDate) {
            return false;
          }
        }
      }

      // Filter by specializaton
      if (specialization) {
        const doc = appointment.doctor;
        // Check both single specialization and list
        const hasSpec =
          doc?.specializationId === specialization ||
          doc?.specializations?.some(
            (s: { id: string }) => s.id === specialization
          );

        if (!hasSpec) return false;
      }

      // Filter by service
      if (service) {
        if (appointment.serviceId !== service) return false;
      }

      return true;
    }
  );

  // Compute calendar events from appointments
  const calendarEvents: CalendarEvent[] = filteredAppointments.map(
    (appointment: Appointment) => {
      const start = new Date(appointment.day);
      const end = new Date(
        start.getTime() + appointment.durationMinutes * 60000
      );
      const colors = getStatusColors(appointment.status?.name || "Scheduled");

      const docName = appointment.doctor
        ? `${appointment.doctor.firstName} ${appointment.doctor.lastName}`
        : "Unknown Doctor";

      const event: CalendarEvent = {
        id: appointment.id,
        title: `${docName} - ${appointment.appointmentType}`,
        start: start.toISOString(),
        end: end.toISOString(),
        extendedProps: {
          appointment,
          patientId: appointment.patientId,
          doctorId: appointment.doctorUserId,
          type: appointment.appointmentType || "in-person",
          status: (appointment.status?.name?.toLowerCase() || "scheduled") as
            | "scheduled"
            | "completed"
            | "cancelled",
        },
      };

      if (colors.bg) event.backgroundColor = colors.bg;
      if (colors.border) event.borderColor = colors.border;
      if (colors.text) event.textColor = colors.text;

      return event;
    }
  );

  return {
    ...state,
    calendarEvents,
    filteredAppointments,
    availableDoctors: state.doctors,
    availableServices: state.services,

    // Actions
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
    loadDoctorsByService,
    loadSpecializationsByService,
  };
};
