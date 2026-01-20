import React, { useEffect, useMemo, useState } from "react";
import { useLocation } from "react-router-dom";
import Header from "@layout/Header";
import { ErrorDisplay, LoadingOverlay } from "@shared/components";
import { useDocuments } from "@shared/hooks/useDocuments";

import { Documents } from "./Documents";
import type { Document, DocumentsPageProps, DocumentType } from "./types";

export const DocumentsPage: React.FC<DocumentsPageProps> = ({
  initialAppointmentId,
  initialPatientId,
}) => {
  const location = useLocation();
  const query = useMemo(
    () => new URLSearchParams(location.search),
    [location.search]
  );

  const patientId = initialPatientId || query.get("patientId") || undefined;
  const appointmentId =
    initialAppointmentId || query.get("appointmentId") || undefined;

  const {
    documents,
    appointments,
    isLoading,
    error,
    downloadDocument,
    refetch,
  } = useDocuments(
    appointmentId || patientId
      ? {
          ...(appointmentId && { appointmentId }),
          ...(patientId && { patientId }),
        }
      : undefined
  );

  const [searchTerm, setSearchTerm] = useState("");
  const [typeFilter, setTypeFilter] = useState<DocumentType | "All">("All");
  const [appointmentFilter, setAppointmentFilter] = useState(
    appointmentId || ""
  );
  const [selectedDocument, setSelectedDocument] = useState<Document | null>(
    null
  );

  // Listen to URL changes for appointment filter and type filter
  useEffect(() => {
    const appointmentIdFromUrl = query.get("appointmentId");
    const filterFromUrl = query.get("filter");

    if (appointmentIdFromUrl) {
      setAppointmentFilter(appointmentIdFromUrl);
    }

    // Handle filter parameter for document types
    if (filterFromUrl === "prescriptions") {
      setTypeFilter("Prescription");
    } else if (filterFromUrl === "medical-records") {
      // Show all except prescriptions for medical records
      setTypeFilter("All");
    }
  }, [location.search, query]);

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-100 pt-16">
        <Header />
        <LoadingOverlay
          isLoading={true}
          message="Loading your medical documents..."
        >
          <div className="min-h-screen" />
        </LoadingOverlay>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100 pt-16">
        <Header />
        <div className="max-w-5xl mx-auto px-4 py-8">
          <h1 className="text-3xl font-bold text-blue-700 mb-6">
            Your Medical Documents
          </h1>
          <ErrorDisplay message={error} onRetry={refetch} />
        </div>
      </div>
    );
  }

  // Get patient name for display when filtering by patient
  const currentPatient = patientId
    ? appointments.find((apt) => apt.patientId === patientId)?.patientName
    : null;

  // Check if we should show medical records only (all except prescriptions)
  const filterFromUrl = query.get("filter");
  const showMedicalRecordsOnly = filterFromUrl === "medical-records";

  return (
    <div className="min-h-screen bg-gray-100 pt-16">
      <Header />
      <div className="max-w-5xl mx-auto px-4 py-8">
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-blue-700">
            {filterFromUrl === "prescriptions"
              ? "Your Prescriptions"
              : filterFromUrl === "medical-records"
                ? "Your Medical Records"
                : "Your Medical Documents"}
          </h1>
          {currentPatient && (
            <p className="text-lg text-gray-600 mt-2">
              Showing documents for:{" "}
              <span className="font-semibold text-blue-600">
                {currentPatient}
              </span>
            </p>
          )}
        </div>

        <Documents
          documents={documents}
          appointments={appointments}
          searchTerm={searchTerm}
          onSearchChange={setSearchTerm}
          typeFilter={typeFilter}
          onTypeFilterChange={setTypeFilter}
          appointmentFilter={appointmentFilter}
          onAppointmentFilterChange={setAppointmentFilter}
          selectedDocument={selectedDocument}
          onDocumentSelect={setSelectedDocument}
          onDocumentDeselect={() => setSelectedDocument(null)}
          onDocumentDownload={downloadDocument}
          showMedicalRecordsOnly={showMedicalRecordsOnly}
        />
      </div>
    </div>
  );
};
