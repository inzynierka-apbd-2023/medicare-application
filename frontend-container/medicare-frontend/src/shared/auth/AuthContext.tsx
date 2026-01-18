import {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import { useNavigate } from "react-router-dom";

import { useApiToastInit } from "../hooks/useApiToastInit";
import {
  authService,
  AuthUser,
  RegisterRequest,
} from "../services/authService";
import { usersApi } from "../services/usersApi";
import { toastMessages, useToast } from "../toast";

interface AuthState {
  user: AuthUser | null;
  loading: boolean;
  error: string | null;
  login: (username: string, password: string) => Promise<boolean>;
  register: (data: RegisterRequest) => Promise<AuthUser>;
  updateProfile: (
    data: {
      phoneNumber?: string;
      dateOfBirth?: string;
      avatarUrl?: string | null;
    },
    userIdOverride?: string
  ) => Promise<void>;
  logout: () => void;
}

interface ApiError {
  response?: {
    data?: {
      message?: string;
    };
  };
}

const Ctx = createContext<AuthState | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const navigate = useNavigate();
  const { showError } = useToast();

  useApiToastInit();

  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /* ... */
  const login = useCallback(
    async (username: string, password: string): Promise<boolean> => {
      setLoading(true);
      setError(null);
      try {
        const authUser = await authService.login(
          username.trim(),
          password.trim()
        );
        setUser(authUser);
        return true;
      } catch (e: unknown) {
        const err = e as ApiError;
        const errorMessage =
          err.response?.data?.message || toastMessages.auth.loginError;
        setError(errorMessage);
        return false;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const register = useCallback(
    async (data: RegisterRequest): Promise<AuthUser> => {
      setLoading(true);
      setError(null);
      try {
        const authUser = await authService.register(data);
        setUser(authUser);
        return authUser;
      } catch (e: unknown) {
        const err = e as ApiError;
        const errorMessage =
          err.response?.data?.message || toastMessages.auth.registerError;
        setError(errorMessage);
        throw e;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const updateProfile = useCallback(
    async (
      data: {
        phoneNumber?: string;
        dateOfBirth?: string;
        avatarUrl?: string | null;
      },
      userIdOverride?: string
    ) => {
      setLoading(true);
      setError(null);
      try {
        const targetUserId = userIdOverride ?? user?.id;
        if (!targetUserId) throw new Error("Missing user id");
        const dto: {
          phoneNumber?: string;
          dateOfBirth?: string;
          avatarUrl?: string | null;
        } = {};
        if (data.phoneNumber !== undefined) dto.phoneNumber = data.phoneNumber;
        if (data.dateOfBirth !== undefined) dto.dateOfBirth = data.dateOfBirth;
        if (data.avatarUrl !== undefined)
          dto.avatarUrl = data.avatarUrl ?? null;
        await usersApi.updateProfile(targetUserId, dto);
        const fresh = await usersApi.getUser(targetUserId);
        setUser((prev: AuthUser | null) =>
          prev ? { ...prev, ...fresh } : fresh
        );
      } catch (e: unknown) {
        const err = e as ApiError;
        const errorMessage =
          err.response?.data?.message || toastMessages.auth.profileUpdateError;
        setError(errorMessage);
        throw e;
      } finally {
        setLoading(false);
      }
    },
    [user?.id]
  );

  const clearAuthSession = useCallback(() => {
    setUser(null);
  }, []);

  const logout = useCallback(() => {
    authService.logout(); // This handles API call and success toast
    clearAuthSession();
  }, [clearAuthSession]);

  const handleAuthLogout = useCallback(() => {
    // Only clear session, don't call API logout again
    clearAuthSession();
    showError(toastMessages.auth.sessionExpired);
    navigate("/login");
  }, [clearAuthSession, navigate, showError]);

  useEffect(() => {
    window.addEventListener("auth:logout", handleAuthLogout);
    return () => window.removeEventListener("auth:logout", handleAuthLogout);
  }, [handleAuthLogout]);

  const ctxValue = useMemo(
    () => ({
      user,
      loading,
      error,
      login,
      register,
      updateProfile,
      logout,
    }),
    [user, loading, error, login, register, updateProfile, logout]
  );

  return <Ctx.Provider value={ctxValue}>{children}</Ctx.Provider>;
};

export const useAuth = () => {
  const ctx = useContext(Ctx);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
};
