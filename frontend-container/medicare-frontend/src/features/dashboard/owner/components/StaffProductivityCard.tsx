import React from "react";
import { Star, UserCheck, Users } from "lucide-react";

import { Card } from "../../../../shared/components";

interface StaffData {
  totalStaff: number;
  totalDoctors: number;
  activeToday: number;
  utilization: number;
  satisfaction: number;
  productivity: {
    appointmentsPerDoctor: number;
    completionRate: number;
    averageConsultationTime: number;
  };
  performance: {
    topPerformer: string;
    averageRating: number;
    patientsServed: number;
  };
}

interface StaffProductivityCardProps {
  data: StaffData;
}

const StaffProductivityCard: React.FC<StaffProductivityCardProps> = ({
  data,
}) => {
  const formatPercentage = (value: number) => {
    return `${value.toFixed(1)}%`;
  };

  const getUtilizationColor = (utilization: number) => {
    if (utilization >= 90) return "text-green-600 bg-green-100";
    if (utilization >= 75) return "text-yellow-600 bg-yellow-100";
    return "text-red-600 bg-red-100";
  };

  const getSatisfactionStars = (rating: number) => {
    return Array.from({ length: 5 }, (_, i) => (
      <Star
        key={i}
        className={`w-3 h-3 ${
          i < Math.floor(rating)
            ? "text-yellow-400 fill-current"
            : "text-gray-300"
        }`}
      />
    ));
  };

  return (
    <Card
      variant="elevated"
      padding="lg"
      className="bg-gradient-to-br from-blue-50 to-indigo-50 border-blue-100"
    >
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2">
          <UserCheck className="w-6 h-6 text-blue-600" />
          <h3 className="text-lg font-semibold text-gray-900">
            Staff Productivity
          </h3>
        </div>
        <Users className="w-5 h-5 text-gray-400" />
      </div>

      {/* Staff Overview */}
      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="text-center">
          <p className="text-2xl font-bold text-blue-600">{data.totalStaff}</p>
          <p className="text-xs text-gray-600">Total Staff</p>
        </div>
        <div className="text-center">
          <p className="text-2xl font-bold text-green-600">
            {data.totalDoctors}
          </p>
          <p className="text-xs text-gray-600">Doctors</p>
        </div>
        <div className="text-center">
          <p className="text-2xl font-bold text-purple-600">
            {data.activeToday}
          </p>
          <p className="text-xs text-gray-600">Active Today</p>
        </div>
      </div>

      {/* Utilization Metric */}
      <div className="bg-white rounded-lg p-4 mb-4">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm font-medium text-gray-700">
            Staff Utilization
          </span>
          <span
            className={`text-sm font-bold px-2 py-1 rounded-full ${getUtilizationColor(data.utilization)}`}
          >
            {formatPercentage(data.utilization)}
          </span>
        </div>
        <div className="bg-gray-200 rounded-full h-2">
          <div
            className="bg-blue-500 rounded-full h-2 transition-all duration-300"
            style={{ width: `${data.utilization}%` }}
          />
        </div>
      </div>

      {/* Performance Metrics */}
      <div className="space-y-3">
        <div className="flex justify-between items-center">
          <span className="text-sm text-gray-600">
            Avg. Appointments/Doctor
          </span>
          <span className="font-semibold text-gray-900">
            {data.productivity.appointmentsPerDoctor}
          </span>
        </div>

        <div className="flex justify-between items-center">
          <span className="text-sm text-gray-600">Completion Rate</span>
          <span className="font-semibold text-green-600">
            {formatPercentage(data.productivity.completionRate)}
          </span>
        </div>

        <div className="flex justify-between items-center">
          <span className="text-sm text-gray-600">Avg. Consultation Time</span>
          <span className="font-semibold text-gray-900">
            {data.productivity.averageConsultationTime} min
          </span>
        </div>

        <div className="flex justify-between items-center">
          <span className="text-sm text-gray-600">Staff Satisfaction</span>
          <div className="flex items-center gap-1">
            {getSatisfactionStars(data.satisfaction)}
            <span className="text-sm font-medium text-gray-700 ml-1">
              {data.satisfaction.toFixed(1)}
            </span>
          </div>
        </div>
      </div>

      {/* Top Performer Highlight */}
      <div className="mt-4 pt-4 border-t border-blue-100">
        <div className="bg-white rounded-lg p-3">
          <div className="flex items-center gap-2 mb-2">
            <Star className="w-4 h-4 text-yellow-500 fill-current" />
            <span className="text-sm font-medium text-gray-900">
              Top Performer
            </span>
          </div>
          <p className="font-semibold text-blue-600 mb-1">
            {data.performance.topPerformer}
          </p>
          <div className="flex justify-between text-xs text-gray-600">
            <span>Rating: {data.performance.averageRating.toFixed(1)}/5.0</span>
            <span>Patients: {data.performance.patientsServed}</span>
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="mt-4 grid grid-cols-2 gap-2">
        <button className="px-3 py-2 text-xs font-medium text-blue-600 bg-blue-50 rounded-lg hover:bg-blue-100 transition-colors">
          View Schedules
        </button>
        <button className="px-3 py-2 text-xs font-medium text-gray-600 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors">
          Performance Reports
        </button>
      </div>
    </Card>
  );
};

export default StaffProductivityCard;
