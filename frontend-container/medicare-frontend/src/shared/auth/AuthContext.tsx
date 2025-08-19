import {
  createContext,
  ReactNode,
  useContext,
  useEffect,
  useMemo,
  useRef,
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
  updateProfile: (
    data: {
      phoneNumber?: string;
      dateOfBirth?: string;
      avatarUrl?: string | null;
    },
    userIdOverride?: string
  ) => Promise<void>;
  logout: () => void;
  error: string | null;
}

const Ctx = createContext<AuthState | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<AuthUser | null>(() => {
    try {
      const raw = sessionStorage.getItem("authUser");
      if (raw) return JSON.parse(raw) as AuthUser;
    } catch {
      /* ignore */
    }
    return null;
  });
  const [token, setToken] = useState<string | null>(authService.getToken());
  const memAccessRef = useRef<string | null>(authService.getToken());
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const applyAuth = (resp: AuthResponse) => {
    const at = resp.accessToken || resp.token || null;
    setToken(at);
    memAccessRef.current = at;
    setUser(resp.user);
    try {
      sessionStorage.setItem("authUser", JSON.stringify(resp.user));
    } catch {
      /* ignore storage errors */
    }
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

  const updateProfile = async (
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
      // Real API: persist and refresh
      const targetUserId = userIdOverride ?? user?.id;
      if (!targetUserId) throw new Error("Missing user id");
      const dto: {
        phoneNumber?: string;
        dateOfBirth?: string;
        avatarUrl?: string | null;
      } = {};
      if (data.phoneNumber !== undefined) dto.phoneNumber = data.phoneNumber;
      if (data.dateOfBirth !== undefined) dto.dateOfBirth = data.dateOfBirth;
      if (data.avatarUrl !== undefined) dto.avatarUrl = data.avatarUrl ?? null;
      await usersApi.updateProfile(targetUserId, dto);
      const fresh = await usersApi.getUser(targetUserId);
      setUser((prev: AuthUser | null) =>
        prev ? { ...prev, ...fresh } : fresh
      );
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
    } catch {
      /* ignore */
    }
    setUser(null);
    setToken(null);
  };

  useEffect(() => {
    // Hydrate from storage on mount if available
    if (!user) {
      try {
        const raw = sessionStorage.getItem("authUser");
        if (raw) {
          const parsed: AuthUser = JSON.parse(raw);
          setUser(parsed);
        }
      } catch {
        /* ignore parse */
      }
    }
    if (!token) {
      const existing = authService.getToken();
      if (existing) {
        setToken(existing);
        memAccessRef.current = existing;
      }
    }
    // We intentionally run only once on mount for initial hydration.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const ctxValue = useMemo(
    () => ({
      user,
      token,
      loading,
      login,
      register,
      updateProfile,
      logout,
      error,
    }),
    // Functions are stable enough; we knowingly exclude them.
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [user, token, loading, error]
  );

  return <Ctx.Provider value={ctxValue}>{children}</Ctx.Provider>;
};

export const useAuth = () => {
  const ctx = useContext(Ctx);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
};
