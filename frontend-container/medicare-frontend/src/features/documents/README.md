# Documents Feature - Restructured Architecture

## Overview
The documents feature has been restructured to be more generic, modular, and reusable. The new architecture follows modern React patterns with clear separation of concerns and matches the structure used in other refactored features. It includes proper API abstraction with mock data for development.

## New Structure

```
src/features/documents/
├── types.ts                          # All TypeScript interfaces and types
├── Documents.tsx                     # Main wrapper component (simplified)
├── DocumentsPage.tsx                # Page-level component with data fetching
├── index.ts                         # Feature exports
├── README.md                        # Documentation
└── components/                      # UI Components
    ├── index.ts                     # Components exports
    ├── DocumentCard.tsx             # Individual document display
    ├── DocumentList.tsx             # Main list component
    ├── DocumentFilter.tsx           # Search and filter controls
    └── DocumentDetailsModal.tsx     # Detailed view modal

src/shared/
├── services/
│   └── documentsApi.ts              # Documents API with mock responses (following established pattern)
└── hooks/
    └── useDocuments.ts              # Documents data management hook (following established pattern)
```

## Key Improvements

### 1. **Type Safety & Clarity**
- **`types.ts`**: Centralized type definitions with strict TypeScript interfaces
- **Document Type System**: Proper typing for different document types (Prescription, Referral, etc.)
- **Proper Type Exports**: All types properly exported and consumed across components

### 2. **Component Separation**
- **`DocumentCard`**: Reusable card component for individual documents with badges and actions
- **`DocumentList`**: Generic list component for displaying documents in a grid
- **`DocumentFilter`**: Dedicated component for search, type filtering, and appointment filtering
- **`DocumentDetailsModal`**: Modal for showing detailed document information with download functionality
- **`Documents`**: Simplified wrapper focusing on composition and state management

### 3. **Improved Reusability**
- Components are now generic and can be easily reused in different contexts
- Clear prop interfaces make integration straightforward
- Separated business logic from presentation logic
- Uses shared components (Card, Badge, Modal, Button, SearchInput) for consistency

### 4. **Better Data Management**
- **API Abstraction**: Clean API service layer with proper error handling
- **Mock Data**: Separated mock data for development and testing
- **Custom Hook**: `useDocuments` hook for data fetching and state management
- **Loading States**: Proper loading and error states with shared components
- **Type-Safe API**: All API responses are properly typed

### 5. **Better Maintainability**
- Each component has a single responsibility
- Types are centralized and consistent
- Easy to extend with new document types or features
- Clear import/export structure

## API Usage Examples

### Using the Documents API Service
```tsx
import { documentsApi } from '@/shared/services/documentsApi';

// Fetch all documents
const response = await documentsApi.getDocuments();
if (response.success) {
  console.log('Documents:', response.data);
}

// Fetch with filters
const filteredResponse = await documentsApi.getDocuments({
  searchTerm: 'prescription',
  typeFilter: 'Prescription',
  appointmentId: 'appt1'
});

// Download document
const downloadResponse = await documentsApi.downloadDocument('d1');
```

### Using the Documents Hook
```tsx
import { useDocuments } from '@/shared/hooks/useDocuments';

function MyComponent() {
  const {
    documents,
    appointments,
    isLoading,
    error,
    downloadDocument,
    refetch
  } = useDocuments();

  if (isLoading) return <Loading />;
  if (error) return <Error message={error} onRetry={refetch} />;

  return (
    <div>
      {documents.map(doc => (
        <div key={doc.id} onClick={() => downloadDocument(doc)}>
          {doc.type}
        </div>
      ))}
    </div>
  );
}
```

## Usage Examples

### Basic Usage
```tsx
import { DocumentList } from '@/features/documents';

<DocumentList
  documents={documents}
  onDocumentClick={handleDocumentClick}
  emptyMessage="No documents available"
/>
```

### Individual Components
```tsx
import { DocumentCard, DocumentFilter, DocumentDetailsModal } from '@/features/documents';

// Use individual card
<DocumentCard 
  document={document} 
  onClick={handleClick}
/>

// Use filter controls
<DocumentFilter
  searchTerm={searchTerm}
  onSearchChange={setSearchTerm}
  typeFilter={typeFilter}
  onTypeFilterChange={setTypeFilter}
  appointmentFilter={appointmentFilter}
  onAppointmentFilterChange={setAppointmentFilter}
  appointments={appointments}
/>

// Use details modal
<DocumentDetailsModal
  document={selectedDocument}
  isOpen={!!selectedDocument}
  onClose={() => setSelectedDocument(null)}
  onDownload={handleDownload}
/>
```

### Complete Feature Usage
```tsx
import { Documents } from '@/features/documents';

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
  onDocumentDownload={handleDownload}
/>
```

## Document Types Supported

1. **Prescription**: Medication details, dosage, frequency, duration, instructions
2. **Referral**: Specialty, referred doctor, validity dates
3. **Sick Leave**: Start/end dates, days off
4. **Visit Card**: Symptoms, findings, diagnosis, recommendations
5. **Other**: Generic document type for extensibility

## Benefits

1. **Generic & Reusable**: Components can be used in different contexts (dashboard, standalone page, etc.)
2. **Type Safe**: Strong TypeScript support with proper interfaces for all document types
3. **Modular**: Each component has clear responsibilities
4. **Extensible**: Easy to add new document types or modify existing ones
5. **Maintainable**: Clear structure makes debugging and updates easier
6. **Testable**: Smaller, focused components are easier to test
7. **Consistent**: Uses shared components for UI consistency across the application

## Integration with Shared Components

The documents feature leverages the shared component library:
- **Card**: For document containers
- **Badge**: For document type indicators with appropriate variants and icons
- **Modal**: For document details popup
- **Button**: For actions (view details, download)
- **SearchInput**: For document search functionality

## Migration

The restructuring maintains the same functionality as the original Documents.jsx but with better architecture:
- All filtering logic is preserved
- Modal functionality is enhanced with shared Modal component
- Document type handling is more robust
- Better responsive design
- Improved accessibility through shared components

This refactoring provides a solid foundation for scaling the documents system while maintaining consistency and reusability across the application.
