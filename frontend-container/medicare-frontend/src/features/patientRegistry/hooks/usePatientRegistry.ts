import { useCallback, useEffect, useState } from "react";

import { PatientRegistryApiService } from "../services/patientRegistryApi";
import type {
  CreatePatientRequest,
  Doctor,
  PatientRegistryFilters,
  PatientRegistryInfo,
} from "../types";

interface UsePatientRegistryOptions {
  page?: number;
  limit?: number;
  filters?: PatientRegistryFilters;
  autoLoad?: boolean;
}

interface UsePatientRegistryReturn {
  // Data
  patients: PatientRegistryInfo[];
  doctors: Doctor[];
  totalCount: number;
  currentPage: number;
  totalPages: number;

  // Loading states
  isLoading: boolean;
  isCreating: boolean;
  isUpdating: boolean;
  isDeleting: boolean;

  // Error states
  error: string | null;

  // Actions
  loadPatients: (
    page?: number,
    filters?: PatientRegistryFilters
  ) => Promise<void>;
  loadDoctors: () => Promise<void>;
  createPatient: (
    patientData: CreatePatientRequest
  ) => Promise<PatientRegistryInfo | null>;
  updatePatient: (
    patientId: string,
    patientData: Partial<PatientRegistryInfo>
  ) => Promise<PatientRegistryInfo | null>;
  deletePatient: (patientId: string) => Promise<boolean>;
  getPatient: (patientId: string) => Promise<PatientRegistryInfo | null>;
  checkEmailAvailability: (email: string) => Promise<boolean>;
  refetch: () => Promise<void>;
  clearError: () => void;
}

export const usePatientRegistry = ({
  page = 1,
  limit = 10,
  filters,
  autoLoad = true,
}: UsePatientRegistryOptions = {}): UsePatientRegistryReturn => {
  // State
  const [patients, setPatients] = useState<PatientRegistryInfo[]>([]);
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(page);
  const [totalPages, setTotalPages] = useState(0);

  // Loading states
  const [isLoading, setIsLoading] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [isUpdating, setIsUpdating] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  // Error state
  const [error, setError] = useState<string | null>(null);

  // Clear error
  const clearError = useCallback(() => {
    setError(null);
  }, []);

  // Load patients
  const loadPatients = useCallback(
    async (pageNum = page, appliedFilters = filters) => {
      try {
        setIsLoading(true);
        setError(null);

        const response = await PatientRegistryApiService.getPatients(
          pageNum,
          limit,
          appliedFilters
        );

        if (response.success) {
          setPatients(response.data.patients);
          setTotalCount(response.data.totalCount);
          setCurrentPage(response.data.currentPage);
          setTotalPages(response.data.totalPages);
        } else {
          setError(response.message || "Failed to load patients");
        }
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "Failed to load patients"
        );
      } finally {
        setIsLoading(false);
      }
    },
    [page, limit, filters]
  );

  // Load doctors
  const loadDoctors = useCallback(async () => {
    try {
      const response = await PatientRegistryApiService.getDoctors();

      if (response.success) {
        setDoctors(response.data);
      } else {
        console.error("Failed to load doctors:", response.message);
      }
    } catch (err) {
      console.error("Failed to load doctors:", err);
    }
  }, []);

  // Create patient
  const createPatient = useCallback(
    async (
      patientData: CreatePatientRequest
    ): Promise<PatientRegistryInfo | null> => {
      try {
        setIsCreating(true);
        setError(null);

        const response =
          await PatientRegistryApiService.createPatient(patientData);

        if (response.success) {
          // Refresh patient list
          await loadPatients(1, filters);
          return response.data;
        } else {
          setError(response.message || "Failed to create patient");
          return null;
        }
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to create patient";
        setError(errorMessage);
        return null;
      } finally {
        setIsCreating(false);
      }
    },
    [loadPatients, filters]
  );

  // Update patient
  const updatePatient = useCallback(
    async (
      patientId: string,
      patientData: Partial<PatientRegistryInfo>
    ): Promise<PatientRegistryInfo | null> => {
      try {
        setIsUpdating(true);
        setError(null);

        const response = await PatientRegistryApiService.updatePatient(
          patientId,
          patientData
        );

        if (response.success) {
          // Update local state
          setPatients((prev) =>
            prev.map((p) => (p.id === patientId ? response.data : p))
          );
          return response.data;
        } else {
          setError(response.message || "Failed to update patient");
          return null;
        }
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to update patient";
        setError(errorMessage);
        return null;
      } finally {
        setIsUpdating(false);
      }
    },
    []
  );

  // Delete patient
  const deletePatient = useCallback(
    async (patientId: string): Promise<boolean> => {
      try {
        setIsDeleting(true);
        setError(null);

        const response =
          await PatientRegistryApiService.deletePatient(patientId);

        if (response.success) {
          // Remove from local state
          setPatients((prev) => prev.filter((p) => p.id !== patientId));
          return true;
        } else {
          setError(response.message || "Failed to delete patient");
          return false;
        }
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to delete patient";
        setError(errorMessage);
        return false;
      } finally {
        setIsDeleting(false);
      }
    },
    []
  );

  // Get single patient
  const getPatient = useCallback(
    async (patientId: string): Promise<PatientRegistryInfo | null> => {
      try {
        const response = await PatientRegistryApiService.getPatient(patientId);

        if (response.success) {
          return response.data;
        } else {
          setError(response.message || "Patient not found");
          return null;
        }
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to get patient";
        setError(errorMessage);
        return null;
      }
    },
    []
  );

  // Check email availability
  const checkEmailAvailability = useCallback(
    async (email: string): Promise<boolean> => {
      try {
        const response =
          await PatientRegistryApiService.checkEmailAvailability(email);

        if (response.success) {
          return response.data.available;
        } else {
          return false;
        }
      } catch (err) {
        console.error("Failed to check email availability:", err);
        return false;
      }
    },
    []
  );

  // Refetch current data
  const refetch = useCallback(async () => {
    await loadPatients(currentPage, filters);
  }, [loadPatients, currentPage, filters]);

  // Auto load on mount and when dependencies change
  useEffect(() => {
    if (autoLoad) {
      loadPatients(page, filters);
    }
  }, [autoLoad, loadPatients, page, filters]);

  // Load doctors once on mount
  useEffect(() => {
    if (autoLoad) {
      loadDoctors();
    }
  }, [autoLoad, loadDoctors]);

  return {
    // Data
    patients,
    doctors,
    totalCount,
    currentPage,
    totalPages,

    // Loading states
    isLoading,
    isCreating,
    isUpdating,
    isDeleting,

    // Error state
    error,

    // Actions
    loadPatients,
    loadDoctors,
    createPatient,
    updatePatient,
    deletePatient,
    getPatient,
    checkEmailAvailability,
    refetch,
    clearError,
  };
};
