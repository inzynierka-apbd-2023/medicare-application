import { useCallback, useEffect, useState } from "react";

import { useLoadingService } from "../../../shared/hooks/useLoadingService";
import {
  Patient,
  Prescription,
  PrescriptionFilter,
  PrescriptionFormData,
} from "../types";

// Mock data for now
const mockPrescriptions: Prescription[] = [
  {
    id: "rx1",
    patientId: "1",
    doctorId: "doc1",
    medications: [
      {
        id: "med1",
        name: "Lisinopril",
        genericName: "Lisinopril",
        dosage: "10mg",
        frequency: "Once daily",
        duration: "30 days",
        instructions: "Take with or without food",
        quantity: 30,
        unit: "tablets",
        refills: 5,
        isGenericAllowed: true,
      },
    ],
    diagnosis: "Hypertension",
    notes: "Monitor blood pressure regularly",
    status: "active",
    createdAt: new Date("2024-01-15"),
    updatedAt: new Date("2024-01-15"),
    validUntil: new Date("2024-07-15"),
    issuedAt: new Date("2024-01-15"),
  },
];

const mockPatients: Patient[] = [
  {
    id: "1",
    name: "John Doe",
    email: "john.doe@email.com",
    phone: "+1-555-0123",
    dateOfBirth: new Date("1985-03-15"),
    allergies: ["Penicillin", "Shellfish"],
    medicalHistory: ["Hypertension", "Type 2 Diabetes"],
  },
];

export const usePrescriptions = () => {
  const [prescriptions, setPrescriptions] =
    useState<Prescription[]>(mockPrescriptions);
  const [patients, setPatients] = useState<Patient[]>(mockPatients);
  const [selectedPrescription, setSelectedPrescription] =
    useState<Prescription | null>(null);
  const [filters, setFilters] = useState<PrescriptionFilter>({});
  const [error, setError] = useState<string | null>(null);
  const { isLoading } = useLoadingService();

  const fetchPrescriptions = useCallback(async () => {
    try {
      setError(null);
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 300));
      setPrescriptions(mockPrescriptions);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to fetch prescriptions"
      );
    }
  }, []);

  const fetchPatients = useCallback(async () => {
    try {
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 200));
      setPatients(mockPatients);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch patients");
    }
  }, []);

  const createPrescription = useCallback(async (data: PrescriptionFormData) => {
    try {
      setError(null);
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 500));

      const newPrescription: Prescription = {
        id: `rx${Date.now()}`,
        patientId: data.patientId,
        doctorId: "doc1",
        medications: data.medications.map((med, index) => ({
          id: `med${Date.now()}_${index}`,
          ...med,
        })),
        diagnosis: data.diagnosis,
        ...(data.notes && { notes: data.notes }),
        status: "active",
        createdAt: new Date(),
        updatedAt: new Date(),
        validUntil: data.validUntil,
        issuedAt: new Date(),
      };

      setPrescriptions((prev) => [newPrescription, ...prev]);
      return newPrescription;
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to create prescription"
      );
      throw err;
    }
  }, []);

  const updatePrescription = useCallback(
    async (id: string, data: Partial<PrescriptionFormData>) => {
      try {
        setError(null);
        // Simulate API call
        await new Promise((resolve) => setTimeout(resolve, 500));

        const updatedPrescription = prescriptions.find((p) => p.id === id);
        if (!updatedPrescription) throw new Error("Prescription not found");

        const updated: Prescription = {
          ...updatedPrescription,
          ...data,
          medications: data.medications
            ? data.medications.map((med, index) => ({
                id: `med${Date.now()}_${index}`,
                ...med,
              }))
            : updatedPrescription.medications,
          updatedAt: new Date(),
        };

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
      }
    },
    [prescriptions, selectedPrescription?.id]
  );

  const deletePrescription = useCallback(
    async (id: string) => {
      try {
        setError(null);
        // Simulate API call
        await new Promise((resolve) => setTimeout(resolve, 300));

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
