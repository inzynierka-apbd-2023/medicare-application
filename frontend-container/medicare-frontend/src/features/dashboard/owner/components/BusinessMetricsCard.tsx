import React from "react";
import { Card } from "@shared/components";
import { Activity, Calendar, Clock, TrendingUp, Users } from "lucide-react";

interface BusinessData {
  metrics: {
    totalPatients: number;
    newPatientsThisMonth: number;
    patientRetention: number;
    appointmentUtilization: number;
    averageWaitTime: number;
    noShowRate: number;
  };
  trends: {
    patientGrowth: number;
    appointmentGrowth: number;
    revenuePerPatient: number;
  };
  goals: {
    monthlyPatientTarget: number;
    utilizationTarget: number;
    retentionTarget: number;
  };
}

interface BusinessMetricsCardProps {
  data: BusinessData;
}

const BusinessMetricsCard: React.FC<BusinessMetricsCardProps> = ({ data }) => {
  const formatPercentage = (value: number) => {
    return `${value.toFixed(1)}%`;
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  };

  const getMetricColor = (value: number, target: number) => {
    const percentage = (value / target) * 100;
    if (percentage >= 95) return "text-green-600 bg-green-100";
    if (percentage >= 80) return "text-yellow-600 bg-yellow-100";
    return "text-red-600 bg-red-100";
  };

  const getTrendColor = (trend: number) => {
    return trend >= 0 ? "text-green-600" : "text-red-600";
  };

  const getTrendIcon = (trend: number) => {
    return trend >= 0 ? (
      <TrendingUp className="w-3 h-3 text-green-500" />
    ) : (
      <TrendingUp className="w-3 h-3 text-red-500 transform rotate-180" />
    );
  };

  return (
    <Card
      variant="elevated"
      padding="lg"
      className="bg-gradient-to-br from-purple-50 to-pink-50 border-purple-100"
    >
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2">
          <Activity className="w-6 h-6 text-purple-600" />
          <h3 className="text-lg font-semibold text-gray-900">
            Business Metrics
          </h3>
        </div>
        <Calendar className="w-5 h-5 text-gray-400" />
      </div>

      {/* Key Performance Indicators */}
      <div className="grid grid-cols-2 gap-3 mb-6">
        <div className="bg-white rounded-lg p-3">
          <div className="flex items-center justify-between mb-1">
            <Users className="w-4 h-4 text-purple-500" />
            <span
              className={`text-xs px-2 py-1 rounded-full ${getMetricColor(
                data.metrics.totalPatients,
                data.goals.monthlyPatientTarget
              )}`}
            >
              {(
                (data.metrics.totalPatients / data.goals.monthlyPatientTarget) *
                100
              ).toFixed(0)}
              %
            </span>
          </div>
          <p className="text-lg font-bold text-gray-900">
            {data.metrics.totalPatients.toLocaleString()}
          </p>
          <p className="text-xs text-gray-600">Total Patients</p>
          <div
            className={`flex items-center gap-1 mt-1 ${getTrendColor(data.trends.patientGrowth)}`}
          >
            {getTrendIcon(data.trends.patientGrowth)}
            <span className="text-xs">
              {formatPercentage(Math.abs(data.trends.patientGrowth))}
            </span>
          </div>
        </div>

        <div className="bg-white rounded-lg p-3">
          <div className="flex items-center justify-between mb-1">
            <Activity className="w-4 h-4 text-blue-500" />
            <span
              className={`text-xs px-2 py-1 rounded-full ${getMetricColor(
                data.metrics.appointmentUtilization,
                data.goals.utilizationTarget
              )}`}
            >
              {formatPercentage(data.metrics.appointmentUtilization)}
            </span>
          </div>
          <p className="text-lg font-bold text-gray-900">
            {formatPercentage(data.metrics.appointmentUtilization)}
          </p>
          <p className="text-xs text-gray-600">Utilization</p>
          <div
            className={`flex items-center gap-1 mt-1 ${getTrendColor(data.trends.appointmentGrowth)}`}
          >
            {getTrendIcon(data.trends.appointmentGrowth)}
            <span className="text-xs">
              {formatPercentage(Math.abs(data.trends.appointmentGrowth))}
            </span>
          </div>
        </div>
      </div>

      {/* Monthly Progress */}
      <div className="space-y-3 mb-4">
        <div>
          <div className="flex justify-between items-center mb-1">
            <span className="text-sm text-gray-600">
              New Patients This Month
            </span>
            <span className="text-sm font-semibold text-gray-900">
              {data.metrics.newPatientsThisMonth} /{" "}
              {data.goals.monthlyPatientTarget}
            </span>
          </div>
          <div className="bg-gray-200 rounded-full h-2">
            <div
              className="bg-purple-500 rounded-full h-2 transition-all duration-300"
              style={{
                width: `${Math.min((data.metrics.newPatientsThisMonth / data.goals.monthlyPatientTarget) * 100, 100)}%`,
              }}
            />
          </div>
        </div>

        <div>
          <div className="flex justify-between items-center mb-1">
            <span className="text-sm text-gray-600">Patient Retention</span>
            <span
              className={`text-sm font-semibold ${
                data.metrics.patientRetention >= data.goals.retentionTarget
                  ? "text-green-600"
                  : "text-red-600"
              }`}
            >
              {formatPercentage(data.metrics.patientRetention)}
            </span>
          </div>
          <div className="bg-gray-200 rounded-full h-2">
            <div
              className={`rounded-full h-2 transition-all duration-300 ${
                data.metrics.patientRetention >= data.goals.retentionTarget
                  ? "bg-green-500"
                  : "bg-red-500"
              }`}
              style={{
                width: `${(data.metrics.patientRetention / 100) * 100}%`,
              }}
            />
          </div>
        </div>
      </div>

      {/* Operational Metrics */}
      <div className="grid grid-cols-2 gap-4 mb-4">
        <div className="text-center">
          <div className="flex items-center justify-center gap-1 mb-1">
            <Clock className="w-4 h-4 text-gray-500" />
            <span className="text-xs text-gray-600">Avg. Wait</span>
          </div>
          <p className="text-lg font-bold text-gray-900">
            {data.metrics.averageWaitTime}m
          </p>
        </div>
        <div className="text-center">
          <div className="flex items-center justify-center gap-1 mb-1">
            <Users className="w-4 h-4 text-gray-500" />
            <span className="text-xs text-gray-600">No-Show</span>
          </div>
          <p className="text-lg font-bold text-red-600">
            {formatPercentage(data.metrics.noShowRate)}
          </p>
        </div>
      </div>

      {/* Revenue Per Patient */}
      <div className="bg-white rounded-lg p-3">
        <div className="text-center">
          <p className="text-sm text-gray-600 mb-1">Revenue per Patient</p>
          <p className="text-xl font-bold text-purple-600">
            {formatCurrency(data.trends.revenuePerPatient)}
          </p>
          <p className="text-xs text-gray-500">Monthly average</p>
        </div>
      </div>

      {/* Quick Insights */}
      <div className="mt-4 pt-4 border-t border-purple-100">
        <h4 className="text-sm font-medium text-gray-900 mb-2">
          Quick Insights
        </h4>
        <div className="space-y-1 text-xs text-gray-600">
          <p>• Peak hours: 9-11 AM, 2-4 PM</p>
          <p>• Busiest day: Tuesday</p>
          <p>• Top service: General Consultation (45%)</p>
        </div>
      </div>
    </Card>
  );
};

export default BusinessMetricsCard;
