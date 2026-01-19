import { toastMessages } from "../toast/toastMessages";

import { api } from "./api";
import type { AuthUser } from "./authService";

export interface UpdateUserDto {
  phoneNumber?: string;
  dateOfBirth?: string;
  avatarUrl?: string | null;
  firstName?: string;
  lastName?: string;
  email?: string;
  addressLine1?: string;
}

interface UserResponseDto {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  role: string;
  dateOfBirth?: string;
  address?: string | null;
}

function mapToAuthUser(u: UserResponseDto): AuthUser {
  return {
    id: u.id,
    username: u.username,
    email: u.email,
    role: u.role,
    firstName: u.firstName,
    lastName: u.lastName,
    ...(u.phoneNumber ? { phoneNumber: u.phoneNumber } : {}),
    ...(u.dateOfBirth ? { dateOfBirth: u.dateOfBirth } : {}),
    ...(u.address ? { address: u.address } : {}),
  } as AuthUser;
}

export const usersApi = {
  async updateProfile(userId: string, dto: UpdateUserDto): Promise<AuthUser> {
    const res = await api.put<UserResponseDto>(
      `/users/${userId}`,
      dto,
      undefined,
      {
        showToastOnSuccess: true,
        successMessage: toastMessages.auth.profileUpdateSuccess,
      }
    );
    return mapToAuthUser(res);
  },

  async getUser(userId: string): Promise<AuthUser> {
    const res = await api.get<UserResponseDto>(`/users/${userId}`);
    return mapToAuthUser(res);
  },
};
