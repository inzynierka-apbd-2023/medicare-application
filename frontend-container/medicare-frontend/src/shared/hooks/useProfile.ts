import { useState, useEffect } from "react";
import { profileApi } from "../services/profileApi";
import type { ProfileData } from "../../features/profile/types";

interface UseProfileResult {
  profileData: ProfileData | null;
  isLoading: boolean;
  error: string | null;
  updateProfile: (data: Partial<ProfileData>) => Promise<void>;
  changePassword: (
    currentPassword: string,
    newPassword: string
  ) => Promise<void>;
  refetch: () => Promise<void>;
}

export const useProfile = (userId?: string): UseProfileResult => {
  const [profileData, setProfileData] = useState<ProfileData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchProfile = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await profileApi.getProfile(userId);

      if (response.success) {
        setProfileData(response.data);
      } else {
        setError(response.message || "Failed to load profile");
      }
    } catch (err) {
      setError("An error occurred while loading your profile");
      console.error("Profile fetch error:", err);
    } finally {
      setIsLoading(false);
    }
  };

  const updateProfile = async (data: Partial<ProfileData>) => {
    try {
      setIsLoading(true);
      const response = await profileApi.updateProfile(data, userId);

      if (response.success) {
        setProfileData((prev) => (prev ? { ...prev, ...data } : null));
      } else {
        throw new Error(response.message || "Failed to update profile");
      }
    } catch (err) {
      console.error("Profile update error:", err);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const changePassword = async (
    currentPassword: string,
    newPassword: string
  ) => {
    try {
      setIsLoading(true);
      const response = await profileApi.changePassword(
        currentPassword,
        newPassword,
        userId
      );

      if (!response.success) {
        throw new Error(response.message || "Failed to change password");
      }
    } catch (err) {
      console.error("Password change error:", err);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchProfile();
  }, [userId]);

  return {
    profileData,
    isLoading,
    error,
    updateProfile,
    changePassword,
    refetch: fetchProfile,
  };
};
