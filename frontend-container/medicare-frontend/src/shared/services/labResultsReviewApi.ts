import { Document } from "../../features/documents/types";

import { ApiResponse, createErrorResponse, createMockResponse } from "./api";

// Lab Result Review specific interfaces that extend existing document model
export interface LabResultReview {
  id: string;
  documentId: string; // References the lab result document
  reviewedById: string;
  reviewStatus:
    | "pending_review"
    | "in_review"
    | "approved"
    | "requires_followup";
  reviewNotes?: string;
  reviewedAt?: string;
  priority: "routine" | "urgent" | "critical";
}

export interface LabResultReviewRequest {
  documentId: string;
  reviewStatus: "approved" | "requires_followup" | "in_review";
  reviewNotes: string;
}

// Mock data aligned with existing document structure
const mockLabResultDocuments: Document[] = [
  {
    id: "doc_lab_1",
    appointmentId: "apt_001",
    patientId: "patient_1",
    type: "Lab_Results",
    createdAt: "2025-08-08T14:30:00Z",
    notes: "Routine blood work ordered due to patient fatigue complaints",
    data: {
      testType: "Complete Blood Count",
      testDate: "2025-08-08",
      laboratory: "City Medical Lab",
      status: "Abnormal",
      results: [
        {
          parameter: "Hemoglobin",
          value: 8.5,
          unit: "g/dL",
          referenceRange: "12.0-15.5",
          status: "Low",
          notes: "Below normal range",
        },
        {
          parameter: "White Blood Cell Count",
          value: 4.8,
          unit: "K/uL",
          referenceRange: "4.0-11.0",
          status: "Normal",
        },
        {
          parameter: "Platelet Count",
          value: 250,
          unit: "K/uL",
          referenceRange: "150-450",
          status: "Normal",
        },
      ],
      interpretation:
        "Low hemoglobin suggests possible anemia. Further evaluation needed.",
    },
  },
  {
    id: "doc_lab_2",
    appointmentId: "apt_002",
    patientId: "patient_2",
    type: "Lab_Results",
    createdAt: "2025-08-07T10:00:00Z",
    data: {
      testType: "Basic Metabolic Panel",
      testDate: "2025-08-07",
      laboratory: "Regional Lab Services",
      status: "Normal",
      results: [
        {
          parameter: "Glucose",
          value: 95,
          unit: "mg/dL",
          referenceRange: "70-100",
          status: "Normal",
        },
        {
          parameter: "Creatinine",
          value: 1.1,
          unit: "mg/dL",
          referenceRange: "0.6-1.2",
          status: "Normal",
        },
        {
          parameter: "Sodium",
          value: 140,
          unit: "mEq/L",
          referenceRange: "136-145",
          status: "Normal",
        },
      ],
      interpretation: "All parameters within normal limits.",
    },
  },
];

const mockLabResultReviews: LabResultReview[] = [
  {
    id: "review_1",
    documentId: "doc_lab_1",
    reviewedById: "doctor_1",
    reviewStatus: "pending_review",
    priority: "urgent",
  },
  {
    id: "review_2",
    documentId: "doc_lab_2",
    reviewedById: "doctor_1",
    reviewStatus: "pending_review",
    priority: "routine",
  },
];

class LabResultsReviewApiService {
  // Get lab result documents that need review
  async getLabResultsForReview(): Promise<ApiResponse<Document[]>> {
    try {
      // Simulate API delay
      await new Promise((resolve) => setTimeout(resolve, 500));

      // Filter only lab result documents
      const labResults = mockLabResultDocuments.filter(
        (doc) => doc.type === "Lab_Results"
      );

      return createMockResponse(labResults);
    } catch (_error) {
      return createErrorResponse("Failed to fetch lab results for review");
    }
  }

  // Get review status for lab results
  async getLabResultReviews(): Promise<ApiResponse<LabResultReview[]>> {
    try {
      await new Promise((resolve) => setTimeout(resolve, 300));
      return createMockResponse(mockLabResultReviews);
    } catch (_error) {
      return createErrorResponse("Failed to fetch lab result reviews");
    }
  }

  // Submit a lab result review
  async submitLabResultReview(
    request: LabResultReviewRequest
  ): Promise<ApiResponse<LabResultReview>> {
    try {
      await new Promise((resolve) => setTimeout(resolve, 800));

      // Find existing review or create new one
      let review = mockLabResultReviews.find(
        (r) => r.documentId === request.documentId
      );

      if (review) {
        // Update existing review
        review.reviewStatus = request.reviewStatus;
        review.reviewNotes = request.reviewNotes;
        review.reviewedAt = new Date().toISOString();
      } else {
        // Create new review
        review = {
          id: `review_${Date.now()}`,
          documentId: request.documentId,
          reviewedById: "current_doctor_id",
          reviewStatus: request.reviewStatus,
          reviewNotes: request.reviewNotes,
          reviewedAt: new Date().toISOString(),
          priority: "routine",
        };
        mockLabResultReviews.push(review);
      }

      return createMockResponse(review);
    } catch (_error) {
      return createErrorResponse("Failed to submit lab result review");
    }
  }

  // Quick approve a lab result
  async quickApproveLabResult(
    documentId: string
  ): Promise<ApiResponse<LabResultReview>> {
    try {
      await new Promise((resolve) => setTimeout(resolve, 400));

      const request: LabResultReviewRequest = {
        documentId,
        reviewStatus: "approved",
        reviewNotes: "Quick approval - results within normal limits",
      };

      return this.submitLabResultReview(request);
    } catch (_error) {
      return createErrorResponse("Failed to approve lab result");
    }
  }

  // Get lab result document by ID
  async getLabResultDocument(
    documentId: string
  ): Promise<ApiResponse<Document | null>> {
    try {
      await new Promise((resolve) => setTimeout(resolve, 200));

      const document = mockLabResultDocuments.find(
        (doc) => doc.id === documentId
      );

      return createMockResponse(document || null);
    } catch (_error) {
      return createErrorResponse("Failed to fetch lab result document");
    }
  }
}

export const labResultsReviewApi = new LabResultsReviewApiService();
