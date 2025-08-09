# Lab Results Review Feature

## Overview
This feature provides a comprehensive interface for doctors to review and approve patient lab results. It includes functionality to examine test results, flag abnormal values, add review notes, and approve or require follow-up for lab results.

## Features
- **Lab Results Review**: View and review lab test results with detailed parameter information
- **Status Management**: Track results through pending, in review, approved, and requires follow-up states
- **Critical Alerts**: Highlight critical and abnormal values that need immediate attention
- **Review Forms**: Add review notes and recommendations for each lab result
- **Search and Filtering**: Find results by patient, test type, status, or priority
- **Quick Actions**: Approve normal results quickly or flag for detailed review

## Components

### Core Components
- **LabResultsReviewFeature**: Main orchestrating component
- **LabResultsReviewList**: Displays list of lab results
- **LabResultReviewCard**: Individual result card with actions
- **LabResultReviewForm**: Review and approval form

### Types
- **LabResult**: Core lab result interface with test data
- **TestResult**: Individual test parameter data
- **LabResultStatus**: Status enum (pending_review, in_review, approved, etc.)
- **LabResultPriority**: Priority levels (routine, urgent, stat, critical)

## Usage

### Basic Implementation
```tsx
import { LabResultsReviewPage } from './features/labResultsReview';

// In your routing
<Route path="/lab-results-review" element={<LabResultsReviewPage />} />
```

### Hook Usage
```tsx
import { useLabResultsReview } from './hooks/useLabResultsReview';

const {
  labResults,
  reviewLabResult,
  approveLabResult,
  filterLabResults
} = useLabResultsReview();
```

## Status Workflow
1. **pending_review**: New results waiting for doctor review
2. **in_review**: Results currently being reviewed by a doctor
3. **approved**: Results reviewed and approved as normal
4. **requires_followup**: Results requiring additional action or follow-up
5. **critical_alert**: Results with critical values requiring immediate attention

## API Integration
The feature is designed to work with mock data initially but can be easily integrated with a backend API. Update the hook methods to call your actual API endpoints.

## Styling
Uses Tailwind CSS for styling with responsive design and accessibility considerations. Components follow the established design system used throughout the application.
