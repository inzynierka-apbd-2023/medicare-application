import type {
  Appointment,
  Document,
  DocumentType,
} from "../../features/documents/types";

import {
  type ApiResponse,
  createErrorResponse,
  createMockResponse,
} from "./api";

// Mock documents data - simulates API response
const mockDocuments: Document[] = [
  {
    id: "d1",
    appointmentId: "appt2",
    patientId: "1", // John Doe
    type: "Prescription",
    createdAt: "2025-05-10",
    notes: "Cholesterol meds",
    data: {
      medication: "Atorvastatin",
      dosage: "20mg",
      frequency: "1x daily",
      duration_days: 30,
      instructions: "Take after dinner",
    },
  },
  {
    id: "d42",
    appointmentId: "appt2",
    patientId: "1", // John Doe
    type: "Prescription",
    createdAt: "2025-05-10",
    notes: "Cholesterol meds renewed",
    data: {
      medication: "Atorvastatin",
      dosage: "20mg",
      frequency: "1x daily",
      duration_days: 30,
      instructions: "Take after dinner",
    },
  },
  {
    id: "d2",
    appointmentId: "appt2",
    patientId: "1", // John Doe
    type: "Referral",
    createdAt: "2025-04-22",
    notes: "Consult cardiologist",
    data: {
      specialty: "Cardiologist",
      referredTo: "Dr. Heart Strong",
      validFrom: "2025-04-22",
      validTo: "2025-06-01",
    },
  },
  {
    id: "d3",
    appointmentId: "appt1",
    patientId: "2", // Adam Nowak
    type: "Sick_Leave",
    createdAt: "2025-03-15",
    notes: "Flu recovery",
    data: {
      startDate: "2025-03-15",
      endDate: "2025-03-22",
      daysOff: 8,
    },
  },
  {
    id: "d4",
    appointmentId: "appt1",
    patientId: "2", // Adam Nowak
    type: "VisitCard",
    createdAt: "2025-03-15",
    notes: "First hypertension check",
    data: {
      symptoms: "Fatigue, high BP",
      findings: "Elevated BP",
      diagnosis: "Hypertension",
      recommendations: "Monitor BP daily, reduce salt",
    },
  },
  {
    id: "d5",
    appointmentId: "appt3",
    patientId: "3", // Emma Watson
    type: "Prescription",
    createdAt: "2025-06-15",
    notes: "Blood pressure medication",
    data: {
      medication: "Lisinopril",
      dosage: "10mg",
      frequency: "1x daily",
      duration_days: 60,
      instructions: "Take in the morning",
    },
  },
  {
    id: "d6",
    appointmentId: "appt3",
    patientId: "3", // Emma Watson
    type: "VisitCard",
    createdAt: "2025-06-15",
    notes: "Follow-up hypertension check",
    data: {
      symptoms: "Improved energy levels",
      findings: "BP within normal range",
      diagnosis: "Well-controlled hypertension",
      recommendations: "Continue current medication",
    },
  },
];

const mockAppointments: Appointment[] = [
  {
    id: "appt1",
    patientId: "2", // Adam Nowak
    patientName: "Adam Nowak",
    date: "2025-03-15",
    doctor: "Dr. Anna Nowak",
    specialization: "Cardiology",
  },
  {
    id: "appt2",
    patientId: "1", // John Doe
    patientName: "John Doe",
    date: "2025-05-10",
    doctor: "Dr. Bob Vessel",
    specialization: "Dermatology",
  },
  {
    id: "appt3",
    patientId: "3", // Emma Watson
    patientName: "Emma Watson",
    date: "2025-06-15",
    doctor: "Dr. Anna Nowak",
    specialization: "Cardiology",
  },
  {
    id: "appt4",
    patientId: "4", // Michael Brown
    patientName: "Michael Brown",
    date: "2025-07-20",
    doctor: "Dr. Sarah Johnson",
    specialization: "General Medicine",
  },
];

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

export const documentsApi = {
  /**
   * Fetch all documents with optional filtering
   */
  getDocuments: async (
    filters?: DocumentsFilterParams
  ): Promise<ApiResponse<Document[]>> => {
    try {
      let filteredDocuments = [...mockDocuments];

      if (filters) {
        const { searchTerm, typeFilter, appointmentId, patientId } = filters;

        if (typeFilter && typeFilter !== "All") {
          filteredDocuments = filteredDocuments.filter(
            (doc) => doc.type === typeFilter
          );
        }

        if (appointmentId) {
          filteredDocuments = filteredDocuments.filter(
            (doc) => doc.appointmentId === appointmentId
          );
        }

        if (patientId) {
          filteredDocuments = filteredDocuments.filter(
            (doc) => doc.patientId === patientId
          );
        }

        if (searchTerm) {
          const searchLower = searchTerm.toLowerCase();
          filteredDocuments = filteredDocuments.filter(
            (doc) =>
              doc.notes?.toLowerCase().includes(searchLower) ||
              doc.type.toLowerCase().includes(searchLower)
          );
        }
      }

      // Sort by creation date (newest first)
      filteredDocuments.sort(
        (a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      );

      return await createMockResponse(filteredDocuments, 300);
    } catch (_error) {
      return createErrorResponse("Failed to fetch documents");
    }
  },

  /**
   * Fetch all appointments for filtering
   */
  getAppointments: async (): Promise<ApiResponse<Appointment[]>> => {
    try {
      // Sort appointments by date (newest first)
      const sortedAppointments = [...mockAppointments].sort(
        (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()
      );

      return await createMockResponse(sortedAppointments, 200);
    } catch (_error) {
      return createErrorResponse("Failed to fetch appointments");
    }
  },

  /**
   * Fetch documents and appointments together for initial page load
   */
  getDocumentsWithAppointments: async (
    filters?: DocumentsFilterParams
  ): Promise<ApiResponse<DocumentsApiResponse>> => {
    try {
      const [documentsResponse, appointmentsResponse] = await Promise.all([
        documentsApi.getDocuments(filters),
        documentsApi.getAppointments(),
      ]);

      if (!documentsResponse.success) {
        return createErrorResponse(
          documentsResponse.error || "Failed to fetch documents"
        );
      }

      if (!appointmentsResponse.success) {
        return createErrorResponse(
          appointmentsResponse.error || "Failed to fetch appointments"
        );
      }

      return await createMockResponse(
        {
          documents: documentsResponse.data,
          appointments: appointmentsResponse.data,
        },
        400
      );
    } catch (_error) {
      return createErrorResponse("Failed to fetch documents data");
    }
  },

  /**
   * Get a single document by ID
   */
  getDocumentById: async (
    documentId: string
  ): Promise<ApiResponse<Document | null>> => {
    try {
      const document = mockDocuments.find((doc) => doc.id === documentId);

      return await createMockResponse(document || null, 100);
    } catch (_error) {
      return createErrorResponse("Failed to fetch document");
    }
  },

  /**
   * Mock download document functionality
   */
  downloadDocument: async (
    documentId: string
  ): Promise<ApiResponse<{ downloadUrl: string }>> => {
    try {
      const document = mockDocuments.find((doc) => doc.id === documentId);

      if (!document) {
        return createErrorResponse("Document not found");
      }

      // In a real app, this would return a download URL or initiate download
      const mockDownloadUrl = `https://api.medicare.com/documents/${documentId}/download`;

      return await createMockResponse({ downloadUrl: mockDownloadUrl }, 500);
    } catch (_error) {
      return createErrorResponse("Failed to download document");
    }
  },
};
