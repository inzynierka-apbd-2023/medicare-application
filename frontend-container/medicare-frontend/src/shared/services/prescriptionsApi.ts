import {
  Doctor,
  Patient,
  Prescription,
  PrescriptionFilter,
  PrescriptionFormData,
} from "../../features/prescriptions/types";
import { toastMessages } from "../toast/toastMessages";

import { api } from "./api";
import { staffApi } from "./staffApi";

interface BackendPrescription {
  id: string;
  medicalRecordId: string;
  patientId: string;
  doctorId: string;
  medicationName: string;
  atcCode?: string;
  dosage: string;
  frequency: string;
  durationDays: number;
  instructions?: string;
  prescribedDate: string;
  status: string;
  createdAt: string;
  updatedAt: string;
}

const mapBackendToFrontend = (bp: BackendPrescription): Prescription => {
  const prescribedDate = new Date(bp.prescribedDate);
  const validUntil = new Date(prescribedDate);
  validUntil.setDate(validUntil.getDate() + bp.durationDays);

  return {
    id: bp.id,
    patientId: bp.patientId,
    doctorId: bp.doctorId,
    medications: [
      {
        id: `med-${bp.id}`,
        name: bp.medicationName,
        atcCode: bp.atcCode,
        genericName: bp.medicationName,
        dosage: bp.dosage,
        frequency: bp.frequency,
        duration: `${bp.durationDays} days`,
        instructions: bp.instructions || "",
        quantity: bp.durationDays,
        unit: "doses",
        refills: 0,
        isGenericAllowed: true,
      },
    ],
    diagnosis: bp.instructions || "See prescription notes",
    notes: bp.instructions || "",
    status: bp.status.toLowerCase() as Prescription["status"],
    createdAt: new Date(bp.createdAt),
    updatedAt: new Date(bp.updatedAt),
    validUntil,
    issuedAt: prescribedDate,
  };
};

class PrescriptionsApi {
  async getPrescriptions(
    filters: PrescriptionFilter = {}
  ): Promise<Prescription[]> {
    const params = new URLSearchParams();
    if (filters.patientId) params.append("patientId", filters.patientId);
    if (filters.doctorId) params.append("doctorId", filters.doctorId);
    if (filters.status) params.append("status", filters.status);
    if (filters.searchTerm) params.append("search", filters.searchTerm);

    const queryString = params.toString();
    const querySuffix = queryString ? `?${queryString}` : "";
    const url = `/medical-records/prescriptions${querySuffix}`;

    const response = await api.get<BackendPrescription[]>(url);
    return response.map(mapBackendToFrontend);
  }

  async getPrescriptionById(id: string): Promise<Prescription | null> {
    const response = await api.get<BackendPrescription>(
      `/medical-records/prescriptions/${id}`
    );
    return mapBackendToFrontend(response);
  }

  async createPrescription(data: PrescriptionFormData): Promise<Prescription> {
    const responses: BackendPrescription[] = [];

    const medications = data.medications || [];

    if (medications.length === 0) {
      throw new Error("No medications to save");
    }

    for (let i = 0; i < medications.length; i++) {
      const medication = medications[i];
      if (!medication.name) continue;

      const payload = {
        medicalRecordId: "00000000-0000-0000-0000-000000000000",
        patientId: data.patientId,
        doctorId: "00000000-0000-0000-0000-000000000000",
        medicationName: medication.name,
        atcCode: medication.atcCode || null,
        dosage: medication.dosage,
        frequency: medication.frequency,
        durationDays: Number.parseInt(medication.duration, 10) || 30,
        instructions: data.notes || medication.instructions,
        prescribedDate: new Date().toISOString(),
      };

      const isLast = i === medications.length - 1;

      const response = await api.post<BackendPrescription>(
        "/medical-records/prescriptions",
        payload,
        undefined,
        {
          showToastOnSuccess: isLast,
          successMessage: toastMessages.prescriptions.createSuccess,
        }
      );
      responses.push(response);
    }

    if (responses.length === 0) {
      throw new Error("No medications were saved");
    }
    return mapBackendToFrontend(responses[0]);
  }

  async updatePrescription(
    id: string,
    data: Partial<PrescriptionFormData>
  ): Promise<Prescription> {
    const medications = data.medications || [];

    if (medications.length === 0) {
      throw new Error("No medications to save");
    }

    const firstMed = medications[0];
    const payload = {
      medicationName: firstMed?.name || "",
      dosage: firstMed?.dosage || "",
      frequency: firstMed?.frequency || "",
      durationDays: firstMed?.duration
        ? Number.parseInt(firstMed.duration, 10)
        : 30,
      instructions: data.notes || firstMed?.instructions,
      status: null,
    };

    const hasMoreMeds = medications.length > 1;

    const response = await api.put<BackendPrescription>(
      `/medical-records/prescriptions/${id}`,
      payload,
      undefined,
      {
        showToastOnSuccess: !hasMoreMeds,
        successMessage: toastMessages.prescriptions.updateSuccess,
      }
    );

    for (let i = 1; i < medications.length; i++) {
      const med = medications[i];
      if (!med.name) continue;

      const newPayload = {
        medicalRecordId: "00000000-0000-0000-0000-000000000000",
        patientId: data.patientId || "",
        doctorId: "00000000-0000-0000-0000-000000000000",
        medicationName: med.name,
        atcCode: med.atcCode || null,
        dosage: med.dosage,
        frequency: med.frequency,
        durationDays: Number.parseInt(med.duration, 10) || 30,
        instructions: data.notes || med.instructions,
        prescribedDate: new Date().toISOString(),
      };

      const isLast = i === medications.length - 1;

      await api.post<BackendPrescription>(
        "/medical-records/prescriptions",
        newPayload,
        undefined,
        {
          showToastOnSuccess: isLast,
          successMessage: toastMessages.prescriptions.updateSuccess,
        }
      );
    }

    return mapBackendToFrontend(response);
  }

  async deletePrescription(id: string): Promise<void> {
    await api.delete(`/medical-records/prescriptions/${id}`, undefined, {
      showToastOnSuccess: true,
      successMessage: toastMessages.prescriptions.deleteSuccess,
    });
  }

  async getPatients(): Promise<Patient[]> {
    return [];
  }

  async getDoctors(): Promise<Doctor[]> {
    const doctors = await staffApi.getStaff({ role: "Doctor" });
    if (doctors) {
      interface DoctorExtended {
        specializations?: Array<{ name: string }>;
        licenseNumber?: string;
      }
      return doctors
        .filter((s) => s.role === "Doctor")
        .map((doc) => {
          const docExtended = doc as typeof doc & DoctorExtended;
          return {
            id: doc.id,
            name: `Dr. ${doc.profile.firstName} ${doc.profile.lastName}`,
            specialization:
              docExtended.specializations?.[0]?.name || "General Practice",
            licenseNumber: docExtended.licenseNumber || "",
            email: doc.profile.email,
            phone: doc.profile.phone || "",
          };
        });
    }
    return [];
  }

  async getPharmacies(): Promise<
    {
      id: string;
      name: string;
      address: string;
      phone: string;
      email: string;
    }[]
  > {
    return [];
  }

  async updatePrescriptionStatus(
    id: string,
    status: Prescription["status"]
  ): Promise<Prescription> {
    const response = await api.put<BackendPrescription>(
      `/medical-records/prescriptions/${id}/status`,
      { status: status.charAt(0).toUpperCase() + status.slice(1) },
      undefined,
      {
        showToastOnSuccess: true,
        successMessage: toastMessages.prescriptions.statusUpdateSuccess,
      }
    );
    return mapBackendToFrontend(response);
  }

  async generatePrescriptionPDF(id: string): Promise<Blob> {
    const pdfContent = `Prescription ID: ${id}\nGenerated on: ${new Date().toISOString()}`;
    return new Blob([pdfContent], { type: "application/pdf" });
  }
}

export const prescriptionsApi = new PrescriptionsApi();
