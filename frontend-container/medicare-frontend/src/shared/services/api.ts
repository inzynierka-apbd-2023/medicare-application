import { toastMessages } from "@shared/toast/toastMessages";
import { AxiosError, AxiosRequestConfig } from "axios";

import { apiClient } from "./apiClient";
import { loadingService } from "./loadingService";

export interface ApiResponse<T> {
  data: T;
  error?: string;
  success: boolean;
  status?: number | undefined;
}

export interface ApiErrorData {
  message?: string;
  errors?: Record<string, string[]>;
}

export const createSuccessResponse = <T>(data: T): ApiResponse<T> => ({
  data,
  success: true,
});

export const createErrorResponse = <T = unknown>(
  error: string,
  status?: number
): ApiResponse<T> => ({
  data: null as T,
  error,
  success: false,
  status,
});

export type StatusCodeHandler = (status: number, message: string) => void;

export const getStatusMessage = (status: number, fallback?: string): string => {
  const messages = toastMessages.httpStatus as Record<number, string>;
  return messages[status] || fallback || "An unexpected error occurred";
};

export const extractErrorMessage = (
  error: AxiosError<ApiErrorData>
): string => {
  if (error.response?.data?.message) {
    return error.response.data.message;
  }
  if (error.response?.data?.errors) {
    const firstError = Object.values(error.response.data.errors)[0];
    if (firstError && firstError.length > 0) {
      return firstError[0];
    }
  }
  if (error.response?.status) {
    return getStatusMessage(error.response.status);
  }
  if (error.code === "ECONNABORTED") {
    return "Request timed out. Please try again.";
  }
  if (error.code === "ERR_NETWORK") {
    return "Network error. Please check your connection.";
  }
  return "An unexpected error occurred";
};

export interface HandleApiCallOptions {
  showToastOnSuccess?: boolean;
  showToastOnError?: boolean;
  successMessage?: string;
  onSuccess?: StatusCodeHandler;
  onError?: StatusCodeHandler;
  skipLoading?: boolean;
}

let globalToastHandler: {
  showSuccess: (msg: string) => void;
  showError: (msg: string) => void;
} | null = null;

export const setGlobalToastHandler = (handler: typeof globalToastHandler) => {
  globalToastHandler = handler;
};

export const handleApiCall = async <T>(
  apiCall: () => Promise<T>,
  options: HandleApiCallOptions = {}
): Promise<ApiResponse<T>> => {
  const {
    showToastOnSuccess = false,
    showToastOnError = true,
    successMessage,
    onSuccess,
    onError,
    skipLoading = false,
  } = options;

  if (!skipLoading) {
    loadingService.show();
  }

  try {
    const data = await apiCall();

    if (!skipLoading) {
      loadingService.hide();
    }

    if (showToastOnSuccess && successMessage && globalToastHandler) {
      globalToastHandler.showSuccess(successMessage);
    }

    onSuccess?.(200, successMessage || "Success");

    return createSuccessResponse(data);
  } catch (error) {
    const axiosError = error as AxiosError<ApiErrorData>;
    const status = axiosError.response?.status || 0;
    const message = extractErrorMessage(axiosError);

    if (!skipLoading) {
      loadingService.hide();
    }

    if (showToastOnError && globalToastHandler) {
      globalToastHandler.showError(message);
    }

    onError?.(status, message);

    return createErrorResponse<T>(message, status);
  }
};

export const handleApiCallWithThrow = async <T>(
  apiCall: () => Promise<T>,
  options: HandleApiCallOptions = {}
): Promise<T> => {
  const result = await handleApiCall(apiCall, options);
  if (!result.success) {
    throw new Error(result.error);
  }
  return result.data;
};

export const api = {
  get: <T>(
    url: string,
    config?: AxiosRequestConfig,
    options?: HandleApiCallOptions
  ) =>
    handleApiCallWithThrow(
      () => apiClient.get<T>(url, config).then((res) => res.data),
      options
    ),

  post: <T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
    options?: HandleApiCallOptions
  ) =>
    handleApiCallWithThrow(
      () => apiClient.post<T>(url, data, config).then((res) => res.data),
      options
    ),

  put: <T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
    options?: HandleApiCallOptions
  ) =>
    handleApiCallWithThrow(
      () => apiClient.put<T>(url, data, config).then((res) => res.data),
      options
    ),

  patch: <T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
    options?: HandleApiCallOptions
  ) =>
    handleApiCallWithThrow(
      () => apiClient.patch<T>(url, data, config).then((res) => res.data),
      options
    ),

  delete: <T>(
    url: string,
    config?: AxiosRequestConfig,
    options?: HandleApiCallOptions
  ) =>
    handleApiCallWithThrow(
      () => apiClient.delete<T>(url, config).then((res) => res.data),
      options
    ),
};
