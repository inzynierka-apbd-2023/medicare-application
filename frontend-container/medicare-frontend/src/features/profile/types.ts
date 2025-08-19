export interface ProfileData {
  id: string;
  name: string;
  firstName?: string;
  lastName?: string;
  email: string;
  phone: string;
  address: string;
  dateOfBirth: string;
  membershipLevel: string;
  membershipName: string;
  // May be empty string if no avatar yet
  profilePicture?: string;
}

export interface ProfilePageProps {
  userId?: string;
}

export interface ProfileProps {
  profileData: ProfileData;
  isEditing: boolean;
  onEditToggle: () => void;
  onSave: (data: Partial<ProfileData>) => Promise<void>;
  onPasswordChange: (
    currentPassword: string,
    newPassword: string
  ) => Promise<void>;
  isLoading?: boolean;
}

export interface ProfileFormProps {
  profileData: ProfileData;
  onSave: (data: Partial<ProfileData>) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export interface ProfileDisplayProps {
  profileData: ProfileData;
  onEdit: () => void;
  onPasswordChange: () => void;
}

export interface ChangePasswordModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (currentPassword: string, newPassword: string) => Promise<void>;
  isLoading?: boolean;
}

export interface PasswordChangeData {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
