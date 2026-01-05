import React from "react";
import { useNavigate } from "react-router-dom";

import { ProfileDisplay, ProfileForm } from "./components";
import type { ProfileProps } from "./types";

export const Profile: React.FC<ProfileProps> = ({
  profileData,
  isEditing,
  onEditToggle,
  onSave,
  isLoading = false,
}) => {
  const navigate = useNavigate();

  const handlePasswordChangeClick = () => {
    navigate("/forgot-password");
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
    </>
  );
};
