import { ReactNode } from "react";

import { Card } from "../../../../../shared/components";

interface ScheduleCardProps {
  title: string;
  children: ReactNode;
  className?: string;
  height?: string;
}

export function ScheduleCard({
  title,
  children,
  className = "",
  height = "h-[600px]",
}: ScheduleCardProps) {
  return (
    <Card
      variant="medical"
      padding="md"
      className={`${height} flex flex-col ${className}`}
    >
      <h2 className="text-xl font-semibold text-blue-600 mb-4">{title}</h2>
      <div className="flex-1 bg-blue-50 rounded-lg p-4 h-full">{children}</div>
    </Card>
  );
}

export type { ScheduleCardProps };
