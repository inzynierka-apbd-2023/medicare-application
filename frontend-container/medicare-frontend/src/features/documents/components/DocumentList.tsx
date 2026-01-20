import React from "react";
import type { DocumentListProps } from "@features/documents/types";
import { EmptyState } from "@shared/components";
import { FileText } from "lucide-react";

import { DocumentCard } from "./DocumentCard";

export const DocumentList: React.FC<DocumentListProps> = ({
  documents,
  onDocumentClick,
  emptyMessage = "No documents found.",
}) => {
  if (documents.length === 0) {
    return (
      <div className="col-span-2">
        <EmptyState
          icon={<FileText className="h-16 w-16 text-gray-400" />}
          title="No documents found"
          description={emptyMessage}
        />
      </div>
    );
  }

  return (
    <div className="grid md:grid-cols-2 gap-6">
      {documents.map((document) => (
        <DocumentCard
          key={document.id}
          document={document}
          onClick={onDocumentClick}
        />
      ))}
    </div>
  );
};
