import { useCallback, useEffect, useMemo, useState } from "react";
import { useToastContext } from "../ui/toast";

import type { Appointment, Document } from "../../features/documents/types";
import { documentsApi } from "../services/documentsApi";
import { useAuth } from "../auth/AuthContext";

interface UseDocumentsParams {
  appointmentId?: string;
  patientId?: string;
}

interface UseDocumentsResult {
  documents: Document[];
  appointments: Appointment[];
  isLoading: boolean;
  error: string | null;
  downloadDocument: (document: Document) => Promise<void>;
  refetch: () => Promise<void>;
}

export const useDocuments = (
  params?: UseDocumentsParams
): UseDocumentsResult => {
  const { user } = useAuth();
  const [documents, setDocuments] = useState<Document[]>([]);
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { showToast } = useToastContext();

  const effectiveFilters = useMemo(() => {
    const filters: { appointmentId?: string; patientId?: string } = {};
    if (params?.appointmentId) filters.appointmentId = params.appointmentId;
    // Default to current user id when patientId not explicitly passed
    const pid = params?.patientId ?? user?.id;
    if (pid) filters.patientId = pid;
    return filters;
  }, [params?.appointmentId, params?.patientId, user?.id]);

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
  const response = await documentsApi.getDocumentsWithAppointments(effectiveFilters);

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
  }, [effectiveFilters]);

  const downloadDocument = async (doc: Document): Promise<void> => {
    const url = `${location.origin}/api/documents/${doc.id}/pdf`;
    try {
      const token = localStorage.getItem("authToken");
      const headers: Record<string, string> = {};
      if (token) headers["Authorization"] = `Bearer ${token}`;
      const res = await fetch(url, {
        method: "GET",
        headers,
      });

      if (!res.ok) {
        if (res.status === 504) {
          showToast("The PDF generator took too long. Please try again in a moment.", { type: "warning" });
        } else if (res.status === 400) {
          const msg = await res.text();
          showToast(msg || "This document type cannot be exported as PDF yet.", { type: "warning" });
        } else {
          showToast(`Failed to download document (HTTP ${res.status})`, { type: "error" });
        }
        return;
      }

      const blob = await res.blob();
      const dlUrl = URL.createObjectURL(blob);
      const link = window.document.createElement("a");
      link.href = dlUrl;
      link.download = `${doc.type.replace("_", "-")}-${doc.id}.pdf`;
      window.document.body.appendChild(link);
      link.click();
      window.document.body.removeChild(link);
      URL.revokeObjectURL(dlUrl);
    } catch (err) {
      console.error("Error downloading document:", err);
      showToast("An error occurred while downloading the document", { type: "error" });
    }
  };

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  return {
    documents,
    appointments,
    isLoading,
    error,
    downloadDocument,
    refetch: fetchData,
  };
};
