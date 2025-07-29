import React, { useMemo } from "react";
import { DocumentList, DocumentFilter, DocumentDetailsModal } from "./components";
import type { Document, DocumentType, Appointment } from "./types";

interface DocumentsProps {
  documents: Document[];
  appointments: Appointment[];
  searchTerm: string;
  onSearchChange: (term: string) => void;
  typeFilter: DocumentType | "All";
  onTypeFilterChange: (type: DocumentType | "All") => void;
  appointmentFilter: string;
  onAppointmentFilterChange: (appointmentId: string) => void;
  selectedDocument: Document | null;
  onDocumentSelect: (document: Document) => void;
  onDocumentDeselect: () => void;
  onDocumentDownload?: (document: Document) => void;
}

export const Documents: React.FC<DocumentsProps> = ({
  documents,
  appointments,
  searchTerm,
  onSearchChange,
  typeFilter,
  onTypeFilterChange,
  appointmentFilter,
  onAppointmentFilterChange,
  selectedDocument,
  onDocumentSelect,
  onDocumentDeselect,
  onDocumentDownload,
}) => {
  const filteredDocuments = useMemo(() => {
    return documents.filter((doc) => {
      const matchesType = typeFilter === "All" || doc.type === typeFilter;
      const matchesAppointment = !appointmentFilter || doc.appointmentId === appointmentFilter;
      const matchesSearch = !searchTerm || 
        doc.notes?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        doc.type.toLowerCase().includes(searchTerm.toLowerCase());
      
      return matchesType && matchesAppointment && matchesSearch;
    });
  }, [documents, typeFilter, appointmentFilter, searchTerm]);

  return (
    <>
      <DocumentFilter
        searchTerm={searchTerm}
        onSearchChange={onSearchChange}
        typeFilter={typeFilter}
        onTypeFilterChange={onTypeFilterChange}
        appointmentFilter={appointmentFilter}
        onAppointmentFilterChange={onAppointmentFilterChange}
        appointments={appointments}
      />
      
      <DocumentList
        documents={filteredDocuments}
        onDocumentClick={onDocumentSelect}
        emptyMessage="No documents found matching your criteria."
      />
      
      <DocumentDetailsModal
        document={selectedDocument}
        isOpen={!!selectedDocument}
        onClose={onDocumentDeselect}
        onDownload={onDocumentDownload}
      />
    </>
  );
};
