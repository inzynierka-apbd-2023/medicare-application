// Types
export type {
  DocumentType,
  DocumentData,
  Document,
  Appointment,
  DocumentListProps,
  DocumentCardProps,
  DocumentDetailsModalProps,
  DocumentFilterProps,
  DocumentsPageProps,
} from "./types";

// Main Components
export { Documents } from "./Documents";
export { DocumentsPage } from "./DocumentsPage";

// Sub-components
export {
  DocumentCard,
  DocumentList,
  DocumentFilter,
  DocumentDetailsModal,
} from "./components";

// Services - re-export from shared services
export { documentsApi } from "../../shared/services/documentsApi";

// Hooks - re-export from shared hooks
export { useDocuments } from "../../shared/hooks/useDocuments";
