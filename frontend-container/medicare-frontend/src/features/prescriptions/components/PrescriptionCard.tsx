import React from "react";

import { PrescriptionCardProps } from "../types";

export const PrescriptionCard: React.FC<PrescriptionCardProps> = ({
  prescription,
  onSelect,
  onEdit,
  onDelete,
}) => {
  const getStatusColor = (status: string) => {
    switch (status) {
      case "active":
        return "bg-green-100 text-green-800";
      case "partially_dispensed":
        return "bg-yellow-100 text-yellow-800";
      case "fully_dispensed":
        return "bg-blue-100 text-blue-800";
      case "expired":
        return "bg-red-100 text-red-800";
      case "cancelled":
        return "bg-gray-100 text-gray-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  const formatDate = (date: Date) => {
    return new Date(date).toLocaleDateString();
  };

  const getMedicationsSummary = () => {
    if (prescription.medications.length === 1) {
      return prescription.medications[0].name;
    }
    return `${prescription.medications[0].name} + ${prescription.medications.length - 1} more`;
  };

  return (
    <div
      className="bg-white rounded-lg shadow p-4 hover:shadow-md transition-shadow cursor-pointer"
      onClick={() => onSelect(prescription)}
    >
      <div className="flex items-start justify-between mb-3">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-1">
            <h3 className="font-medium text-gray-900">
              Prescription #{prescription.id}
            </h3>
            <span
              className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(prescription.status)}`}
            >
              {prescription.status.replace("_", " ")}
            </span>
          </div>
          <p className="text-sm text-gray-600 mb-1">
            Diagnosis: {prescription.diagnosis}
          </p>
          <p className="text-sm text-gray-500">
            Medications: {getMedicationsSummary()}
          </p>
        </div>
        <div className="flex items-center gap-2 ml-4">
          <button
            className="px-3 py-1 text-sm border border-gray-300 rounded hover:bg-gray-50 transition-colors"
            onClick={(e: React.MouseEvent) => {
              e.stopPropagation();
              onEdit(prescription);
            }}
          >
            Edit
          </button>
          <button
            className="px-3 py-1 text-sm border border-red-300 text-red-600 rounded hover:bg-red-50 transition-colors"
            onClick={(e: React.MouseEvent) => {
              e.stopPropagation();
              onDelete(prescription.id);
            }}
          >
            Delete
          </button>
        </div>
      </div>

      <div className="flex items-center justify-between text-sm text-gray-500">
        <span>Issued: {formatDate(prescription.issuedAt)}</span>
        <span>Valid until: {formatDate(prescription.validUntil)}</span>
      </div>

      {prescription.notes && (
        <div className="mt-2 p-2 bg-gray-50 rounded text-sm text-gray-600">
          <strong>Notes:</strong> {prescription.notes}
        </div>
      )}
    </div>
  );
};
