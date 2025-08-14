export type StaffRole = "Doctor" | "Receptionist";

export interface UserProfile {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  dateOfBirth?: string;
  gender?: "Male" | "Female" | "Other";
  avatarUrl?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
}

export interface Doctor {
  id: string;
  profile: UserProfile;
  role: "Doctor";
  licenseNumber?: string;
  yearsExperience?: number;
  biography?: string;
  officeAddress?: string;
  specializations: Specialization[];
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Receptionist {
  id: string;
  profile: UserProfile;
  role: "Receptionist";
  department?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Specialization {
  id: string;
  name: string;
  description?: string;
  serviceName: string;
  isPrimary?: boolean;
  certifiedDate?: string;
}

export interface Service {
  id: string;
  name: string;
  description?: string;
  durationMinutes: number;
  isActive: boolean;
}

export type StaffMember = Doctor | Receptionist;

export interface CreateStaffRequest {
  role: StaffRole;
  profile: UserProfile;
  // Doctor specific fields
  licenseNumber?: string;
  yearsExperience?: number;
  biography?: string;
  officeAddress?: string;
  specializations?: string[]; // Array of specialization IDs
  // Receptionist specific fields
  department?: string;
}

export interface UpdateStaffRequest extends Partial<CreateStaffRequest> {
  id: string;
}

export interface StaffListProps {
  staff: StaffMember[];
  onStaffClick: (staff: StaffMember) => void;
  searchTerm?: string;
  roleFilter?: StaffRole | "All";
  emptyMessage?: string;
}

export interface StaffCardProps {
  staff: StaffMember;
  onClick: (staff: StaffMember) => void;
}

export interface StaffDetailsModalProps {
  staff: StaffMember | null;
  isOpen: boolean;
  onClose: () => void;
  onEdit?: (staff: StaffMember) => void;
  onDelete?: (staff: StaffMember) => void;
}

export interface StaffFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (data: CreateStaffRequest | UpdateStaffRequest) => void;
  staff?: StaffMember | null; // null for create, StaffMember for edit
  availableSpecializations: Specialization[];
}

export interface StaffFilterProps {
  searchTerm: string;
  onSearchChange: (term: string) => void;
  roleFilter: StaffRole | "All";
  onRoleFilterChange: (role: StaffRole | "All") => void;
}

export interface StaffManagementProps {
  staff: StaffMember[];
  specializations: Specialization[];
  searchTerm: string;
  onSearchChange: (term: string) => void;
  roleFilter: StaffRole | "All";
  onRoleFilterChange: (role: StaffRole | "All") => void;
  selectedStaff: StaffMember | null;
  onStaffSelect: (staff: StaffMember) => void;
  onStaffDeselect: () => void;
  onStaffCreate: (data: CreateStaffRequest) => Promise<boolean>;
  onStaffUpdate: (data: UpdateStaffRequest) => Promise<boolean>;
  onStaffDelete: (id: string) => Promise<boolean>;
}

export interface StaffManagementPageProps {
  initialRoleFilter?: StaffRole;
}
