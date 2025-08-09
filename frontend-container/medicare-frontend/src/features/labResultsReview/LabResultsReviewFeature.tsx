import React, { useState } from "react";

import { useLabResultsReview } from "./hooks/useLabResultsReview";
import {
  LabResultDetailsModal,
  LabResultFilters,
  LabResultReviewForm,
  LabResultsReviewList,
} from "./components";
import {
  LabResultFilter,
  LabResultReviewFormData,
  LabResultWithReview,
} from "./types";

export const LabResultsReviewFeature: React.FC = () => {
  const {
    labResults,
    isLoading,
    error,
    reviewLabResult,
    approveLabResult,
    filterLabResults,
    setSelectedResult,
    clearError,
    refreshData: _refreshData,
  } = useLabResultsReview();

  const [showReviewForm, setShowReviewForm] = useState(false);
  const [showDetailsModal, setShowDetailsModal] = useState(false);
  const [reviewingResult, setReviewingResult] =
    useState<LabResultWithReview | null>(null);
  const [selectedResultForDetails, setSelectedResultForDetails] =
    useState<LabResultWithReview | null>(null);
  const [filters, setFilters] = useState<LabResultFilter>({});

  // Filter results
  const filteredResults = filterLabResults(filters);

  const handleResultSelect = (result: LabResultWithReview) => {
    setSelectedResult(result);
    setSelectedResultForDetails(result);
    setShowDetailsModal(true);
  };

  const handleResultReview = (result: LabResultWithReview) => {
    setReviewingResult(result);
    setShowReviewForm(true);
  };

  const handleQuickApprove = async (documentId: string) => {
    if (window.confirm("Are you sure you want to approve this lab result?")) {
      try {
        await approveLabResult(documentId);
      } catch (_error) {
        // Error is handled by the hook
      }
    }
  };

  const handleReviewSubmit = async (data: LabResultReviewFormData) => {
    if (reviewingResult) {
      try {
        await reviewLabResult(reviewingResult.document.id, data);
        setShowReviewForm(false);
        setReviewingResult(null);
      } catch (_error) {
        // Error is handled by the hook
      }
    }
  };

  const handleReviewCancel = () => {
    setShowReviewForm(false);
    setReviewingResult(null);
  };

  const handleDetailsClose = () => {
    setShowDetailsModal(false);
    setSelectedResultForDetails(null);
  };

  // Helper function to get review status for stats
  const getReviewStatus = (result: LabResultWithReview) => {
    return result.review?.reviewStatus || "pending_review";
  };

  // Helper function to get priority for stats
  const getPriority = (result: LabResultWithReview) => {
    return result.review?.priority || "routine";
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6 space-y-6">
      {/* Header */}
      <div className="text-center">
        <h1 className="text-3xl font-bold text-gray-900">Lab Results Review</h1>
        <p className="text-lg text-gray-600 mt-2">
          Review and approve patient lab results
        </p>
        <div className="mt-4 text-sm text-gray-500">
          {filteredResults.length} results found
        </div>
      </div>

      {/* Error Display */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-md p-4">
          <div className="flex items-center">
            <div className="text-red-800">
              <h3 className="text-sm font-medium">Error</h3>
              <p className="text-sm mt-1">{error}</p>
            </div>
            <button
              onClick={clearError}
              className="ml-auto text-red-400 hover:text-red-600"
            >
              ×
            </button>
          </div>
        </div>
      )}

      <LabResultFilters
        filters={filters}
        onFiltersChange={setFilters}
        onClearFilters={() => setFilters({})}
      />

      {/* Quick Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white p-4 rounded-lg shadow">
          <div className="text-2xl font-bold text-red-600">
            {labResults.filter((r) => getPriority(r) === "critical").length}
          </div>
          <div className="text-sm text-gray-600">Critical Priority</div>
        </div>
        <div className="bg-white p-4 rounded-lg shadow">
          <div className="text-2xl font-bold text-yellow-600">
            {
              labResults.filter((r) => getReviewStatus(r) === "pending_review")
                .length
            }
          </div>
          <div className="text-sm text-gray-600">Pending Review</div>
        </div>
        <div className="bg-white p-4 rounded-lg shadow">
          <div className="text-2xl font-bold text-blue-600">
            {
              labResults.filter(
                (r) => getReviewStatus(r) === "requires_followup"
              ).length
            }
          </div>
          <div className="text-sm text-gray-600">Requires Follow-up</div>
        </div>
        <div className="bg-white p-4 rounded-lg shadow">
          <div className="text-2xl font-bold text-green-600">
            {labResults.filter((r) => getReviewStatus(r) === "approved").length}
          </div>
          <div className="text-sm text-gray-600">Approved</div>
        </div>
      </div>

      {/* Results List */}
      <div className="bg-white rounded-lg shadow p-6">
        <LabResultsReviewList
          labResults={filteredResults}
          onResultSelect={handleResultSelect}
          onResultReview={handleResultReview}
          onResultApprove={handleQuickApprove}
          isLoading={isLoading}
        />
      </div>

      {/* Review Form Modal */}
      {showReviewForm && reviewingResult && (
        <LabResultReviewForm
          labResult={reviewingResult}
          onSubmit={handleReviewSubmit}
          onCancel={handleReviewCancel}
          isLoading={false}
        />
      )}

      {/* Details Modal */}
      {showDetailsModal && selectedResultForDetails && (
        <LabResultDetailsModal
          labResult={selectedResultForDetails}
          onClose={handleDetailsClose}
        />
      )}
    </div>
  );
};
