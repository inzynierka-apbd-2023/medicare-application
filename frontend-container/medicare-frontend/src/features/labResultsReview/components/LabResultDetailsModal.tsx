import React from "react";
import { LabTestResult } from "@features/documents/types";
import { LabResultWithReview } from "@features/labResultsReview/types";
import { AlertTriangle, CheckCircle, Clock, Eye, X } from "lucide-react";

interface LabResultDetailsModalProps {
  labResult: LabResultWithReview;
  onClose: () => void;
}

export const LabResultDetailsModal: React.FC<LabResultDetailsModalProps> = ({
  labResult,
  onClose,
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

  const results = document.data.results || [];

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        {/* Modal Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div className="flex-1">
            <h2 className="text-xl font-semibold text-gray-900">
              {document.data.testType || "Lab Result Details"}
            </h2>
            <p className="text-sm text-gray-600 mt-1">
              Test Date: {document.data.testDate} • Laboratory:{" "}
              {document.data.laboratory}
            </p>
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Modal Content */}
        <div className="p-6 space-y-6">
          {/* Status and Priority */}
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2">
              <span
                className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(reviewStatus)} flex items-center gap-1`}
              >
                {getStatusIcon(reviewStatus)}
                {reviewStatus.replace("_", " ").toUpperCase()}
              </span>
            </div>
            <div className="flex items-center gap-2">
              <span
                className={`text-sm font-medium ${getPriorityColor(priority)}`}
              >
                {priority.toUpperCase()} PRIORITY
              </span>
            </div>
          </div>

          {/* Patient Information */}
          <div className="bg-gray-50 rounded-lg p-4">
            <h3 className="font-medium text-gray-900 mb-2">
              Patient Information
            </h3>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-gray-600">Patient ID:</span>
                <span className="ml-2 font-medium">{document.patientId}</span>
              </div>
              <div>
                <span className="text-gray-600">Appointment ID:</span>
                <span className="ml-2 font-medium">
                  {document.appointmentId}
                </span>
              </div>
            </div>
          </div>

          {/* Test Results */}
          <div>
            <h3 className="font-medium text-gray-900 mb-4">Test Results</h3>
            <div className="space-y-3">
              {results.map((result: LabTestResult, index: number) => (
                <div
                  key={index}
                  className="border border-gray-200 rounded-lg p-4"
                >
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <h4 className="font-medium text-gray-900">
                        {result.parameter}
                      </h4>
                      <p className="text-sm text-gray-600 mt-1">
                        Reference Range: {result.referenceRange || "N/A"}
                      </p>
                      {result.notes && (
                        <p className="text-sm text-gray-600 mt-1">
                          {result.notes}
                        </p>
                      )}
                    </div>
                    <div className="text-right ml-4">
                      <div className="flex items-center gap-2">
                        <span className="text-lg font-bold text-gray-900">
                          {result.value} {result.unit}
                        </span>
                        {result.status && result.status !== "Normal" && (
                          <span
                            className={`px-2 py-1 rounded text-xs font-medium ${
                              result.status === "High" ||
                              result.status === "Low"
                                ? "bg-yellow-100 text-yellow-800"
                                : result.status === "Critical"
                                  ? "bg-red-100 text-red-800"
                                  : "bg-gray-100 text-gray-800"
                            }`}
                          >
                            {result.status}
                          </span>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Clinical Notes */}
          {document.notes && (
            <div>
              <h3 className="font-medium text-gray-900 mb-2">Clinical Notes</h3>
              <div className="bg-gray-50 rounded-lg p-4">
                <p className="text-sm text-gray-700">{document.notes}</p>
              </div>
            </div>
          )}

          {/* Review Information */}
          {review && (
            <div>
              <h3 className="font-medium text-gray-900 mb-2">
                Review Information
              </h3>
              <div className="bg-gray-50 rounded-lg p-4 space-y-2">
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <span className="text-gray-600">Reviewed By:</span>
                    <span className="ml-2 font-medium">
                      {review.reviewedById}
                    </span>
                  </div>
                  {review.reviewedAt && (
                    <div>
                      <span className="text-gray-600">Review Date:</span>
                      <span className="ml-2 font-medium">
                        {new Date(review.reviewedAt).toLocaleDateString()}
                      </span>
                    </div>
                  )}
                </div>
                {review.reviewNotes && (
                  <div className="mt-3">
                    <span className="text-gray-600 text-sm">Review Notes:</span>
                    <p className="text-sm text-gray-700 mt-1">
                      {review.reviewNotes}
                    </p>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Lab Summary */}
          {document.data.interpretation && (
            <div>
              <h3 className="font-medium text-gray-900 mb-2">
                Laboratory Interpretation
              </h3>
              <div className="bg-blue-50 rounded-lg p-4">
                <p className="text-sm text-blue-900">
                  {document.data.interpretation}
                </p>
              </div>
            </div>
          )}
        </div>

        {/* Modal Footer */}
        <div className="border-t border-gray-200 px-6 py-4">
          <div className="flex justify-end">
            <button
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200 transition-colors"
            >
              Close
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
