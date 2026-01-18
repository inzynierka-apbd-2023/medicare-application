import React from "react";
import { Activity, TrendingUp, Users } from "lucide-react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

import { Card } from "../../../shared/components";
import { SpecializationStats } from "../../../shared/services/analyticsApi";

interface SpecializationStatsCardProps {
  data: SpecializationStats[];
}

const SpecializationStatsCard: React.FC<SpecializationStatsCardProps> = ({
  data,
}) => {
  // Colors for charts
  const COLORS = [
    "#3b82f6",
    "#10b981",
    "#f59e0b",
    "#ef4444",
    "#8b5cf6",
    "#06b6d4",
    "#84cc16",
    "#f97316",
    "#ec4899",
    "#6b7280",
  ];

  // Prepare data for pie chart (appointments by specialization)
  const pieData = data.map((spec, index) => ({
    name: spec.specialization,
    value: spec.totalAppointments,
    color: COLORS[index % COLORS.length],
  }));

  // Prepare data for revenue bar chart
  const revenueData = data
    .sort((a, b) => b.revenue - a.revenue)
    .map((spec) => ({
      name: spec.specialization,
      revenue: spec.revenue,
      appointments: spec.totalAppointments,
      completion: spec.completionRate,
    }));

  // Calculate totals
  const totalAppointments = data.reduce(
    (sum, spec) => sum + spec.totalAppointments,
    0
  );
  const totalPatients = data.reduce((sum, spec) => sum + spec.totalPatients, 0);
  const totalRevenue = data.reduce((sum, spec) => sum + spec.revenue, 0);
  const averageCompletion =
    data.reduce((sum, spec) => sum + spec.completionRate, 0) / data.length;

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
      minimumFractionDigits: 0,
    }).format(value);
  };

  const formatNumber = (value: number) => {
    return new Intl.NumberFormat("en-US").format(value);
  };

  return (
    <div className="space-y-6">
      {/* Summary Statistics */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card variant="elevated" className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">
                Total Appointments
              </p>
              <p className="text-2xl font-bold text-gray-900">
                {formatNumber(totalAppointments)}
              </p>
            </div>
            <Activity className="w-8 h-8 text-blue-500" />
          </div>
        </Card>

        <Card variant="elevated" className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">
                Total Patients
              </p>
              <p className="text-2xl font-bold text-gray-900">
                {formatNumber(totalPatients)}
              </p>
            </div>
            <Users className="w-8 h-8 text-green-500" />
          </div>
        </Card>

        <Card variant="elevated" className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">Total Revenue</p>
              <p className="text-2xl font-bold text-gray-900">
                {formatCurrency(totalRevenue)}
              </p>
            </div>
            <TrendingUp className="w-8 h-8 text-yellow-500" />
          </div>
        </Card>

        <Card variant="elevated" className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">
                Avg Completion
              </p>
              <p className="text-2xl font-bold text-gray-900">
                {averageCompletion.toFixed(1)}%
              </p>
            </div>
            <Activity className="w-8 h-8 text-purple-500" />
          </div>
        </Card>
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Appointments Distribution Pie Chart */}
        <Card variant="elevated" className="p-6">
          <div className="flex items-center gap-3 mb-6">
            <Activity className="w-6 h-6 text-blue-500" />
            <h3 className="text-lg font-semibold text-gray-900">
              Appointments by Specialization
            </h3>
          </div>

          <div className="h-80">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={pieData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={({ name, percent }) =>
                    `${name} ${((percent || 0) * 100).toFixed(0)}%`
                  }
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="value"
                >
                  {pieData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip
                  formatter={(value: number) => [
                    formatNumber(value),
                    "Appointments",
                  ]}
                  contentStyle={{
                    backgroundColor: "#fff",
                    border: "1px solid #e5e7eb",
                    borderRadius: "8px",
                    boxShadow: "0 4px 6px -1px rgba(0, 0, 0, 0.1)",
                  }}
                />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </Card>

        {/* Revenue by Specialization Bar Chart */}
        <Card variant="elevated" className="p-6">
          <div className="flex items-center gap-3 mb-6">
            <TrendingUp className="w-6 h-6 text-green-500" />
            <h3 className="text-lg font-semibold text-gray-900">
              Revenue by Specialization
            </h3>
          </div>

          <div className="h-80">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart
                data={revenueData}
                margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
              >
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis
                  dataKey="name"
                  stroke="#6b7280"
                  fontSize={12}
                  angle={-45}
                  textAnchor="end"
                  height={80}
                />
                <YAxis stroke="#6b7280" fontSize={12} />
                <Tooltip
                  formatter={(value: number, name: string) => [
                    name === "revenue"
                      ? formatCurrency(value)
                      : formatNumber(value),
                    name === "revenue" ? "Revenue" : "Appointments",
                  ]}
                  contentStyle={{
                    backgroundColor: "#fff",
                    border: "1px solid #e5e7eb",
                    borderRadius: "8px",
                    boxShadow: "0 4px 6px -1px rgba(0, 0, 0, 0.1)",
                  }}
                />
                <Legend />
                <Bar dataKey="revenue" fill="#10b981" name="Revenue" />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Card>
      </div>

      {/* Detailed Specialization Table */}
      <Card variant="elevated" className="p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-6">
          Detailed Specialization Statistics
        </h3>

        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Specialization
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Appointments
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Patients
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Doctors
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Avg Duration
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Completion Rate
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Avg Rating
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Revenue
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {data
                .sort((a, b) => b.totalAppointments - a.totalAppointments)
                .map((spec, index) => (
                  <tr key={spec.specialization} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <div
                          className="w-3 h-3 rounded-full mr-3"
                          style={{
                            backgroundColor: COLORS[index % COLORS.length],
                          }}
                        />
                        <span className="text-sm font-medium text-gray-900">
                          {spec.specialization}
                        </span>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {formatNumber(spec.totalAppointments)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {formatNumber(spec.totalPatients)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {spec.totalDoctors}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {spec.averageAppointmentDuration} min
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <div className="flex-1 bg-gray-200 rounded-full h-2 mr-2">
                          <div
                            className="bg-green-500 h-2 rounded-full"
                            style={{ width: `${spec.completionRate}%` }}
                          />
                        </div>
                        <span className="text-sm text-gray-900">
                          {spec.completionRate.toFixed(1)}%
                        </span>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <span className="text-sm text-gray-900">
                          {spec.averageRating.toFixed(1)}/5.0
                        </span>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {formatCurrency(spec.revenue)}
                    </td>
                  </tr>
                ))}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
};

export default SpecializationStatsCard;
