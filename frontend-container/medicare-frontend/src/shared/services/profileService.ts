import type { ProfileData } from "../../features/profile/types";
import { toastMessages } from "../toast/toastMessages";

import { api } from "./api";

interface UserDto {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  role: string;
  dateOfBirth?: string;
  avatarUrl?: string | null;
  address?: string | null;
}

function mapToProfileData(user: UserDto): ProfileData {
  const base: ProfileData = {
    id: user.id,
    name: `${user.firstName} ${user.lastName}`.trim(),
    firstName: user.firstName,
    lastName: user.lastName,
    email: user.email,
    phone: user.phoneNumber || "",
    address: user.address || "",
    dateOfBirth: user.dateOfBirth ? user.dateOfBirth.slice(0, 10) : "",
    membershipLevel: "",
    membershipName:
      user.role && user.role.toLowerCase() !== "patient" ? user.role : "",
    profilePicture: "",
  };
  if (user.avatarUrl) base.profilePicture = user.avatarUrl;
  return base;
}

export const profileService = {
  async getProfile(userId: string): Promise<ProfileData> {
    const user = await api.get<UserDto>(`/users/${userId}`);
    return mapToProfileData(user);
  },

  async updateProfile(
    userId: string,
    data: Partial<ProfileData>
  ): Promise<ProfileData> {
    const dto: {
      phoneNumber?: string;
      dateOfBirth?: string;
      firstName?: string;
      lastName?: string;
      email?: string;
      addressLine1?: string;
    } = {};

    if (data.phone !== undefined) dto.phoneNumber = data.phone;
    if (data.dateOfBirth) dto.dateOfBirth = data.dateOfBirth;
    if (data.firstName) dto.firstName = data.firstName;
    if (data.lastName) dto.lastName = data.lastName;
    if (data.email) dto.email = data.email;
    if (data.address) dto.addressLine1 = data.address;

    const fresh = await api.put<UserDto>(`/users/${userId}`, dto, undefined, {
      showToastOnSuccess: true,
      successMessage: toastMessages.auth.profileUpdateSuccess,
    });
    return mapToProfileData(fresh);
  },

  async changePassword(
    _userId: string,
    currentPassword: string,
    newPassword: string
  ): Promise<void> {
    const { authService } = await import("./authService");
    await authService.changePassword(currentPassword, newPassword);
  },
};

export type ProfileService = typeof profileService;
