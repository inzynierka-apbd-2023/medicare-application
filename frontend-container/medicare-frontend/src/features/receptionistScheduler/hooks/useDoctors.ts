import { useCallback, useEffect, useState } from "react";
import { ReceptionistSchedulerApiService } from "@features/receptionistScheduler/services/receptionistSchedulerApiService";
import type { Doctor, TimeSlot } from "@features/receptionistScheduler/types";

export const useDoctors = () => {
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadDoctors = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);

      const doctorsData = await ReceptionistSchedulerApiService.getDoctors();
      setDoctors(doctorsData);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "An unexpected error occurred"
      );
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Initial load
  useEffect(() => {
    loadDoctors();
  }, [loadDoctors]);

  return {
    doctors,
    isLoading,
    error,
    loadDoctors,
  };
};

export const useDoctorAvailability = (doctorId?: string, date?: string) => {
  const [timeSlots, setTimeSlots] = useState<TimeSlot[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadAvailability = useCallback(
    async (docId: string, selectedDate: string) => {
      try {
        setIsLoading(true);
        setError(null);

        const availability =
          await ReceptionistSchedulerApiService.getDoctorAvailability(
            docId,
            selectedDate
          );
        setTimeSlots(availability);
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "An unexpected error occurred"
        );
      } finally {
        setIsLoading(false);
      }
    },
    []
  );

  useEffect(() => {
    if (doctorId && date) {
      loadAvailability(doctorId, date);
    }
  }, [doctorId, date, loadAvailability]);

  return {
    timeSlots,
    isLoading,
    error,
    loadAvailability,
  };
};
