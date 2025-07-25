import { AlertCircle, X } from "lucide-react";

interface ErrorDisplayProps {
  error: string | null;
  onClose?: () => void;
  className?: string;
  variant?: "inline" | "overlay" | "banner";
}

export function ErrorDisplay({
  error,
  onClose,
  className = "",
  variant = "inline",
}: ErrorDisplayProps) {
  if (!error) return null;

  const baseStyles = "flex items-center gap-3 text-red-600";
  
  const variantStyles = {
    inline: "p-4 bg-red-50 border border-red-200 rounded-lg",
    overlay: "fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50",
    banner: "w-full p-3 bg-red-100 border-b border-red-200",
  };

  const contentStyles = {
    inline: "",
    overlay: "bg-white p-6 rounded-lg shadow-lg max-w-md mx-4",
    banner: "",
  };

  if (variant === "overlay") {
    return (
      <div className={`${variantStyles.overlay} ${className}`}>
        <div className={contentStyles.overlay}>
          <div className={baseStyles}>
            <AlertCircle size={20} />
            <span className="flex-1">{error}</span>
            {onClose && (
              <button
                onClick={onClose}
                className="text-red-400 hover:text-red-600 transition-colors"
              >
                <X size={20} />
              </button>
            )}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className={`${variantStyles[variant]} ${baseStyles} ${className}`}>
      <AlertCircle size={20} />
      <span className="flex-1">{error}</span>
      {onClose && (
        <button
          onClick={onClose}
          className="text-red-400 hover:text-red-600 transition-colors"
        >
          <X size={16} />
        </button>
      )}
    </div>
  );
}

export type { ErrorDisplayProps };
