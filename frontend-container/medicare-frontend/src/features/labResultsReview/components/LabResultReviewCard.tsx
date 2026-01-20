import React from "react";
import { LabTestResult } from "@features/documents/types";
import { LabResultReviewCardProps } from "@features/labResultsReview/types";
import { AlertTriangle, CheckCircle, Clock, Eye } from "lucide-react";

export const LabResultReviewCard: React.FC<LabResultReviewCardProps> = ({
  labResult,
  onSelect,
  onReview,
  onApprove,
}) => {
  const { document, review } = labResult;
  const reviewStatus = review?.reviewStatus || "pending_review";
  const priority = review?.priority || "routine";

  const getStatusColor = (status: string) => {
    switch (status) {
      case "pending_review":
        return "bg-yellow-100 text-yellow-800";
      case "in_review":
        return "bg-blue-100 text-blue-800";
      case "approved":
        return "bg-green-100 text-green-800";
      case "requires_followup":
        return "bg-orange-100 text-orange-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case "critical":
        return "text-red-600";
      case "urgent":
        return "text-yellow-600";
      case "routine":
        return "text-gray-600";
      default:
        return "text-gray-600";
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case "pending_review":
        return <Clock className="w-4 h-4" />;
      case "in_review":
        return <Eye className="w-4 h-4" />;
      case "approved":
        return <CheckCircle className="w-4 h-4" />;
      case "requires_followup":
        return <AlertTriangle className="w-4 h-4" />;
      default:
        return <Clock className="w-4 h-4" />;
    }
  };

  // Check for critical or abnormal results
  const results = document.data.results || [];
  const hasCriticalResults = results.some(
    (result: LabTestResult) => result.status === "Critical"
  );
  const hasAbnormalResults = results.some(
    (result: LabTestResult) =>
      result.status === "High" || result.status === "Low"
  );

  return (
    <div className="bg-white rounded-lg shadow-md border border-gray-200 p-6 hover:shadow-lg transition-shadow">
      {/* Header */}
      <div className="flex items-start justify-between mb-4">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-2">
            <h3 className="text-lg font-semibold text-gray-900">
              {document.data.testType || "Lab Result"}
            </h3>
            <span
              className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(reviewStatus)}`}
            >
              {getStatusIcon(reviewStatus)}
              <span className="ml-1">
                {reviewStatus.replace("_", " ").toUpperCase()}
              </span>
            </span>
          </div>
          <p className="text-sm text-gray-600">
            Document Type: {document.type}
          </p>
          <p className="text-sm text-gray-500">
            Test Date:{" "}
            {document.data.testDate
              ? new Date(document.data.testDate).toLocaleDateString()
              : "N/A"}
          </p>
        </div>
        <div className="text-right">
          <span className={`text-sm font-medium ${getPriorityColor(priority)}`}>
            {priority.toUpperCase()}
          </span>
        </div>
      </div>

      {/* Patient Info */}
      <div className="mb-4 p-3 bg-gray-50 rounded-md">
        <p className="text-sm font-medium text-gray-900">
          Patient ID: {document.patientId}
        </p>
        <div className="flex justify-between text-sm text-gray-600 mt-1">
          <span>
            Created: {new Date(document.createdAt).toLocaleDateString()}
          </span>
          <span>Laboratory: {document.data.laboratory || "N/A"}</span>
        </div>
      </div>

      {/* Results Summary */}
      {results.length > 0 && (
        <div className="mb-4">
          <h4 className="text-sm font-medium text-gray-700 mb-2">
            Results Summary:
          </h4>
          <div className="space-y-1">
            {results.slice(0, 3).map((result: LabTestResult, index: number) => (
              <div
                key={index}
                className="flex justify-between items-center text-sm"
              >
                <span className="text-gray-600">{result.parameter}</span>
                <div className="flex items-center gap-2">
                  <span className="font-medium">
                    {result.value} {result.unit}
                  </span>
                  {result.status && result.status !== "Normal" && (
                    <span
                      className={`px-1 py-0.5 rounded text-xs font-bold ${
                        result.status === "Critical"
                          ? "bg-red-100 text-red-800"
                          : result.status === "High" || result.status === "Low"
                            ? "bg-yellow-100 text-yellow-800"
                            : "bg-green-100 text-green-800"
                      }`}
                    >
                      {result.status}
                    </span>
                  )}
                </div>
              </div>
            ))}
            {results.length > 3 && (
              <p className="text-xs text-gray-500">
                + {results.length - 3} more parameters
              </p>
            )}
          </div>
        </div>
      )}

      {/* Alert Indicators */}
      {(hasCriticalResults || hasAbnormalResults) && (
        <div className="mb-4 p-2 rounded-md bg-red-50 border border-red-200">
          <div className="flex items-center gap-2">
            <AlertTriangle className="w-4 h-4 text-red-600" />
            <span className="text-sm font-medium text-red-800">
              {hasCriticalResults
                ? "Critical values detected"
                : "Abnormal values detected"}
            </span>
          </div>
        </div>
      )}

      {/* Notes */}
      {document.notes && (
        <div className="mb-4 p-2 bg-blue-50 rounded-md">
          <p className="text-sm text-blue-800">{document.notes}</p>
        </div>
      )}

      {/* Review Notes */}
      {review?.reviewNotes && (
        <div className="mb-4 p-2 bg-green-50 rounded-md">
          <p className="text-xs text-green-600 font-medium mb-1">
            Review Notes:
          </p>
          <p className="text-sm text-green-800">{review.reviewNotes}</p>
        </div>
      )}

      {/* Actions */}
      <div className="flex gap-2 pt-4 border-t border-gray-200">
        <button
          onClick={() => onSelect(labResult)}
          className="flex-1 px-4 py-2 text-sm font-medium text-blue-600 bg-blue-50 rounded-md hover:bg-blue-100 transition-colors"
        >
          View Details
        </button>

        {reviewStatus === "pending_review" && (
          <>
            <button
              onClick={() => onReview(labResult)}
              className="flex-1 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 transition-colors"
            >
              Review
            </button>
            <button
              onClick={() => onApprove(document.id)}
              className="flex-1 px-4 py-2 text-sm font-medium text-white bg-green-600 rounded-md hover:bg-green-700 transition-colors"
            >
              Quick Approve
            </button>
          </>
        )}

        {reviewStatus === "in_review" && (
          <button
            onClick={() => onReview(labResult)}
            className="flex-1 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 transition-colors"
          >
            Continue Review
          </button>
        )}

        {reviewStatus === "requires_followup" && (
          <button
            onClick={() => onReview(labResult)}
            className="flex-1 px-4 py-2 text-sm font-medium text-white bg-orange-600 rounded-md hover:bg-orange-700 transition-colors"
          >
            Continue Review
          </button>
        )}
      </div>
    </div>
  );
};
