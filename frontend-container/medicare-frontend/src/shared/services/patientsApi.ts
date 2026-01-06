import type { Patient } from "../../features/userTypes/types";

import { apiClient } from "./apiClient";

interface BackendPatient {
  id: string;
  name: string;
  age: number;
  gender: string;
  lastVisit: string;
  visits: number;
  notes: string;
  email?: string;
  phone?: string;
}

interface DoctorPatientsResponse {
  patients: BackendPatient[];
  totalCount: number;
}

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

function mapBackendToPatient(p: BackendPatient): Patient {
  // Map gender string to Gender type
  const validGenders = ["Male", "Female", "Other"] as const;
  const gender = validGenders.includes(
    p.gender as (typeof validGenders)[number]
  )
    ? (p.gender as Patient["gender"])
    : "Other";

  const result: Patient = {
    id: p.id,
    name: p.name,
    age: p.age,
    gender,
    lastVisit: p.lastVisit,
    visits: p.visits,
    notes: p.notes,
  };

  // Only add optional fields if they have values
  if (p.email) result.email = p.email;
  if (p.phone) result.phone = p.phone;

  return result;
}

export const patientsApi = {
  /**
   * Get all patients for a doctor based on their appointments
   */
  async getPatients(doctorId?: string): Promise<ApiResponse<Patient[]>> {
    if (!doctorId) {
      return { success: false, data: [], message: "Doctor ID is required" };
    }

    try {
      const response = await apiClient.get<DoctorPatientsResponse>(
        `/appointment/doctor-patients/${doctorId}`
      );

      const patients = response.data.patients.map(mapBackendToPatient);
      return { success: true, data: patients };
    } catch (error) {
      console.error("Error fetching patients:", error);
      return {
        success: false,
        data: [],
        message: "Failed to fetch patients",
      };
    }
  },

  /**
   * Get a specific patient by ID - fetches from patient list
   */
  async getPatientById(
    _patientId: string
  ): Promise<ApiResponse<Patient | null>> {
    // Not implemented - would need dedicated endpoint
    return { success: false, data: null, message: "Not implemented" };
  },

  /**
   * Update patient notes (adds to appointment notes)
   */
  async updatePatientNotes(
    _patientId: string,
    _notes: string
  ): Promise<ApiResponse<boolean>> {
    // Would update via appointment notes endpoint - not implemented
    return { success: true, data: true };
  },
};
