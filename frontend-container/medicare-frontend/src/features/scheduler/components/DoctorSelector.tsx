import React from "react";
import type { DoctorSelectorProps } from "../types";

export const DoctorSelector: React.FC<DoctorSelectorProps> = ({
  doctors,
  selectedDoctor,
  onDoctorChange,
  disabled = false,
}) => {
  return (
    <select
      value={selectedDoctor}
      onChange={(e) => onDoctorChange(e.target.value)}
      disabled={disabled}
      className="flex-1 p-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed"
    >
      <option value="">Select Doctor</option>
      {doctors.map((doctor) => (
        <option key={doctor.id} value={doctor.id}>
          {doctor.name}
        </option>
      ))}
    </select>
  );
};
