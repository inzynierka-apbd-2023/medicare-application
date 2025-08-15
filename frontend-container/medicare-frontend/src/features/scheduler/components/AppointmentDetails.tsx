/**
 * AppointmentDetails Component
 * Modular component for displaying selected appointment details
 */

import React from "react";
import { useNavigate } from "react-router-dom";
import { Calendar, MapPin, Phone, User, Video } from "lucide-react";

import { Button } from "../../../shared/components";
import type { Appointment } from "../types";

interface AppointmentDetailsProps {
  appointment: Appointment | null;
  onMarkAsCompleted: (appointmentId: string) => void;
  onMarkAsNoShow: (appointmentId: string) => void;
  onStartVideoCall: (appointmentId: string) => void;
  isLoading?: boolean;
}

export const AppointmentDetails: React.FC<AppointmentDetailsProps> = ({
  appointment,
  onMarkAsCompleted,
  onMarkAsNoShow,
  onStartVideoCall,
  isLoading = false,
}) => {
  const navigate = useNavigate();

  const handleViewPatientRecords = () => {
    if (appointment?.patient?.id) {
      navigate(`/medical-records/${appointment.patient.id}`);
    }
  };

  if (!appointment) {
    return (
      <div className="text-center text-gray-500 py-8">
        <Calendar size={48} className="mx-auto mb-4 opacity-50" />
        <p>Click on an appointment in the calendar to view details</p>
      </div>
    );
  }

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case "completed":
        return "bg-green-100 text-green-800";
      case "confirmed":
        return "bg-blue-100 text-blue-800";
      case "pending":
        return "bg-yellow-100 text-yellow-800";
      case "cancelled":
        return "bg-red-100 text-red-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  const getAppointmentTypeIcon = (type: string) => {
    switch (type) {
      case "virtual":
        return <Video size={14} className="mr-1" />;
      case "phone":
        return <Phone size={14} className="mr-1" />;
      default:
        return <MapPin size={14} className="mr-1" />;
    }
  };

  return (
    <div className={`space-y-4 ${isLoading ? "opacity-50" : ""}`}>
      {/* Patient Info */}
      <div>
        <h4 className="font-medium text-gray-900 flex items-center">
          <User size={16} className="mr-2" />
          {appointment.patient
            ? `${appointment.patient.firstName} ${appointment.patient.lastName}`
            : "Unknown Patient"}
        </h4>
        <p className="text-sm text-gray-600 flex items-center mt-1">
          <Phone size={14} className="mr-2" />
          {appointment.patient?.phone || "No phone"}
        </p>
      </div>

      {/* Appointment Details */}
      <div className="border-t pt-4">
        <div className="space-y-2 text-sm">
          <div className="flex justify-between">
            <span className="text-gray-600">Time:</span>
            <span className="font-medium">
              {new Date(appointment.day).toLocaleString()}
            </span>
          </div>

          <div className="flex justify-between">
            <span className="text-gray-600">Type:</span>
            <span className="font-medium capitalize flex items-center">
              {getAppointmentTypeIcon(appointment.appointmentType)}
              {appointment.appointmentType}
            </span>
          </div>

          <div className="flex justify-between">
            <span className="text-gray-600">Duration:</span>
            <span className="font-medium">
              {appointment.durationMinutes} min
            </span>
          </div>

          <div className="flex justify-between">
            <span className="text-gray-600">Status:</span>
            <span
              className={`font-medium px-2 py-1 rounded text-xs ${getStatusColor(
                appointment.status?.name || "unknown"
              )}`}
            >
              {appointment.status?.name || "Unknown"}
            </span>
          </div>
        </div>
      </div>

      {/* Description */}
      {appointment.description && (
        <div className="border-t pt-4">
          <h5 className="font-medium text-gray-900 mb-2">Notes:</h5>
          <p className="text-sm text-gray-600">{appointment.description}</p>
        </div>
      )}

      {/* Quick Actions */}
      <div className="border-t pt-4 space-y-2">
        <h5 className="font-medium text-gray-900 mb-2">Quick Actions:</h5>

        {appointment.status?.name === "confirmed" && (
          <>
            <Button
              variant="primary"
              size="sm"
              className="w-full bg-green-600 hover:bg-green-700"
              onClick={() => onMarkAsCompleted(appointment.id)}
              disabled={isLoading}
            >
              Mark as Completed
            </Button>
            <Button
              variant="outline"
              size="sm"
              className="w-full"
              onClick={() => onMarkAsNoShow(appointment.id)}
              disabled={isLoading}
            >
              Mark as No Show
            </Button>
          </>
        )}

        <Button
          variant="outline"
          size="sm"
          className="w-full"
          onClick={handleViewPatientRecords}
          disabled={isLoading}
        >
          View Patient Records
        </Button>

        {appointment.appointmentType === "virtual" && (
          <Button
            variant="primary"
            size="sm"
            className="w-full"
            onClick={() => onStartVideoCall(appointment.id)}
            disabled={isLoading}
          >
            <Video size={16} className="mr-2" />
            Video Call Info
          </Button>
        )}

        {appointment.appointmentType === "phone" && (
          <div className="p-3 bg-blue-50 rounded-lg">
            <p className="text-sm text-blue-800">
              <Phone size={14} className="inline mr-1" />
              Phone: {appointment.patient?.phone || "No phone number"}
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

export default AppointmentDetails;
