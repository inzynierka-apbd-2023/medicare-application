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

// Cold-start resilience: long timeout for scale-to-zero services
const API_TIMEOUT_MS = 120000; // 2 minutes

// Retry configuration for cold-start resilience
const MAX_RETRIES = 3;
const INITIAL_RETRY_DELAY_MS = 5000; // 5 seconds

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: API_TIMEOUT_MS,
  headers: { "Content-Type": "application/json" },
});

// Extend AxiosRequestConfig to track retry count
interface RetryableRequestConfig extends AxiosRequestConfig {
  __retryCount?: number;
}

// Cold-start retry interceptor: retries on 502/503/504 and network timeouts
apiClient.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error: AxiosError) => {
    const config = error.config as RetryableRequestConfig;
    if (!config) {
      return Promise.reject(error);
    }

    // Initialize retry count
    config.__retryCount = config.__retryCount || 0;

    // Don't retry if already exhausted retries
    if (config.__retryCount >= MAX_RETRIES) {
      return Promise.reject(error);
    }

    // Determine if this is a cold-start related error worth retrying
    const isColdStartError =
      !error.response || // Network error (service not yet accepting connections)
      error.code === "ECONNABORTED" || // Timeout
      error.code === "ERR_NETWORK" || // Network failure
      [502, 503, 504].includes(error.response?.status || 0); // Gateway errors

    if (isColdStartError) {
      config.__retryCount += 1;
      const delay =
        INITIAL_RETRY_DELAY_MS * Math.pow(2, config.__retryCount - 1);
      console.log(
        `[Cold-start retry] Attempt ${config.__retryCount}/${MAX_RETRIES} for ${config.url}, waiting ${delay}ms...`
      );
      await new Promise((resolve) => setTimeout(resolve, delay));
      return apiClient(config);
    }

    return Promise.reject(error);
  }
);

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
    const url = original?.url || "";

    // Skip refresh logic for auth endpoints - these handle their own errors
    const isAuthEndpoint =
      url.includes("/auth/login") ||
      url.includes("/auth/register") ||
      url.includes("/auth/refresh");

    if (
      error.response?.status === 401 &&
      !original?._retry &&
      !isAuthEndpoint
    ) {
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

        if (!newToken) {
          authService.logout();
          if (typeof window !== "undefined") {
            window.dispatchEvent(new Event("auth:logout"));
          }
          await processQueue(null, null);
          return Promise.reject(error);
        }

        await processQueue(newToken, null);
        if (newToken && original.headers)
          original.headers.Authorization = `Bearer ${newToken}`;
        return apiClient(original);
      } catch (e) {
        await processQueue(null, e);
        authService.logout();
        if (typeof window !== "undefined") {
          window.dispatchEvent(new Event("auth:logout"));
        }
        return Promise.reject(e);
      } finally {
        isRefreshing = false;
      }
    }
    return Promise.reject(error);
  }
);
