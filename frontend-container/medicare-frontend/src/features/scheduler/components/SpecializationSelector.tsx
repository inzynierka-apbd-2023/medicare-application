import React from "react";

import type { SpecializationSelectorProps } from "../types";

export const SpecializationSelector: React.FC<SpecializationSelectorProps> = ({
  specializations,
  selectedSpecialization,
  onSpecializationChange,
  disabled = false,
}) => {
  return (
    <select
      value={selectedSpecialization}
      onChange={(e) => onSpecializationChange(e.target.value)}
      disabled={disabled}
      className="flex-1 p-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed"
    >
      <option value="">Select Specialization</option>
      {specializations.map((specialization) => (
        <option key={specialization.id} value={specialization.id}>
          {specialization.name}
        </option>
      ))}
    </select>
  );
};
