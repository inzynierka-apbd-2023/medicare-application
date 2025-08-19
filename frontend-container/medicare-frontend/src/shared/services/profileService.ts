import type { ProfileData } from "../../features/profile/types";

import { usersApi } from "./usersApi";

interface BasicUserDto {
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

function mapToProfileData(user: BasicUserDto): ProfileData {
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
    // Treat backend role separately; avoid showing raw 'patient' as a membership
    membershipName:
      user.role && user.role.toLowerCase() !== "patient" ? user.role : "",
    profilePicture: "",
  };
  if (user.avatarUrl) base.profilePicture = user.avatarUrl;
  return base;
}

export const profileService = {
  async getProfile(userId: string): Promise<ProfileData> {
    const user = (await usersApi.getUser(userId)) as BasicUserDto;
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
    if (data.address) dto.addressLine1 = data.address; // simple mapping
    if (data.firstName !== undefined) dto.firstName = data.firstName;
    if (data.lastName !== undefined) dto.lastName = data.lastName;
    if (data.email !== undefined) dto.email = data.email;
    await usersApi.updateProfile(userId, dto);
    const fresh = (await usersApi.getUser(userId)) as BasicUserDto;
    return mapToProfileData(fresh);
  },

  async changePassword(
    _userId: string,
    _currentPassword: string,
    _newPassword: string
  ): Promise<void> {
    // Not implemented yet server-side.
    return Promise.resolve();
  },
};

export type ProfileService = typeof profileService;
