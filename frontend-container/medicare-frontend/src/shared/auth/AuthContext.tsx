import React, {
  createContext,
  ReactNode,
  useContext,
  useEffect,
  useState,
} from "react";

import { AuthResponse, authService, AuthUser } from "../services/authService";
import { mockAuthService } from "../services/mockAuthService";

// Set to true to use mock authentication for testing
const USE_MOCK_AUTH = true;

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
  }) => Promise<void>;
  logout: () => void;
  error: string | null;
}

const Ctx = createContext<AuthState | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const service = USE_MOCK_AUTH ? mockAuthService : authService;
  const [user, setUser] = useState<AuthUser | null>(null);
  const [token, setToken] = useState<string | null>(service.getToken());
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
      const service = USE_MOCK_AUTH ? mockAuthService : authService;
      const resp = await service.login(username, password);
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
  }) => {
    setLoading(true);
    setError(null);
    try {
      const service = USE_MOCK_AUTH ? mockAuthService : authService;
      const resp = await service.register(data);
      applyAuth(resp);
    } catch (e: unknown) {
      const error = e as { response?: { data?: { message?: string } } };
      setError(error.response?.data?.message || "Registration failed");
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    const service = USE_MOCK_AUTH ? mockAuthService : authService;
    service.logout();
    setUser(null);
    setToken(null);
  };

  useEffect(() => {
    /* placeholder for future token decode */
  }, []);

  return (
    <Ctx.Provider
      value={{ user, token, loading, login, register, logout, error }}
    >
      {children}
    </Ctx.Provider>
  );
};

export const useAuth = () => {
  const ctx = useContext(Ctx);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
};
