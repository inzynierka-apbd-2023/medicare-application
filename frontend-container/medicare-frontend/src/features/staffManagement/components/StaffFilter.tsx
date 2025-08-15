import React from "react";

import { SearchInput } from "../../../shared/components";
import type { StaffFilterProps, StaffRole } from "../types";

export const StaffFilter: React.FC<StaffFilterProps> = ({
  searchTerm,
  onSearchChange,
  roleFilter,
  onRoleFilterChange,
}) => {
  const roleOptions = [
    { value: "All", label: "All Roles" },
    { value: "Doctor", label: "Doctors" },
    { value: "Receptionist", label: "Receptionists" },
  ];

  const handleRoleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    onRoleFilterChange(e.target.value as StaffRole | "All");
  };

  return (
    <div className="flex flex-col sm:flex-row gap-4 mb-6">
      {/* Search Input */}
      <div className="flex-1">
        <SearchInput
          value={searchTerm}
          onChange={(e) => onSearchChange(e.target.value)}
          placeholder="Search by name, email, specialization, or department..."
          className="w-full"
        />
      </div>

      {/* Role Filter */}
      <div className="w-full sm:w-48">
        <select
          className="w-full px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-200 text-sm"
          value={roleFilter}
          onChange={handleRoleChange}
        >
          {roleOptions.map((role) => (
            <option key={role.value} value={role.value}>
              {role.label}
            </option>
          ))}
        </select>
      </div>
    </div>
  );
};
