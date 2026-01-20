import React from "react";
import { Star, UserCheck } from "lucide-react";
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

interface DoctorPerformance {
  id: string;
  name: string;
  specialization: string;
  totalAppointments: number;
  completedAppointments: number;
  cancelledAppointments: number;
  noShowAppointments: number;
  averageRating: number;
  totalRatings: number;
  revenue: number;
  utilizationRate: number;
}

interface DoctorPerformanceCardProps {
  data: DoctorPerformance[];
}

const DoctorPerformanceCard: React.FC<DoctorPerformanceCardProps> = ({
  data,
}) => {
  // Colors for the pie chart
  const COLORS = ["#10b981", "#f59e0b", "#ef4444", "#6b7280"];

  // Safely handle undefined or empty data
  const safeData = (data ?? []).filter((doc) => doc && doc.name);

  // Return empty state if no data
  if (safeData.length === 0) {
    return (
      <div className="p-8 text-center text-gray-500">
        <p className="text-lg font-medium">
          No doctor performance data available
        </p>
        <p className="text-sm mt-2">
          Data will appear once there are appointments in the system.
        </p>
      </div>
    );
  }

  // Calculate completion rates for bar chart
  const chartData = safeData.map((doctor) => ({
    name: (doctor.name || "Unknown").split(" ").pop() || "Doc",
    completed: Number(doctor.completedAppointments || 0),
    cancelled: Number(doctor.cancelledAppointments || 0),
    noShow: Number(doctor.noShowAppointments || 0),
    total: Number(doctor.totalAppointments || 0),
    rating: Number(doctor.averageRating || 0),
    revenue: Number(doctor.revenue || 0),
  }));

  // Top performers by different metrics
  const topByAppointments = [...safeData].sort(
    (a, b) => b.totalAppointments - a.totalAppointments
  )[0];
  const topByRating = [...safeData].sort(
    (a, b) => b.averageRating - a.averageRating
  )[0];
  const topByRevenue = [...safeData].sort((a, b) => b.revenue - a.revenue)[0];

  // Aggregate data for pie chart
  const totalCompleted = safeData.reduce(
    (sum, doc) => sum + (doc.completedAppointments || 0),
    0
  );
  const totalCancelled = safeData.reduce(
    (sum, doc) => sum + (doc.cancelledAppointments || 0),
    0
  );
  const totalNoShow = safeData.reduce(
    (sum, doc) => sum + (doc.noShowAppointments || 0),
    0
  );

  const pieData = [
    { name: "Completed", value: totalCompleted, color: COLORS[0] },
    { name: "Cancelled", value: totalCancelled, color: COLORS[1] },
    { name: "No Show", value: totalNoShow, color: COLORS[2] },
  ];

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
      minimumFractionDigits: 0,
    }).format(value);
  };

  return (
    <div className="space-y-6">
      {/* Top Performers */}
      <Card variant="elevated" className="p-6">
        <div className="flex items-center gap-3 mb-6">
          <Star className="w-6 h-6 text-yellow-500" />
          <h3 className="text-lg font-semibold text-gray-900">
            Top Performers
          </h3>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="text-center p-4 bg-blue-50 rounded-lg">
            <h4 className="text-sm font-medium text-blue-600 mb-2">
              Most Appointments
            </h4>
            <p className="text-lg font-bold text-gray-900">
              {topByAppointments?.name}
            </p>
            <p className="text-sm text-gray-600">
              {topByAppointments?.totalAppointments} appointments
            </p>
          </div>

          <div className="text-center p-4 bg-yellow-50 rounded-lg">
            <h4 className="text-sm font-medium text-yellow-600 mb-2">
              Highest Rated
            </h4>
            <p className="text-lg font-bold text-gray-900">
              {topByRating?.name}
            </p>
            <p className="text-sm text-gray-600">
              {topByRating?.averageRating.toFixed(1)}/5.0 rating
            </p>
          </div>

          <div className="text-center p-4 bg-green-50 rounded-lg">
            <h4 className="text-sm font-medium text-green-600 mb-2">
              Highest Revenue
            </h4>
            <p className="text-lg font-bold text-gray-900">
              {topByRevenue?.name}
            </p>
            <p className="text-sm text-gray-600">
              {formatCurrency(topByRevenue?.revenue || 0)}
            </p>
          </div>
        </div>
      </Card>

      {/* Performance Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Doctor Performance Bar Chart */}
        <Card variant="elevated" className="p-6">
          <div className="flex items-center gap-3 mb-6">
            <UserCheck className="w-6 h-6 text-blue-500" />
            <h3 className="text-lg font-semibold text-gray-900">
              Doctor Performance
            </h3>
          </div>

          <div className="h-80">
            <ResponsiveContainer
              width="100%"
              height="100%"
              key={chartData.length}
            >
              <BarChart
                data={chartData}
                margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
              >
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis
                  dataKey="name"
                  type="category"
                  stroke="#6b7280"
                  fontSize={12}
                  angle={-45}
                  textAnchor="end"
                  height={60}
                  interval={0}
                />
                <YAxis type="number" stroke="#6b7280" fontSize={12} />
                <Tooltip
                  cursor={{ fill: "rgba(0, 0, 0, 0.05)" }}
                  contentStyle={{
                    backgroundColor: "#fff",
                    border: "1px solid #e5e7eb",
                    borderRadius: "8px",
                    boxShadow: "0 4px 6px -1px rgba(0, 0, 0, 0.1)",
                  }}
                />
                <Legend />
                <Bar
                  dataKey="completed"
                  fill="#10b981"
                  name="Completed"
                  radius={[4, 4, 0, 0]}
                />
                <Bar
                  dataKey="cancelled"
                  fill="#f59e0b"
                  name="Cancelled"
                  radius={[4, 4, 0, 0]}
                />
                <Bar
                  dataKey="noShow"
                  fill="#ef4444"
                  name="No Show"
                  radius={[4, 4, 0, 0]}
                />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Card>

        {/* Overall Status Distribution */}
        <Card variant="elevated" className="p-6">
          <div className="flex items-center gap-3 mb-6">
            <UserCheck className="w-6 h-6 text-purple-500" />
            <h3 className="text-lg font-semibold text-gray-900">
              Appointment Status Distribution
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
                  formatter={(value: number | undefined) => [
                    (value || 0).toLocaleString(),
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

          {/* Statistics */}
          <div className="mt-4 grid grid-cols-3 gap-4 text-center">
            <div>
              <p className="text-2xl font-bold text-green-600">
                {totalCompleted}
              </p>
              <p className="text-sm text-gray-600">Completed</p>
            </div>
            <div>
              <p className="text-2xl font-bold text-yellow-600">
                {totalCancelled}
              </p>
              <p className="text-sm text-gray-600">Cancelled</p>
            </div>
            <div>
              <p className="text-2xl font-bold text-red-600">{totalNoShow}</p>
              <p className="text-sm text-gray-600">No Show</p>
            </div>
          </div>
        </Card>
      </div>

      {/* Detailed Doctor Table */}
      <Card variant="elevated" className="p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-6">
          Detailed Doctor Performance
        </h3>

        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Doctor
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Specialization
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Appointments
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Completion Rate
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Rating
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Revenue
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {safeData.map((doctor) => (
                <tr key={doctor.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div>
                      <div className="text-sm font-medium text-gray-900">
                        {doctor.name}
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                      {doctor.specialization}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {doctor.totalAppointments}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      <div className="flex-1 bg-gray-200 rounded-full h-2 mr-2">
                        <div
                          className="bg-green-500 h-2 rounded-full"
                          style={{
                            width: `${(doctor.completedAppointments / doctor.totalAppointments) * 100}%`,
                          }}
                        />
                      </div>
                      <span className="text-sm text-gray-900">
                        {(
                          (doctor.completedAppointments /
                            doctor.totalAppointments) *
                          100
                        ).toFixed(1)}
                        %
                      </span>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      <Star className="w-4 h-4 text-yellow-400 fill-current mr-1" />
                      <span className="text-sm text-gray-900">
                        {doctor.averageRating.toFixed(1)} ({doctor.totalRatings}
                        )
                      </span>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {formatCurrency(doctor.revenue)}
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

export default DoctorPerformanceCard;
