import { useEffect, useState } from "react";

import type { ProfileData } from "../../features/profile/types";
import { useAuth } from "../auth/AuthContext";
import { profileService } from "../services/profileService";

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
  const { user } = useAuth();
  const [profileData, setProfileData] = useState<ProfileData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const effectiveUserId = userId || user?.id;

  const fetchProfile = async () => {
    setIsLoading(true);
    setError(null);

    try {
      if (!effectiveUserId) throw new Error("Missing user id");
      const data = await profileService.getProfile(effectiveUserId);
      setProfileData(data);
    } catch (_err) {
      setError("An error occurred while loading your profile");
    } finally {
      setIsLoading(false);
    }
  };

  const updateProfile = async (data: Partial<ProfileData>) => {
    setIsLoading(true);
    try {
      if (!effectiveUserId) throw new Error("Missing user id");
      const updated = await profileService.updateProfile(effectiveUserId, data);
      setProfileData(updated);
    } finally {
      setIsLoading(false);
    }
  };

  const changePassword = async (
    currentPassword: string,
    newPassword: string
  ) => {
    setIsLoading(true);
    try {
      if (!effectiveUserId) throw new Error("Missing user id");
      await profileService.changePassword(
        effectiveUserId,
        currentPassword,
        newPassword
      );
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchProfile();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return {
    profileData,
    isLoading,
    error,
    updateProfile,
    changePassword,
    refetch: fetchProfile,
  };
};
