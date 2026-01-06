import {
  Doctor,
  Patient,
  Prescription,
  PrescriptionFilter,
  PrescriptionFormData,
} from "../../features/prescriptions/types";

import { apiClient } from "./apiClient";
import { staffApi } from "./staffApi";

// Backend prescription model (single medication per row)
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

// Map backend prescription to frontend format
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
    try {
      const params = new URLSearchParams();
      if (filters.patientId) params.append("patientId", filters.patientId);
      if (filters.doctorId) params.append("doctorId", filters.doctorId);
      if (filters.status) params.append("status", filters.status);
      if (filters.searchTerm) params.append("search", filters.searchTerm);

      const queryString = params.toString();
      const url = `/medical/prescriptions${queryString ? `?${queryString}` : ""}`;

      const response = await apiClient.get<BackendPrescription[]>(url);
      return response.data.map(mapBackendToFrontend);
    } catch (error) {
      console.error("Failed to fetch prescriptions:", error);
      return [];
    }
  }

  async getPrescriptionById(id: string): Promise<Prescription | null> {
    try {
      const response = await apiClient.get<BackendPrescription>(
        `/medical/prescriptions/${id}`
      );
      return mapBackendToFrontend(response.data);
    } catch (error) {
      console.error("Failed to fetch prescription:", error);
      return null;
    }
  }

  async createPrescription(data: PrescriptionFormData): Promise<Prescription> {
    // Create a prescription for each medication since backend stores one medication per row
    const responses: BackendPrescription[] = [];

    for (const medication of data.medications) {
      if (!medication.name) continue; // Skip empty medications

      const payload = {
        medicalRecordId: "00000000-0000-0000-0000-000000000000", // Placeholder - should be real
        patientId: data.patientId,
        doctorId: "00000000-0000-0000-0000-000000000000", // Will be set by auth context
        medicationName: medication.name,
        atcCode: null,
        dosage: medication.dosage,
        frequency: medication.frequency,
        durationDays: parseInt(medication.duration) || 30,
        instructions: data.notes || medication.instructions,
        prescribedDate: new Date().toISOString(),
      };

      const response = await apiClient.post<BackendPrescription>(
        "/medical/prescriptions",
        payload
      );
      responses.push(response.data);
    }

    // Return the first prescription for backward compatibility
    if (responses.length === 0) {
      throw new Error("No medications to save");
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

    // Update the existing prescription with the first medication
    const firstMed = medications[0];
    const payload = {
      medicationName: firstMed?.name || "",
      dosage: firstMed?.dosage || "",
      frequency: firstMed?.frequency || "",
      durationDays: firstMed?.duration ? parseInt(firstMed.duration) : 30,
      instructions: data.notes || firstMed?.instructions,
      status: null,
    };

    const response = await apiClient.put<BackendPrescription>(
      `/medical/prescriptions/${id}`,
      payload
    );

    // Create new prescriptions for any additional medications (starting from index 1)
    for (let i = 1; i < medications.length; i++) {
      const med = medications[i];
      if (!med.name) continue; // Skip empty medications

      const newPayload = {
        medicalRecordId: "00000000-0000-0000-0000-000000000000",
        patientId: data.patientId || "",
        doctorId: "00000000-0000-0000-0000-000000000000",
        medicationName: med.name,
        atcCode: null,
        dosage: med.dosage,
        frequency: med.frequency,
        durationDays: parseInt(med.duration) || 30,
        instructions: data.notes || med.instructions,
        prescribedDate: new Date().toISOString(),
      };

      await apiClient.post<BackendPrescription>(
        "/medical/prescriptions",
        newPayload
      );
    }

    return mapBackendToFrontend(response.data);
  }

  async deletePrescription(id: string): Promise<void> {
    await apiClient.delete(`/medical/prescriptions/${id}`);
  }

  async getPatients(): Promise<Patient[]> {
    try {
      // Use the patientsApi to get patients
      // This requires a user ID, so we'll try to get from auth
      const token = localStorage.getItem("authToken");
      if (!token) return [];

      // Parse user ID from token or use a workaround
      // For now, return empty and let the component handle it
      return [];
    } catch (error) {
      console.error("Failed to fetch patients:", error);
      return [];
    }
  }

  async getDoctors(): Promise<Doctor[]> {
    try {
      const response = await staffApi.getStaff({ role: "Doctor" });
      if (response.success && response.data) {
        interface DoctorExtended {
          specializations?: Array<{ name: string }>;
          licenseNumber?: string;
        }
        return response.data
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
    } catch (error) {
      console.error("Failed to fetch doctors:", error);
      return [];
    }
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
    // Pharmacies are not yet in backend - return empty
    return [];
  }

  async updatePrescriptionStatus(
    id: string,
    status: Prescription["status"]
  ): Promise<Prescription> {
    const response = await apiClient.put<BackendPrescription>(
      `/medical/prescriptions/${id}/status`,
      { status: status.charAt(0).toUpperCase() + status.slice(1) }
    );
    return mapBackendToFrontend(response.data);
  }

  async generatePrescriptionPDF(id: string): Promise<Blob> {
    // PDF generation not yet in backend
    const pdfContent = `Prescription ID: ${id}\nGenerated on: ${new Date().toISOString()}`;
    return new Blob([pdfContent], { type: "application/pdf" });
  }
}

export const prescriptionsApi = new PrescriptionsApi();
