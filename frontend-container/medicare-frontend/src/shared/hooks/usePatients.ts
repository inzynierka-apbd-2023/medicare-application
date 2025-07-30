import { useEffect, useState } from "react";

import type { Patient } from "../../features/userTypes/types";
import { patientsApi } from "../services/patientsApi";

interface UsePatientsResult {
  patients: Patient[];
  isLoading: boolean;
  error: string | null;
  updatePatientNotes: (patientId: number, notes: string) => Promise<void>;
  refetch: () => Promise<void>;
}

export const usePatients = (doctorId?: string): UsePatientsResult => {
  const [patients, setPatients] = useState<Patient[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchPatients = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await patientsApi.getPatients(doctorId);

      if (response.success) {
        setPatients(response.data);
      } else {
        setError(response.message || "Failed to load patients");
      }
    } catch (err) {
      setError("An error occurred while loading patients");
      console.error("Patients fetch error:", err);
    } finally {
      setIsLoading(false);
    }
  };

  const updatePatientNotes = async (patientId: number, notes: string) => {
    try {
      const response = await patientsApi.updatePatientNotes(patientId, notes);

      if (response.success) {
        setPatients((prev) =>
          prev.map((patient) =>
            patient.id === patientId ? { ...patient, notes } : patient
          )
        );
      } else {
        throw new Error(response.message || "Failed to update patient notes");
      }
    } catch (err) {
      console.error("Patient notes update error:", err);
      throw err;
    }
  };

  useEffect(() => {
    fetchPatients();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [doctorId]);

  return {
    patients,
    isLoading,
    error,
    updatePatientNotes,
    refetch: fetchPatients,
  };
};
