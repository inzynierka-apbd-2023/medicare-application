import React, { useState } from "react";
import { TrendingUp } from "lucide-react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

import { Button, Card } from "../../../shared/components";

export interface TrendData {
  date: string;
  appointments: number;
  completed: number;
  cancelled: number;
  noShow: number;
  revenue: number;
}

interface AppointmentTrendsCardProps {
  data: TrendData[];
  title?: string;
}

const AppointmentTrendsCard: React.FC<AppointmentTrendsCardProps> = ({
  data,
  title = "Appointment Trends",
}) => {
  const [viewType, setViewType] = useState<"appointments" | "revenue">(
    "appointments"
  );

  const formatTooltipValue = (value: number, name: string) => {
    if (name === "revenue") {
      return [
        new Intl.NumberFormat("pl-PL", {
          style: "currency",
          currency: "PLN",
        }).format(value),
        "Revenue",
      ];
    }
    return [value, name];
  };

  return (
    <Card variant="elevated" className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-3">
          <TrendingUp className="w-6 h-6 text-blue-500" />
          <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
        </div>
        <div className="flex gap-2">
          <Button
            variant={viewType === "appointments" ? "primary" : "outline"}
            size="sm"
            onClick={() => setViewType("appointments")}
          >
            Appointments
          </Button>
          <Button
            variant={viewType === "revenue" ? "primary" : "outline"}
            size="sm"
            onClick={() => setViewType("revenue")}
          >
            Revenue
          </Button>
        </div>
      </div>

      <div className="h-80">
        <ResponsiveContainer width="100%" height="100%">
          {viewType === "appointments" ? (
            <LineChart data={data}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis dataKey="date" stroke="#6b7280" fontSize={12} />
              <YAxis stroke="#6b7280" fontSize={12} />
              <Tooltip
                contentStyle={{
                  backgroundColor: "#fff",
                  border: "1px solid #e5e7eb",
                  borderRadius: "8px",
                  boxShadow: "0 4px 6px -1px rgba(0, 0, 0, 0.1)",
                }}
              />
              <Legend />
              <Line
                type="monotone"
                dataKey="appointments"
                stroke="#3b82f6"
                strokeWidth={3}
                dot={{ fill: "#3b82f6", strokeWidth: 2, r: 4 }}
                name="Total Appointments"
              />
              <Line
                type="monotone"
                dataKey="completed"
                stroke="#10b981"
                strokeWidth={2}
                dot={{ fill: "#10b981", strokeWidth: 2, r: 3 }}
                name="Completed"
              />
              <Line
                type="monotone"
                dataKey="cancelled"
                stroke="#f59e0b"
                strokeWidth={2}
                dot={{ fill: "#f59e0b", strokeWidth: 2, r: 3 }}
                name="Cancelled"
              />
              <Line
                type="monotone"
                dataKey="noShow"
                stroke="#ef4444"
                strokeWidth={2}
                dot={{ fill: "#ef4444", strokeWidth: 2, r: 3 }}
                name="No Show"
              />
            </LineChart>
          ) : (
            <BarChart data={data}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis dataKey="date" stroke="#6b7280" fontSize={12} />
              <YAxis stroke="#6b7280" fontSize={12} />
              <Tooltip
                formatter={formatTooltipValue}
                contentStyle={{
                  backgroundColor: "#fff",
                  border: "1px solid #e5e7eb",
                  borderRadius: "8px",
                  boxShadow: "0 4px 6px -1px rgba(0, 0, 0, 0.1)",
                }}
              />
              <Legend />
              <Bar
                dataKey="revenue"
                fill="#8b5cf6"
                name="Revenue"
                radius={[4, 4, 0, 0]}
              />
            </BarChart>
          )}
        </ResponsiveContainer>
      </div>

      {/* Summary Stats */}
      <div className="mt-6 pt-4 border-t border-gray-200">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div className="text-center">
            <p className="text-sm text-gray-600">Total Appointments</p>
            <p className="text-lg font-semibold text-gray-900">
              {data
                .reduce((sum, item) => sum + item.appointments, 0)
                .toLocaleString()}
            </p>
          </div>
          <div className="text-center">
            <p className="text-sm text-gray-600">Completion Rate</p>
            <p className="text-lg font-semibold text-green-600">
              {(
                (data.reduce((sum, item) => sum + item.completed, 0) /
                  data.reduce((sum, item) => sum + item.appointments, 0)) *
                100
              ).toFixed(1)}
              %
            </p>
          </div>
          <div className="text-center">
            <p className="text-sm text-gray-600">No-Show Rate</p>
            <p className="text-lg font-semibold text-red-600">
              {(
                (data.reduce((sum, item) => sum + item.noShow, 0) /
                  data.reduce((sum, item) => sum + item.appointments, 0)) *
                100
              ).toFixed(1)}
              %
            </p>
          </div>
          <div className="text-center">
            <p className="text-sm text-gray-600">Total Revenue</p>
            <p className="text-lg font-semibold text-purple-600">
              {new Intl.NumberFormat("pl-PL", {
                style: "currency",
                currency: "PLN",
                minimumFractionDigits: 0,
              }).format(data.reduce((sum, item) => sum + item.revenue, 0))}
            </p>
          </div>
        </div>
      </div>
    </Card>
  );
};

export default AppointmentTrendsCard;
