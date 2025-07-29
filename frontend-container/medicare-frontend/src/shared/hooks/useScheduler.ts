import { useState, useEffect } from "react";
import { schedulerApi } from "../services/schedulerApi";
import type {
  Service,
  Specialization,
  Doctor,
  TimeSlot,
  CalendarEvent,
  AppointmentBooking,
} from "../../features/scheduler/types";

export const useScheduler = () => {
  const [services, setServices] = useState<Service[]>([]);
  const [specializations, setSpecializations] = useState<Specialization[]>([]);
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [timeSlots, setTimeSlots] = useState<TimeSlot[]>([]);
  const [events, setEvents] = useState<CalendarEvent[]>([]);

  const [selectedService, setSelectedService] = useState<string>("");
  const [selectedSpecialization, setSelectedSpecialization] =
    useState<string>("");
  const [selectedDoctor, setSelectedDoctor] = useState<string>("");

  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Load initial data
  useEffect(() => {
    const loadInitialData = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const [servicesResult, specializationsResult, doctorsResult] =
          await Promise.all([
            schedulerApi.getServices(),
            schedulerApi.getSpecializations(),
            schedulerApi.getDoctors(),
          ]);

        if (servicesResult.success) {
          setServices(servicesResult.data);
        } else {
          throw new Error(servicesResult.error);
        }

        if (specializationsResult.success) {
          setSpecializations(specializationsResult.data);
        } else {
          throw new Error(specializationsResult.error);
        }

        if (doctorsResult.success) {
          setDoctors(doctorsResult.data);
        } else {
          throw new Error(doctorsResult.error);
        }
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "Failed to load scheduler data"
        );
      } finally {
        setIsLoading(false);
      }
    };

    loadInitialData();
  }, []);

  // Load time slots when doctor is selected
  useEffect(() => {
    if (!selectedDoctor) {
      setTimeSlots([]);
      return;
    }

    const loadTimeSlots = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const result = await schedulerApi.getTimeSlots(selectedDoctor);

        if (result.success) {
          setTimeSlots(result.data);
        } else {
          throw new Error(result.error);
        }
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "Failed to load time slots"
        );
      } finally {
        setIsLoading(false);
      }
    };

    loadTimeSlots();
  }, [selectedDoctor]);

  // Selection handlers
  const handleServiceChange = (serviceId: string) => {
    setSelectedService(serviceId);

    if (serviceId) {
      // Auto-select related specialization and clear doctor
      const service = services.find((s) => s.id === serviceId);
      if (service) {
        setSelectedSpecialization(service.specializationId);
      }
      setSelectedDoctor("");
    } else {
      setSelectedSpecialization("");
      setSelectedDoctor("");
    }
  };

  const handleSpecializationChange = (specializationId: string) => {
    setSelectedSpecialization(specializationId);

    if (specializationId) {
      // Clear service and doctor selections
      setSelectedService("");
      setSelectedDoctor("");
    }
  };

  const handleDoctorChange = (doctorId: string) => {
    setSelectedDoctor(doctorId);

    if (doctorId) {
      // Clear service and specialization selections
      setSelectedService("");
      setSelectedSpecialization("");
    }
  };

  // Book appointment
  const bookAppointment = async (booking: AppointmentBooking) => {
    setIsLoading(true);
    setError(null);

    try {
      const result = await schedulerApi.bookAppointment(booking);

      if (result.success) {
        // Refresh time slots after booking
        if (selectedDoctor) {
          const slotsResult = await schedulerApi.getTimeSlots(selectedDoctor);
          if (slotsResult.success) {
            setTimeSlots(slotsResult.data);
          }
        }

        return result.data;
      } else {
        throw new Error(result.error);
      }
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to book appointment";
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  // Reset selections
  const resetSelections = () => {
    setSelectedService("");
    setSelectedSpecialization("");
    setSelectedDoctor("");
    setTimeSlots([]);
    setError(null);
  };

  return {
    // Data
    services,
    specializations,
    doctors,
    timeSlots,
    events,

    // Selections
    selectedService,
    selectedSpecialization,
    selectedDoctor,

    // State
    isLoading,
    error,

    // Actions
    handleServiceChange,
    handleSpecializationChange,
    handleDoctorChange,
    bookAppointment,
    resetSelections,
  };
};
