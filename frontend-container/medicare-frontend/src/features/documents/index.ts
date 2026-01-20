// Types
export type {
  Appointment,
  Document,
  DocumentCardProps,
  DocumentData,
  DocumentDetailsModalProps,
  DocumentFilterProps,
  DocumentListProps,
  DocumentsPageProps,
  DocumentType,
} from "./types";

// Main Components
export { Documents } from "./Documents";
export { DocumentsPage } from "./DocumentsPage";

// Sub-components
export {
  DocumentCard,
  DocumentDetailsModal,
  DocumentFilter,
  DocumentList,
} from "./components";

// Services - re-export from shared services
export { documentsApi } from "@shared/services/documentsApi";

// Hooks - re-export from shared hooks
export { useDocuments } from "@shared/hooks/useDocuments";
