// Types
export type {
  CreateStaffRequest,
  Doctor,
  Receptionist,
  Service,
  Specialization,
  StaffCardProps,
  StaffDetailsModalProps,
  StaffFilterProps,
  StaffFormModalProps,
  StaffListProps,
  StaffManagementPageProps,
  StaffManagementProps,
  StaffMember,
  StaffRole,
  UpdateStaffRequest,
  UserProfile,
} from "./types";

// Main Components
export { StaffManagement } from "./StaffManagement";
export { StaffManagementPage } from "./StaffManagementPage";

// Sub-components
export {
  StaffCard,
  StaffDetailsModal,
  StaffFilter,
  StaffFormModal,
  StaffList,
} from "./components";

// Hooks
export { useStaffManagement } from "./hooks/useStaffManagement";

// Services - re-export from shared services
export { staffApi } from "@shared/services/staffApi";
