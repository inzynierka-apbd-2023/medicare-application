import { toastMessages } from "../toast/toastMessages";

import { handleApiCall } from "./api";
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
  medicalRecordId: string;
  icd10Code: string;
  description: string;
  type: string;
  status?: string;
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

export interface MedicalRecordsApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

export const medicalRecordsApi = {
  getPatientHistory: async (
    patientId: string
  ): Promise<MedicalRecordsApiResponse<BackendPatientHistory | null>> => {
    const result = await handleApiCall(
      () =>
        apiClient
          .get<BackendPatientHistory>(
            `/medical-records/records/patient-history/${patientId}`
          )
          .then((res) => res.data),
      { showToastOnError: false }
    );

    if (result.success) {
      return { success: true, data: result.data };
    }

    if (result.status === 404) {
      return { success: true, data: null };
    }

    return {
      success: false,
      data: null,
      message: result.error || toastMessages.medicalRecords.fetchHistoryError,
    };
  },
};
