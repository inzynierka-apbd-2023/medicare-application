import React, { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import Header from "../../layout/Header";
import { Documents } from "./Documents";
import { useDocuments } from "../../shared/hooks/useDocuments";
import { LoadingOverlay, ErrorDisplay } from "../../shared/components";
import type { Document, DocumentType, DocumentsPageProps } from "./types";

export const DocumentsPage: React.FC<DocumentsPageProps> = ({
  initialAppointmentId,
}) => {
  const location = useLocation();
  const query = new URLSearchParams(location.search);
  
  const {
    documents,
    appointments,
    isLoading,
    error,
    downloadDocument,
    refetch,
  } = useDocuments(initialAppointmentId);
  
  const [searchTerm, setSearchTerm] = useState("");
  const [typeFilter, setTypeFilter] = useState<DocumentType | "All">("All");
  const [appointmentFilter, setAppointmentFilter] = useState(
    initialAppointmentId || query.get("appointmentId") || ""
  );
  const [selectedDocument, setSelectedDocument] = useState<Document | null>(null);

  // Listen to URL changes for appointment filter
  useEffect(() => {
    const appointmentIdFromUrl = query.get("appointmentId");
    if (appointmentIdFromUrl) {
      setAppointmentFilter(appointmentIdFromUrl);
    }
  }, [location.search]);

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <LoadingOverlay isLoading={true} message="Loading your medical documents...">
          <div className="min-h-screen" />
        </LoadingOverlay>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <div className="max-w-5xl mx-auto px-4 py-8">
          <h1 className="text-3xl font-bold text-blue-700 mb-6">
            Your Medical Documents
          </h1>
          <ErrorDisplay 
            message={error}
            onRetry={refetch}
          />
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      <Header />
      <div className="max-w-5xl mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold text-blue-700 mb-6">
          Your Medical Documents
        </h1>
        
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
        />
      </div>
    </div>
  );
};
