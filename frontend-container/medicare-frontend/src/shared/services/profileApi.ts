import type { ProfileData } from "../../features/profile/types";

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

// Mock profile data - in real app this would connect to actual API
const mockProfileData: ProfileData = {
  id: "user-1",
  name: "John Doe",
  email: "john.doe@example.com",
  phone: "+1234567890",
  address: "Słoneczna 3, 12-254, Warsaw",
  dateOfBirth: "1990-01-01",
  membershipLevel: "gold",
  membershipName: "Gold Health Membership",
  profilePicture: undefined,
};

export const profileApi = {
  async getProfile(userId?: string): Promise<ApiResponse<ProfileData>> {
    // Simulate API delay
    await new Promise((resolve) => setTimeout(resolve, 800));

    return {
      success: true,
      data: mockProfileData,
    };
  },

  async updateProfile(
    data: Partial<ProfileData>,
    userId?: string
  ): Promise<ApiResponse<ProfileData>> {
    // Simulate API delay
    await new Promise((resolve) => setTimeout(resolve, 500));

    // Mock validation
    if (data.email && !data.email.includes("@")) {
      return {
        success: false,
        data: mockProfileData,
        message: "Invalid email format",
      };
    }

    // Update mock data
    Object.assign(mockProfileData, data);

    return {
      success: true,
      data: mockProfileData,
    };
  },

  async changePassword(
    currentPassword: string,
    newPassword: string,
    userId?: string
  ): Promise<ApiResponse<null>> {
    // Simulate API delay
    await new Promise((resolve) => setTimeout(resolve, 500));

    // Mock validation
    if (!currentPassword || !newPassword) {
      return {
        success: false,
        data: null,
        message: "Both current and new passwords are required",
      };
    }

    if (newPassword.length < 8) {
      return {
        success: false,
        data: null,
        message: "New password must be at least 8 characters long",
      };
    }

    // Mock current password validation
    if (currentPassword !== "oldpassword") {
      return {
        success: false,
        data: null,
        message: "Current password is incorrect",
      };
    }

    return {
      success: true,
      data: null,
      message: "Password changed successfully",
    };
  },
};
