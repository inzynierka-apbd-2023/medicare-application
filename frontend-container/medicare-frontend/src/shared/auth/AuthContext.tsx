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

// ===== DEVELOPMENT MOCK =====
const DEV_MOCK_OWNER = true; // Set to false to disable mock
const MOCK_OWNER_USER: AuthUser = {
  id: "mock-owner-id",
  username: "owner-dev",
  email: "owner@dev.com",
  firstName: "Dev",
  lastName: "Owner",
  role: "Owner",
  phoneNumber: "+1234567890",
  dateOfBirth: "1980-01-01",
  avatarUrl: null,
  address: "123 Dev Street",
};
const MOCK_TOKEN = "mock-owner-token-for-development";

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
  // === DEVELOPMENT MOCK SETUP ===
  const [user, setUser] = useState<AuthUser | null>(() => {
    if (DEV_MOCK_OWNER) {
      console.log("🚧 DEV MODE: Using mock owner user");
      return MOCK_OWNER_USER;
    }
    
    try {
      const raw = sessionStorage.getItem("authUser");
      if (raw) return JSON.parse(raw) as AuthUser;
    } catch {
      /* ignore */
    }
    return null;
  });
  
  const [token, setToken] = useState<string | null>(() => {
    if (DEV_MOCK_OWNER) {
      return MOCK_TOKEN;
    }
    return authService.getToken();
  });
  
  const memAccessRef = useRef<string | null>(DEV_MOCK_OWNER ? MOCK_TOKEN : authService.getToken());
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
    if (DEV_MOCK_OWNER) {
      console.log("🚧 DEV MODE: Mock login as owner");
      setUser(MOCK_OWNER_USER);
      setToken(MOCK_TOKEN);
      memAccessRef.current = MOCK_TOKEN;
      return;
    }
    
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
    // Skip storage hydration in dev mock mode
    if (DEV_MOCK_OWNER) {
      console.log("🚧 DEV MODE: Skipping storage hydration, using mock data");
      return;
    }
    
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
