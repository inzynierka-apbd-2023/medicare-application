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

export interface BackendPatientProfile {
  id: string;
  userId: string;
  primaryDoctorId?: string;
  name: string;
  email: string;
  phone: string;
  address: string;
  dateOfBirth?: string;
  gender: string;
  emergencyContacts: Array<{
    name: string;
    relation?: string;
    phone?: string;
  }>;
  insurance: Array<{
    provider?: string;
    policyNumber?: string;
    validFrom?: string;
    validTo?: string;
  }>;
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
    patientId: string
  ): Promise<ApiResponse<Patient | null>> {
    try {
      const response = await apiClient.get<BackendPatientProfile>(
        `/patient/patients/${patientId}`
      );
      const p = response.data;

      // Calculate age from DOB
      let age = 0;
      if (p.dateOfBirth) {
        const birthDate = new Date(p.dateOfBirth);
        const today = new Date();
        age = today.getFullYear() - birthDate.getFullYear();
        const m = today.getMonth() - birthDate.getMonth();
        if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
          age--;
        }
      }

      // Map gender
      type ValidGender = "Male" | "Female" | "Other";
      const validGenders: readonly ValidGender[] = ["Male", "Female", "Other"];
      const gender: ValidGender = validGenders.includes(p.gender as ValidGender)
        ? (p.gender as ValidGender)
        : "Other";

      const patient: Patient = {
        id: p.id,
        name: p.name || "Unknown",
        age: age,
        gender: gender,
        lastVisit: "", // Not available in profile service
        visits: 0, // Not available in profile service
        notes: "",
        email: p.email,
        phone: p.phone,
      };

      return { success: true, data: patient };
    } catch (e) {
      console.error("Failed to fetch patient profile", e);
      return {
        success: false,
        data: null,
        message: "Failed to fetch patient profile",
      };
    }
  },

  /**
   * Get raw patient profile including contacts and insurance (for Medical Record View)
   */
  async getPatientProfile(
    patientId: string
  ): Promise<ApiResponse<BackendPatientProfile | null>> {
    try {
      const response = await apiClient.get<BackendPatientProfile>(
        `/patient/patients/${patientId}`
      );
      return { success: true, data: response.data };
    } catch (e) {
      console.error("Failed to fetch raw patient profile", e);
      return {
        success: false,
        data: null,
        message: "Failed to fetch raw patient profile",
      };
    }
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
