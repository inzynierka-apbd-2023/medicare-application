import axios, { AxiosResponse } from "axios";

// Resolve API base URL ensuring we keep the current origin (with port) to avoid connection issues.
function resolveBaseUrl(): string {
  const path = (import.meta as any).env?.VITE_API_BASE_URL || "/api";
  // If an absolute URL is provided, use it as-is
  if (/^https?:\/\//i.test(path)) return path;
  // Otherwise, prefix with current origin, preserving the port
  const origin = typeof window !== "undefined" && window.location?.origin ? window.location.origin : "";
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;
  return `${origin}${normalizedPath}`;
}

export const API_BASE_URL = resolveBaseUrl();

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: { "Content-Type": "application/json" },
});

apiClient.interceptors.request.use((cfg) => {
  const token = localStorage.getItem("authToken");
  if (token && cfg.headers) {
    cfg.headers.Authorization = `Bearer ${token}`;
  }
  return cfg;
});

apiClient.interceptors.response.use(
  (r: AxiosResponse) => r,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  (err: any) => {
    if (err?.response?.status === 401) {
      localStorage.removeItem("authToken");
    }
    return Promise.reject(err instanceof Error ? err : new Error("API error"));
  }
);
