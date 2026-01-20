import { useCallback, useEffect, useState } from "react";
import {
  Patient,
  Prescription,
  PrescriptionFilter,
  PrescriptionFormData,
} from "@features/prescriptions/types";
import { useAuth } from "@shared/auth/AuthContext";
import { patientsApi } from "@shared/services/patientsApi";
import { prescriptionsApi } from "@shared/services/prescriptionsApi";

export const usePrescriptions = () => {
  const { user } = useAuth();
  const [prescriptions, setPrescriptions] = useState<Prescription[]>([]);
  const [patients, setPatients] = useState<Patient[]>([]);
  const [selectedPrescription, setSelectedPrescription] =
    useState<Prescription | null>(null);
  const [filters, setFilters] = useState<PrescriptionFilter>({});
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const fetchPrescriptions = useCallback(async () => {
    try {
      setError(null);
      setIsLoading(true);
      const data = await prescriptionsApi.getPrescriptions(filters);
      setPrescriptions(data);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to fetch prescriptions"
      );
    } finally {
      setIsLoading(false);
    }
  }, [filters]);

  const fetchPatients = useCallback(async () => {
    try {
      if (!user?.id) return;

      const data = await patientsApi.getPatients(user.id);

      const mappedPatients: Patient[] = data.map((p) => {
        const today = new Date();
        const year = today.getFullYear() - (p.age || 0);
        const approximateDob = new Date(year, 0, 1);

        return {
          id: p.id,
          name: p.name,
          email: p.email || "",
          phone: p.phone || "",
          dateOfBirth: approximateDob,
          allergies: [],
          medicalHistory: [],
        };
      });
      setPatients(mappedPatients);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch patients");
    }
  }, [user?.id]);

  const createPrescription = useCallback(async (data: PrescriptionFormData) => {
    try {
      setError(null);
      setIsLoading(true);
      const newPrescription = await prescriptionsApi.createPrescription(data);
      setPrescriptions((prev) => [newPrescription, ...prev]);
      return newPrescription;
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to create prescription"
      );
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const updatePrescription = useCallback(
    async (id: string, data: Partial<PrescriptionFormData>) => {
      try {
        setError(null);
        setIsLoading(true);
        const updated = await prescriptionsApi.updatePrescription(id, data);

        setPrescriptions((prev) =>
          prev.map((prescription) =>
            prescription.id === id ? updated : prescription
          )
        );

        if (selectedPrescription?.id === id) {
          setSelectedPrescription(updated);
        }
        return updated;
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "Failed to update prescription"
        );
        throw err;
      } finally {
        setIsLoading(false);
      }
    },
    [selectedPrescription?.id]
  );

  const deletePrescription = useCallback(
    async (id: string) => {
      try {
        setError(null);
        setIsLoading(true);
        await prescriptionsApi.deletePrescription(id);

        setPrescriptions((prev) =>
          prev.filter((prescription) => prescription.id !== id)
        );
        if (selectedPrescription?.id === id) {
          setSelectedPrescription(null);
        }
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "Failed to delete prescription"
        );
        throw err;
      } finally {
        setIsLoading(false);
      }
    },
    [selectedPrescription?.id]
  );

  const updateFilters = useCallback((newFilters: PrescriptionFilter) => {
    setFilters((prev) => ({ ...prev, ...newFilters }));
  }, []);

  const clearFilters = useCallback(() => {
    setFilters({});
  }, []);

  const refreshData = useCallback(async () => {
    await Promise.all([fetchPrescriptions(), fetchPatients()]);
  }, [fetchPrescriptions, fetchPatients]);

  // Initial data load
  useEffect(() => {
    refreshData();
  }, [refreshData]);

  return {
    // State
    prescriptions,
    patients,
    selectedPrescription,
    filters,
    error,
    isLoading,

    // Actions
    createPrescription,
    updatePrescription,
    deletePrescription,
    setSelectedPrescription,
    updateFilters,
    clearFilters,
    refreshData,

    // Helpers
    clearError: () => setError(null),
  };
};
