type LoadingListener = (isLoading: boolean) => void;
type ErrorListener = (error: string | null) => void;

class LoadingService {
  private isLoading: boolean = false;
  private error: string | null = null;
  private loadingListeners: LoadingListener[] = [];
  private errorListeners: ErrorListener[] = [];

  show(): void {
    this.isLoading = true;
    this.error = null;
    this.notifyLoadingListeners();
    this.notifyErrorListeners();
  }

  hide(): void {
    this.isLoading = false;
    this.notifyLoadingListeners();
  }

  setError(error: string | null): void {
    this.error = error;
    this.isLoading = false;
    this.notifyLoadingListeners();
    this.notifyErrorListeners();
  }

  clearError(): void {
    this.error = null;
    this.notifyErrorListeners();
  }

  getIsLoading(): boolean {
    return this.isLoading;
  }

  getError(): string | null {
    return this.error;
  }

  addLoadingListener(listener: LoadingListener): () => void {
    this.loadingListeners.push(listener);
    return () => {
      this.loadingListeners = this.loadingListeners.filter(
        (l) => l !== listener
      );
    };
  }

  addErrorListener(listener: ErrorListener): () => void {
    this.errorListeners.push(listener);
    return () => {
      this.errorListeners = this.errorListeners.filter((l) => l !== listener);
    };
  }

  async executeWithLoading<T>(asyncOperation: () => Promise<T>): Promise<T> {
    this.show();
    const result = await asyncOperation();
    this.hide();
    return result;
  }

  private notifyLoadingListeners(): void {
    this.loadingListeners.forEach((listener) => listener(this.isLoading));
  }

  private notifyErrorListeners(): void {
    this.errorListeners.forEach((listener) => listener(this.error));
  }
}

export const loadingService = new LoadingService();
export type { ErrorListener, LoadingListener };
