import { useState, useEffect } from "react";
import { documentsApi } from "../services/documentsApi";
import type { Document, Appointment, DocumentType } from "../../features/documents/types";

interface UseDocumentsResult {
  documents: Document[];
  appointments: Appointment[];
  isLoading: boolean;
  error: string | null;
  downloadDocument: (document: Document) => Promise<void>;
  refetch: () => Promise<void>;
}

export const useDocuments = (initialAppointmentId?: string): UseDocumentsResult => {
  const [documents, setDocuments] = useState<Document[]>([]);
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchData = async () => {
    setIsLoading(true);
    setError(null);
    
    try {
      const response = await documentsApi.getDocumentsWithAppointments();
      
      if (response.success) {
        setDocuments(response.data.documents);
        setAppointments(response.data.appointments);
      } else {
        setError(response.error || "Failed to load documents");
      }
    } catch (err) {
      setError("An unexpected error occurred");
      console.error("Error fetching documents:", err);
    } finally {
      setIsLoading(false);
    }
  };

  const downloadDocument = async (document: Document): Promise<void> => {
    try {
      const response = await documentsApi.downloadDocument(document.id);
      
      if (response.success) {
        // In a real app, this would trigger the actual download
        console.log("Download URL:", response.data.downloadUrl);
        alert(`Document "${document.type.replace('_', ' ')}" download started`);
      } else {
        alert(response.error || "Failed to download document");
      }
    } catch (err) {
      console.error("Error downloading document:", err);
      alert("An error occurred while downloading the document");
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  return {
    documents,
    appointments,
    isLoading,
    error,
    downloadDocument,
    refetch: fetchData,
  };
};
