import axios, { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";

interface ViteImportMeta {
  env?: {
    VITE_API_BASE_URL?: string;
  };
}

declare const importMeta: ViteImportMeta;

function resolveBaseUrl(): string {
  const meta: ViteImportMeta =
    typeof import.meta !== "undefined"
      ? (import.meta as unknown as ViteImportMeta)
      : importMeta;
  const path = meta?.env?.VITE_API_BASE_URL || "/api";
  if (/^https?:\/\//i.test(path)) return path;
  const origin =
    typeof window !== "undefined" && window.location?.origin
      ? window.location.origin
      : "";
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;
  return `${origin}${normalizedPath}`;
}

export const API_BASE_URL = resolveBaseUrl();

const API_TIMEOUT_MS = 120000;
const MAX_RETRIES = 3;
const INITIAL_RETRY_DELAY_MS = 5000;

interface RetryableRequestConfig extends AxiosRequestConfig {
  __retryCount?: number;
}

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: API_TIMEOUT_MS,
  headers: { "Content-Type": "application/json" },
  withCredentials: true,
});

apiClient.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error: AxiosError) => {
    const config = error.config as RetryableRequestConfig | undefined;
    if (!config) {
      return Promise.reject(error);
    }

    config.__retryCount = config.__retryCount || 0;

    if (config.__retryCount >= MAX_RETRIES) {
      return Promise.reject(error);
    }

    const isColdStartError =
      !error.response ||
      error.code === "ECONNABORTED" ||
      error.code === "ERR_NETWORK" ||
      [502, 503, 504].includes(error.response?.status || 0);

    if (isColdStartError) {
      config.__retryCount += 1;
      const delay =
        INITIAL_RETRY_DELAY_MS * Math.pow(2, config.__retryCount - 1);
      await new Promise((resolve) => setTimeout(resolve, delay));
      return apiClient(config);
    }

    if (error.response?.status === 401) {
      const url = config.url || "";
      const isAuthEndpoint =
        /\/(auth\/login|auth\/register|auth\/refresh)/.test(url);
      if (!isAuthEndpoint) {
        window.dispatchEvent(new CustomEvent("auth:logout"));
      }
    }

    return Promise.reject(error);
  }
);
