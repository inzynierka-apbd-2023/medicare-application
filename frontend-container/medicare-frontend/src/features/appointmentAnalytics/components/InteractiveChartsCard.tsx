import { useState } from "react";
import { Activity, TrendingUp, Users } from "lucide-react";

import type {
  AppointmentsBySpecializationData,
  DoctorProductivityData,
  RevenueChartData,
} from "../../../shared/services/dashboardApi";

interface InteractiveChartsCardProps {
  revenueData: RevenueChartData[];
  specializationData: AppointmentsBySpecializationData[];
  productivityData: DoctorProductivityData[];
  isLoading?: boolean;
}

type ChartType = "revenue" | "specialization" | "productivity";

const chartOptions = [
  {
    key: "revenue" as ChartType,
    label: "Revenue Over Time",
    icon: TrendingUp,
    description: "Daily revenue and appointment trends",
  },
  {
    key: "specialization" as ChartType,
    label: "Appointments by Specialization",
    icon: Users,
    description: "Distribution of appointments across medical specialties",
  },
  {
    key: "productivity" as ChartType,
    label: "Doctor Productivity",
    icon: Activity,
    description: "Doctor performance and completion rates",
  },
];

export default function InteractiveChartsCard({
  revenueData,
  specializationData,
  productivityData,
  isLoading = false,
}: InteractiveChartsCardProps) {
  const [activeChart, setActiveChart] = useState<ChartType>("revenue");

  if (isLoading) {
    return (
      <div className="bg-white rounded-2xl shadow-md p-6">
        <div className="animate-pulse">
          <div className="h-6 bg-gray-200 rounded w-48 mb-4"></div>
          <div className="h-96 bg-gray-100 rounded"></div>
        </div>
      </div>
    );
  }

  const renderRevenueChart = () => {
    const chartWidth = 600;
    const chartHeight = 300;
    const padding = { top: 20, right: 40, bottom: 60, left: 80 };

    const maxRevenue = Math.max(...revenueData.map((d) => d.revenue));
    const minRevenue = Math.min(...revenueData.map((d) => d.revenue));
    const maxAppointments = Math.max(...revenueData.map((d) => d.appointments));

    // Create points for revenue line
    const revenuePoints = revenueData.map((item, index) => {
      const x =
        padding.left +
        (index * (chartWidth - padding.left - padding.right)) /
          (revenueData.length - 1);
      const y =
        padding.top +
        (chartHeight - padding.top - padding.bottom) *
          (1 - (item.revenue - minRevenue) / (maxRevenue - minRevenue));
      return { x, y, value: item.revenue, date: item.date };
    });

    // Create points for appointments line
    const appointmentPoints = revenueData.map((item, index) => {
      const x =
        padding.left +
        (index * (chartWidth - padding.left - padding.right)) /
          (revenueData.length - 1);
      const y =
        padding.top +
        (chartHeight - padding.top - padding.bottom) *
          (1 - item.appointments / maxAppointments);
      return { x, y, value: item.appointments, date: item.date };
    });

    const revenuePathData = revenuePoints
      .map((point, index) => `${index === 0 ? "M" : "L"} ${point.x} ${point.y}`)
      .join(" ");

    const appointmentPathData = appointmentPoints
      .map((point, index) => `${index === 0 ? "M" : "L"} ${point.x} ${point.y}`)
      .join(" ");

    return (
      <div className="space-y-4">
        <div className="flex justify-center">
          <svg
            width={chartWidth}
            height={chartHeight + 40}
            className="border border-gray-200 rounded"
          >
            {/* Grid lines */}
            {[0, 1, 2, 3, 4].map((i) => (
              <g key={i}>
                <line
                  x1={padding.left}
                  y1={
                    padding.top +
                    (i * (chartHeight - padding.top - padding.bottom)) / 4
                  }
                  x2={chartWidth - padding.right}
                  y2={
                    padding.top +
                    (i * (chartHeight - padding.top - padding.bottom)) / 4
                  }
                  stroke="#f3f4f6"
                  strokeWidth="1"
                />
              </g>
            ))}

            {/* Y-axis (Revenue) */}
            <line
              x1={padding.left}
              y1={padding.top}
              x2={padding.left}
              y2={chartHeight - padding.bottom}
              stroke="#374151"
              strokeWidth="2"
            />

            {/* X-axis */}
            <line
              x1={padding.left}
              y1={chartHeight - padding.bottom}
              x2={chartWidth - padding.right}
              y2={chartHeight - padding.bottom}
              stroke="#374151"
              strokeWidth="2"
            />

            {/* Y-axis labels (Revenue) */}
            {[0, 1, 2, 3, 4].map((i) => {
              const value =
                minRevenue + (maxRevenue - minRevenue) * (1 - i / 4);
              const y =
                padding.top +
                (i * (chartHeight - padding.top - padding.bottom)) / 4;
              return (
                <text
                  key={i}
                  x={padding.left - 10}
                  y={y + 5}
                  textAnchor="end"
                  fontSize="12"
                  fill="#6b7280"
                >
                  €{(value / 1000).toFixed(0)}k
                </text>
              );
            })}

            {/* Y-axis label */}
            <text
              x={20}
              y={chartHeight / 2}
              textAnchor="middle"
              fontSize="12"
              fill="#374151"
              transform={`rotate(-90, 20, ${chartHeight / 2})`}
            >
              Revenue (EUR)
            </text>

            {/* X-axis labels */}
            {revenuePoints.map((point, index) => (
              <text
                key={index}
                x={point.x}
                y={chartHeight - padding.bottom + 20}
                textAnchor="middle"
                fontSize="10"
                fill="#6b7280"
              >
                {new Date(point.date).toLocaleDateString("en-US", {
                  month: "short",
                  day: "numeric",
                })}
              </text>
            ))}

            {/* X-axis label */}
            <text
              x={chartWidth / 2}
              y={chartHeight + 35}
              textAnchor="middle"
              fontSize="12"
              fill="#374151"
            >
              Date
            </text>

            {/* Revenue line */}
            <path
              d={revenuePathData}
              stroke="#3b82f6"
              strokeWidth="3"
              fill="none"
            />

            {/* Revenue points */}
            {revenuePoints.map((point, index) => (
              <circle
                key={index}
                cx={point.x}
                cy={point.y}
                r="4"
                fill="#3b82f6"
                stroke="white"
                strokeWidth="2"
              />
            ))}

            {/* Appointments line (secondary Y-axis) */}
            <path
              d={appointmentPathData}
              stroke="#10b981"
              strokeWidth="2"
              fill="none"
              strokeDasharray="5,5"
            />

            {/* Secondary Y-axis (Appointments) */}
            <line
              x1={chartWidth - padding.right}
              y1={padding.top}
              x2={chartWidth - padding.right}
              y2={chartHeight - padding.bottom}
              stroke="#10b981"
              strokeWidth="2"
            />

            {/* Secondary Y-axis labels (Appointments) */}
            {[0, 1, 2, 3, 4].map((i) => {
              const value = maxAppointments * (1 - i / 4);
              const y =
                padding.top +
                (i * (chartHeight - padding.top - padding.bottom)) / 4;
              return (
                <text
                  key={i}
                  x={chartWidth - padding.right + 10}
                  y={y + 5}
                  textAnchor="start"
                  fontSize="12"
                  fill="#10b981"
                >
                  {Math.round(value)}
                </text>
              );
            })}

            {/* Secondary Y-axis label */}
            <text
              x={chartWidth - 20}
              y={chartHeight / 2}
              textAnchor="middle"
              fontSize="12"
              fill="#10b981"
              transform={`rotate(90, ${chartWidth - 20}, ${chartHeight / 2})`}
            >
              Appointments
            </text>
          </svg>
        </div>

        {/* Legend */}
        <div className="flex justify-center space-x-6 pt-4">
          <div className="flex items-center space-x-2">
            <div className="w-4 h-0.5 bg-blue-500"></div>
            <span className="text-sm text-gray-600">Revenue (EUR)</span>
          </div>
          <div className="flex items-center space-x-2">
            <div className="w-4 h-0.5 bg-green-500 border-dashed border-t-2 border-green-500"></div>
            <span className="text-sm text-gray-600">Appointments</span>
          </div>
        </div>
      </div>
    );
  };

  const renderSpecializationChart = () => {
    const chartWidth = 600;
    const chartHeight = 300;
    const padding = { top: 20, right: 40, bottom: 100, left: 80 };

    const maxAppointments = Math.max(
      ...specializationData.map((d) => d.appointments)
    );
    const barWidth =
      ((chartWidth - padding.left - padding.right) /
        specializationData.length) *
      0.7;
    const barSpacing =
      (chartWidth - padding.left - padding.right) / specializationData.length;

    return (
      <div className="space-y-4">
        <div className="flex justify-center">
          <svg
            width={chartWidth}
            height={chartHeight + 40}
            className="border border-gray-200 rounded"
          >
            {/* Grid lines */}
            {[0, 1, 2, 3, 4].map((i) => (
              <line
                key={i}
                x1={padding.left}
                y1={
                  padding.top +
                  (i * (chartHeight - padding.top - padding.bottom)) / 4
                }
                x2={chartWidth - padding.right}
                y2={
                  padding.top +
                  (i * (chartHeight - padding.top - padding.bottom)) / 4
                }
                stroke="#f3f4f6"
                strokeWidth="1"
              />
            ))}

            {/* Y-axis */}
            <line
              x1={padding.left}
              y1={padding.top}
              x2={padding.left}
              y2={chartHeight - padding.bottom}
              stroke="#374151"
              strokeWidth="2"
            />

            {/* X-axis */}
            <line
              x1={padding.left}
              y1={chartHeight - padding.bottom}
              x2={chartWidth - padding.right}
              y2={chartHeight - padding.bottom}
              stroke="#374151"
              strokeWidth="2"
            />

            {/* Y-axis labels */}
            {[0, 1, 2, 3, 4].map((i) => {
              const value = maxAppointments * (1 - i / 4);
              const y =
                padding.top +
                (i * (chartHeight - padding.top - padding.bottom)) / 4;
              return (
                <text
                  key={i}
                  x={padding.left - 10}
                  y={y + 5}
                  textAnchor="end"
                  fontSize="12"
                  fill="#6b7280"
                >
                  {Math.round(value)}
                </text>
              );
            })}

            {/* Y-axis label */}
            <text
              x={20}
              y={chartHeight / 2}
              textAnchor="middle"
              fontSize="12"
              fill="#374151"
              transform={`rotate(-90, 20, ${chartHeight / 2})`}
            >
              Number of Appointments
            </text>

            {/* Bars */}
            {specializationData.map((item, index) => {
              const barHeight =
                (item.appointments / maxAppointments) *
                (chartHeight - padding.top - padding.bottom);
              const x =
                padding.left + index * barSpacing + (barSpacing - barWidth) / 2;
              const y = chartHeight - padding.bottom - barHeight;

              return (
                <g key={index}>
                  <rect
                    x={x}
                    y={y}
                    width={barWidth}
                    height={barHeight}
                    fill={item.color}
                    opacity="0.8"
                  />
                  <text
                    x={x + barWidth / 2}
                    y={y - 5}
                    textAnchor="middle"
                    fontSize="11"
                    fill="#374151"
                    fontWeight="600"
                  >
                    {item.appointments}
                  </text>
                </g>
              );
            })}

            {/* X-axis labels */}
            {specializationData.map((item, index) => {
              const x = padding.left + index * barSpacing + barSpacing / 2;
              return (
                <text
                  key={index}
                  x={x}
                  y={chartHeight - padding.bottom + 20}
                  textAnchor="middle"
                  fontSize="10"
                  fill="#6b7280"
                  transform={`rotate(-45, ${x}, ${chartHeight - padding.bottom + 20})`}
                >
                  {item.specialization}
                </text>
              );
            })}

            {/* X-axis label */}
            <text
              x={chartWidth / 2}
              y={chartHeight + 35}
              textAnchor="middle"
              fontSize="12"
              fill="#374151"
            >
              Medical Specialization
            </text>
          </svg>
        </div>
      </div>
    );
  };

  const renderProductivityChart = () => {
    const chartWidth = 600;
    const chartHeight = 300;
    const padding = { top: 20, right: 40, bottom: 80, left: 80 };

    const maxRevenue = Math.max(...productivityData.map((d) => d.totalRevenue));
    const maxAppointments = Math.max(
      ...productivityData.map((d) => d.completedAppointments)
    );

    return (
      <div className="space-y-4">
        <div className="flex justify-center">
          <svg
            width={chartWidth}
            height={chartHeight + 40}
            className="border border-gray-200 rounded"
          >
            {/* Grid lines */}
            {[0, 1, 2, 3, 4].map((i) => (
              <g key={i}>
                <line
                  x1={padding.left}
                  y1={
                    padding.top +
                    (i * (chartHeight - padding.top - padding.bottom)) / 4
                  }
                  x2={chartWidth - padding.right}
                  y2={
                    padding.top +
                    (i * (chartHeight - padding.top - padding.bottom)) / 4
                  }
                  stroke="#f3f4f6"
                  strokeWidth="1"
                />
                <line
                  x1={
                    padding.left +
                    (i * (chartWidth - padding.left - padding.right)) / 4
                  }
                  y1={padding.top}
                  x2={
                    padding.left +
                    (i * (chartWidth - padding.left - padding.right)) / 4
                  }
                  y2={chartHeight - padding.bottom}
                  stroke="#f3f4f6"
                  strokeWidth="1"
                />
              </g>
            ))}

            {/* Y-axis */}
            <line
              x1={padding.left}
              y1={padding.top}
              x2={padding.left}
              y2={chartHeight - padding.bottom}
              stroke="#374151"
              strokeWidth="2"
            />

            {/* X-axis */}
            <line
              x1={padding.left}
              y1={chartHeight - padding.bottom}
              x2={chartWidth - padding.right}
              y2={chartHeight - padding.bottom}
              stroke="#374151"
              strokeWidth="2"
            />

            {/* Y-axis labels (Revenue) */}
            {[0, 1, 2, 3, 4].map((i) => {
              const value = maxRevenue * (1 - i / 4);
              const y =
                padding.top +
                (i * (chartHeight - padding.top - padding.bottom)) / 4;
              return (
                <text
                  key={i}
                  x={padding.left - 10}
                  y={y + 5}
                  textAnchor="end"
                  fontSize="12"
                  fill="#6b7280"
                >
                  €{(value / 1000).toFixed(0)}k
                </text>
              );
            })}

            {/* X-axis labels (Appointments) */}
            {[0, 1, 2, 3, 4].map((i) => {
              const value = (maxAppointments * i) / 4;
              const x =
                padding.left +
                (i * (chartWidth - padding.left - padding.right)) / 4;
              return (
                <text
                  key={i}
                  x={x}
                  y={chartHeight - padding.bottom + 20}
                  textAnchor="middle"
                  fontSize="12"
                  fill="#6b7280"
                >
                  {Math.round(value)}
                </text>
              );
            })}

            {/* Y-axis label */}
            <text
              x={20}
              y={chartHeight / 2}
              textAnchor="middle"
              fontSize="12"
              fill="#374151"
              transform={`rotate(-90, 20, ${chartHeight / 2})`}
            >
              Total Revenue (EUR)
            </text>

            {/* X-axis label */}
            <text
              x={chartWidth / 2}
              y={chartHeight + 35}
              textAnchor="middle"
              fontSize="12"
              fill="#374151"
            >
              Completed Appointments
            </text>

            {/* Scatter plot points */}
            {productivityData.map((doctor, index) => {
              const x =
                padding.left +
                (doctor.completedAppointments / maxAppointments) *
                  (chartWidth - padding.left - padding.right);
              const y =
                chartHeight -
                padding.bottom -
                (doctor.totalRevenue / maxRevenue) *
                  (chartHeight - padding.top - padding.bottom);
              const colors = [
                "#3b82f6",
                "#10b981",
                "#f59e0b",
                "#ef4444",
                "#8b5cf6",
                "#06b6d4",
                "#f97316",
                "#84cc16",
              ];

              return (
                <g key={index}>
                  <circle
                    cx={x}
                    cy={y}
                    r="6"
                    fill={colors[index % colors.length]}
                    opacity="0.8"
                    stroke="white"
                    strokeWidth="2"
                  />
                  <text
                    x={x}
                    y={y - 10}
                    textAnchor="middle"
                    fontSize="10"
                    fill="#374151"
                    fontWeight="600"
                  >
                    {doctor.doctorName.split(" ")[1]}
                  </text>
                </g>
              );
            })}
          </svg>
        </div>

        {/* Legend */}
        <div className="grid grid-cols-2 gap-2 text-xs">
          {productivityData.map((doctor, index) => {
            const colors = [
              "#3b82f6",
              "#10b981",
              "#f59e0b",
              "#ef4444",
              "#8b5cf6",
              "#06b6d4",
              "#f97316",
              "#84cc16",
            ];
            return (
              <div key={index} className="flex items-center space-x-2">
                <div
                  className="w-3 h-3 rounded-full"
                  style={{ backgroundColor: colors[index % colors.length] }}
                ></div>
                <span className="text-gray-600">{doctor.doctorName}</span>
              </div>
            );
          })}
        </div>
      </div>
    );
  };

  const renderActiveChart = () => {
    switch (activeChart) {
      case "revenue":
        return renderRevenueChart();
      case "specialization":
        return renderSpecializationChart();
      case "productivity":
        return renderProductivityChart();
      default:
        return renderRevenueChart();
    }
  };

  return (
    <div className="bg-white rounded-2xl shadow-md p-6">
      {/* Chart Type Selector */}
      <div className="flex flex-wrap gap-2 mb-6 border-b pb-4">
        {chartOptions.map((option) => {
          const Icon = option.icon;
          const isActive = activeChart === option.key;

          return (
            <button
              key={option.key}
              onClick={() => setActiveChart(option.key)}
              className={`flex items-center space-x-2 px-4 py-2 rounded-lg font-medium transition-all ${
                isActive
                  ? "bg-blue-100 text-blue-700 border-2 border-blue-200"
                  : "bg-gray-50 text-gray-600 border-2 border-transparent hover:bg-gray-100"
              }`}
            >
              <Icon className="h-4 w-4" />
              <span className="text-sm">{option.label}</span>
            </button>
          );
        })}
      </div>

      {/* Chart Description */}
      <div className="mb-4">
        <h3 className="text-lg font-semibold text-gray-800">
          {chartOptions.find((opt) => opt.key === activeChart)?.label}
        </h3>
        <p className="text-sm text-gray-600">
          {chartOptions.find((opt) => opt.key === activeChart)?.description}
        </p>
      </div>

      {/* Chart Content */}
      <div className="min-h-[400px]">{renderActiveChart()}</div>
    </div>
  );
}
