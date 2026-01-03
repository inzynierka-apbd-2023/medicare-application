import { apiClient } from "./apiClient";

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

export interface AuthResponse {
  accessToken: string; // adapted for backward compatibility mapping
  token?: string; // legacy field from older responses (still accept)
  user: AuthUser;
  refreshToken?: string;
  accessTokenExpiresAt?: string;
  refreshTokenExpiresAt?: string;
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
}

const TOKEN_KEY = "authToken";
const REFRESH_KEY = "refreshToken";
const ACCESS_EXP_KEY = "accessTokenExpires";
const REFRESH_EXP_KEY = "refreshTokenExpires";

export const authService = {
  async login(username: string, password: string): Promise<AuthResponse> {
    const res = await apiClient.post<AuthResponse>("/auth/login", {
      username,
      password,
    });
    persistTokens(res.data);
    return res.data;
  },
  async register(req: RegisterRequest): Promise<AuthResponse> {
    const res = await apiClient.post<AuthResponse>("/auth/register", {
      username: req.username,
      email: req.email,
      password: req.password,
      firstName: req.firstName,
      lastName: req.lastName,
      phoneNumber: req.phoneNumber,
      role: req.role ?? "Patient",
      dateOfBirth: req.dateOfBirth || null,
      planId: req.planId || null,
    });
    persistTokens(res.data);
    return res.data;
  },
  logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
    localStorage.removeItem(ACCESS_EXP_KEY);
    localStorage.removeItem(REFRESH_EXP_KEY);
  },
  getToken() {
    return localStorage.getItem(TOKEN_KEY);
  },
  getRefreshToken() {
    return localStorage.getItem(REFRESH_KEY);
  },
  async refresh(): Promise<string | null> {
    const existing = localStorage.getItem(REFRESH_KEY);
    if (!existing) return null;
    try {
      const res = await apiClient.post<AuthResponse>("/auth/refresh", {
        refreshToken: existing,
      });
      persistTokens(res.data);
      return res.data.accessToken || res.data.token || null;
    } catch {
      this.logout();
      return null;
    }
  },
};

function persistTokens(r: AuthResponse) {
  const access = r.accessToken || r.token || "";
  if (access) localStorage.setItem(TOKEN_KEY, access);
  if (r.refreshToken) localStorage.setItem(REFRESH_KEY, r.refreshToken);
  if (r.accessTokenExpiresAt)
    localStorage.setItem(ACCESS_EXP_KEY, r.accessTokenExpiresAt);
  if (r.refreshTokenExpiresAt)
    localStorage.setItem(REFRESH_EXP_KEY, r.refreshTokenExpiresAt);
}
