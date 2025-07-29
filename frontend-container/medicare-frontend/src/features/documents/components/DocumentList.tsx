import React from "react";
import { DocumentCard } from "./DocumentCard";
import type { DocumentListProps } from "../types";

export const DocumentList: React.FC<DocumentListProps> = ({
  documents,
  onDocumentClick,
  emptyMessage = "No documents found.",
}) => {
  if (documents.length === 0) {
    return (
      <div className="text-gray-500 text-center py-20 col-span-2">
        {emptyMessage}
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
