import React, { useState } from "react";
import { LabTestResult } from "@features/documents/types";
import { LabResultReviewFormProps } from "@features/labResultsReview/types";
import { X } from "lucide-react";

export const LabResultReviewForm: React.FC<LabResultReviewFormProps> = ({
  labResult,
  onSubmit,
  onCancel,
  isLoading = false,
}) => {
  const { document, review } = labResult;

  const [formData, setFormData] = useState({
    reviewStatus: review?.reviewStatus || "pending_review",
    reviewNotes: review?.reviewNotes || "",
    priority: review?.priority || "routine",
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit({
      reviewStatus: formData.reviewStatus as
        | "approved"
        | "requires_followup"
        | "in_review",
      reviewNotes: formData.reviewNotes,
    });
  };

  const results = document.data.results || [];

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div>
            <h2 className="text-xl font-semibold text-gray-900">
              Review Lab Result
            </h2>
            <p className="text-sm text-gray-600">
              {document.data.testType || "Lab Result"}
            </p>
          </div>
          <button
            onClick={onCancel}
            className="p-2 hover:bg-gray-100 rounded-md transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6">
          {/* Lab Result Details */}
          {results.length > 0 && (
            <div className="mb-6 p-4 bg-gray-50 rounded-lg">
              <h3 className="font-medium text-gray-900 mb-3">Test Results</h3>
              <div className="space-y-3">
                {results.map((result: LabTestResult, index: number) => (
                  <div
                    key={index}
                    className="flex justify-between items-center p-3 bg-white rounded border"
                  >
                    <div>
                      <p className="font-medium text-gray-900">
                        {result.parameter}
                      </p>
                      <p className="text-sm text-gray-600">
                        Reference: {result.referenceRange || "N/A"}
                      </p>
                    </div>
                    <div className="text-right">
                      <div className="flex items-center gap-2">
                        <span className="font-bold">
                          {result.value} {result.unit}
                        </span>
                        {result.status && result.status !== "Normal" && (
                          <span
                            className={`px-2 py-1 rounded text-xs font-bold ${
                              result.status === "Critical"
                                ? "bg-red-100 text-red-800"
                                : result.status === "High" ||
                                    result.status === "Low"
                                  ? "bg-yellow-100 text-yellow-800"
                                  : "bg-green-100 text-green-800"
                            }`}
                          >
                            {result.status}
                          </span>
                        )}
                      </div>
                      <p
                        className={`text-sm font-medium ${
                          result.status === "Critical"
                            ? "text-red-600"
                            : result.status === "High" ||
                                result.status === "Low"
                              ? "text-yellow-600"
                              : "text-green-600"
                        }`}
                      >
                        {result.status || "Normal"}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Review Status */}
          <div className="mb-6">
            <label className="block text-sm font-medium text-gray-700 mb-3">
              Review Status
            </label>
            <div className="space-y-2">
              {[
                {
                  value: "in_review",
                  label: "In Review - Currently reviewing",
                  color: "blue",
                },
                {
                  value: "approved",
                  label: "Approved - Normal results",
                  color: "green",
                },
                {
                  value: "requires_followup",
                  label: "Requires Follow-up",
                  color: "orange",
                },
              ].map((option) => (
                <label key={option.value} className="flex items-center">
                  <input
                    type="radio"
                    name="reviewStatus"
                    value={option.value}
                    checked={formData.reviewStatus === option.value}
                    onChange={(e) =>
                      setFormData((prev) => ({
                        ...prev,
                        reviewStatus: e.target.value as
                          | "approved"
                          | "requires_followup"
                          | "in_review",
                      }))
                    }
                    className={`mr-3 text-${option.color}-600 focus:ring-${option.color}-500`}
                  />
                  <span className="text-sm text-gray-700">{option.label}</span>
                </label>
              ))}
            </div>
          </div>

          {/* Review Notes */}
          <div className="mb-6">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Review Notes
            </label>
            <textarea
              value={formData.reviewNotes}
              onChange={(e) =>
                setFormData((prev) => ({
                  ...prev,
                  reviewNotes: e.target.value,
                }))
              }
              rows={4}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Add your review notes, interpretation, or recommendations..."
            />
          </div>

          {/* Original Notes */}
          {document.notes && (
            <div className="mb-6 p-4 bg-blue-50 rounded-lg">
              <h4 className="text-sm font-medium text-blue-900 mb-2">
                Original Notes:
              </h4>
              <p className="text-sm text-blue-800">{document.notes}</p>
            </div>
          )}

          {/* Actions */}
          <div className="flex gap-3 pt-6 border-t border-gray-200">
            <button
              type="button"
              onClick={onCancel}
              className="flex-1 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 transition-colors"
              disabled={isLoading}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="flex-1 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 transition-colors disabled:opacity-50"
              disabled={isLoading}
            >
              {isLoading ? "Submitting..." : "Submit Review"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
