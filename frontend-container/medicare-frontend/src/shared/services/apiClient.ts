import axios, { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";

import { authService } from "./authService";

// Resolve API base URL ensuring we keep the current origin (with port) to avoid connection issues.
interface ImportMetaEnv {
  VITE_API_BASE_URL?: string;
}
declare const importMeta: { env?: ImportMetaEnv };
function resolveBaseUrl(): string {
  // Attempt to read via global import.meta if available; fallback to declared importMeta for typing
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const meta: any =
    typeof import.meta !== "undefined" ? import.meta : importMeta;
  const path = meta?.env?.VITE_API_BASE_URL || "/api";
  // If an absolute URL is provided, use it as-is
  if (/^https?:\/\//i.test(path)) return path;
  // Otherwise, prefix with current origin, preserving the port
  const origin =
    typeof window !== "undefined" && window.location?.origin
      ? window.location.origin
      : "";
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

let isRefreshing = false;
let pending: Array<{
  resolve: (token: string | null) => void;
  reject: (e: unknown) => void;
}> = [];

async function processQueue(token: string | null, error: unknown) {
  pending.forEach((p) => (error ? p.reject(error) : p.resolve(token)));
  pending = [];
}

interface RetriableRequestConfig extends AxiosRequestConfig {
  _retry?: boolean;
}

apiClient.interceptors.response.use(
  (r: AxiosResponse) => r,
  async (error: AxiosError) => {
    const original = error.config as RetriableRequestConfig;
    if (error.response?.status === 401 && !original?._retry) {
      original._retry = true;
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          pending.push({
            resolve: (token) => {
              if (token && original.headers)
                original.headers.Authorization = `Bearer ${token}`;
              resolve(apiClient(original));
            },
            reject,
          });
        });
      }
      isRefreshing = true;
      try {
        const newToken = await authService.refresh();
        await processQueue(newToken, null);
        if (newToken && original.headers)
          original.headers.Authorization = `Bearer ${newToken}`;
        return apiClient(original);
      } catch (e) {
        await processQueue(null, e);
        authService.logout();
        return Promise.reject(e);
      } finally {
        isRefreshing = false;
      }
    }
    return Promise.reject(error);
  }
);
