import React from "react";
import type { DoctorAvailability } from "@features/dashboard/receptionist/types";
import { Badge, Card } from "@shared/components";
import { Clock, Stethoscope, User } from "lucide-react";

interface DoctorAvailabilityListProps {
  doctors: DoctorAvailability[];
  isLoading?: boolean;
  onDoctorClick?: (doctor: DoctorAvailability) => void;
}

const getStatusColor = (
  status: DoctorAvailability["status"]
): "default" | "success" | "warning" | "error" | "info" => {
  switch (status) {
    case "available":
      return "success";
    case "busy":
      return "warning";
    case "off-duty":
      return "error";
    default:
      return "default";
  }
};

const getStatusLabel = (status: DoctorAvailability["status"]): string => {
  switch (status) {
    case "available":
      return "Available";
    case "busy":
      return "Busy";
    case "off-duty":
      return "Off Duty";
    default:
      return "Unknown";
  }
};

export const DoctorAvailabilityList: React.FC<DoctorAvailabilityListProps> = ({
  doctors,
  isLoading,
  onDoctorClick,
}) => {
  if (isLoading) {
    return (
      <Card variant="medical" padding="md" className="h-full">
        <h3 className="text-lg font-semibold text-blue-600 mb-4">
          Doctor Availability
        </h3>
        <div className="space-y-3">
          {[...Array(5)].map((_, index) => (
            <div key={index} className="animate-pulse">
              <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                <div className="space-y-2">
                  <div className="h-4 bg-gray-200 rounded w-32"></div>
                  <div className="h-3 bg-gray-200 rounded w-24"></div>
                </div>
                <div className="space-y-2">
                  <div className="h-6 bg-gray-200 rounded w-16"></div>
                  <div className="h-3 bg-gray-200 rounded w-20"></div>
                </div>
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
        Doctor Availability
      </h3>
      {doctors.length === 0 ? (
        <div className="text-center py-8 text-gray-500">
          <Stethoscope className="h-12 w-12 mx-auto mb-4 text-gray-300" />
          <p>No doctors available</p>
        </div>
      ) : (
        <div className="space-y-3 max-h-96 overflow-y-auto">
          {doctors.map((doctor) => (
            <div
              key={doctor.id}
              className={`p-3 border border-gray-200 rounded-lg hover:shadow-md transition-shadow ${
                onDoctorClick ? "cursor-pointer hover:bg-gray-50" : ""
              }`}
              onClick={() => onDoctorClick?.(doctor)}
            >
              <div className="flex items-center justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-1">
                    <Stethoscope className="h-4 w-4 text-gray-400" />
                    <span className="font-medium text-gray-900">
                      {doctor.name}
                    </span>
                  </div>
                  <div className="text-sm text-gray-600 mb-1">
                    {doctor.specialization}
                  </div>
                  {doctor.currentPatient && (
                    <div className="text-xs text-gray-500 flex items-center gap-1">
                      <User className="h-3 w-3" />
                      {doctor.currentPatient}
                    </div>
                  )}
                  {doctor.nextAvailable && doctor.status === "busy" && (
                    <div className="text-xs text-gray-500 flex items-center gap-1 mt-1">
                      <Clock className="h-3 w-3" />
                      Next: {doctor.nextAvailable}
                    </div>
                  )}
                </div>
                <div className="flex flex-col items-end gap-2">
                  <Badge variant={getStatusColor(doctor.status)} size="sm">
                    {getStatusLabel(doctor.status)}
                  </Badge>
                  <div className="text-xs text-gray-500 text-right">
                    <div>
                      {doctor.completedToday}/{doctor.totalAppointments} today
                    </div>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
};
