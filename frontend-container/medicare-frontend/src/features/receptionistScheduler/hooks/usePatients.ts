import { useCallback, useEffect, useState } from "react";
import { ReceptionistSchedulerApiService } from "@features/receptionistScheduler/services/receptionistSchedulerApiService";
import type { Patient } from "@features/receptionistScheduler/types";

export const usePatients = () => {
  const [patients, setPatients] = useState<Patient[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadPatients = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);

      const patientsData = await ReceptionistSchedulerApiService.getPatients();
      setPatients(patientsData);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "An unexpected error occurred"
      );
    } finally {
      setIsLoading(false);
    }
  }, []);

  const searchPatients = useCallback(
    async (query: string): Promise<Patient[]> => {
      try {
        setError(null);

        if (query.trim().length === 0) {
          return patients;
        }

        const searchResults =
          await ReceptionistSchedulerApiService.searchPatients(query);
        return searchResults;
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "An unexpected error occurred"
        );
        return [];
      }
    },
    [patients]
  );

  // Initial load
  useEffect(() => {
    loadPatients();
  }, [loadPatients]);

  return {
    patients,
    isLoading,
    error,
    loadPatients,
    searchPatients,
  };
};
