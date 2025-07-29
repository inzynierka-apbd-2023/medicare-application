import React, { useState } from "react";
import { ProfileDisplay, ProfileForm, ChangePasswordModal } from "./components";
import type { ProfileProps } from "./types";

export const Profile: React.FC<ProfileProps> = ({
  profileData,
  isEditing,
  onEditToggle,
  onSave,
  onPasswordChange,
  isLoading = false,
}) => {
  const [showPasswordModal, setShowPasswordModal] = useState(false);

  const handlePasswordChangeClick = () => {
    setShowPasswordModal(true);
  };

  const handlePasswordSubmit = async (
    currentPassword: string,
    newPassword: string
  ) => {
    try {
      // Call the actual password change function passed as prop
      await onPasswordChange(currentPassword, newPassword);
      setShowPasswordModal(false);
    } catch (error) {
      console.error("Password change error:", error);
      throw error;
    }
  };

  const handleSave = async (data: Partial<typeof profileData>) => {
    try {
      await onSave(data);
      onEditToggle();
    } catch (error) {
      console.error("Profile save error:", error);
      throw error;
    }
  };

  if (isEditing) {
    return (
      <ProfileForm
        profileData={profileData}
        onSave={handleSave}
        onCancel={onEditToggle}
        isLoading={isLoading}
      />
    );
  }

  return (
    <>
      <ProfileDisplay
        profileData={profileData}
        onEdit={onEditToggle}
        onPasswordChange={handlePasswordChangeClick}
      />

      <ChangePasswordModal
        isOpen={showPasswordModal}
        onClose={() => setShowPasswordModal(false)}
        onSubmit={handlePasswordSubmit}
        isLoading={isLoading}
      />
    </>
  );
};
