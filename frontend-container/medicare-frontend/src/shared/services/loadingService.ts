type LoadingListener = (isLoading: boolean) => void;
type ErrorListener = (error: string | null) => void;

class LoadingService {
  private isLoading: boolean = false;
  private error: string | null = null;
  private loadingListeners: LoadingListener[] = [];
  private errorListeners: ErrorListener[] = [];

  // Loading state management
  show(): void {
    this.isLoading = true;
    this.error = null; // Clear any previous errors
    this.notifyLoadingListeners();
    this.notifyErrorListeners();
  }

  hide(): void {
    this.isLoading = false;
    this.notifyLoadingListeners();
  }

  // Error state management
  setError(error: string | null): void {
    this.error = error;
    this.isLoading = false; // Hide loading when error occurs
    this.notifyLoadingListeners();
    this.notifyErrorListeners();
  }

  clearError(): void {
    this.error = null;
    this.notifyErrorListeners();
  }

  // Getters
  getIsLoading(): boolean {
    return this.isLoading;
  }

  getError(): string | null {
    return this.error;
  }

  // Listener management
  addLoadingListener(listener: LoadingListener): () => void {
    this.loadingListeners.push(listener);
    // Return unsubscribe function
    return () => {
      this.loadingListeners = this.loadingListeners.filter(l => l !== listener);
    };
  }

  addErrorListener(listener: ErrorListener): () => void {
    this.errorListeners.push(listener);
    // Return unsubscribe function
    return () => {
      this.errorListeners = this.errorListeners.filter(l => l !== listener);
    };
  }

  // Helper method for async operations
  async executeWithLoading<T>(asyncOperation: () => Promise<T>): Promise<T> {
    try {
      this.show();
      const result = await asyncOperation();
      this.hide();
      return result;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'An unexpected error occurred';
      this.setError(errorMessage);
      throw error;
    }
  }

  private notifyLoadingListeners(): void {
    this.loadingListeners.forEach(listener => listener(this.isLoading));
  }

  private notifyErrorListeners(): void {
    this.errorListeners.forEach(listener => listener(this.error));
  }
}

// Export singleton instance
export const loadingService = new LoadingService();
export type { LoadingListener, ErrorListener };
