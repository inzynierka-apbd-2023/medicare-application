import React from "react";
import {
  Prescription,
  PrescriptionListProps,
} from "@features/prescriptions/types";

import { PrescriptionCard } from "./PrescriptionCard";

export const PrescriptionList: React.FC<PrescriptionListProps> = ({
  prescriptions,
  onPrescriptionSelect,
  onPrescriptionEdit,
  onPrescriptionDelete,
  isLoading = false,
}) => {
  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
      </div>
    );
  }

  if (prescriptions.length === 0) {
    return (
      <div className="text-center py-12">
        <div className="text-6xl mb-4">📋</div>
        <h3 className="text-lg font-medium text-gray-900 mb-2">
          No prescriptions found
        </h3>
        <p className="text-gray-500">
          No prescriptions match your current filters.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {prescriptions.map((prescription: Prescription) => (
        <PrescriptionCard
          key={prescription.id}
          prescription={prescription}
          onSelect={onPrescriptionSelect}
          onEdit={onPrescriptionEdit}
          onDelete={onPrescriptionDelete}
        />
      ))}
    </div>
  );
};
