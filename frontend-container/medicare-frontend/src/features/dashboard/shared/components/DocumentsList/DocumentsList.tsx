interface Document {
  id: string;
  title: string;
  date: string;
  type?: string;
  size?: string;
}

interface DocumentsListProps {
  documents: Document[];
  maxVisible?: number;
  className?: string;
  onDocumentClick?: (documentId: string) => void;
}

export function DocumentsList({
  documents,
  maxVisible = 3,
  className = "",
  onDocumentClick,
}: DocumentsListProps) {
  const visibleDocuments = documents.slice(0, maxVisible);

  const handleDocumentClick = (document: Document) => {
    if (onDocumentClick) {
      onDocumentClick(document.id);
    }
  };

  if (visibleDocuments.length === 0) {
    return (
      <div className={`text-gray-500 text-sm text-center ${className}`}>
        No documents available
      </div>
    );
  }

  return (
    <ul
      className={`list-disc list-inside text-left space-y-2 text-sm text-gray-700 w-full ${className}`}
    >
      {visibleDocuments.map((document) => (
        <li
          key={document.id}
          className={`cursor-pointer hover:text-blue-600 transition-colors duration-150 ${
            onDocumentClick ? "cursor-pointer" : ""
          }`}
          onClick={() => handleDocumentClick(document)}
          title={document.size ? `Size: ${document.size}` : undefined}
        >
          <span className="font-medium">{document.title}</span>
          <span className="ml-2 text-gray-500">({document.date})</span>
        </li>
      ))}
    </ul>
  );
}

export type { Document, DocumentsListProps };
