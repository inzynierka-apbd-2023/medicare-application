import { ReactNode } from "react";

interface LoadingOverlayProps {
  isLoading: boolean;
  message?: string;
  children: ReactNode;
  className?: string;
}

export function LoadingOverlay({
  isLoading,
  message = "Loading...",
  children,
  className = "",
}: LoadingOverlayProps) {
  return (
    <div className={`relative ${className}`}>
      {children}
      {isLoading && (
        <div className="absolute inset-0 bg-white bg-opacity-75 flex items-center justify-center z-50">
          <div className="flex flex-col items-center space-y-4">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <div className="text-blue-600 text-lg font-medium">{message}</div>
          </div>
        </div>
      )}
    </div>
  );
}

export type { LoadingOverlayProps };
