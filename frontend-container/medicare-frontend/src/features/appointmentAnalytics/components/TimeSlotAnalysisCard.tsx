import React, { useState } from "react";
import { Calendar, Clock, TrendingUp } from "lucide-react";
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

interface TimeSlotData {
  hour: number;
  timeSlot: string;
  monday: number;
  tuesday: number;
  wednesday: number;
  thursday: number;
  friday: number;
  saturday: number;
  sunday: number;
  totalAppointments: number;
  averageRevenue: number;
  completionRate: number;
}

interface DayData {
  day: string;
  totalAppointments: number;
  peakHour: string;
  revenue: number;
  utilizationRate: number;
}

interface TimeSlotAnalysisCardProps {
  data: TimeSlotData[];
  weeklyData: DayData[];
}

const TimeSlotAnalysisCard: React.FC<TimeSlotAnalysisCardProps> = ({
  data,
  weeklyData,
}) => {
  const [viewMode, setViewMode] = useState<"heatmap" | "trends">("heatmap");

  // Days of the week
  const daysOfWeek = [
    "monday",
    "tuesday",
    "wednesday",
    "thursday",
    "friday",
    "saturday",
    "sunday",
  ];
  const dayLabels = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

  // Calculate peak hours and utilization
  const peakTimeSlot = data.reduce((max, slot) =>
    slot.totalAppointments > max.totalAppointments ? slot : max
  );

  const totalWeeklyAppointments = weeklyData.reduce(
    (sum, day) => sum + day.totalAppointments,
    0
  );
  const averageUtilization =
    weeklyData.reduce((sum, day) => sum + day.utilizationRate, 0) /
    weeklyData.length;

  // Prepare data for trends chart
  const trendsData = data.map((slot) => ({
    time: slot.timeSlot,
    appointments: slot.totalAppointments,
    revenue: slot.averageRevenue,
    completion: slot.completionRate,
  }));

  // Get intensity for heatmap cell (0-1 scale)
  const getIntensity = (value: number, maxValue: number) => {
    return maxValue > 0 ? value / maxValue : 0;
  };

  // Get color intensity for heatmap
  const getHeatmapColor = (intensity: number) => {
    const alpha = Math.max(0.1, intensity);
    return `rgba(59, 130, 246, ${alpha})`;
  };

  // Find max appointments for normalization
  const maxAppointments = Math.max(
    ...data.flatMap((slot) =>
      daysOfWeek.map((day) => slot[day as keyof TimeSlotData] as number)
    )
  );

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
      minimumFractionDigits: 0,
    }).format(value);
  };

  return (
    <div className="space-y-6">
      {/* Summary Statistics */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card variant="elevated" className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">
                Peak Time Slot
              </p>
              <p className="text-2xl font-bold text-gray-900">
                {peakTimeSlot.timeSlot}
              </p>
              <p className="text-sm text-gray-500">
                {peakTimeSlot.totalAppointments} appointments
              </p>
            </div>
            <Clock className="w-8 h-8 text-blue-500" />
          </div>
        </Card>

        <Card variant="elevated" className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">
                Weekly Appointments
              </p>
              <p className="text-2xl font-bold text-gray-900">
                {totalWeeklyAppointments}
              </p>
            </div>
            <Calendar className="w-8 h-8 text-green-500" />
          </div>
        </Card>

        <Card variant="elevated" className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">
                Avg Utilization
              </p>
              <p className="text-2xl font-bold text-gray-900">
                {averageUtilization.toFixed(1)}%
              </p>
            </div>
            <TrendingUp className="w-8 h-8 text-yellow-500" />
          </div>
        </Card>

        <Card variant="elevated" className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">Best Day</p>
              <p className="text-2xl font-bold text-gray-900">
                {
                  weeklyData.reduce((max, day) =>
                    day.totalAppointments > max.totalAppointments ? day : max
                  ).day
                }
              </p>
            </div>
            <Calendar className="w-8 h-8 text-purple-500" />
          </div>
        </Card>
      </div>

      {/* View Toggle */}
      <Card variant="elevated" className="p-6">
        <div className="flex items-center justify-between mb-6">
          <h3 className="text-lg font-semibold text-gray-900">
            Time Slot Analysis
          </h3>
          <div className="flex gap-2">
            <Button
              variant={viewMode === "heatmap" ? "primary" : "outline"}
              onClick={() => setViewMode("heatmap")}
              className="px-4 py-2"
            >
              Heatmap View
            </Button>
            <Button
              variant={viewMode === "trends" ? "primary" : "outline"}
              onClick={() => setViewMode("trends")}
              className="px-4 py-2"
            >
              Trends View
            </Button>
          </div>
        </div>

        {viewMode === "heatmap" ? (
          <div className="space-y-4">
            {/* Heatmap */}
            <div className="overflow-x-auto">
              <div className="min-w-max">
                {/* Header */}
                <div className="grid grid-cols-8 gap-1 mb-2">
                  <div className="p-2 text-center text-sm font-medium text-gray-600">
                    Time
                  </div>
                  {dayLabels.map((day) => (
                    <div
                      key={day}
                      className="p-2 text-center text-sm font-medium text-gray-600"
                    >
                      {day}
                    </div>
                  ))}
                </div>

                {/* Heatmap Grid */}
                {data.map((slot) => (
                  <div key={slot.hour} className="grid grid-cols-8 gap-1 mb-1">
                    <div className="p-3 text-center text-sm font-medium text-gray-700 bg-gray-50 rounded">
                      {slot.timeSlot}
                    </div>
                    {daysOfWeek.map((day, dayIndex) => {
                      const value = slot[day as keyof TimeSlotData] as number;
                      const intensity = getIntensity(value, maxAppointments);
                      return (
                        <div
                          key={`${slot.hour}-${day}`}
                          className="p-3 text-center text-sm font-medium rounded cursor-pointer hover:opacity-80 transition-opacity"
                          style={{
                            backgroundColor: getHeatmapColor(intensity),
                          }}
                          title={`${dayLabels[dayIndex]} ${slot.timeSlot}: ${value} appointments`}
                        >
                          <span
                            className={
                              intensity > 0.5 ? "text-white" : "text-gray-900"
                            }
                          >
                            {value}
                          </span>
                        </div>
                      );
                    })}
                  </div>
                ))}
              </div>
            </div>

            {/* Legend */}
            <div className="flex items-center gap-4 text-sm text-gray-600">
              <span>Low</span>
              <div className="flex gap-1">
                {[0.1, 0.3, 0.5, 0.7, 0.9].map((intensity, index) => (
                  <div
                    key={index}
                    className="w-4 h-4 rounded"
                    style={{ backgroundColor: getHeatmapColor(intensity) }}
                  />
                ))}
              </div>
              <span>High</span>
            </div>
          </div>
        ) : (
          <div className="h-80">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart
                data={trendsData}
                margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
              >
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis
                  dataKey="time"
                  stroke="#6b7280"
                  fontSize={12}
                  angle={-45}
                  textAnchor="end"
                  height={60}
                />
                <YAxis stroke="#6b7280" fontSize={12} />
                <Tooltip
                  formatter={(value: number, name: string) => [
                    name === "revenue"
                      ? formatCurrency(value)
                      : name === "completion"
                        ? `${value.toFixed(1)}%`
                        : value,
                    name === "appointments"
                      ? "Appointments"
                      : name === "revenue"
                        ? "Avg Revenue"
                        : "Completion Rate",
                  ]}
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
                  strokeWidth={2}
                  name="Appointments"
                />
                <Line
                  type="monotone"
                  dataKey="completion"
                  stroke="#10b981"
                  strokeWidth={2}
                  name="Completion Rate (%)"
                />
              </LineChart>
            </ResponsiveContainer>
          </div>
        )}
      </Card>

      {/* Weekly Summary */}
      <Card variant="elevated" className="p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-6">
          Weekly Summary
        </h3>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* Daily Appointments Chart */}
          <div>
            <h4 className="text-md font-medium text-gray-700 mb-4">
              Daily Appointments
            </h4>
            <div className="h-64">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart
                  data={weeklyData}
                  margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis dataKey="day" stroke="#6b7280" fontSize={12} />
                  <YAxis stroke="#6b7280" fontSize={12} />
                  <Tooltip
                    formatter={(value: number) => [value, "Appointments"]}
                    contentStyle={{
                      backgroundColor: "#fff",
                      border: "1px solid #e5e7eb",
                      borderRadius: "8px",
                      boxShadow: "0 4px 6px -1px rgba(0, 0, 0, 0.1)",
                    }}
                  />
                  <Bar dataKey="totalAppointments" fill="#3b82f6" />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>

          {/* Weekly Statistics Table */}
          <div>
            <h4 className="text-md font-medium text-gray-700 mb-4">
              Daily Statistics
            </h4>
            <div className="overflow-hidden">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">
                      Day
                    </th>
                    <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">
                      Appointments
                    </th>
                    <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">
                      Peak Hour
                    </th>
                    <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">
                      Revenue
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {weeklyData.map((day) => (
                    <tr key={day.day} className="hover:bg-gray-50">
                      <td className="px-4 py-2 text-sm font-medium text-gray-900">
                        {day.day}
                      </td>
                      <td className="px-4 py-2 text-sm text-gray-900">
                        {day.totalAppointments}
                      </td>
                      <td className="px-4 py-2 text-sm text-gray-900">
                        {day.peakHour}
                      </td>
                      <td className="px-4 py-2 text-sm text-gray-900">
                        {formatCurrency(day.revenue)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </Card>
    </div>
  );
};

export default TimeSlotAnalysisCard;
