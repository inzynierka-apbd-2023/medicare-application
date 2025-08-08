import React from "react";
import {
  AlertTriangle,
  Calendar,
  CheckCircle,
  Clock,
  FileText,
  MessageCircle,
  Phone,
  User,
  XCircle,
} from "lucide-react";

import { Badge, Button, Card } from "../../../shared/components";
import type { TodayAppointmentCardProps } from "../types";

export const TodayAppointmentCard: React.FC<TodayAppointmentCardProps> = ({
  appointment,
  timeStatus,
  onDetails,
  onMarkCompleted,
  onMarkNoShow,
  onContactPatient,
  showCompletionActions = true,
}) => {
  const getTimeStatusBadge = () => {
    switch (timeStatus) {
      case "current":
        return (
          <Badge variant="warning" icon={<Clock size={14} />}>
            Current
          </Badge>
        );
      case "overdue":
        return (
          <Badge variant="error" icon={<AlertTriangle size={14} />}>
            Overdue
          </Badge>
        );
      case "upcoming":
        return (
          <Badge variant="info" icon={<Calendar size={14} />}>
            Upcoming
          </Badge>
        );
      case "completed":
        return (
          <Badge variant="success" icon={<CheckCircle size={14} />}>
            Completed
          </Badge>
        );
      case "no-show":
        return (
          <Badge variant="error" icon={<XCircle size={14} />}>
            No Show
          </Badge>
        );
      default:
        return null;
    }
  };

  const getCardBorderClass = () => {
    switch (timeStatus) {
      case "current":
        return "border-l-4 border-l-yellow-500";
      case "overdue":
        return "border-l-4 border-l-red-500";
      case "upcoming":
        return "border-l-4 border-l-blue-500";
      case "completed":
        return "border-l-4 border-l-green-500";
      case "no-show":
        return "border-l-4 border-l-red-500";
      default:
        return "";
    }
  };

  const formatTime = (time: string) => {
    const [hours, minutes] = time.split(":");
    const hour = parseInt(hours);
    const ampm = hour >= 12 ? "PM" : "AM";
    const displayHour = hour > 12 ? hour - 12 : hour === 0 ? 12 : hour;
    return `${displayHour}:${minutes} ${ampm}`;
  };

  const getEndTime = () => {
    const [hours, minutes] = appointment.time.split(":").map(Number);
    const endMinutes = hours * 60 + minutes + appointment.duration;
    const endHour = Math.floor(endMinutes / 60);
    const endMin = endMinutes % 60;
    const endTimeString = `${endHour.toString().padStart(2, "0")}:${endMin.toString().padStart(2, "0")}`;
    return formatTime(endTimeString);
  };

  return (
    <Card
      variant="medical"
      padding="md"
      className={`transition-all duration-200 hover:shadow-lg ${getCardBorderClass()}`}
    >
      <div className="space-y-4">
        {/* Header */}
        <div className="flex items-start justify-between">
          <div className="flex items-center space-x-3">
            <div className="flex-shrink-0">
              <User className="h-10 w-10 text-blue-600 bg-blue-100 rounded-full p-2" />
            </div>
            <div>
              <h3 className="text-lg font-semibold text-gray-900">
                {appointment.patient.name}
              </h3>
              <p className="text-sm text-gray-600">
                Age {appointment.patient.age} • {appointment.appointmentType}
              </p>
            </div>
          </div>
          <div className="flex items-center space-x-2">
            {getTimeStatusBadge()}
          </div>
        </div>

        {/* Time and Duration */}
        <div className="flex items-center space-x-4 text-sm text-gray-600">
          <div className="flex items-center space-x-1">
            <Clock className="h-4 w-4" />
            <span>
              {formatTime(appointment.time)} - {getEndTime()}(
              {appointment.duration} min)
            </span>
          </div>
          <div className="flex items-center space-x-1">
            <Phone className="h-4 w-4" />
            <span>{appointment.patient.phone}</span>
          </div>
        </div>

        {/* Chief Complaint */}
        {appointment.chiefComplaint && (
          <div className="bg-gray-50 rounded-lg p-3">
            <p className="text-sm font-medium text-gray-700 mb-1">
              Chief Complaint:
            </p>
            <p className="text-sm text-gray-600">
              {appointment.chiefComplaint}
            </p>
          </div>
        )}

        {/* Medical Info */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-3 text-sm">
          {appointment.patient.medicalHistory &&
            appointment.patient.medicalHistory.length > 0 && (
              <div>
                <p className="font-medium text-gray-700 mb-1">
                  Medical History:
                </p>
                <p className="text-gray-600">
                  {appointment.patient.medicalHistory.slice(0, 2).join(", ")}
                  {appointment.patient.medicalHistory.length > 2 && "..."}
                </p>
              </div>
            )}

          {appointment.patient.allergies &&
            appointment.patient.allergies.length > 0 && (
              <div>
                <p className="font-medium text-gray-700 mb-1">Allergies:</p>
                <p className="text-red-600">
                  {appointment.patient.allergies.slice(0, 2).join(", ")}
                  {appointment.patient.allergies.length > 2 && "..."}
                </p>
              </div>
            )}

          {appointment.patient.currentMedications &&
            appointment.patient.currentMedications.length > 0 && (
              <div>
                <p className="font-medium text-gray-700 mb-1">
                  Current Medications:
                </p>
                <p className="text-gray-600">
                  {appointment.patient.currentMedications
                    .slice(0, 2)
                    .join(", ")}
                  {appointment.patient.currentMedications.length > 2 && "..."}
                </p>
              </div>
            )}
        </div>

        {/* Notes (for completed appointments) */}
        {appointment.notes && (
          <div className="bg-green-50 rounded-lg p-3">
            <p className="text-sm font-medium text-green-700 mb-1">Notes:</p>
            <p className="text-sm text-green-600">{appointment.notes}</p>
          </div>
        )}

        {/* Action Buttons */}
        <div className="flex flex-wrap gap-2 pt-2 border-t border-gray-100">
          <Button
            variant="outline"
            size="sm"
            onClick={() => onDetails(appointment)}
            className="flex items-center space-x-1"
          >
            <FileText className="h-4 w-4" />
            <span>Details</span>
          </Button>

          {onContactPatient && (
            <Button
              variant="outline"
              size="sm"
              onClick={() => onContactPatient(appointment.patient.id)}
              className="flex items-center space-x-1"
            >
              <MessageCircle className="h-4 w-4" />
              <span>Message</span>
            </Button>
          )}

          {showCompletionActions && appointment.status === "scheduled" && (
            <>
              {onMarkCompleted && (
                <Button
                  variant="primary"
                  size="sm"
                  onClick={() => onMarkCompleted(appointment.id)}
                  className="flex items-center space-x-1"
                >
                  <CheckCircle className="h-4 w-4" />
                  <span>Complete</span>
                </Button>
              )}

              {onMarkNoShow && (
                <Button
                  variant="secondary"
                  size="sm"
                  onClick={() => onMarkNoShow(appointment.id)}
                  className="flex items-center space-x-1"
                >
                  <XCircle className="h-4 w-4" />
                  <span>No Show</span>
                </Button>
              )}
            </>
          )}
        </div>
      </div>
    </Card>
  );
};
