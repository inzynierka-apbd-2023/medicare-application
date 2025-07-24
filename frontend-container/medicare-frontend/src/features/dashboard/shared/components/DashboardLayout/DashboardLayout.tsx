import { ReactNode } from "react";

interface DashboardLayoutProps {
  title: string;
  children: ReactNode;
  className?: string;
}

export function DashboardLayout({
  title,
  children,
  className = "",
}: DashboardLayoutProps) {
  return (
    <main className={`pt-24 px-8 pb-10 ${className}`}>
      <h1 className="text-3xl font-bold text-blue-700 mb-8">{title}</h1>
      {children}
    </main>
  );
}

export type { DashboardLayoutProps };
