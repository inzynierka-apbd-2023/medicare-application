import React from "react";
import { Loading } from "./Loading";

export interface LoadingOverlayProps {
  isLoading: boolean;
  message?: string;
  children: React.ReactNode;
  className?: string;
}

const LoadingOverlay: React.FC<LoadingOverlayProps> = ({
  isLoading,
  message = "Loading...",
  children,
  className = "",
}) => {
  return (
    <div className={`relative ${className}`}>
      {children}
      {isLoading && (
        <div className="absolute inset-0 bg-white bg-opacity-80 flex items-center justify-center z-50">
          <div className="flex flex-col items-center space-y-4">
            <Loading size="lg" />
            {message && <p className="text-gray-600 font-medium">{message}</p>}
          </div>
        </div>
      )}
    </div>
  );
};

export { LoadingOverlay };
