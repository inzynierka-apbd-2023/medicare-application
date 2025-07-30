# Development Guide

## Architecture Overview

This project follows a features-based architecture with strict module boundaries. Each feature is self-contained with its own components, types, and business logic.

### Directory Structure

```
src/
├── features/           # Feature modules (appointments, documents, etc.)
│   └── [feature]/
│       ├── index.ts    # Public API exports
│       ├── types.ts    # Feature-specific types
│       ├── components/ # Feature components
│       └── README.md   # Feature documentation
├── shared/             # Shared utilities and components
│   ├── components/     # Reusable UI components
│   ├── hooks/          # Custom React hooks
│   └── services/       # API services
└── layout/             # Layout components
```

## Import Rules

### ✅ Allowed Imports

- Features can import from `@shared/*`
- Features can import from `@layout/*`
- Any module can import from `@/*` (root src)
- Use absolute imports with path aliases

### ❌ Forbidden Imports

- Deep imports into features: `import from '@features/appointments/components/internal'`
- Cross-feature imports: `appointments` importing directly from `documents`
- Direct imports from feature internals

### Example Import Patterns

```typescript
// ✅ Good - using feature's public API
import { AppointmentsFeature } from '@features/appointments';

// ✅ Good - using shared components
import { Button } from '@shared/components';

// ❌ Bad - deep import into feature
import { AppointmentCard } from '@features/appointments/components/AppointmentCard';

// ❌ Bad - cross-feature import
import { DocumentType } from '@features/documents/types';
```

## Development Scripts

### Linting and Type Checking

```bash
# Run all linting checks
npm run lint

# Fix auto-fixable linting issues
npm run lint:fix

# Check architectural boundaries
npm run lint:architecture

# TypeScript type checking
npm run type-check
```

### Code Formatting

```bash
# Format all code
npm run format

# Check if code is properly formatted
npm run format:check
```

### Git Hooks

Pre-commit hooks automatically run:
- ESLint with architectural checks
- Prettier formatting
- TypeScript type checking

## Adding New Features

1. Create feature directory: `src/features/[feature-name]/`
2. Add required files:
   - `index.ts` - Public API exports
   - `types.ts` - Feature types
   - `README.md` - Feature documentation
   - `components/` - Feature components
3. Export public API through `index.ts`
4. Document the feature in its README

## Shared Components

When creating components that will be used across features:
1. Add to `src/shared/components/`
2. Export through `src/shared/components/index.ts`
3. Document the component
4. Ensure it has no feature-specific dependencies

## Best Practices

### Component Organization
- Keep components small and focused
- Use TypeScript for type safety
- Prefer composition over inheritance
- Extract custom hooks for complex logic

### State Management
- Use React hooks for local state
- Share state through context when needed
- Keep business logic in custom hooks

### API Integration
- Use shared API services in `@shared/services`
- Create feature-specific hooks for data fetching
- Handle loading and error states consistently

### Testing
- Test components in isolation
- Mock external dependencies
- Focus on user interactions and business logic

## Troubleshooting

### ESLint Errors

If you see architectural boundary violations:
1. Check if you're using deep imports
2. Ensure you're importing from feature's public API
3. Move shared code to `@shared` if needed

### Import Resolution Issues

If imports aren't resolving:
1. Check path aliases in `vite.config.js`
2. Verify TypeScript paths in `tsconfig.json`
3. Restart your development server

### Type Errors

For TypeScript issues:
1. Run `npm run type-check` for detailed errors
2. Ensure all dependencies are properly typed
3. Check if you need to add type definitions

## Code Review Checklist

- [ ] No architectural boundary violations
- [ ] Proper use of TypeScript types
- [ ] Components are properly tested
- [ ] Documentation is updated
- [ ] No console.log statements in production code
- [ ] Error handling is implemented
- [ ] Loading states are handled
- [ ] Responsive design is maintained
