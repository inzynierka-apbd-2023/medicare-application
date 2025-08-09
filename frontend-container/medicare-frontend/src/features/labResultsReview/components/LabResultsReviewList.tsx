import React from "react";

import { LabResultsReviewListProps, LabResultWithReview } from "../types";

import { LabResultReviewCard } from "./LabResultReviewCard";

export const LabResultsReviewList: React.FC<LabResultsReviewListProps> = ({
  labResults,
  onResultSelect,
  onResultReview,
  onResultApprove,
  isLoading = false,
}) => {
  if (isLoading) {
    return (
      <div className="space-y-4">
        {[...Array(3)].map((_, index) => (
          <div
            key={index}
            className="bg-white rounded-lg shadow-md border border-gray-200 p-6 animate-pulse"
          >
            <div className="h-4 bg-gray-300 rounded w-3/4 mb-2"></div>
            <div className="h-3 bg-gray-300 rounded w-1/2 mb-4"></div>
            <div className="space-y-2">
              <div className="h-3 bg-gray-300 rounded w-full"></div>
              <div className="h-3 bg-gray-300 rounded w-5/6"></div>
            </div>
          </div>
        ))}
      </div>
    );
  }

  if (labResults.length === 0) {
    return (
      <div className="text-center py-12">
        <div className="w-24 h-24 mx-auto mb-4 text-gray-300">
          <svg fill="currentColor" viewBox="0 0 24 24">
            <path d="M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-5 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z" />
          </svg>
        </div>
        <h3 className="text-lg font-medium text-gray-900 mb-2">
          No lab results found
        </h3>
        <p className="text-gray-500">
          No lab results match your current filters or there are no results
          pending review.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {labResults.map((labResult: LabResultWithReview) => (
        <LabResultReviewCard
          key={labResult.document.id}
          labResult={labResult}
          onSelect={onResultSelect}
          onReview={onResultReview}
          onApprove={onResultApprove}
        />
      ))}
    </div>
  );
};
