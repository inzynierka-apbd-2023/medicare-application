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
import { getStatusColors } from "../utils/statusColors";

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

        // Join doctor details onto appointments for downstream UIs (dashboard, lists)
        let joinedAppointments = appointments.map((apt) => {
          const doc = doctors.find(
            (d) => d.id === apt.doctorUserId || d.userId === apt.doctorUserId
          );
          return {
            ...apt,
            doctor: doc ?? apt.doctor,
          };
        });

        // Fetch any missing doctors directly by id and update the appointments
        const missing = joinedAppointments.filter((a) => !a.doctor);
        if (missing.length > 0) {
          const fetchedPairs = await Promise.all(
            missing.map(async (a) => {
              try {
                const d = await SchedulerApiService.getDoctorById(
                  a.doctorUserId
                );
                return { id: a.doctorUserId, doctor: d };
              } catch {
                return {
                  id: a.doctorUserId,
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

    if (patientId) {
      loadInitialData();
    }
  }, [patientId]);

  // Refresh appointments
  const refreshAppointments = useCallback(async () => {
    setState((prev: SchedulerState) => ({
      ...prev,
      isLoading: true,
      error: null,
    }));

    try {
      const appointments =
        await SchedulerApiService.getPatientAppointments(patientId);
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

      const missing = joinedAppointments.filter((a) => !a.doctor);
      if (missing.length > 0) {
        const fetchedPairs = await Promise.all(
          missing.map(async (a) => {
            try {
              const d = await SchedulerApiService.getDoctorById(a.doctorUserId);
              return { id: a.doctorUserId, doctor: d };
            } catch {
              return {
                id: a.doctorUserId,
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
    async (appointmentData: CreateAppointmentRequest) => {
      setState((prev: SchedulerState) => ({
        ...prev,
        isLoading: true,
        error: null,
      }));

      try {
        let newAppointment = await SchedulerApiService.createAppointment(
          patientId,
          appointmentData
        );
        // Attach doctor if known in current state
        const matchedDoctor = state.doctors.find(
          (d: Doctor) =>
            d.id === newAppointment.doctorUserId ||
            d.userId === newAppointment.doctorUserId
        );
        if (matchedDoctor) {
          newAppointment = { ...newAppointment, doctor: matchedDoctor };
        } else {
          try {
            const d = await SchedulerApiService.getDoctorById(
              newAppointment.doctorUserId
            );
            newAppointment = { ...newAppointment, doctor: d };
          } catch {
            // ignore, doctor name will fill after next refresh
          }
        }
        setState((prev: SchedulerState) => ({
          ...prev,
          appointments: [...prev.appointments, newAppointment],
          isLoading: false,
        }));
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
    [patientId, state.doctors]
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
        const updatedAppointment = await SchedulerApiService.updateAppointment(
          appointmentId,
          updates
        );
        setState((prev: SchedulerState) => ({
          ...prev,
          appointments: prev.appointments.map((apt: Appointment) =>
            apt.id === appointmentId ? updatedAppointment : apt
          ),
          selectedAppointment:
            prev.selectedAppointment?.id === appointmentId
              ? updatedAppointment
              : prev.selectedAppointment,
          isLoading: false,
        }));
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
    []
  );

  // Cancel appointment
  const cancelAppointment = useCallback(async (appointmentId: string) => {
    setState((prev: SchedulerState) => ({
      ...prev,
      isLoading: true,
      error: null,
    }));

    try {
      await SchedulerApiService.cancelAppointment(appointmentId);
      setState((prev: SchedulerState) => ({
        ...prev,
        appointments: prev.appointments.filter(
          (apt: Appointment) => apt.id !== appointmentId
        ),
        selectedAppointment:
          prev.selectedAppointment?.id === appointmentId
            ? null
            : prev.selectedAppointment,
        isLoading: false,
      }));
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
  }, []);

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

      // Filter by date range
      if (dateRange) {
        const appointmentDate = new Date(appointment.day);
        const startDate = new Date(dateRange.start);
        const endDate = new Date(dateRange.end);

        if (appointmentDate < startDate || appointmentDate > endDate) {
          return false;
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
      const toLocalIso = (d: Date) => {
        const pad = (n: number) => String(n).padStart(2, "0");
        return (
          `${d.getFullYear()}-` +
          `${pad(d.getMonth() + 1)}-` +
          `${pad(d.getDate())}T` +
          `${pad(d.getHours())}:` +
          `${pad(d.getMinutes())}:` +
          `${pad(d.getSeconds())}`
        );
      };
      const doctor =
        state.doctors.find((d: Doctor) => d.id === appointment.doctorUserId) ||
        appointment.doctor;
      const status = state.appointmentStatuses.find(
        (s: { id: string }) => s.id === appointment.statusId
      );
      const colors = getStatusColors(status?.name);

      return {
        id: appointment.id,
        title:
          `${doctor?.firstName ?? ""} ${doctor?.lastName ?? ""} - ${appointment.description || "Appointment"}`.trim(),
        start: appointment.day,
        end: (() => {
          const s = new Date(appointment.day);
          const e = new Date(s.getTime() + appointment.durationMinutes * 60000);
          return toLocalIso(e);
        })(),
        backgroundColor: status?.colorCode || colors.bg,
        borderColor: status?.colorCode || colors.border,
        textColor: colors.text || "#ffffff",
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

  // Get available doctors based on filters
  const availableDoctors = state.filters.specialization
    ? state.doctors.filter((doctor: Doctor) =>
        doctor.specializations?.some(
          (spec: { id: string }) => spec.id === state.filters.specialization
        )
      )
    : state.doctors;

  // Get available services based on filters
  const availableServices = state.filters.specialization
    ? state.services.filter((service: Service) =>
        state.specializations.some(
          (spec: { id: string; serviceId: string }) =>
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
    loadDoctorsByService,
    loadSpecializationsByService,
    calendarEvents,
    filteredAppointments,
    availableDoctors,
    availableServices,
  };
};

export default useScheduler;
