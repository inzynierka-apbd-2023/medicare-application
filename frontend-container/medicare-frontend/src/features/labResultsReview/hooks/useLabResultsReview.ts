import { useCallback, useEffect, useState } from "react";

import { labResultsReviewApi } from "../../../shared/services/labResultsReviewApi";
import { LabTestResult } from "../../documents/types";
import {
  LabResultFilter,
  LabResultReviewFormData,
  LabResultWithReview,
} from "../types";

export const useLabResultsReview = () => {
  const [labResults, setLabResults] = useState<LabResultWithReview[]>([]);
  const [selectedResult, setSelectedResult] =
    useState<LabResultWithReview | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Load lab results and their review status
  const loadLabResults = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);

      // Fetch lab result documents
      const documentsResponse =
        await labResultsReviewApi.getLabResultsForReview();
      if (!documentsResponse.success) {
        throw new Error(
          documentsResponse.error || "Failed to fetch lab results"
        );
      }

      // Fetch review statuses
      const reviewsResponse = await labResultsReviewApi.getLabResultReviews();
      if (!reviewsResponse.success) {
        throw new Error(reviewsResponse.error || "Failed to fetch review data");
      }

      // Combine documents with their review status
      const combinedResults: LabResultWithReview[] = documentsResponse.data.map(
        (document) => {
          const review = reviewsResponse.data.find(
            (r) => r.documentId === document.id
          );
          const result: LabResultWithReview = {
            document,
            ...(review && { review }),
          };
          return result;
        }
      );

      setLabResults(combinedResults);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to load lab results"
      );
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Initialize data
  useEffect(() => {
    loadLabResults();
  }, [loadLabResults]);

  const reviewLabResult = useCallback(
    async (documentId: string, reviewData: LabResultReviewFormData) => {
      try {
        const response = await labResultsReviewApi.submitLabResultReview({
          documentId,
          reviewStatus: reviewData.reviewStatus,
          reviewNotes: reviewData.reviewNotes,
        });

        if (!response.success) {
          throw new Error(response.error || "Failed to submit review");
        }

        // Update local state
        setLabResults((prev) =>
          prev.map((item) =>
            item.document.id === documentId
              ? { ...item, review: response.data }
              : item
          )
        );

        return true;
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "Failed to review lab result"
        );
        throw err;
      }
    },
    []
  );

  const approveLabResult = useCallback(async (documentId: string) => {
    try {
      const response =
        await labResultsReviewApi.quickApproveLabResult(documentId);

      if (!response.success) {
        throw new Error(response.error || "Failed to approve lab result");
      }

      // Update local state
      setLabResults((prev) =>
        prev.map((item) =>
          item.document.id === documentId
            ? { ...item, review: response.data }
            : item
        )
      );

      return true;
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to approve lab result"
      );
      throw err;
    }
  }, []);

  const filterLabResults = useCallback(
    (filters: LabResultFilter) => {
      return labResults.filter((item) => {
        const { document, review } = item;

        // Filter by review status
        if (filters.reviewStatus) {
          const currentStatus = review?.reviewStatus || "pending_review";
          if (currentStatus !== filters.reviewStatus) return false;
        }

        // Filter by priority
        if (filters.priority && review?.priority !== filters.priority)
          return false;

        // Filter by test type
        if (
          filters.testType &&
          !document.data.testType
            ?.toLowerCase()
            .includes(filters.testType.toLowerCase())
        )
          return false;

        // Filter by search term
        if (filters.searchTerm) {
          const term = filters.searchTerm.toLowerCase();
          const matchesSearch =
            document.data.testType?.toLowerCase().includes(term) ||
            document.notes?.toLowerCase().includes(term) ||
            document.data.results?.some((r: LabTestResult) =>
              r.parameter.toLowerCase().includes(term)
            );
          if (!matchesSearch) return false;
        }

        // Filter by date range
        if (filters.dateFrom && document.data.testDate) {
          const testDate = new Date(document.data.testDate);
          if (testDate < filters.dateFrom) return false;
        }
        if (filters.dateTo && document.data.testDate) {
          const testDate = new Date(document.data.testDate);
          if (testDate > filters.dateTo) return false;
        }

        return true;
      });
    },
    [labResults]
  );

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const refreshData = useCallback(() => {
    loadLabResults();
  }, [loadLabResults]);

  return {
    labResults,
    selectedResult,
    isLoading,
    error,
    reviewLabResult,
    approveLabResult,
    filterLabResults,
    setSelectedResult,
    clearError,
    refreshData,
  };
};
