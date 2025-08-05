import React from "react";

export interface InfoCardProps {
  title?: string;
  children: React.ReactNode;
  variant?: "default" | "highlighted" | "bordered";
  className?: string;
}

export const InfoCard: React.FC<InfoCardProps> = ({
  title,
  children,
  variant = "default",
  className = "",
}) => {
  const baseClasses = "rounded-lg p-4";
  const variantClasses = {
    default: "bg-gray-50 border border-gray-200",
    highlighted: "bg-blue-50 border border-blue-200",
    bordered: "bg-white border border-gray-200",
  };

  return (
    <div className={`${baseClasses} ${variantClasses[variant]} ${className}`}>
      {title && (
        <h3 className="text-lg font-medium text-gray-900 mb-4">{title}</h3>
      )}
      {children}
    </div>
  );
};
