import type { Patient } from "../../features/userTypes/types";

import { api } from "./api";

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

function mapBackendToPatient(p: BackendPatient): Patient {
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
  async getPatients(doctorId?: string): Promise<Patient[]> {
    if (!doctorId) {
      throw new Error("Doctor ID is required");
    }

    const response = await api.get<DoctorPatientsResponse>(
      `/appointment/doctor-patients/${doctorId}`,
      undefined,
      {
        showToastOnError: true,
        showToastOnSuccess: false,
      }
    );

    return response.patients.map(mapBackendToPatient);
  },

  async getPatientById(patientId: string): Promise<Patient | null> {
    const p = await api.get<BackendPatientProfile>(
      `/patient/patients/${patientId}`,
      undefined,
      {
        showToastOnError: true,
        showToastOnSuccess: false,
      }
    );

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

    return patient;
  },

  async getPatientProfile(
    patientId: string
  ): Promise<BackendPatientProfile | null> {
    const response = await api.get<BackendPatientProfile>(
      `/patient/patients/${patientId}`,
      undefined,
      {
        showToastOnError: true,
        showToastOnSuccess: false,
      }
    );
    return response;
  },
};
