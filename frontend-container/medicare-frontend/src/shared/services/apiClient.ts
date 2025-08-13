import axios, { AxiosResponse } from "axios";

// Derive API base URL from Vite env (injected at build) or fallback to same-origin proxy or localhost.
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "/api";

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
