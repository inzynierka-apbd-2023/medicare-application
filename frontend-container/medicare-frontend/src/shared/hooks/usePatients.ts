import { useCallback, useEffect, useState } from "react";
import type { Patient } from "@features/userTypes/types";
import { patientsApi } from "@shared/services/patientsApi";

interface UsePatientsResult {
  patients: Patient[];
  isLoading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

export const usePatients = (doctorId?: string): UsePatientsResult => {
  const [patients, setPatients] = useState<Patient[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchPatients = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const data = await patientsApi.getPatients(doctorId);
      setPatients(data);
    } catch (err) {
      const message =
        err instanceof Error
          ? err.message
          : "An error occurred while loading patients";
      setError(message);
    } finally {
      setIsLoading(false);
    }
  }, [doctorId]);

  useEffect(() => {
    fetchPatients();
  }, [fetchPatients]);

  return {
    patients,
    isLoading,
    error,
    refetch: fetchPatients,
  };
};
