import React, {
  createContext,
  ReactNode,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";

import { AuthResponse, authService, AuthUser } from "../services/authService";
import { usersApi } from "../services/usersApi";


interface AuthState {
  user: AuthUser | null;
  token: string | null;
  loading: boolean;
  login: (username: string, password: string) => Promise<void>;
  register: (data: {
    username: string;
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    phoneNumber?: string;
    dateOfBirth?: string;
    role?: string;
  }) => Promise<AuthResponse>;
  updateProfile: (data: {
    phoneNumber?: string;
    dateOfBirth?: string;
    avatarUrl?: string | null;
  }, userIdOverride?: string) => Promise<void>;
  logout: () => void;
  error: string | null;
}

const Ctx = createContext<AuthState | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [token, setToken] = useState<string | null>(authService.getToken());
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const applyAuth = (resp: AuthResponse) => {
    setToken(resp.token);
    setUser(resp.user);
  };

  const login = async (username: string, password: string) => {
    setLoading(true);
    setError(null);
    try {
  // Trim inputs to avoid accidental whitespace issues from paste/typing
  const resp = await authService.login(username.trim(), password.trim());
      applyAuth(resp);
    } catch (e: unknown) {
      const error = e as { response?: { data?: { message?: string } } };
      setError(error.response?.data?.message || "Login failed");
    } finally {
      setLoading(false);
    }
  };

  const register = async (data: {
    username: string;
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    phoneNumber?: string;
    dateOfBirth?: string;
    role?: string;
  }): Promise<AuthResponse> => {
    setLoading(true);
    setError(null);
    try {
      const resp = await authService.register(data);
      applyAuth(resp);
      return resp;
    } catch (e: unknown) {
      const error = e as { response?: { data?: { message?: string } } };
      setError(error.response?.data?.message || "Registration failed");
      throw e;
    } finally {
      setLoading(false);
    }
  };

  const updateProfile = async (data: {
    phoneNumber?: string;
    dateOfBirth?: string;
    avatarUrl?: string | null;
  }, userIdOverride?: string) => {
    setLoading(true);
    setError(null);
    try {
      // Real API: persist and refresh
      const targetUserId = userIdOverride ?? user?.id;
      if (!targetUserId) throw new Error("Missing user id");
      const dto: { phoneNumber?: string; dateOfBirth?: string; avatarUrl?: string | null } = {};
      if (data.phoneNumber !== undefined) dto.phoneNumber = data.phoneNumber;
      if (data.dateOfBirth !== undefined) dto.dateOfBirth = data.dateOfBirth;
      if (data.avatarUrl !== undefined) dto.avatarUrl = data.avatarUrl ?? null;
      await usersApi.updateProfile(targetUserId, dto);
      const fresh = await usersApi.getUser(targetUserId);
      setUser((prev: AuthUser | null) => (prev ? { ...prev, ...fresh } : fresh));
    } catch (e: unknown) {
      const error = e as { response?: { data?: { message?: string } } };
      setError(error.response?.data?.message || "Failed to update profile");
      throw e;
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    // Clear persisted token and any user state
    authService.logout();
    try {
      sessionStorage.clear();
      localStorage.removeItem("authToken");
    } catch {}
    setUser(null);
    setToken(null);
  };

  useEffect(() => {
    /* placeholder for future token decode */
  }, []);

  const ctxValue = useMemo(
    () => ({ user, token, loading, login, register, updateProfile, logout, error }),
    [user, token, loading, error]
  );

  return (
    <Ctx.Provider value={ctxValue}>
      {children}
    </Ctx.Provider>
  );
};

export const useAuth = () => {
  const ctx = useContext(Ctx);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
};
