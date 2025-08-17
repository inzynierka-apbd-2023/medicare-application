import { apiClient } from './apiClient';

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
}

export interface AuthResponse {
  token: string;
  user: AuthUser;
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
}

const TOKEN_KEY = 'authToken';

export const authService = {
  async login(username: string, password: string): Promise<AuthResponse> {
    const res = await apiClient.post<AuthResponse>('/auth/login', { username, password });
    persistToken(res.data.token);
    return res.data;
  },
  async register(req: RegisterRequest): Promise<AuthResponse> {
    const res = await apiClient.post<AuthResponse>('/auth/register', {
      username: req.username,
      email: req.email,
      password: req.password,
      firstName: req.firstName,
      lastName: req.lastName,
      phoneNumber: req.phoneNumber,
      role: req.role ?? 'Patient',
      dateOfBirth: req.dateOfBirth || null
    });
    persistToken(res.data.token);
    return res.data;
  },
  logout() {
    localStorage.removeItem(TOKEN_KEY);
  },
  getToken() { return localStorage.getItem(TOKEN_KEY); },
};

function persistToken(token: string) {
  localStorage.setItem(TOKEN_KEY, token);
}
