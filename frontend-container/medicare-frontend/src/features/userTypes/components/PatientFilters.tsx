import React from "react";
import type { PatientFiltersProps, SortKey } from "@features/userTypes/types";
import { SearchInput } from "@shared/components";

const sortOptions: { value: SortKey; label: string }[] = [
  { value: "name", label: "Sort: Name" },
  { value: "lastVisit", label: "Sort: Last Visit" },
  { value: "visits", label: "Sort: Total Visits" },
  { value: "age", label: "Sort: Age" },
];

export const PatientFilters: React.FC<PatientFiltersProps> = ({
  searchTerm,
  onSearchChange,
  sortKey,
  onSortChange,
}) => {
  return (
    <div className="flex flex-col md:flex-row md:items-center md:space-x-4 mb-6 space-y-4 md:space-y-0">
      <SearchInput
        placeholder="Search patients..."
        value={searchTerm}
        onChange={(e) => onSearchChange(e.target.value)}
        className="flex-1"
      />

      <div>
        <select
          className="bg-white border border-gray-200 px-3 py-2 rounded-lg text-sm text-gray-800 shadow-sm focus:outline-none focus:ring-1 focus:ring-blue-100 focus:border-blue-400"
          value={sortKey}
          onChange={(e) => onSortChange(e.target.value as SortKey)}
        >
          {sortOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>
    </div>
  );
};
