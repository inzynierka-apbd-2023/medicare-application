import { apiClient } from "./apiClient";

export interface BackendMedicalRecord {
  id: string;
  patientId: string;
  doctorId: string;
  appointmentId?: string;
  visitDate: string;
  chiefComplaint?: string;
  historyOfPresentIllness?: string;
  physicalExamination?: string;
  assessment?: string;
  plan?: string;
  notes?: string;
}

export interface BackendDiagnosis {
  id: string;
  medicalRecordId: string; // Visits linkage
  icd10Code: string;
  description: string;
  type: string; // Primary/Secondary
  status?: string; // Not in backend entity explicitly but maybe we can infer
  createdAt: string;
}

export interface BackendPrescription {
  id: string;
  medicalRecordId: string;
  patientId: string;
  doctorId: string;
  medicationName: string;
  dosage: string;
  frequency: string;
  durationDays?: number;
  instructions?: string;
  prescribedDate: string;
  status: string;
}

export interface BackendVitalSigns {
  id: string;
  medicalRecordId: string;
  patientId: string;
  measuredAt: string;
  temperature?: number;
  systolicBP?: number;
  diastolicBP?: number;
  heartRate?: number;
  respiratoryRate?: number;
  oxygenSaturation?: number;
  height?: number;
  weight?: number;
}

export interface BackendPatientHistory {
  patientId: string;
  records: BackendMedicalRecord[];
  conditions: BackendDiagnosis[];
  medications: BackendPrescription[];
  vitals: BackendVitalSigns[];
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

export const medicalRecordsApi = {
  async getPatientHistory(
    patientId: string
  ): Promise<ApiResponse<BackendPatientHistory | null>> {
    try {
      const response = await apiClient.get<BackendPatientHistory>(
        `/medical-records/records/patient-history/${patientId}`
      );
      return { success: true, data: response.data };
    } catch (e: unknown) {
      // If 404, it means no history found, which is valid for new patients.
      const axiosError = e as { response?: { status?: number } };
      if (axiosError.response && axiosError.response.status === 404) {
        return { success: true, data: null };
      }
      // Suppress console log as per user request
      return {
        success: false,
        data: null,
        message: "Failed to fetch patient history",
      };
    }
  },
};
