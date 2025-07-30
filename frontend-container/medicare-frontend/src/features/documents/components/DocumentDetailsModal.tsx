import React from "react";

import { Button, Modal } from "../../../shared/components";
import type {
  Document,
  DocumentDetailsModalProps,
  DocumentType,
} from "../types";

const renderDocumentDetails = (document: Document) => {
  switch (document.type as DocumentType) {
    case "Prescription":
      return (
        <div className="space-y-2">
          <div>
            <span className="font-medium">Medication:</span>{" "}
            {document.data.medication}
          </div>
          <div>
            <span className="font-medium">Dosage:</span> {document.data.dosage}
          </div>
          <div>
            <span className="font-medium">Frequency:</span>{" "}
            {document.data.frequency}
          </div>
          <div>
            <span className="font-medium">Duration (days):</span>{" "}
            {document.data.duration_days}
          </div>
          <div>
            <span className="font-medium">Instructions:</span>{" "}
            {document.data.instructions}
          </div>
        </div>
      );
    case "Referral":
      return (
        <div className="space-y-2">
          <div>
            <span className="font-medium">Specialty:</span>{" "}
            {document.data.specialty}
          </div>
          <div>
            <span className="font-medium">Referred To:</span>{" "}
            {document.data.referredTo}
          </div>
          <div>
            <span className="font-medium">Valid From:</span>{" "}
            {document.data.validFrom}
          </div>
          <div>
            <span className="font-medium">Valid To:</span>{" "}
            {document.data.validTo}
          </div>
        </div>
      );
    case "Sick_Leave":
      return (
        <div className="space-y-2">
          <div>
            <span className="font-medium">Start Date:</span>{" "}
            {document.data.startDate}
          </div>
          <div>
            <span className="font-medium">End Date:</span>{" "}
            {document.data.endDate}
          </div>
          <div>
            <span className="font-medium">Days Off:</span>{" "}
            {document.data.daysOff}
          </div>
        </div>
      );
    case "VisitCard":
      return (
        <div className="space-y-2">
          <div>
            <span className="font-medium">Symptoms:</span>{" "}
            {document.data.symptoms}
          </div>
          <div>
            <span className="font-medium">Findings:</span>{" "}
            {document.data.findings}
          </div>
          <div>
            <span className="font-medium">Diagnosis:</span>{" "}
            {document.data.diagnosis}
          </div>
          <div>
            <span className="font-medium">Recommendations:</span>{" "}
            {document.data.recommendations}
          </div>
        </div>
      );
    default:
      return (
        <div className="text-gray-500">No additional details available.</div>
      );
  }
};

export const DocumentDetailsModal: React.FC<DocumentDetailsModalProps> = ({
  document,
  isOpen,
  onClose,
  onDownload,
}) => {
  if (!document) return null;

  const handleDownload = () => {
    if (onDownload) {
      onDownload(document);
    } else {
      alert("Download functionality not implemented");
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={`${document.type.replace("_", " ")} Details`}
      size="lg"
    >
      <div className="space-y-4">
        <div className="text-gray-700">
          <span className="font-medium">Issued:</span>{" "}
          {new Date(document.createdAt).toLocaleDateString()}
        </div>

        {document.notes && (
          <div className="text-gray-700">
            <span className="font-medium">Notes:</span> {document.notes}
          </div>
        )}

        <div className="border-t pt-4">{renderDocumentDetails(document)}</div>

        <div className="flex pt-4">
          <Button variant="primary" onClick={handleDownload} className="flex-1">
            Download PDF
          </Button>
        </div>
      </div>
    </Modal>
  );
};
