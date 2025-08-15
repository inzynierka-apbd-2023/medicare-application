import React from "react";
import { Activity, Calendar, CreditCard, UserPlus, X } from "lucide-react";

import { Card } from "../../../../shared/components";
import type { RecentActivity } from "../types";

interface RecentActivityListProps {
  activities: RecentActivity[];
  isLoading?: boolean;
}

const getActivityIcon = (type: RecentActivity["type"]) => {
  switch (type) {
    case "appointment_created":
      return Calendar;
    case "appointment_cancelled":
      return X;
    case "patient_registered":
      return UserPlus;
    case "payment_processed":
      return CreditCard;
    default:
      return Activity;
  }
};

const getActivityColor = (type: RecentActivity["type"]): string => {
  switch (type) {
    case "appointment_created":
      return "text-green-600 bg-green-50";
    case "appointment_cancelled":
      return "text-red-600 bg-red-50";
    case "patient_registered":
      return "text-blue-600 bg-blue-50";
    case "payment_processed":
      return "text-purple-600 bg-purple-50";
    default:
      return "text-gray-600 bg-gray-50";
  }
};

const formatTimestamp = (timestamp: string): string => {
  const date = new Date(timestamp);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / (1000 * 60));

  if (diffMins < 1) return "Just now";
  if (diffMins < 60) return `${diffMins}m ago`;

  const diffHours = Math.floor(diffMins / 60);
  if (diffHours < 24) return `${diffHours}h ago`;

  return date.toLocaleDateString();
};

export const RecentActivityList: React.FC<RecentActivityListProps> = ({
  activities,
  isLoading,
}) => {
  if (isLoading) {
    return (
      <Card variant="medical" padding="md" className="h-full">
        <h3 className="text-lg font-semibold text-blue-600 mb-4">
          Recent Activity
        </h3>
        <div className="space-y-3">
          {[...Array(5)].map((_, index) => (
            <div key={index} className="animate-pulse">
              <div className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg">
                <div className="h-10 w-10 bg-gray-200 rounded-full"></div>
                <div className="flex-1 space-y-2">
                  <div className="h-4 bg-gray-200 rounded w-3/4"></div>
                  <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                </div>
                <div className="h-3 bg-gray-200 rounded w-16"></div>
              </div>
            </div>
          ))}
        </div>
      </Card>
    );
  }

  return (
    <Card variant="medical" padding="md" className="h-full">
      <h3 className="text-lg font-semibold text-blue-600 mb-4">
        Recent Activity
      </h3>
      {activities.length === 0 ? (
        <div className="text-center py-8 text-gray-500">
          <Activity className="h-12 w-12 mx-auto mb-4 text-gray-300" />
          <p>No recent activity</p>
        </div>
      ) : (
        <div className="space-y-3 max-h-96 overflow-y-auto">
          {activities.map((activity) => {
            const ActivityIcon = getActivityIcon(activity.type);
            const colorClasses = getActivityColor(activity.type);

            return (
              <div
                key={activity.id}
                className="flex items-start gap-3 p-3 border border-gray-200 rounded-lg"
              >
                <div className={`p-2 rounded-full ${colorClasses}`}>
                  <ActivityIcon className="h-4 w-4" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-900 mb-1">
                    {activity.message}
                  </p>
                  <div className="flex flex-wrap gap-1 text-xs text-gray-600">
                    {activity.patientName && (
                      <span className="bg-gray-100 px-2 py-1 rounded">
                        {activity.patientName}
                      </span>
                    )}
                    {activity.doctorName && (
                      <span className="bg-blue-100 px-2 py-1 rounded">
                        {activity.doctorName}
                      </span>
                    )}
                  </div>
                </div>
                <div className="text-xs text-gray-500 whitespace-nowrap">
                  {formatTimestamp(activity.timestamp)}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </Card>
  );
};
