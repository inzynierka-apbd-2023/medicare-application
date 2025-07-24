import { ReactNode } from "react";
import { Card } from "../../../../../shared/components";

interface DashboardCardProps {
  title: string;
  children: ReactNode;
  action?: {
    label: string;
    onClick: () => void;
    variant?: "primary" | "secondary" | "outline";
  };
  className?: string;
  contentClassName?: string;
  titleClassName?: string;
}

export function DashboardCard({
  title,
  children,
  action,
  className = "",
  contentClassName = "",
  titleClassName = "",
}: DashboardCardProps) {
  // Use medical variant to match original styling (bg-white rounded-2xl shadow-md)
  return (
    <Card
      variant="medical"
      padding="md"
      className={`w-full flex flex-col items-center ${className}`}
    >
      <h2
        className={`text-xl font-semibold text-blue-600 mb-4 text-center ${titleClassName}`}
      >
        {title}
      </h2>
      <div className={`flex flex-col items-center w-full ${contentClassName}`}>
        {children}
        {action && (
          <button
            onClick={action.onClick}
            className="mt-4 w-full px-4 py-2 bg-blue-100 text-blue-700 rounded-lg hover:bg-blue-200 transition duration-150"
          >
            {action.label}
          </button>
        )}
      </div>
    </Card>
  );
}

export type { DashboardCardProps };
