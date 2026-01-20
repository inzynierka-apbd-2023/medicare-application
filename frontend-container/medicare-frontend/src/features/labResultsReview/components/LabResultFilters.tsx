import React from "react";
import { LabResultFiltersProps } from "@features/labResultsReview/types";

export const LabResultFilters: React.FC<LabResultFiltersProps> = ({
  filters,
  onFiltersChange,
  onClearFilters,
}) => {
  const handleReviewStatusChange = (value: string) => {
    if (value === "all") {
      const { reviewStatus: _removed, ...rest } = filters;
      onFiltersChange(rest);
    } else {
      onFiltersChange({
        ...filters,
        reviewStatus: value as
          | "pending_review"
          | "in_review"
          | "approved"
          | "requires_followup",
      });
    }
  };

  const handlePriorityChange = (value: string) => {
    if (value === "all") {
      const { priority: _removed, ...rest } = filters;
      onFiltersChange(rest);
    } else {
      onFiltersChange({
        ...filters,
        priority: value as "routine" | "urgent" | "critical",
      });
    }
  };

  const handleTestTypeChange = (value: string) => {
    if (value === "") {
      const { testType: _removed, ...rest } = filters;
      onFiltersChange(rest);
    } else {
      onFiltersChange({ ...filters, testType: value });
    }
  };

  const handleSearchChange = (value: string) => {
    if (value === "") {
      const { searchTerm: _removed, ...rest } = filters;
      onFiltersChange(rest);
    } else {
      onFiltersChange({ ...filters, searchTerm: value });
    }
  };

  return (
    <div className="bg-white rounded-lg shadow p-6 mb-6">
      <h3 className="text-lg font-medium text-gray-900 mb-4">Filters</h3>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        {/* Search */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Search
          </label>
          <input
            type="text"
            value={filters.searchTerm || ""}
            onChange={(e) => handleSearchChange(e.target.value)}
            placeholder="Search tests, patients..."
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Review Status Filter */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Review Status
          </label>
          <select
            value={filters.reviewStatus || "all"}
            onChange={(e) => handleReviewStatusChange(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="all">All Statuses</option>
            <option value="pending_review">Pending Review</option>
            <option value="in_review">In Review</option>
            <option value="approved">Approved</option>
            <option value="requires_followup">Requires Follow-up</option>
          </select>
        </div>

        {/* Priority Filter */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Priority
          </label>
          <select
            value={filters.priority || "all"}
            onChange={(e) => handlePriorityChange(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="all">All Priorities</option>
            <option value="critical">Critical</option>
            <option value="urgent">Urgent</option>
            <option value="routine">Routine</option>
          </select>
        </div>

        {/* Test Type Filter */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Test Type
          </label>
          <input
            type="text"
            value={filters.testType || ""}
            onChange={(e) => handleTestTypeChange(e.target.value)}
            placeholder="Filter by test type..."
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      {/* Clear Filters Button */}
      <div className="mt-4 flex justify-end">
        <button
          onClick={onClearFilters}
          className="px-4 py-2 text-sm font-medium text-gray-600 bg-gray-100 rounded-md hover:bg-gray-200 transition-colors"
        >
          Clear Filters
        </button>
      </div>
    </div>
  );
};
