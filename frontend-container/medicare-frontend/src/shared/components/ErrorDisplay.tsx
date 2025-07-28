import React from "react";
import { AlertCircle, RefreshCw } from "lucide-react";
import { Button } from "./Button";
import { Card } from "./Card";

export interface ErrorDisplayProps {
  title?: string;
  message: string;
  onRetry?: () => void;
  retryText?: string;
  className?: string;
}

const ErrorDisplay: React.FC<ErrorDisplayProps> = ({
  title = "Something went wrong",
  message,
  onRetry,
  retryText = "Try again",
  className = "",
}) => {
  return (
    <Card variant="default" padding="lg" className={`text-center ${className}`}>
      <div className="flex flex-col items-center space-y-4">
        <div className="flex items-center justify-center w-16 h-16 bg-red-100 rounded-full">
          <AlertCircle className="w-8 h-8 text-red-600" />
        </div>

        <div className="space-y-2">
          <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
          <p className="text-gray-600 max-w-md">{message}</p>
        </div>

        {onRetry && (
          <Button
            variant="primary"
            onClick={onRetry}
            leftIcon={<RefreshCw size={16} />}
          >
            {retryText}
          </Button>
        )}
      </div>
    </Card>
  );
};

export { ErrorDisplay };
