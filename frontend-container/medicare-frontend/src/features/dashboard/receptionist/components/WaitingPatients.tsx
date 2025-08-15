import React from "react";
import { Clock, MapPin, Phone, Video } from "lucide-react";

import { Badge, Button, Card } from "../../../../shared/components";
import type { WaitingPatient } from "../types";

interface WaitingPatientsProps {
  patients: WaitingPatient[];
  isLoading?: boolean;
  onStatusUpdate?: (
    patientId: string,
    status: WaitingPatient["status"]
  ) => void;
}

const getStatusColor = (
  status: WaitingPatient["status"]
): "default" | "success" | "warning" | "error" | "info" => {
  switch (status) {
    case "waiting":
      return "warning";
    case "called":
      return "info";
    case "in-room":
      return "success";
    default:
      return "default";
  }
};

const getTypeIcon = (type: WaitingPatient["type"]) => {
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

const formatWaitTime = (minutes: number): string => {
  if (minutes === 0) return "Just arrived";
  if (minutes === 1) return "1 minute";
  return `${minutes} minutes`;
};

export const WaitingPatients: React.FC<WaitingPatientsProps> = ({
  patients,
  isLoading,
  onStatusUpdate,
}) => {
  if (isLoading) {
    return (
      <Card variant="medical" padding="md" className="h-full">
        <h3 className="text-lg font-semibold text-blue-600 mb-4">
          Waiting Patients
        </h3>
        <div className="space-y-3">
          {[...Array(3)].map((_, index) => (
            <div key={index} className="animate-pulse">
              <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                <div className="space-y-2">
                  <div className="h-4 bg-gray-200 rounded w-32"></div>
                  <div className="h-3 bg-gray-200 rounded w-24"></div>
                </div>
                <div className="space-y-2">
                  <div className="h-6 bg-gray-200 rounded w-16"></div>
                  <div className="h-8 bg-gray-200 rounded w-20"></div>
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
        Waiting Patients
      </h3>
      {patients.length === 0 ? (
        <div className="text-center py-8 text-gray-500">
          <Clock className="h-12 w-12 mx-auto mb-4 text-gray-300" />
          <p>No patients currently waiting</p>
        </div>
      ) : (
        <div className="space-y-3 max-h-96 overflow-y-auto">
          {patients.map((patient) => {
            const TypeIcon = getTypeIcon(patient.type);

            return (
              <div
                key={patient.id}
                className="p-3 border border-gray-200 rounded-lg"
              >
                <div className="flex items-center justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-2 mb-1">
                      <TypeIcon className="h-4 w-4 text-gray-400" />
                      <span className="font-medium text-gray-900">
                        {patient.name}
                      </span>
                    </div>
                    <div className="text-sm text-gray-600 mb-1">
                      {patient.doctorName} • {patient.appointmentTime}
                    </div>
                    <div className="text-xs text-gray-500">
                      Waiting: {formatWaitTime(patient.waitTime)}
                    </div>
                  </div>
                  <div className="flex flex-col items-end gap-2">
                    <Badge variant={getStatusColor(patient.status)} size="sm">
                      {patient.status}
                    </Badge>
                    {onStatusUpdate && patient.status === "waiting" && (
                      <div className="flex gap-1">
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={() => onStatusUpdate(patient.id, "called")}
                        >
                          Call
                        </Button>
                        <Button
                          size="sm"
                          variant="primary"
                          onClick={() => onStatusUpdate(patient.id, "in-room")}
                        >
                          Room
                        </Button>
                      </div>
                    )}
                    {onStatusUpdate && patient.status === "called" && (
                      <Button
                        size="sm"
                        variant="primary"
                        onClick={() => onStatusUpdate(patient.id, "in-room")}
                      >
                        In Room
                      </Button>
                    )}
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
