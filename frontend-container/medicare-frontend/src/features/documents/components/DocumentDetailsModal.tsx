import React from "react";
import { useNavigate } from "react-router-dom";
import type {
  Document,
  DocumentDetailsModalProps,
  DocumentType,
} from "@features/documents/types";
import { Button, DefinitionList, InfoCard, Modal } from "@shared/components";

// Component for prescription details
const PrescriptionDetails: React.FC<{ document: Document }> = ({
  document,
}) => (
  <DefinitionList
    variant="bordered"
    items={[
      { label: "Medication", value: document.data.medication },
      { label: "Dosage", value: document.data.dosage },
      { label: "Frequency", value: document.data.frequency },
      { label: "Duration (days)", value: document.data.duration_days },
      { label: "Instructions", value: document.data.instructions },
    ]}
  />
);

// Component for referral details
const ReferralDetails: React.FC<{ document: Document }> = ({ document }) => (
  <DefinitionList
    variant="bordered"
    items={[
      { label: "Specialty", value: document.data.specialty },
      { label: "Referred To", value: document.data.referredTo },
      { label: "Valid From", value: document.data.validFrom },
      { label: "Valid To", value: document.data.validTo },
    ]}
  />
);

// Component for sick leave details
const SickLeaveDetails: React.FC<{ document: Document }> = ({ document }) => (
  <DefinitionList
    variant="bordered"
    items={[
      { label: "Start Date", value: document.data.startDate },
      { label: "End Date", value: document.data.endDate },
      { label: "Days Off", value: document.data.daysOff },
    ]}
  />
);

// Component for visit card details
const VisitCardDetails: React.FC<{ document: Document }> = ({ document }) => (
  <DefinitionList
    variant="bordered"
    items={[
      { label: "Symptoms", value: document.data.symptoms },
      { label: "Findings", value: document.data.findings },
      { label: "Diagnosis", value: document.data.diagnosis },
      { label: "Recommendations", value: document.data.recommendations },
    ]}
  />
);

// Component for lab results redirect
const LabResultsRedirect: React.FC<{
  document: Document;
  onClose: () => void;
}> = ({ document, onClose }) => {
  const navigate = useNavigate();

  const handleViewDetails = () => {
    onClose();
    navigate(`/lab-results/${document.id}`);
  };

  return (
    <div className="text-center py-6 bg-purple-50 rounded-lg border border-purple-200">
      <div className="mb-4">
        <h4 className="text-lg font-medium text-purple-900 mb-2">
          Laboratory Results Available
        </h4>
        <p className="text-purple-700 text-sm">
          This document contains detailed laboratory test results. View them in
          the dedicated lab results page for better readability.
        </p>
      </div>

      <div className="space-y-2">
        {document.data.testType && (
          <p className="text-sm text-purple-800">
            <span className="font-medium">Test Type:</span>{" "}
            {document.data.testType}
          </p>
        )}
        {document.data.status && (
          <p className="text-sm text-purple-800">
            <span className="font-medium">Status:</span> {document.data.status}
          </p>
        )}
      </div>

      <Button
        onClick={handleViewDetails}
        variant="primary"
        className="mt-4 bg-purple-600 hover:bg-purple-700"
      >
        View Detailed Results
      </Button>
    </div>
  );
};

const renderDocumentDetails = (document: Document, onClose: () => void) => {
  switch (document.type as DocumentType) {
    case "Prescription":
      return <PrescriptionDetails document={document} />;
    case "Referral":
      return <ReferralDetails document={document} />;
    case "Sick_Leave":
      return <SickLeaveDetails document={document} />;
    case "VisitCard":
      return <VisitCardDetails document={document} />;
    case "Lab_Results":
      return <LabResultsRedirect document={document} onClose={onClose} />;
    default:
      return (
        <div className="text-center py-8 text-gray-500">
          <p>No additional details available for this document type.</p>
        </div>
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
      <div className="space-y-6">
        {/* Document Header Info */}
        <InfoCard variant="default">
          <DefinitionList
            variant="compact"
            items={[
              {
                label: "Issued",
                value: new Date(document.createdAt).toLocaleDateString(),
              },
              {
                label: "Document Type",
                value: document.type.replace("_", " "),
              },
              {
                label: "Notes",
                value: document.notes,
                show: !!document.notes,
              },
            ]}
          />
        </InfoCard>

        {/* Document Details */}
        <InfoCard title="Document Details" variant="bordered">
          {renderDocumentDetails(document, onClose)}
        </InfoCard>

        {/* Actions */}
        {document.type !== "Lab_Results" && (
          <div className="flex justify-end pt-4 border-t border-gray-200">
            <Button variant="primary" onClick={handleDownload}>
              Download PDF
            </Button>
          </div>
        )}
      </div>
    </Modal>
  );
};
