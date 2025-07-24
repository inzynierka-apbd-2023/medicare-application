# Shared Components Documentation

This directory contains reusable UI components for the Medicare application. All components follow consistent design patterns and include TypeScript definitions.

## Components Overview

### Button
A versatile button component with multiple variants and sizes.

**Props:**
- `variant`: 'primary' | 'secondary' | 'success' | 'warning' | 'danger' | 'ghost' | 'outline' | 'emerald' | 'purple' | 'gray'
- `size`: 'sm' | 'md' | 'lg' | 'xl' | 'icon'
- `loading`: boolean - shows loading spinner
- `leftIcon`: React.ReactNode - icon on the left
- `rightIcon`: React.ReactNode - icon on the right

**Example:**
```tsx
import { Button } from '@/shared/components';

<Button variant="primary" size="md" loading={false}>
  Save Changes
</Button>
```

### Input
Form input component with label, error handling, and icon support.

**Props:**
- `label`: string - input label
- `error`: string - error message
- `helperText`: string - helper text
- `leftIcon`: React.ReactNode - icon on the left
- `rightIcon`: React.ReactNode - icon on the right
- `variant`: 'default' | 'medical'

**Example:**
```tsx
import { Input } from '@/shared/components';

<Input
  label="Card Number"
  placeholder="Enter card number"
  error={errors.cardNumber}
  variant="medical"
/>
```

### Modal
Modal dialog component with backdrop and customizable content.

**Props:**
- `isOpen`: boolean - controls visibility
- `onClose`: () => void - close handler
- `title`: string - modal title
- `size`: 'sm' | 'md' | 'lg' | 'xl'
- `showCloseButton`: boolean - show X button
- `closeOnOverlayClick`: boolean - close on backdrop click

**Example:**
```tsx
import { Modal } from '@/shared/components';

<Modal
  isOpen={showModal}
  onClose={() => setShowModal(false)}
  title="Change Password"
  size="md"
>
  <form>...</form>
</Modal>
```

### Card
Container component for grouping related content.

**Props:**
- `variant`: 'default' | 'medical' | 'elevated'
- `padding`: 'none' | 'sm' | 'md' | 'lg'
- `header`: React.ReactNode - card header
- `footer`: React.ReactNode - card footer

**Example:**
```tsx
import { Card } from '@/shared/components';

<Card variant="medical" padding="md">
  <h3>Patient Information</h3>
  <p>Details...</p>
</Card>
```

### Badge
Status indicator component for displaying states.

**Props:**
- `variant`: 'default' | 'success' | 'warning' | 'error' | 'info' | 'paid' | 'unpaid' | 'partial'
- `size`: 'sm' | 'md' | 'lg'
- `icon`: React.ReactNode - optional icon

**Example:**
```tsx
import { Badge } from '@/shared/components';
import { CheckCircle } from 'lucide-react';

<Badge variant="paid" icon={<CheckCircle size={14} />}>
  Paid
</Badge>
```

### IconButton
Button component designed specifically for icons.

**Props:**
- `variant`: 'default' | 'primary' | 'success' | 'warning' | 'danger' | 'ghost'
- `size`: 'sm' | 'md' | 'lg'
- `icon`: React.ReactNode - the icon to display
- `tooltip`: string - tooltip text

**Example:**
```tsx
import { IconButton } from '@/shared/components';
import { Calendar } from 'lucide-react';

<IconButton
  variant="primary"
  icon={<Calendar size={16} />}
  tooltip="View appointments"
  onClick={() => navigate('/appointments')}
/>
```

### Dropdown
Dropdown menu component with customizable items.

**Props:**
- `trigger`: React.ReactNode - element that triggers dropdown
- `items`: DropdownItem[] - menu items
- `align`: 'left' | 'right' - alignment relative to trigger

**Example:**
```tsx
import { Dropdown } from '@/shared/components';

const items = [
  { id: 'profile', label: 'My Profile', href: '/profile' },
  { id: 'logout', label: 'Logout', onClick: handleLogout },
];

<Dropdown
  trigger={<button>Menu</button>}
  items={items}
  align="right"
/>
```

### Loading
Loading indicator component with multiple variants.

**Props:**
- `size`: 'sm' | 'md' | 'lg' | 'xl'
- `variant`: 'spinner' | 'dots' | 'pulse'
- `text`: string - loading text

**Example:**
```tsx
import { Loading } from '@/shared/components';

<Loading size="md" variant="spinner" text="Loading patient data..." />
```

### Table
Data table component with sorting and custom rendering.

**Props:**
- `columns`: TableColumn[] - column definitions
- `data`: T[] - table data
- `loading`: boolean - loading state
- `emptyText`: string - text when no data
- `rowKey`: string | function - unique row identifier

**Example:**
```tsx
import { Table } from '@/shared/components';

const columns = [
  { key: 'name', title: 'Patient Name' },
  { key: 'age', title: 'Age', align: 'center' },
  {
    key: 'actions',
    title: 'Actions',
    render: (_, record) => (
      <IconButton icon={<Calendar />} onClick={() => viewAppointments(record.id)} />
    )
  }
];

<Table columns={columns} data={patients} loading={loading} />
```

### SearchInput
Search input component with debouncing and loading state.

**Props:**
- `onSearch`: (value: string) => void - search handler
- `loading`: boolean - loading state
- `debounceMs`: number - debounce delay (default: 300ms)

**Example:**
```tsx
import { SearchInput } from '@/shared/components';

<SearchInput
  placeholder="Search patients..."
  onSearch={handleSearch}
  loading={searching}
  debounceMs={500}
/>
```

## Usage Patterns

### Import Components
```tsx
// Import individual components
import { Button, Modal, Card } from '@/shared/components';

// Or import specific types
import { type ButtonProps } from '@/shared/components';
```

### Consistent Styling
All components follow the Medicare application's design system:
- Primary color: Blue (#1d4ed8)
- Success: Green
- Warning: Yellow
- Error: Red
- Consistent border radius and shadows

### Accessibility
All components include proper ARIA attributes and keyboard navigation support where applicable.

## Migration Guide

When replacing existing inline components with these shared components:

1. **Buttons**: Replace `<button className="...">` with `<Button variant="...">`
2. **Modals**: Replace custom modal markup with `<Modal>`
3. **Cards**: Replace `<div className="bg-white rounded-...">` with `<Card>`
4. **Status indicators**: Replace custom spans with `<Badge>`
5. **Tables**: Replace table markup with `<Table>` component

This ensures consistency across the application and makes maintenance easier.
