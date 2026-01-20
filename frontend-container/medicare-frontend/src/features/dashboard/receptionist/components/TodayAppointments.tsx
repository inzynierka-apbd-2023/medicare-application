import React from "react";
import type { QuickAppointment } from "@features/dashboard/receptionist/types";
import { Badge, Card } from "@shared/components";
import { Clock, MapPin, Phone, Video } from "lucide-react";

interface TodayAppointmentsProps {
  appointments: QuickAppointment[];
  isLoading?: boolean;
  onAppointmentClick?: (appointment: QuickAppointment) => void;
}

const getStatusColor = (
  status: QuickAppointment["status"]
): "default" | "success" | "warning" | "error" | "info" => {
  switch (status) {
    case "completed":
      return "success";
    case "in-progress":
      return "warning";
    case "waiting":
      return "info";
    case "cancelled":
      return "error";
    default:
      return "default";
  }
};

const getTypeIcon = (type: QuickAppointment["type"]) => {
  switch (type) {
    case "video-call":
      return Video;
    case "phone":
      return Phone;
    case "in-person":
    default:
      return MapPin;
  }
};

export const TodayAppointments: React.FC<TodayAppointmentsProps> = ({
  appointments,
  isLoading,
  onAppointmentClick,
}) => {
  if (isLoading) {
    return (
      <Card variant="medical" padding="md" className="h-full">
        <h3 className="text-lg font-semibold text-blue-600 mb-4">
          Today's Appointments
        </h3>
        <div className="space-y-3">
          {[...Array(5)].map((_, index) => (
            <div key={index} className="animate-pulse">
              <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                <div className="space-y-2">
                  <div className="h-4 bg-gray-200 rounded w-32"></div>
                  <div className="h-3 bg-gray-200 rounded w-24"></div>
                </div>
                <div className="h-6 bg-gray-200 rounded w-16"></div>
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
        Today's Appointments
      </h3>
      {appointments.length === 0 ? (
        <div className="text-center py-8 text-gray-500">
          <Clock className="h-12 w-12 mx-auto mb-4 text-gray-300" />
          <p>No appointments scheduled for today</p>
        </div>
      ) : (
        <div className="space-y-3 max-h-96 overflow-y-auto">
          {appointments.map((appointment) => {
            const TypeIcon = getTypeIcon(appointment.type);

            return (
              <div
                key={appointment.id}
                className={`p-3 border border-gray-200 rounded-lg hover:shadow-md transition-shadow ${
                  onAppointmentClick ? "cursor-pointer hover:bg-gray-50" : ""
                }`}
                onClick={() => onAppointmentClick?.(appointment)}
              >
                <div className="flex items-center justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-2 mb-1">
                      <TypeIcon className="h-4 w-4 text-gray-400" />
                      <span className="font-medium text-gray-900">
                        {appointment.patientName}
                      </span>
                    </div>
                    <div className="text-sm text-gray-600">
                      {appointment.doctorName}
                    </div>
                    {appointment.room && (
                      <div className="text-xs text-gray-500 mt-1">
                        {appointment.room}
                      </div>
                    )}
                  </div>
                  <div className="flex flex-col items-end gap-2">
                    <span className="text-sm font-medium text-gray-900">
                      {appointment.time}
                    </span>
                    <Badge
                      variant={getStatusColor(appointment.status)}
                      size="sm"
                    >
                      {appointment.status}
                    </Badge>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </Card>
  );
};
