import React, { useState } from "react";

import Header from "../../layout/Header";
import { ErrorDisplay, LoadingOverlay } from "../../shared/components";
import { useProfile } from "../../shared/hooks/useProfile";

import { Profile } from "./Profile";
import type { ProfilePageProps } from "./types";

export const ProfilePage: React.FC<ProfilePageProps> = ({ userId }) => {
  const {
    profileData,
    isLoading,
    error,
    updateProfile,
    changePassword,
    refetch,
  } = useProfile(userId);

  const [isEditing, setIsEditing] = useState(false);

  const handlePasswordChange = async (
    currentPassword: string,
    newPassword: string
  ) => {
    await changePassword(currentPassword, newPassword);
  };

  if (isLoading && !profileData) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <LoadingOverlay isLoading={true} message="Loading your profile...">
          <div className="min-h-screen" />
        </LoadingOverlay>
      </div>
    );
  }

  if (error || !profileData) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <div className="max-w-xl mx-auto px-4 py-8">
          <h1 className="text-3xl font-bold text-blue-700 mb-6 text-center">
            My Profile
          </h1>
          <ErrorDisplay
            message={error || "Failed to load profile"}
            onRetry={refetch}
          />
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      <Header />
      <main className="pt-24 px-4 pb-10 flex justify-center">
        <div className="w-full max-w-xl">
          <h1 className="text-3xl font-bold text-blue-700 mb-8 text-center">
            {isEditing ? "Edit Profile" : "My Profile"}
          </h1>

          <Profile
            profileData={profileData}
            isEditing={isEditing}
            onEditToggle={() => setIsEditing(!isEditing)}
            onSave={updateProfile}
            onPasswordChange={handlePasswordChange}
            isLoading={isLoading}
          />
        </div>
      </main>
    </div>
  );
};
