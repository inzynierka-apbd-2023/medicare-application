import { toastMessages } from "@shared/toast/toastMessages";

import { api, type ApiResponse, handleApiCall } from "./api";

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

const mapPriority = (priority: string): "routine" | "urgent" | "critical" => {
  const lower = priority.toLowerCase();
  if (lower === "urgent" || lower === "high") return "urgent";
  if (lower === "critical") return "critical";
  return "routine";
};

const mapReviewStatus = (status: string): LabResultReview["reviewStatus"] => {
  const lower = status.toLowerCase();
  if (lower === "reviewed" || lower === "approved") return "approved";
  if (lower === "requiresfollowup" || lower === "requires_followup")
    return "requires_followup";
  if (lower === "in_review") return "in_review";
  return "pending_review";
};

const transformPendingResultToDocument = (
  r: BackendPendingLabResult
): LabDocument => ({
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
});

const transformPendingResultToReview = (
  r: BackendPendingLabResult
): LabResultReview => ({
  id: `review_${r.id}`,
  documentId: r.id,
  reviewedById: "",
  reviewStatus: mapReviewStatus(r.reviewStatus),
  priority: mapPriority(r.priority),
});

const transformBackendReviewToReview = (
  response: BackendLabResultReview,
  status: LabResultReview["reviewStatus"] = mapReviewStatus(
    response.reviewStatus
  )
): LabResultReview => ({
  id: response.id,
  documentId: response.labResultId,
  reviewedById: response.reviewedByDoctorId,
  reviewStatus: status,
  reviewNotes: response.reviewNotes || "",
  reviewedAt: response.reviewedAt,
  priority: "routine",
});

const transformDetailToDocument = (
  detail: BackendLabResultDetail
): LabDocument => ({
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
});

const mapReviewStatusToBackend = (
  status: LabResultReviewRequest["reviewStatus"]
): string => {
  if (status === "approved") return "Reviewed";
  if (status === "requires_followup") return "RequiresFollowUp";
  return "InReview";
};

export const labResultsReviewApi = {
  getLabResultsForReview: async (): Promise<ApiResponse<LabDocument[]>> => {
    return handleApiCall(
      async () => {
        const data = await api.get<BackendPendingLabResult[]>(
          "/lab/labresults/pending-review"
        );
        return data.map(transformPendingResultToDocument);
      },
      { showToastOnSuccess: false }
    );
  },

  getLabResultReviews: async (): Promise<ApiResponse<LabResultReview[]>> => {
    return handleApiCall(
      async () => {
        const data = await api.get<BackendPendingLabResult[]>(
          "/lab/labresults/pending-review"
        );
        return data.map(transformPendingResultToReview);
      },
      { showToastOnSuccess: false }
    );
  },

  submitLabResultReview: async (
    request: LabResultReviewRequest
  ): Promise<ApiResponse<LabResultReview>> => {
    return handleApiCall(
      async () => {
        const backendStatus = mapReviewStatusToBackend(request.reviewStatus);

        const response = await api.post<BackendLabResultReview>(
          `/lab/labresults/${request.documentId}/review`,
          {
            reviewStatus: backendStatus,
            reviewNotes: request.reviewNotes,
            recommendations: null,
          }
        );

        return transformBackendReviewToReview(response);
      },
      {
        showToastOnSuccess: true,
        successMessage: toastMessages.labResultsReview.submitReviewSuccess,
      }
    );
  },

  quickApproveLabResult: async (
    documentId: string
  ): Promise<ApiResponse<LabResultReview>> => {
    return handleApiCall(
      async () => {
        const response = await api.post<BackendLabResultReview>(
          `/lab/labresults/${documentId}/quick-approve`,
          {}
        );

        return transformBackendReviewToReview(response, "approved");
      },
      {
        showToastOnSuccess: true,
        successMessage: toastMessages.labResultsReview.quickApproveSuccess,
      }
    );
  },

  getLabResultDocument: async (
    documentId: string
  ): Promise<ApiResponse<LabDocument | null>> => {
    return handleApiCall(
      async () => {
        const detail = await api.get<BackendLabResultDetail>(
          `/lab/labresults/${documentId}/detail`
        );
        return transformDetailToDocument(detail);
      },
      { showToastOnSuccess: false }
    );
  },
};
