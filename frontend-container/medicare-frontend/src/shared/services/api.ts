// Base API service with common functionality
export const fetchData = async (url: string) => {
  const response = await fetch(url);
  return response.json();
};

// API response wrapper for consistent error handling
export interface ApiResponse<T> {
  data: T;
  error?: string;
  success: boolean;
}

// Generic API helper for creating mock responses
export const createMockResponse = <T>(data: T, delay = 100): Promise<ApiResponse<T>> => {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve({
        data,
        success: true,
      });
    }, delay);
  });
};

// Error response helper
export const createErrorResponse = (error: string): ApiResponse<any> => ({
  data: null,
  error,
  success: false,
});
