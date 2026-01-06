import { apiClient } from "./apiClient";

// Backend response types
interface BackendPendingLabResult {
  id: string;
  patientId: string;
  labTestId: string;
  testName: string;
  loincCode: string;
  value?: string;
  unit?: string;
  referenceRange?: string;
  flag?: string;
  comments?: string;
  resultDate: string;
  reviewStatus: string;
  priority: string;
  orderNotes?: string;
  createdAt: string;
}

interface BackendLabResultDetail {
  result: {
    id: string;
    labTestId: string;
    patientId: string;
    value?: string;
    unit?: string;
    referenceRange?: string;
    flag?: string;
    comments?: string;
    resultDate: string;
    reviewedByDoctorId?: string;
    reviewedAt?: string;
    reviewStatus: string;
    createdAt: string;
  };
  test?: {
    id: string;
    labOrderId: string;
    loincCode: string;
    testName: string;
    status: string;
    instructions?: string;
  };
  order?: {
    id: string;
    patientId: string;
    orderingDoctorId: string;
    orderedDate: string;
    status: string;
    clinicalNotes?: string;
    priority: string;
  };
  reviews: Array<{
    id: string;
    labResultId: string;
    reviewedByDoctorId: string;
    reviewedAt: string;
    reviewStatus: string;
    reviewNotes?: string;
    recommendations?: string;
  }>;
}

interface BackendLabResultReview {
  id: string;
  labResultId: string;
  reviewedByDoctorId: string;
  reviewedAt: string;
  reviewStatus: string;
  reviewNotes?: string;
  recommendations?: string;
}

// Frontend-compatible types
export interface LabResultReview {
  id: string;
  documentId: string;
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

// Document type for compatibility
interface LabDocument {
  id: string;
  patientId: string;
  type: "Lab_Results";
  createdAt: string;
  notes?: string;
  data: {
    testType: string;
    testDate: string;
    laboratory: string;
    status: string;
    results: Array<{
      parameter: string;
      value: number | string;
      unit: string;
      referenceRange: string;
      status: string;
      notes?: string;
    }>;
    interpretation?: string;
  };
}

interface ApiResponse<T> {
  success: boolean;
  data: T;
  error?: string;
}

// Map backend priority to frontend priority
const mapPriority = (priority: string): "routine" | "urgent" | "critical" => {
  const lower = priority.toLowerCase();
  if (lower === "urgent" || lower === "high") return "urgent";
  if (lower === "critical") return "critical";
  return "routine";
};

// Map backend review status to frontend
const mapReviewStatus = (status: string): LabResultReview["reviewStatus"] => {
  const lower = status.toLowerCase();
  if (lower === "reviewed" || lower === "approved") return "approved";
  if (lower === "requiresfollowup" || lower === "requires_followup")
    return "requires_followup";
  if (lower === "in_review") return "in_review";
  return "pending_review";
};

class LabResultsReviewApiService {
  // Get lab result documents that need review
  async getLabResultsForReview(): Promise<ApiResponse<LabDocument[]>> {
    try {
      const response = await apiClient.get<BackendPendingLabResult[]>(
        "/lab/labresults/pending-review"
      );

      // Transform backend results to Document format
      const documents: LabDocument[] = response.data.map((r) => ({
        id: r.id,
        patientId: r.patientId,
        type: "Lab_Results" as const,
        createdAt: r.createdAt,
        notes: r.orderNotes || "",
        data: {
          testType: r.testName,
          testDate: r.resultDate.split("T")[0],
          laboratory: "Medical Laboratory",
          status: r.flag === "Normal" ? "Normal" : "Abnormal",
          results: [
            {
              parameter: r.testName,
              value: r.value || "",
              unit: r.unit || "",
              referenceRange: r.referenceRange || "",
              status: r.flag || "Normal",
            },
          ],
          interpretation: r.comments || "",
        },
      }));

      return { success: true, data: documents };
    } catch (error) {
      console.error("Failed to fetch lab results:", error);
      return {
        success: false,
        data: [],
        error: "Failed to fetch lab results for review",
      };
    }
  }

  // Get review status for lab results
  async getLabResultReviews(): Promise<ApiResponse<LabResultReview[]>> {
    try {
      const response = await apiClient.get<BackendPendingLabResult[]>(
        "/lab/labresults/pending-review"
      );

      // Transform to review format
      const reviews: LabResultReview[] = response.data.map((r) => ({
        id: `review_${r.id}`,
        documentId: r.id,
        reviewedById: "",
        reviewStatus: mapReviewStatus(r.reviewStatus),
        priority: mapPriority(r.priority),
      }));

      return { success: true, data: reviews };
    } catch (error) {
      console.error("Failed to fetch lab reviews:", error);
      return {
        success: false,
        data: [],
        error: "Failed to fetch lab result reviews",
      };
    }
  }

  // Submit a lab result review
  async submitLabResultReview(
    request: LabResultReviewRequest
  ): Promise<ApiResponse<LabResultReview>> {
    try {
      // Get user ID from localStorage token
      const token = localStorage.getItem("authToken");
      let doctorId = "00000000-0000-0000-0000-000000000000";
      if (token) {
        try {
          const payload = JSON.parse(atob(token.split(".")[1]));
          doctorId = payload.sub || payload.userId || doctorId;
        } catch {
          // Use default
        }
      }

      const backendStatus =
        request.reviewStatus === "approved"
          ? "Reviewed"
          : request.reviewStatus === "requires_followup"
            ? "RequiresFollowUp"
            : "InReview";

      const response = await apiClient.post<BackendLabResultReview>(
        `/lab/labresults/${request.documentId}/review`,
        {
          reviewedByDoctorId: doctorId,
          reviewStatus: backendStatus,
          reviewNotes: request.reviewNotes,
          recommendations: null,
        }
      );

      const review: LabResultReview = {
        id: response.data.id,
        documentId: response.data.labResultId,
        reviewedById: response.data.reviewedByDoctorId,
        reviewStatus: mapReviewStatus(response.data.reviewStatus),
        reviewNotes: response.data.reviewNotes || "",
        reviewedAt: response.data.reviewedAt,
        priority: "routine",
      };

      return { success: true, data: review };
    } catch (error) {
      console.error("Failed to submit review:", error);
      return {
        success: false,
        data: {} as LabResultReview,
        error: "Failed to submit lab result review",
      };
    }
  }

  // Quick approve a lab result
  async quickApproveLabResult(
    documentId: string
  ): Promise<ApiResponse<LabResultReview>> {
    try {
      // Get user ID from localStorage token
      const token = localStorage.getItem("authToken");
      let doctorId = "00000000-0000-0000-0000-000000000000";
      if (token) {
        try {
          const payload = JSON.parse(atob(token.split(".")[1]));
          doctorId = payload.sub || payload.userId || doctorId;
        } catch {
          // Use default
        }
      }

      const response = await apiClient.post<BackendLabResultReview>(
        `/lab/labresults/${documentId}/quick-approve`,
        { doctorId }
      );

      const review: LabResultReview = {
        id: response.data.id,
        documentId: response.data.labResultId,
        reviewedById: response.data.reviewedByDoctorId,
        reviewStatus: "approved",
        reviewNotes: response.data.reviewNotes || "",
        reviewedAt: response.data.reviewedAt,
        priority: "routine",
      };

      return { success: true, data: review };
    } catch (error) {
      console.error("Failed to quick approve:", error);
      return {
        success: false,
        data: {} as LabResultReview,
        error: "Failed to approve lab result",
      };
    }
  }

  // Get lab result document by ID
  async getLabResultDocument(
    documentId: string
  ): Promise<ApiResponse<LabDocument | null>> {
    try {
      const response = await apiClient.get<BackendLabResultDetail>(
        `/lab/labresults/${documentId}/detail`
      );

      const detail = response.data;
      const document: LabDocument = {
        id: detail.result.id,
        patientId: detail.result.patientId,
        type: "Lab_Results",
        createdAt: detail.result.createdAt,
        notes: detail.order?.clinicalNotes || "",
        data: {
          testType: detail.test?.testName || "Lab Test",
          testDate: detail.result.resultDate.split("T")[0],
          laboratory: "Medical Laboratory",
          status: detail.result.flag === "Normal" ? "Normal" : "Abnormal",
          results: [
            {
              parameter: detail.test?.testName || "Result",
              value: detail.result.value || "",
              unit: detail.result.unit || "",
              referenceRange: detail.result.referenceRange || "",
              status: detail.result.flag || "Normal",
            },
          ],
          interpretation: detail.result.comments || "",
        },
      };

      return { success: true, data: document };
    } catch (error) {
      console.error("Failed to fetch lab result document:", error);
      return {
        success: false,
        data: null,
        error: "Failed to fetch lab result document",
      };
    }
  }
}

export const labResultsReviewApi = new LabResultsReviewApiService();
