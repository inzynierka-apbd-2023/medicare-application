import React from "react";
import {
  Calendar,
  Clock,
  DollarSign,
  Star,
  TrendingUp,
  Users,
} from "lucide-react";

import { Card } from "../../../shared/components";

interface AppointmentMetric {
  id: string;
  title: string;
  value: number;
  change: number;
  period: string;
  icon: "calendar" | "trending" | "users" | "clock" | "dollar" | "star";
}

interface AppointmentMetricsCardProps {
  metrics: AppointmentMetric[];
}

const AppointmentMetricsCard: React.FC<AppointmentMetricsCardProps> = ({
  metrics,
}) => {
  const getIcon = (iconType: string) => {
    switch (iconType) {
      case "calendar":
        return <Calendar className="w-6 h-6 text-blue-500" />;
      case "trending":
        return <TrendingUp className="w-6 h-6 text-green-500" />;
      case "users":
        return <Users className="w-6 h-6 text-purple-500" />;
      case "clock":
        return <Clock className="w-6 h-6 text-orange-500" />;
      case "dollar":
        return <DollarSign className="w-6 h-6 text-emerald-500" />;
      case "star":
        return <Star className="w-6 h-6 text-yellow-500" />;
      default:
        return <Calendar className="w-6 h-6 text-gray-500" />;
    }
  };

  const formatValue = (value: number, title: string) => {
    if (
      title.toLowerCase().includes("revenue") ||
      title.toLowerCase().includes("cost")
    ) {
      return new Intl.NumberFormat("pl-PL", {
        style: "currency",
        currency: "PLN",
        minimumFractionDigits: 0,
      }).format(value);
    }
    return value.toLocaleString();
  };

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      {metrics.map((metric) => (
        <Card key={metric.id} variant="elevated" className="p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">
                {metric.title}
              </p>
              <p className="text-2xl font-bold text-gray-900">
                {formatValue(metric.value, metric.title)}
              </p>
              <div className="flex items-center mt-2">
                <span
                  className={`text-sm font-medium ${
                    metric.change >= 0 ? "text-green-600" : "text-red-600"
                  }`}
                >
                  {metric.change >= 0 ? "+" : ""}
                  {metric.change.toFixed(1)}%
                </span>
                <span className="text-sm text-gray-500 ml-2">
                  {metric.period}
                </span>
              </div>
            </div>
            {getIcon(metric.icon)}
          </div>
        </Card>
      ))}
    </div>
  );
};

export default AppointmentMetricsCard;
