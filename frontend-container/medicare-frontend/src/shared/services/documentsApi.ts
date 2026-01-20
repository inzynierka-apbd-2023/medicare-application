import type {
  Appointment,
  Document,
  DocumentType,
  LabTestResult,
} from "@features/documents/types";
import { toastMessages } from "@shared/toast/toastMessages";

import { api, type ApiResponse, handleApiCall } from "./api";
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

interface BackendPrescription {
  medication?: string;
  dosage?: string;
  frequency?: string;
  durationDays?: number;
  instructions?: string;
}

interface BackendReferral {
  speciality?: string;
  specialty?: string;
  referredTo?: string;
  validFrom?: string;
  validTo?: string;
}

interface BackendSickLeave {
  startDate?: string;
  endDate?: string;
  daysOff?: number;
}

interface BackendVisitDocument {
  symptoms?: string;
  findings?: string;
  diagnosis?: string;
  recommendations?: string;
}

interface BackendLabResult {
  status?: unknown;
  isAbnormal?: boolean;
  parameterName?: string;
  loincCode?: string;
  numericValue?: string | number;
  value?: string | number;
  unit?: string;
  referenceRange?: string;
  notes?: string;
}

interface BackendLabResults {
  testType?: string;
  testDate?: string;
  laboratory?: string;
  interpretation?: string;
  referenceRanges?: string;
  results?: BackendLabResult[];
}

interface BackendDocumentAssignment {
  appointmentId: string | number;
}

interface BackendDocument {
  id: string | number;
  patientId?: string | number;
  type?: number;
  createdAt?: string;
  notes?: string;
  assignments?: BackendDocumentAssignment[];
  prescription?: BackendPrescription;
  referral?: BackendReferral;
  sickLeave?: BackendSickLeave;
  visitDocument?: BackendVisitDocument;
  labResults?: BackendLabResults;
}

interface SourceAppointment {
  id: string | number;
  date: string | number;
  doctor?: string | null;
  specialization?: string | null;
}

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

const typeUiToInt: Record<DocumentType, number> = {
  VisitCard: 1,
  Prescription: 2,
  Referral: 3,
  Sick_Leave: 4,
  Lab_Results: 5,
  Other: 0,
};

const mapLabData = (lab: BackendLabResults): Document["data"] => {
  const data: Document["data"] = {};

  if (lab.testType) data.testType = lab.testType;
  if (lab.testDate) data.testDate = lab.testDate;
  if (lab.laboratory) data.laboratory = lab.laboratory;
  if (lab.interpretation) data.interpretation = lab.interpretation;
  if (lab.referenceRanges) data.referenceRanges = lab.referenceRanges;

  const results = Array.isArray(lab.results) ? lab.results : [];
  data.results = results.map((r: BackendLabResult): LabTestResult => {
    const rawStatus = (r.status ?? "").toString();
    const rawLower = rawStatus.toLowerCase();
    const isAbn = r.isAbnormal === true || rawLower === "abnormal";

    let mappedStatus: "Normal" | "High" | "Low" | "Critical";
    if (rawLower === "critical") mappedStatus = "Critical";
    else if (rawLower === "high") mappedStatus = "High";
    else if (rawLower === "low") mappedStatus = "Low";
    else if (isAbn) mappedStatus = "High";
    else mappedStatus = "Normal";

    const result: LabTestResult = {
      parameter: r.parameterName ?? r.loincCode ?? "",
      value: r.numericValue ?? r.value ?? "",
      status: mappedStatus,
    };

    if (r.unit) result.unit = r.unit;
    if (r.referenceRange) result.referenceRange = r.referenceRange;
    if (r.notes) result.notes = r.notes;

    return result;
  });

  const hasCritical = (data.results || []).some((r) => r.status === "Critical");
  const hasNonNormal = (data.results || []).some((r) => r.status !== "Normal");
  if (hasCritical) data.status = "Critical";
  else if (hasNonNormal) data.status = "Abnormal";
  else data.status = "Normal";

  return data;
};

const firstAssignedAppointmentId = (row: BackendDocument): string => {
  return Array.isArray(row.assignments) && row.assignments.length > 0
    ? String(row.assignments[0].appointmentId)
    : "";
};

const mapBackendDocument = (row: BackendDocument): Document => {
  const type = typeIntToUi(Number(row.type ?? 0));
  const data: Document["data"] = {};

  if (type === "Prescription" && row.prescription) {
    const p = row.prescription;
    data.medication = String(p.medication ?? "");
    if (p.dosage) data.dosage = p.dosage;
    if (p.frequency) data.frequency = p.frequency;
    if (p.durationDays !== undefined) data.duration_days = p.durationDays;
    if (p.instructions) data.instructions = p.instructions;
  }

  if (type === "Referral" && row.referral) {
    const r = row.referral;
    if (r.speciality) data.specialty = r.speciality;
    else if (r.specialty) data.specialty = r.specialty;
    if (r.referredTo) data.referredTo = r.referredTo;
    if (r.validFrom) data.validFrom = r.validFrom;
    if (r.validTo) data.validTo = r.validTo;
  }

  if (type === "Sick_Leave" && row.sickLeave) {
    const s = row.sickLeave;
    if (s.startDate) data.startDate = s.startDate;
    if (s.endDate) data.endDate = s.endDate;
    if (s.daysOff !== undefined) data.daysOff = s.daysOff;
  }

  if (type === "VisitCard" && row.visitDocument) {
    const v = row.visitDocument;
    if (v.symptoms) data.symptoms = v.symptoms;
    if (v.findings) data.findings = v.findings;
    if (v.diagnosis) data.diagnosis = v.diagnosis;
    if (v.recommendations) data.recommendations = v.recommendations;
  }

  if (type === "Lab_Results" && row.labResults) {
    const mapped = mapLabData(row.labResults);
    Object.assign(data, mapped);
  }

  const created = row.createdAt
    ? new Date(row.createdAt).toISOString()
    : new Date().toISOString();

  const doc: Document = {
    id: String(row.id),
    appointmentId: firstAssignedAppointmentId(row),
    type,
    createdAt: created,
    data,
  };

  if (row.patientId !== undefined) doc.patientId = String(row.patientId);
  if (row.notes) doc.notes = row.notes;

  return doc;
};

export const documentsApi = {
  getDocuments: async (
    filters?: DocumentsFilterParams
  ): Promise<ApiResponse<Document[]>> => {
    return handleApiCall<Document[]>(
      async () => {
        const params: Record<string, string> = {};
        if (filters?.patientId) params.patientId = filters.patientId;
        if (filters?.appointmentId)
          params.appointmentId = filters.appointmentId;
        if (filters?.typeFilter && filters.typeFilter !== "All") {
          params.type = String(typeUiToInt[filters.typeFilter]);
        }

        const items = await api.get<BackendDocument[]>("/documents", {
          params,
        });
        const mapped = (Array.isArray(items) ? items : []).map(
          mapBackendDocument
        );

        let docs = mapped;
        if (filters?.searchTerm) {
          const s = filters.searchTerm.toLowerCase();
          docs = docs.filter(
            (d) =>
              d.notes?.toLowerCase().includes(s) ||
              d.type.toLowerCase().includes(s)
          );
        }

        docs.sort(
          (a, b) =>
            new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );

        return docs;
      },
      {
        showToastOnSuccess: false,
        showToastOnError: true,
      }
    );
  },

  getAppointments: async (
    patientId: string
  ): Promise<ApiResponse<Appointment[]>> => {
    return handleApiCall<Appointment[]>(
      async () => {
        const resp = await appointmentsApi.getAppointmentsForPatient(patientId);
        if (!resp.success) {
          throw new Error(resp.error || toastMessages.appointments.fetchError);
        }

        const sourceList = (resp.data || []) as unknown as SourceAppointment[];

        return sourceList.map((a) => ({
          id: String(a.id),
          date: String(a.date),
          doctor: String(a.doctor ?? ""),
          specialization: a.specialization ?? "",
        }));
      },
      {
        showToastOnSuccess: false,
        showToastOnError: true,
      }
    );
  },

  getDocumentsWithAppointments: async (
    filters?: DocumentsFilterParams
  ): Promise<ApiResponse<DocumentsApiResponse>> => {
    return handleApiCall<DocumentsApiResponse>(
      async () => {
        const [docsResp, apptsResp] = await Promise.all([
          documentsApi.getDocuments(filters),
          filters?.patientId
            ? documentsApi.getAppointments(filters.patientId)
            : Promise.resolve({ data: [], success: true } as ApiResponse<
                Appointment[]
              >),
        ]);

        if (!docsResp.success) {
          throw new Error(
            docsResp.error || toastMessages.documents.downloadError
          );
        }
        if (!apptsResp.success) {
          throw new Error(
            apptsResp.error || toastMessages.appointments.fetchError
          );
        }

        return { documents: docsResp.data, appointments: apptsResp.data };
      },
      {
        showToastOnSuccess: false,
        showToastOnError: true,
      }
    );
  },

  getDocumentById: async (
    documentId: string
  ): Promise<ApiResponse<Document | null>> => {
    return handleApiCall<Document | null>(
      async () => {
        const item = await api.get<BackendDocument>(`/documents/${documentId}`);
        return mapBackendDocument(item);
      },
      {
        showToastOnSuccess: false,
        showToastOnError: true,
      }
    );
  },

  downloadDocument: async (
    documentId: string
  ): Promise<ApiResponse<{ downloadUrl: string }>> => {
    return handleApiCall<{ downloadUrl: string }>(
      async () => {
        const url = `${location.origin}/api/documents/${documentId}/pdf`;
        return { downloadUrl: url };
      },
      {
        showToastOnSuccess: false,
        showToastOnError: true,
      }
    );
  },
};
