import React from "react";
import { SearchInput } from "../../../shared/components";
import type { DocumentFilterProps, DocumentType } from "../types";

const documentTypes: { value: DocumentType | "All"; label: string }[] = [
  { value: "All", label: "All Types" },
  { value: "Prescription", label: "Prescription" },
  { value: "Referral", label: "Referral" },
  { value: "Sick_Leave", label: "Sick Leave" },
  { value: "VisitCard", label: "Visit Card" },
];

export const DocumentFilter: React.FC<DocumentFilterProps> = ({
  searchTerm,
  onSearchChange,
  typeFilter,
  onTypeFilterChange,
  appointmentFilter,
  onAppointmentFilterChange,
  appointments,
}) => {
  return (
    <div className="flex flex-wrap gap-4 mb-6 items-center">
      <SearchInput
        placeholder="Search documents..."
        value={searchTerm}
        onChange={(e) => onSearchChange(e.target.value)}
        className="w-64"
      />
      
      <select
        className="px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-200 text-sm"
        value={typeFilter}
        onChange={(e) => onTypeFilterChange(e.target.value as DocumentType | "All")}
      >
        {documentTypes.map((type) => (
          <option key={type.value} value={type.value}>
            {type.label}
          </option>
        ))}
      </select>
      
      <select
        className="px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-200 text-sm"
        value={appointmentFilter}
        onChange={(e) => onAppointmentFilterChange(e.target.value)}
      >
        <option value="">All Appointments</option>
        {appointments.map((appointment) => (
          <option key={appointment.id} value={appointment.id}>
            {new Date(appointment.date).toLocaleDateString()} – {appointment.specialization} ({appointment.doctor})
          </option>
        ))}
      </select>
    </div>
  );
};
