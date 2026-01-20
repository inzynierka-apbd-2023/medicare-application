/**
 * Visit Note API Service
 *
 * Handles all API interactions for visit notes in the doctor scheduler.
 * This service provides functions to create, update, and retrieve visit notes
 * associated with appointments.
 */

import type { VisitNoteData } from "@features/scheduler/components/VisitNoteModal";
import { AxiosError } from "axios";

import { api, type ApiResponse, handleApiCall } from "./api";
import { apiClient } from "./apiClient";

export interface CreateDocumentRequest {
  patientId: string;
  doctorId: string;
  notes?: string;
  documentTypeCode: string;
  type?: number;
}

export interface VisitNoteRequest {
  symptoms?: string;
  findings?: string;
  diagnosis?: string;
  recommendations?: string;
  vitalSignsJson?: string;
  treatmentPlan?: string;
  followUpDate?: string;
}

export interface AssignDocumentRequest {
  appointmentId: string;
}

interface BackendDocument {
  id: string;
  patientId: string;
  doctorId: string;
  type: number;
  createdAt: string;
  notes?: string;
  visitDocument?: {
    symptoms?: string;
    findings?: string;
    diagnosis?: string;
    recommendations?: string;
    vitalSignsJson?: string;
    treatmentPlan?: string;
    followUpDate?: string;
  };
  assignments?: Array<{ appointmentId: string }>;
}

export interface VisitNoteInfo {
  documentId: string;
  hasVisitNote: boolean;
  visitNote?: VisitNoteData;
}

export const visitNoteApi = {
  /**
   * Get visit note for an appointment if one exists
   */
  getVisitNoteForAppointment: async (
    appointmentId: string
  ): Promise<ApiResponse<VisitNoteInfo | null>> => {
    return handleApiCall<VisitNoteInfo | null>(
      async () => {
        // Fetch documents assigned to this appointment
        const documents = await api.get<BackendDocument[]>("/documents", {
          params: { appointmentId, type: 1 }, // type 1 = VisitNote
        });

        if (!documents || documents.length === 0) {
          return null;
        }

        // Find the visit note document
        const visitNoteDoc = documents.find(
          (doc) => doc.type === 1 && doc.visitDocument
        );

        if (!visitNoteDoc) {
          return null;
        }

        return {
          documentId: visitNoteDoc.id,
          hasVisitNote: true,
          visitNote: {
            documentId: visitNoteDoc.id,
            symptoms: visitNoteDoc.visitDocument?.symptoms ?? "",
            findings: visitNoteDoc.visitDocument?.findings ?? "",
            diagnosis: visitNoteDoc.visitDocument?.diagnosis ?? "",
            recommendations: visitNoteDoc.visitDocument?.recommendations ?? "",
            vitalSignsJson: visitNoteDoc.visitDocument?.vitalSignsJson ?? "",
            treatmentPlan: visitNoteDoc.visitDocument?.treatmentPlan ?? "",
            followUpDate: visitNoteDoc.visitDocument?.followUpDate ?? "",
          },
        };
      },
      {
        showToastOnSuccess: false,
        showToastOnError: false, // Silently handle - no visit note is normal
      }
    );
  },

  /**
   * Create a new document and attach a visit note to it
   */
  createVisitNote: async (
    patientId: string,
    doctorId: string,
    appointmentId: string,
    visitNoteData: VisitNoteData
  ): Promise<ApiResponse<{ documentId: string }>> => {
    return handleApiCall<{ documentId: string }>(
      async () => {
        // Step 1: Create the document
        const doc = await api.post<BackendDocument>("/documents", {
          patientId,
          doctorId,
          notes: visitNoteData.diagnosis || "Visit note",
          documentTypeCode: "VISIT_NOTE",
          type: 1, // VisitNote
        } as CreateDocumentRequest);

        const documentId = doc.id;

        // Step 2: Attach the visit note data
        await api.post(`/documents/${documentId}/visit-note`, {
          symptoms: visitNoteData.symptoms,
          findings: visitNoteData.findings,
          diagnosis: visitNoteData.diagnosis,
          recommendations: visitNoteData.recommendations,
          vitalSignsJson: visitNoteData.vitalSignsJson,
          treatmentPlan: visitNoteData.treatmentPlan,
          followUpDate: visitNoteData.followUpDate
            ? new Date(visitNoteData.followUpDate).toISOString()
            : null,
        } as VisitNoteRequest);

        // Step 3: Assign the document to the appointment
        await api.post(`/documents/${documentId}/assign`, {
          appointmentId,
        } as AssignDocumentRequest);

        return { documentId };
      },
      {
        showToastOnSuccess: true,
        showToastOnError: true,
        successMessage: "Visit note created successfully",
      }
    );
  },

  /**
   * Update an existing visit note
   */
  updateVisitNote: async (
    documentId: string,
    visitNoteData: VisitNoteData
  ): Promise<ApiResponse<void>> => {
    return handleApiCall<void>(
      async () => {
        const payload = {
          symptoms: visitNoteData.symptoms,
          findings: visitNoteData.findings,
          diagnosis: visitNoteData.diagnosis,
          recommendations: visitNoteData.recommendations,
          vitalSignsJson: visitNoteData.vitalSignsJson,
          treatmentPlan: visitNoteData.treatmentPlan,
          followUpDate: visitNoteData.followUpDate
            ? new Date(visitNoteData.followUpDate).toISOString()
            : null,
        } as VisitNoteRequest;

        try {
          await apiClient.put(`/documents/${documentId}/visit-note`, payload);
        } catch (error) {
          const axiosError = error as AxiosError;
          if (axiosError.response?.status === 405) {
            await apiClient.post(
              `/documents/${documentId}/visit-note`,
              payload
            );
            return;
          }
          throw error;
        }
      },
      {
        showToastOnSuccess: true,
        showToastOnError: true,
        successMessage: "Visit note updated successfully",
      }
    );
  },

  /**
   * Get a document by ID (to retrieve existing visit note for editing)
   */
  getDocumentById: async (
    documentId: string
  ): Promise<ApiResponse<VisitNoteData | null>> => {
    return handleApiCall<VisitNoteData | null>(
      async () => {
        const doc = await api.get<BackendDocument>(`/documents/${documentId}`);

        if (!doc?.visitDocument) {
          return null;
        }

        return {
          documentId: doc.id,
          symptoms: doc.visitDocument.symptoms ?? "",
          findings: doc.visitDocument.findings ?? "",
          diagnosis: doc.visitDocument.diagnosis ?? "",
          recommendations: doc.visitDocument.recommendations ?? "",
          vitalSignsJson: doc.visitDocument.vitalSignsJson ?? "",
          treatmentPlan: doc.visitDocument.treatmentPlan ?? "",
          followUpDate: doc.visitDocument.followUpDate ?? "",
        };
      },
      {
        showToastOnSuccess: false,
        showToastOnError: true,
      }
    );
  },
};

export default visitNoteApi;
