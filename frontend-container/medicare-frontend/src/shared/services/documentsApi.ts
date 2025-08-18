import type { Appointment, Document, DocumentType } from "../../features/documents/types";
import { type ApiResponse, createErrorResponse } from "./api";
import { apiClient as api } from "./apiClient";
import { appointmentsApi } from "./appointmentsApi";

export interface DocumentsFilterParams {
  searchTerm?: string;
  typeFilter?: DocumentType | "All";
  appointmentId?: string;
  patientId?: string;
}

export interface DocumentsApiResponse {
  documents: Document[];
  appointments: Appointment[];
}

// Helpers to keep mapping readable
const typeIntToUi = (typeInt: number): DocumentType => {
  switch (typeInt) {
    case 1:
      return "VisitCard";
    case 2:
      return "Prescription";
    case 3:
      return "Referral";
    case 4:
      return "Sick_Leave";
    case 5:
      return "Lab_Results";
    default:
      return "Other";
  }
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const mapLabData = (lab: any): Document["data"] => {
  const data: Document["data"] = {
    testType: lab.testType ?? undefined,
    testDate: lab.testDate ?? undefined,
    laboratory: lab.laboratory ?? undefined,
    interpretation: lab.interpretation ?? undefined,
    referenceRanges: lab.referenceRanges ?? undefined,
  };
  const results = Array.isArray(lab.results) ? lab.results : [];
  data.results = results.map((r: any) => {
    const rawStatus = (r.status ?? "").toString();
    const rawLower = rawStatus.toLowerCase();
    const isAbn = r.isAbnormal === true || rawLower === "abnormal";
    let mappedStatus: string;
    if (rawLower === "critical") mappedStatus = "Critical";
    else if (rawLower === "high" || rawLower === "low") mappedStatus = rawStatus;
    else if (isAbn) mappedStatus = "High"; // treat flagged abnormal as non-normal
    else mappedStatus = "Normal";

    return {
      parameter: r.parameterName ?? r.loincCode ?? "",
      value: r.numericValue ?? r.value ?? "",
      unit: r.unit ?? undefined,
      referenceRange: r.referenceRange ?? undefined,
      status: mappedStatus,
      notes: r.notes ?? undefined,
    };
  });
  const hasCritical = (data.results || []).some((r) => r.status === "Critical");
  const hasNonNormal = (data.results || []).some((r) => r.status !== "Normal");
  if (hasCritical) data.status = "Critical";
  else if (hasNonNormal) data.status = "Abnormal";
  else data.status = "Normal";
  return data;
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const firstAssignedAppointmentId = (row: any): string => {
  return Array.isArray(row.assignments) && row.assignments.length > 0
    ? String(row.assignments[0].appointmentId)
    : "";
};

// Map backend DocumentsService entity to UI Document
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const mapBackendDocument = (row: any): Document => {
  const type = typeIntToUi(Number(row.type ?? 0));
  const data: Document["data"] = {};
  if (type === "Prescription" && row.prescription) {
    data.medication = String(row.prescription.medication ?? "");
    data.dosage = row.prescription.dosage ?? undefined;
    data.frequency = row.prescription.frequency ?? undefined;
    data.duration_days = row.prescription.durationDays ?? undefined;
    data.instructions = row.prescription.instructions ?? undefined;
  }
  if (type === "Referral" && row.referral) {
    data.specialty = row.referral.speciality ?? row.referral.specialty ?? undefined;
    data.referredTo = row.referral.referredTo ?? undefined;
    data.validFrom = row.referral.validFrom ?? undefined;
    data.validTo = row.referral.validTo ?? undefined;
  }
  if (type === "Sick_Leave" && row.sickLeave) {
    data.startDate = row.sickLeave.startDate ?? undefined;
    data.endDate = row.sickLeave.endDate ?? undefined;
    data.daysOff = row.sickLeave.daysOff ?? undefined;
  }
  if (type === "VisitCard" && row.visitDocument) {
    data.symptoms = row.visitDocument.symptoms ?? undefined;
    data.findings = row.visitDocument.findings ?? undefined;
    data.diagnosis = row.visitDocument.diagnosis ?? undefined;
    data.recommendations = row.visitDocument.recommendations ?? undefined;
  }
  if (type === "Lab_Results" && row.labResults) {
    const lab = row.labResults;
    const mapped = mapLabData(lab);
    Object.assign(data, mapped);
  }

  const created = row.createdAt ? new Date(row.createdAt).toISOString() : new Date().toISOString();
  return {
    id: String(row.id),
    appointmentId: firstAssignedAppointmentId(row),
    patientId: String(row.patientId ?? ""),
    type,
    createdAt: created,
    notes: row.notes ?? undefined,
    data,
  } as Document;
};

export const documentsApi = {
  // Fetch documents from backend with optional filtering
  getDocuments: async (
    filters?: DocumentsFilterParams
  ): Promise<ApiResponse<Document[]>> => {
    try {
      const params: Record<string, string> = {};
      if (filters?.patientId) params.patientId = filters.patientId;
      if (filters?.appointmentId) params.appointmentId = filters.appointmentId;
      if (filters?.typeFilter && filters.typeFilter !== "All") {
        // Map UI type -> backend int if needed
        const map: Record<DocumentType, number> = {
          VisitCard: 1,
          Prescription: 2,
          Referral: 3,
          Sick_Leave: 4,
          Lab_Results: 5,
          Other: 0,
        };
        params.type = String(map[filters.typeFilter]);
      }

      const res = await api.get("/documents", { params });
      const items = Array.isArray(res.data) ? res.data : [];
      const mapped = items.map(mapBackendDocument);

      // Apply client-side search if provided
      let docs = mapped;
      if (filters?.searchTerm) {
        const s = filters.searchTerm.toLowerCase();
        docs = docs.filter(
          (d) => d.notes?.toLowerCase().includes(s) || d.type.toLowerCase().includes(s)
        );
      }

      // Sort by createdAt desc
      docs.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
      return { data: docs, success: true };
    } catch (error) {
      console.error("Failed to fetch documents", error);
      return createErrorResponse("Failed to fetch documents");
    }
  },

  // Fetch appointments for the given patient (delegates to AppointmentService client)
  getAppointments: async (
    patientId: string
  ): Promise<ApiResponse<Appointment[]>> => {
    try {
      const resp = await appointmentsApi.getAppointmentsForPatient(patientId);
      if (!resp.success) return resp as unknown as ApiResponse<Appointment[]>;
      // Map to documents feature Appointment shape
      const docsAppointments: Appointment[] = (resp.data || []).map((a: any) => ({
        id: String(a.id),
        date: String(a.date),
        doctor: String(a.doctor ?? ""),
        specialization: a.specialization ?? "",
      }));
      return { data: docsAppointments, success: true };
    } catch (error) {
      console.error("Failed to fetch appointments", error);
      return createErrorResponse("Failed to fetch appointments");
    }
  },

  // Fetch documents and appointments together
  getDocumentsWithAppointments: async (
    filters?: DocumentsFilterParams
  ): Promise<ApiResponse<DocumentsApiResponse>> => {
    try {
      const [docsResp, apptsResp] = await Promise.all([
        documentsApi.getDocuments(filters),
        filters?.patientId ? documentsApi.getAppointments(filters.patientId) : Promise.resolve({ data: [], success: true } as ApiResponse<Appointment[]>),
      ]);

      if (!docsResp.success) return createErrorResponse(docsResp.error || "Failed to fetch documents");
      if (!apptsResp.success) return createErrorResponse(apptsResp.error || "Failed to fetch appointments");

      return { data: { documents: docsResp.data, appointments: apptsResp.data }, success: true };
    } catch (error) {
      console.error("Failed to fetch documents data", error);
      return createErrorResponse("Failed to fetch documents data");
    }
  },

  // Get a single document by ID
  getDocumentById: async (
    documentId: string
  ): Promise<ApiResponse<Document | null>> => {
    try {
      const res = await api.get(`/documents/${documentId}`);
      const doc = mapBackendDocument(res.data);
      return { data: doc, success: true };
    } catch (error) {
      console.error("Failed to fetch document", error);
      return createErrorResponse("Failed to fetch document");
    }
  },

  // Download document placeholder: returns a URL to backend resource
  downloadDocument: async (
    documentId: string
  ): Promise<ApiResponse<{ downloadUrl: string }>> => {
    try {
      const url = `${location.origin}/api/documents/${documentId}`;
      return { data: { downloadUrl: url }, success: true };
    } catch (error) {
      console.error("Failed to prepare document download", error);
      return createErrorResponse("Failed to download document");
    }
  },
};
