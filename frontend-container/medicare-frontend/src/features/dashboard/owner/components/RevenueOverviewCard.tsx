import React from "react";
import { Calendar, DollarSign, TrendingDown, TrendingUp } from "lucide-react";

import { Card } from "../../../../shared/components";

interface RevenueData {
  daily: number;
  weekly: number;
  monthly: number;
  yearly: number;
  growth: {
    daily: number;
    weekly: number;
    monthly: number;
    yearly: number;
  };
}

interface RevenueOverviewCardProps {
  data: RevenueData;
  selectedPeriod?: "daily" | "weekly" | "monthly" | "yearly";
  onPeriodChange?: (period: "daily" | "weekly" | "monthly" | "yearly") => void;
}

const RevenueOverviewCard: React.FC<RevenueOverviewCardProps> = ({
  data,
  selectedPeriod = "monthly",
  onPeriodChange,
}) => {
  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  };

  const formatPercentage = (value: number) => {
    return `${value >= 0 ? "+" : ""}${value.toFixed(1)}%`;
  };

  const getGrowthIcon = (growth: number) => {
    return growth >= 0 ? (
      <TrendingUp className="w-4 h-4 text-green-500" />
    ) : (
      <TrendingDown className="w-4 h-4 text-red-500" />
    );
  };

  const getGrowthColor = (growth: number) => {
    return growth >= 0 ? "text-green-600" : "text-red-600";
  };

  const getCurrentRevenue = () => {
    switch (selectedPeriod) {
      case "daily":
        return data.daily;
      case "weekly":
        return data.weekly;
      case "monthly":
        return data.monthly;
      case "yearly":
        return data.yearly;
      default:
        return data.monthly;
    }
  };

  const getCurrentGrowth = () => {
    switch (selectedPeriod) {
      case "daily":
        return data.growth.daily;
      case "weekly":
        return data.growth.weekly;
      case "monthly":
        return data.growth.monthly;
      case "yearly":
        return data.growth.yearly;
      default:
        return data.growth.monthly;
    }
  };

  const periods = [
    { key: "daily" as const, label: "Daily" },
    { key: "weekly" as const, label: "Weekly" },
    { key: "monthly" as const, label: "Monthly" },
    { key: "yearly" as const, label: "Yearly" },
  ];

  return (
    <Card
      variant="elevated"
      padding="lg"
      className="bg-gradient-to-br from-green-50 to-emerald-50 border-green-100"
    >
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2">
          <DollarSign className="w-6 h-6 text-green-600" />
          <h3 className="text-lg font-semibold text-gray-900">
            Revenue Overview
          </h3>
        </div>
        <Calendar className="w-5 h-5 text-gray-400" />
      </div>

      {/* Period Selector */}
      <div className="flex gap-1 mb-6 bg-white rounded-lg p-1">
        {periods.map((period) => (
          <button
            key={period.key}
            onClick={() => onPeriodChange?.(period.key)}
            className={`flex-1 px-3 py-2 text-xs font-medium rounded-md transition-colors ${
              selectedPeriod === period.key
                ? "bg-green-600 text-white"
                : "text-gray-600 hover:text-gray-900 hover:bg-gray-50"
            }`}
          >
            {period.label}
          </button>
        ))}
      </div>

      {/* Main Revenue Display */}
      <div className="text-center mb-6">
        <p className="text-sm text-gray-600 mb-2 capitalize">
          {selectedPeriod} Revenue
        </p>
        <p className="text-4xl font-bold text-green-600 mb-2">
          {formatCurrency(getCurrentRevenue())}
        </p>
        <div
          className={`flex items-center justify-center gap-1 ${getGrowthColor(getCurrentGrowth())}`}
        >
          {getGrowthIcon(getCurrentGrowth())}
          <span className="text-sm font-medium">
            {formatPercentage(getCurrentGrowth())} vs last{" "}
            {selectedPeriod.replace("ly", "")}
          </span>
        </div>
      </div>

      {/* Revenue Breakdown */}
      <div className="grid grid-cols-2 gap-4">
        <div className="bg-white rounded-lg p-3">
          <p className="text-xs text-gray-500 mb-1">Monthly</p>
          <p className="text-lg font-semibold text-gray-900">
            {formatCurrency(data.monthly)}
          </p>
          <div
            className={`flex items-center gap-1 mt-1 ${getGrowthColor(data.growth.monthly)}`}
          >
            {getGrowthIcon(data.growth.monthly)}
            <span className="text-xs">
              {formatPercentage(data.growth.monthly)}
            </span>
          </div>
        </div>
        <div className="bg-white rounded-lg p-3">
          <p className="text-xs text-gray-500 mb-1">Yearly</p>
          <p className="text-lg font-semibold text-gray-900">
            {formatCurrency(data.yearly)}
          </p>
          <div
            className={`flex items-center gap-1 mt-1 ${getGrowthColor(data.growth.yearly)}`}
          >
            {getGrowthIcon(data.growth.yearly)}
            <span className="text-xs">
              {formatPercentage(data.growth.yearly)}
            </span>
          </div>
        </div>
      </div>

      {/* Revenue Targets */}
      <div className="mt-4 pt-4 border-t border-green-100">
        <div className="flex justify-between items-center text-sm">
          <span className="text-gray-600">Monthly Target:</span>
          <span className="font-medium text-gray-900">
            {formatCurrency(300000)}
          </span>
        </div>
        <div className="mt-2 bg-gray-200 rounded-full h-2">
          <div
            className="bg-green-500 rounded-full h-2 transition-all duration-300"
            style={{
              width: `${Math.min((data.monthly / 300000) * 100, 100)}%`,
            }}
          />
        </div>
        <p className="text-xs text-gray-500 mt-1">
          {((data.monthly / 300000) * 100).toFixed(1)}% of monthly target
          achieved
        </p>
      </div>
    </Card>
  );
};

export default RevenueOverviewCard;
