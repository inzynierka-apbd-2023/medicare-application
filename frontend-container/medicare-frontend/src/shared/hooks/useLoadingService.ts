import { useCallback, useEffect, useState } from "react";
import { loadingService } from "@shared/services/loadingService";

export function useLoadingService() {
  const [isLoading, setIsLoading] = useState(loadingService.getIsLoading());
  const [error, setError] = useState(loadingService.getError());

  useEffect(() => {
    const unsubscribeLoading = loadingService.addLoadingListener(setIsLoading);
    const unsubscribeError = loadingService.addErrorListener(setError);

    return () => {
      unsubscribeLoading();
      unsubscribeError();
    };
  }, []);

  const clearError = useCallback(() => {
    loadingService.clearError();
  }, []);

  const show = useCallback(() => {
    loadingService.show();
  }, []);

  const hide = useCallback(() => {
    loadingService.hide();
  }, []);

  const setErrorMessage = useCallback((error: string) => {
    loadingService.setError(error);
  }, []);

  // Enhanced executeWithLoading that accepts options
  const executeWithLoading = useCallback(
    async <T>(
      asyncOperation: () => Promise<T>,
      options?: { skipLoading?: boolean }
    ): Promise<T> => {
      if (options?.skipLoading) {
        // Execute without showing loading state
        try {
          return await asyncOperation();
        } catch (error) {
          const errorMessage =
            error instanceof Error
              ? error.message
              : "An unexpected error occurred";
          loadingService.setError(errorMessage);
          throw error;
        }
      }

      // Use the original executeWithLoading method
      return loadingService.executeWithLoading(asyncOperation);
    },
    []
  );

  // New method for initial data loading (with loading state)
  const executeInitialLoad = useCallback(
    async <T>(asyncOperation: () => Promise<T>): Promise<T> => {
      return executeWithLoading(asyncOperation);
    },
    [executeWithLoading]
  );

  // New method for subsequent operations (without loading state)
  const executeQuietly = useCallback(
    async <T>(asyncOperation: () => Promise<T>): Promise<T> => {
      return executeWithLoading(asyncOperation, { skipLoading: true });
    },
    [executeWithLoading]
  );

  return {
    isLoading,
    error,
    clearError,
    show,
    hide,
    setError: setErrorMessage,
    executeWithLoading,
    executeInitialLoad,
    executeQuietly,
  };
}
