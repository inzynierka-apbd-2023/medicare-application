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
    id: "lab1",
    appointmentId: "appt1",
    patientId: "1", // John Doe
    type: "Lab_Results",
    createdAt: "2025-08-01",
    notes: "Routine blood work - annual checkup",
    data: {
      testType: "Complete Blood Count & Lipid Panel",
      testDate: "2025-07-30",
      laboratory: "IMUP Medical Laboratory",
      status: "Abnormal",
      interpretation:
        "Cholesterol levels are elevated. Recommend dietary changes and continue medication. All other values within normal range.",
      referenceRanges:
        "Reference ranges are established for adults 18-65 years. Individual variations may apply.",
      results: [
        {
          parameter: "Total Cholesterol",
          value: 240,
          unit: "mg/dL",
          referenceRange: "<200",
          status: "High",
          notes: "Elevated - continue statin therapy",
        },
        {
          parameter: "LDL Cholesterol",
          value: 155,
          unit: "mg/dL",
          referenceRange: "<100",
          status: "High",
          notes: "Target <100 for cardiovascular risk",
        },
        {
          parameter: "HDL Cholesterol",
          value: 45,
          unit: "mg/dL",
          referenceRange: ">40 (M), >50 (F)",
          status: "Normal",
        },
        {
          parameter: "Triglycerides",
          value: 180,
          unit: "mg/dL",
          referenceRange: "<150",
          status: "High",
          notes: "Moderate elevation",
        },
        {
          parameter: "Hemoglobin",
          value: 14.2,
          unit: "g/dL",
          referenceRange: "12.0-15.5",
          status: "Normal",
        },
        {
          parameter: "White Blood Cell Count",
          value: 6800,
          unit: "/µL",
          referenceRange: "4500-11000",
          status: "Normal",
        },
        {
          parameter: "Platelet Count",
          value: 285000,
          unit: "/µL",
          referenceRange: "150000-450000",
          status: "Normal",
        },
      ],
    },
  },
  {
    id: "lab2",
    appointmentId: "appt3",
    patientId: "1", // John Doe
    type: "Lab_Results",
    createdAt: "2025-06-15",
    notes: "Thyroid function test as requested",
    data: {
      testType: "Thyroid Function Panel",
      testDate: "2025-06-12",
      laboratory: "IMUP Medical Laboratory",
      status: "Normal",
      interpretation:
        "All thyroid hormone levels are within normal limits. No thyroid dysfunction detected.",
      results: [
        {
          parameter: "TSH",
          value: 2.1,
          unit: "mIU/L",
          referenceRange: "0.4-4.0",
          status: "Normal",
        },
        {
          parameter: "Free T4",
          value: 1.3,
          unit: "ng/dL",
          referenceRange: "0.8-1.8",
          status: "Normal",
        },
        {
          parameter: "Free T3",
          value: 3.2,
          unit: "pg/mL",
          referenceRange: "2.3-4.2",
          status: "Normal",
        },
      ],
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
    appointmentId: "appt3",
    patientId: "2", // Jane Smith
    type: "Prescription",
    createdAt: "2025-04-22",
    notes: "Diabetes management",
    data: {
      medication: "Metformin",
      dosage: "500mg",
      frequency: "2x daily",
      duration_days: 90,
      instructions: "Take with meals",
    },
  },
  {
    id: "lab3",
    appointmentId: "appt4",
    patientId: "2", // Jane Smith
    type: "Lab_Results",
    createdAt: "2025-05-20",
    notes: "Diabetes monitoring - HbA1c and glucose levels",
    data: {
      testType: "Diabetes Panel",
      testDate: "2025-05-18",
      laboratory: "IMUP Medical Laboratory",
      status: "Normal",
      interpretation:
        "HbA1c shows good glycemic control. Continue current diabetes management plan.",
      results: [
        {
          parameter: "HbA1c",
          value: 6.8,
          unit: "%",
          referenceRange: "<7.0 (diabetic target)",
          status: "Normal",
          notes: "Good diabetic control",
        },
        {
          parameter: "Fasting Glucose",
          value: 118,
          unit: "mg/dL",
          referenceRange: "70-100",
          status: "High",
          notes: "Slightly elevated but acceptable for diabetic patient",
        },
        {
          parameter: "Creatinine",
          value: 0.9,
          unit: "mg/dL",
          referenceRange: "0.6-1.2",
          status: "Normal",
          notes: "Kidney function normal",
        },
      ],
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
