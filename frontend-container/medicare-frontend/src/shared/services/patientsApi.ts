import type { Patient } from "../../features/userTypes/types";

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

// Mock patient data - in real app this would connect to actual API
const mockPatients: Patient[] = [
  {
    id: 1,
    name: "John Doe",
    age: 45,
    gender: "Male",
    lastVisit: "2025-05-18",
    visits: 4,
    notes: "High cholesterol, regular check-ups.",
    email: "john.doe@example.com",
    phone: "+1234567890",
  },
  {
    id: 2,
    name: "Maria Smith",
    age: 33,
    gender: "Female",
    lastVisit: "2025-05-12",
    visits: 2,
    notes: "Post-surgery recovery.",
    email: "maria.smith@example.com",
    phone: "+1234567891",
  },
  {
    id: 3,
    name: "Adam Nowak",
    age: 52,
    gender: "Male",
    lastVisit: "2025-04-29",
    visits: 8,
    notes: "Diabetic, hypertension.",
    email: "adam.nowak@example.com",
    phone: "+1234567892",
  },
  {
    id: 4,
    name: "Paulina Zielińska",
    age: 29,
    gender: "Female",
    lastVisit: "2025-03-10",
    visits: 1,
    notes: "",
    email: "paulina.z@example.com",
    phone: "+1234567893",
  },
];

export const patientsApi = {
  async getPatients(_doctorId?: string): Promise<ApiResponse<Patient[]>> {
    // Simulate API delay
    await new Promise((resolve) => setTimeout(resolve, 800));

    return {
      success: true,
      data: mockPatients,
    };
  },

  async getPatientById(
    patientId: number
  ): Promise<ApiResponse<Patient | null>> {
    // Simulate API delay
    await new Promise((resolve) => setTimeout(resolve, 300));

    const patient = mockPatients.find((p) => p.id === patientId);

    return {
      success: true,
      data: patient || null,
    };
  },

  async updatePatientNotes(
    patientId: number,
    notes: string
  ): Promise<ApiResponse<Patient>> {
    // Simulate API delay
    await new Promise((resolve) => setTimeout(resolve, 500));

    const patientIndex = mockPatients.findIndex((p) => p.id === patientId);
    if (patientIndex === -1) {
      return {
        success: false,
        data: mockPatients[0], // fallback
        message: "Patient not found",
      };
    }

    mockPatients[patientIndex].notes = notes;

    return {
      success: true,
      data: mockPatients[patientIndex],
    };
  },
};
