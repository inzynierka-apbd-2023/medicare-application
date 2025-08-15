import React from "react";
import { Calendar, Stethoscope, Users } from "lucide-react";

import { DashboardCard } from "../../shared/components";
import type { ReceptionistDashboardStats } from "../types";

interface StatsCardsProps {
  stats: ReceptionistDashboardStats;
  isLoading?: boolean;
}

export const StatsCards: React.FC<StatsCardsProps> = ({ stats, isLoading }) => {
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
        {[...Array(6)].map((_, index) => (
          <div
            key={index}
            className="bg-white rounded-2xl shadow-md p-6 animate-pulse"
          >
            <div className="h-4 bg-gray-200 rounded w-3/4 mb-4"></div>
            <div className="h-8 bg-gray-200 rounded w-1/2"></div>
          </div>
        ))}
      </div>
    );
  }

  const statItems = [
    {
      title: "Today's Appointments",
      value: stats.todayAppointments,
      total: stats.totalAppointments,
      icon: Calendar,
      color: "text-blue-600",
      bgColor: "bg-blue-50",
    },
    {
      title: "Total Doctors",
      value: stats.totalDoctors,
      icon: Users,
      color: "text-green-600",
      bgColor: "bg-green-50",
    },
    {
      title: "Available Doctors",
      value: stats.availableDoctors,
      total: stats.totalDoctors,
      icon: Stethoscope,
      color: "text-purple-600",
      bgColor: "bg-purple-50",
    },
  ];

  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8 justify-items-center">
      {statItems.map((item, index) => (
        <DashboardCard
          key={index}
          title={item.title}
          className="min-h-[140px] w-full max-w-sm"
          contentClassName="flex-1 flex flex-col items-center justify-center"
        >
          <div className={`p-3 rounded-full ${item.bgColor} mb-3`}>
            <item.icon className={`h-6 w-6 ${item.color}`} />
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold text-gray-900">
              {item.value}
              {item.total && (
                <span className="text-sm text-gray-500 font-normal">
                  /{item.total}
                </span>
              )}
            </div>
          </div>
        </DashboardCard>
      ))}
    </div>
  );
};
