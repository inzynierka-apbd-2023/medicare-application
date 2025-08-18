# Medicare Application - Frontend

A modern React + Vite frontend application for healthcare management, featuring appointment scheduling, patient management, and doctor dashboard functionality.

## 🚀 Quick Start

### Prerequisites
- Node.js (v16 or higher)
- npm or yarn

### Installation & Setup
```bash
# Navigate to frontend directory
cd frontend-container/medicare-frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

### Available Scripts
- `npm run dev` - Start development server with hot reload
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

---

## API Base and Sign-on Flow

- All API calls go through `/api` which is proxied by Nginx to the UserService. In Docker, this is configured via `VITE_API_BASE_URL=/api`.
- Registration is a two-step flow: Register (create account) then Complete Profile (update profile with returned `user.id`).
- JWT is persisted in `localStorage` as `authToken` and attached in the `Authorization` header.
- The Register page enforces password strength and checks for duplicate emails via `/api/users/availability` with debouncing.

## Scheduler and calendars

* Availability is sourced from PractitionerService weekly schedules; the UI computes 30-minute slots and subtracts booked appointments from AppointmentService.
* Appointment times are treated as local wall-clock in both UI and API payloads to avoid timezone drift. Naive datetimes from the backend are parsed as local.
* Status-based colors are centralized in `src/features/scheduler/utils/statusColors.ts` and applied to:
	* `appointment-scheduler` views
	* Patient dashboard calendar
	The mapping includes Scheduled, Confirmed, InProgress, Completed, Cancelled, Overdue, and No-Show (with aliases like Pending).

# Git Hooks & Commit Convention Documentation

This project uses automated Git hooks to enforce code quality and commit message standards.

##  Husky Setup

This project uses [Husky](https://typicode.github.io/husky/) to manage Git hooks that run automatically during the commit process.

### Hooks Configuration

####  commit-msg
**Purpose**: Validates commit messages according to Conventional Commits specification
**Location**: .husky/commit-msg
**What it does**:
- Changes to the frontend project directory
- Runs commitlint to validate the commit message format
- Blocks commits that don't follow the healthcare-specific convention

####  pre-commit
**Purpose**: Runs code quality checks before allowing commits
**Location**: .husky/pre-commit
**What it does**:
- Branch protection (prevents direct commits to main-v2)
- Runs ESLint and Prettier via lint-staged
- Checks for console.log statements
- Warns about TODO/FIXME comments
- Validates TypeScript compilation
- Ensures code quality standards

---

# Medicare Application - Commit Message Convention

This project uses **Conventional Commits** specification to ensure consistent and meaningful commit messages.

##  Format
type(scope): subject

[optional body]

[optional footer]

##  Types
- **feat**: New features (e.g., feat(auth): add two-factor authentication)
- **fix**: Bug fixes (e.g., fix(appointments): resolve booking time conflicts)
- **docs**: Documentation changes
- **style**: Code style changes (formatting, no logic changes)
- **refactor**: Code refactoring without changing functionality
- **perf**: Performance improvements
- **test**: Adding or updating tests
- **chore**: Maintenance tasks, dependency updates
- **ci**: CI/CD pipeline changes
- **security**: Security-related changes

##  Scopes (Healthcare Domain-Specific)
- **auth**: Authentication/authorization
- **appointments**: Appointment management
- **patients**: Patient management
- **doctors**: Doctor features
- **dashboard**: Dashboard components
- **profile**: User profile features
- **wallet**: Payment/subscription features
- **documents**: Document management
- **scheduler**: Calendar/scheduling
- **api**: API integration
- **ui**: UI components
- **config**: Configuration changes
- **deps**: Dependencies

##  Examples - Good Commit Messages
feat(appointments): add appointment reminder notifications
fix(auth): resolve token expiration handling
docs(api): update authentication endpoints documentation
refactor(dashboard): simplify patient data rendering
perf(scheduler): optimize calendar loading performance
security(auth): implement rate limiting for login attempts
test(appointments): add unit tests for booking validation
chore(deps): update react and related dependencies

##  Examples - Bad Commit Messages
update stuff          # Too vague, no type/scope
fix bug               # Too short, no scope, not descriptive
add feature           # No scope, not descriptive
wip                   # Work in progress, not allowed
changes               # Too vague

##  Rules Enforced
- Subject must be 10-72 characters
- Use lowercase for type and subject
- Don't end subject with period
- Use imperative mood (add not added or adds)
- Scope is required and must be from predefined list
- Type is required and must be from predefined list
- Body and footer are optional but recommended for complex changes

##  Breaking Changes
For breaking changes, add ! after type/scope or include BREAKING CHANGE: in footer:
feat(api)!: redesign authentication API

##  What Happens on Violation
If your commit message doesn't follow the convention:
1. The commit will be **blocked**
2. You'll see an error message explaining what's wrong
3. You'll need to fix the commit message and try again

##  Technical Details
- **Commitlint Config**: Located in frontend-container/medicare-frontend/commitlint.config.js
- **Lint-staged Config**: Located in frontend-container/medicare-frontend/package.json
- **Hook Execution**: Hooks run from repository root but execute frontend commands in the correct directory

##  Benefits
- **Consistency**: All commits follow the same format
- **Automation**: Automatic changelog generation possible
- **Clarity**: Easy to understand what each commit does
- **Healthcare Focus**: Scopes tailored to medical application domains
- **Quality**: Code is automatically checked before commits
- **Team Collaboration**: Clear communication through standardized messages

---

*This documentation ensures all team members understand the commit standards and Git hook setup for the Medicare Application.*
