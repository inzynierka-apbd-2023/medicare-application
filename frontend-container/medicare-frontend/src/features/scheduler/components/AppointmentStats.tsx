/**
 * AppointmentStats Component
 * Displays daily appointment statistics
 */

import React from "react";
import { Card } from "@shared/components";

interface AppointmentStatsProps {
  stats: {
    total: number;
    completed: number;
    pending: number;
    cancelled: number;
    noShow: number;
  };
  title?: string;
}

export const AppointmentStats: React.FC<AppointmentStatsProps> = ({
  stats,
  title = "Today's Overview",
}) => {
  return (
    <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mb-6">
      <Card className="p-4 text-center">
        <div className="text-2xl font-bold text-blue-600">{stats.total}</div>
        <div className="text-sm text-gray-600">
          Total {title.includes("Today") ? "Today" : ""}
        </div>
      </Card>

      <Card className="p-4 text-center">
        <div className="text-2xl font-bold text-green-600">
          {stats.completed}
        </div>
        <div className="text-sm text-gray-600">Completed</div>
      </Card>

      <Card className="p-4 text-center">
        <div className="text-2xl font-bold text-yellow-600">
          {stats.pending}
        </div>
        <div className="text-sm text-gray-600">Pending</div>
      </Card>

      <Card className="p-4 text-center">
        <div className="text-2xl font-bold text-red-600">{stats.cancelled}</div>
        <div className="text-sm text-gray-600">Cancelled</div>
      </Card>

      <Card className="p-4 text-center">
        <div className="text-2xl font-bold text-gray-600">{stats.noShow}</div>
        <div className="text-sm text-gray-600">No Show</div>
      </Card>
    </div>
  );
};

export default AppointmentStats;
