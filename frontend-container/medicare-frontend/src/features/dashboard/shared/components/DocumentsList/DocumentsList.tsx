interface Document {
  id: string;
  title: string;
  date: string;
  type?: string;
}

interface DocumentsListProps {
  documents: Document[];
  maxVisible?: number;
  className?: string;
}

export function DocumentsList({
  documents,
  maxVisible = 3,
  className = "",
}: DocumentsListProps) {
  const visibleDocuments = documents.slice(0, maxVisible);

  return (
    <ul
      className={`list-disc list-inside text-left space-y-2 text-sm text-gray-700 w-full ${className}`}
    >
      {visibleDocuments.map((document) => (
        <li key={document.id}>
          {document.title} on {document.date}
        </li>
      ))}
    </ul>
  );
}

export type { Document, DocumentsListProps };
