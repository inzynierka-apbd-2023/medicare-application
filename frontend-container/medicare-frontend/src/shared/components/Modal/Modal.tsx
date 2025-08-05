import React from "react";
import { X } from "lucide-react";

export interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  children: React.ReactNode;
  size?: "sm" | "md" | "lg" | "xl";
  showCloseButton?: boolean;
  closeOnOverlayClick?: boolean;
  className?: string;
}

const Modal: React.FC<ModalProps> = ({
  isOpen,
  onClose,
  title,
  children,
  size = "md",
  showCloseButton = true,
  closeOnOverlayClick = true,
  className = "",
}) => {
  if (!isOpen) return null;

  const sizeClasses = {
    sm: "max-w-sm max-h-[80vh]",
    md: "max-w-md max-h-[80vh]",
    lg: "max-w-2xl max-h-[90vh]",
    xl: "max-w-4xl max-h-[90vh]",
  };

  const handleOverlayClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (closeOnOverlayClick && e.target === e.currentTarget) {
      onClose();
    }
  };

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black bg-opacity-40 z-40 transition-opacity"
        onClick={handleOverlayClick}
      />

      {/* Modal */}
      <div className="fixed inset-0 flex items-center justify-center z-50 p-4">
        <div
          className={`bg-white rounded-2xl shadow-lg w-full ${sizeClasses[size]} relative ${className} flex flex-col`}
        >
          {/* Header */}
          {(title || showCloseButton) && (
            <div className="flex items-center justify-between p-6 pb-4 flex-shrink-0">
              {title && (
                <h2 className="text-2xl font-semibold text-blue-600 absolute left-1/2 transform -translate-x-1/2">
                  {title}
                </h2>
              )}
              {showCloseButton && (
                <button
                  className="text-blue-300 hover:text-blue-500 transition p-1 ml-auto"
                  onClick={onClose}
                  aria-label="Close modal"
                >
                  <X size={24} />
                </button>
              )}
            </div>
          )}

          {/* Content */}
          <div
            className={`flex-1 overflow-y-auto ${title || showCloseButton ? "px-6 pb-6" : "p-6"}`}
          >
            {children}
          </div>
        </div>
      </div>
    </>
  );
};

export { Modal };
