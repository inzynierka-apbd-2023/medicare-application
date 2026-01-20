import { toastMessages } from "@shared/toast/toastMessages";

import { api } from "./api";

export interface AuthUser {
  id: string;
  username: string;
  email: string;
  role: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  avatarUrl?: string | null;
  address?: string | null;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  role?: string;
  dateOfBirth?: string;
  planId?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
  avatarUrl?: string | null;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export const authService = {
  async login(username: string, password: string): Promise<AuthUser> {
    return api.post<AuthUser>(
      "/auth/login",
      { username, password },
      undefined,
      {
        showToastOnSuccess: true,
        successMessage: toastMessages.auth.loginSuccess,
      }
    );
  },

  async register(req: RegisterRequest): Promise<AuthUser> {
    return api.post<AuthUser>(
      "/auth/register",
      {
        username: req.username,
        email: req.email,
        password: req.password,
        firstName: req.firstName,
        lastName: req.lastName,
        phoneNumber: req.phoneNumber,
        role: req.role ?? "Patient",
        dateOfBirth: req.dateOfBirth || null,
        planId: req.planId || null,
        addressLine1: req.addressLine1,
        addressLine2: req.addressLine2,
        city: req.city,
        state: req.state,
        zipCode: req.zipCode,
        country: req.country,
        avatarUrl: req.avatarUrl,
      },
      undefined,
      {
        showToastOnSuccess: true,
        successMessage: toastMessages.auth.registerSuccess,
      }
    );
  },

  async logout(): Promise<void> {
    return api.post<void>("/auth/logout", undefined, undefined, {
      showToastOnSuccess: true,
      successMessage: toastMessages.auth.logoutSuccess,
    });
  },

  async refresh(): Promise<AuthUser> {
    return api.post<AuthUser>("/auth/refresh", {}, undefined, {
      showToastOnError: false,
    });
  },

  async forgotPassword(email: string): Promise<void> {
    return api.post<void>("/auth/forgot-password", { email }, undefined, {
      showToastOnSuccess: true,
      successMessage: toastMessages.auth.forgotPasswordSuccess,
    });
  },

  async resetPassword(token: string, newPassword: string): Promise<void> {
    return api.post<void>(
      "/auth/reset-password",
      { token, newPassword },
      undefined,
      {
        showToastOnSuccess: true,
        successMessage: toastMessages.auth.resetPasswordSuccess,
      }
    );
  },

  async changePassword(
    currentPassword: string,
    newPassword: string
  ): Promise<void> {
    return api.post<void>(
      "/auth/change-password",
      { currentPassword, newPassword },
      undefined,
      {
        showToastOnSuccess: true,
        successMessage: toastMessages.auth.changePasswordSuccess,
      }
    );
  },
};
