import { apiClient } from "./apiClient";
import type { AuthUser } from "./authService";

// Backend DTO for updating the user profile
export interface UpdateUserDto {
  phoneNumber?: string;
  dateOfBirth?: string; // ISO date string (YYYY-MM-DD)
  avatarUrl?: string | null; // null clears avatar
  firstName?: string;
  lastName?: string;
  email?: string;
  addressLine1?: string;
}

// Backend response shape (subset mapped to AuthUser)
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
    // Prefer PUT; if backend expects PATCH, adjust method accordingly
    const res = await apiClient.put<UserResponseDto>(`/users/${userId}`, dto);
    return mapToAuthUser(res.data);
  },

  async getUser(userId: string): Promise<AuthUser> {
    const res = await apiClient.get<UserResponseDto>(`/users/${userId}`);
    return mapToAuthUser(res.data);
  },
};
