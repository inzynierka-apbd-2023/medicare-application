import React from "react";
import type { TodayAppointment } from "@features/appointments/types";
import { Badge, Modal } from "@shared/components";
import { Calendar, Clock, FileText, Phone, User } from "lucide-react";

interface TodayAppointmentDetailsModalProps {
  isOpen: boolean;
  appointment: TodayAppointment | null;
  onClose: () => void;
}

const TodayAppointmentDetailsModal: React.FC<
  TodayAppointmentDetailsModalProps
> = ({ isOpen, appointment, onClose }) => {
  if (!appointment) return null;

  const getStatusBadge = () => {
    switch (appointment.status) {
      case "scheduled":
        return <Badge variant="info">Scheduled</Badge>;
      case "completed":
        return <Badge variant="success">Completed</Badge>;
      case "no-show":
        return <Badge variant="error">No Show</Badge>;
      case "cancelled":
        return <Badge variant="error">Cancelled</Badge>;
      default:
        return <Badge variant="default">{appointment.status}</Badge>;
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
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Appointment Details"
      size="lg"
    >
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-start justify-between">
          <div>
            <h2 className="text-xl font-semibold text-gray-900">
              {appointment.patient.name}
            </h2>
            <p className="text-gray-600">
              {appointment.appointmentType} • Age {appointment.patient.age}
            </p>
          </div>
          {getStatusBadge()}
        </div>

        {/* Basic Information */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-3">
            <div className="flex items-center space-x-2">
              <Calendar className="h-5 w-5 text-blue-600" />
              <div>
                <p className="text-sm font-medium text-gray-700">Date</p>
                <p className="text-gray-900">
                  {new Date(appointment.date).toLocaleDateString()}
                </p>
              </div>
            </div>

            <div className="flex items-center space-x-2">
              <Clock className="h-5 w-5 text-blue-600" />
              <div>
                <p className="text-sm font-medium text-gray-700">Time</p>
                <p className="text-gray-900">
                  {formatTime(appointment.time)} - {getEndTime()} (
                  {appointment.duration} min)
                </p>
              </div>
            </div>
          </div>

          <div className="space-y-3">
            <div className="flex items-center space-x-2">
              <Phone className="h-5 w-5 text-blue-600" />
              <div>
                <p className="text-sm font-medium text-gray-700">Phone</p>
                <p className="text-gray-900">{appointment.patient.phone}</p>
              </div>
            </div>

            {appointment.patient.email && (
              <div className="flex items-center space-x-2">
                <User className="h-5 w-5 text-blue-600" />
                <div>
                  <p className="text-sm font-medium text-gray-700">Email</p>
                  <p className="text-gray-900">{appointment.patient.email}</p>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Chief Complaint */}
        {appointment.chiefComplaint && (
          <div className="bg-blue-50 rounded-lg p-4">
            <div className="flex items-start space-x-2">
              <FileText className="h-5 w-5 text-blue-600 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-blue-700 mb-2">
                  Chief Complaint
                </p>
                <p className="text-blue-800">{appointment.chiefComplaint}</p>
              </div>
            </div>
          </div>
        )}

        {/* Medical Information */}
        <div className="space-y-4">
          <h3 className="text-lg font-medium text-gray-900">
            Medical Information
          </h3>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* Medical History */}
            <div className="bg-gray-50 rounded-lg p-4">
              <h4 className="font-medium text-gray-700 mb-2">
                Medical History
              </h4>
              {appointment.patient.medicalHistory &&
              appointment.patient.medicalHistory.length > 0 ? (
                <ul className="space-y-1">
                  {appointment.patient.medicalHistory.map(
                    (condition, index) => (
                      <li key={index} className="text-sm text-gray-600">
                        • {condition}
                      </li>
                    )
                  )}
                </ul>
              ) : (
                <p className="text-sm text-gray-500 italic">
                  No medical history recorded
                </p>
              )}
            </div>

            {/* Allergies */}
            <div className="bg-red-50 rounded-lg p-4">
              <h4 className="font-medium text-red-700 mb-2">Allergies</h4>
              {appointment.patient.allergies &&
              appointment.patient.allergies.length > 0 ? (
                <ul className="space-y-1">
                  {appointment.patient.allergies.map((allergy, index) => (
                    <li key={index} className="text-sm text-red-600">
                      • {allergy}
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="text-sm text-red-500 italic">
                  No known allergies
                </p>
              )}
            </div>

            {/* Current Medications */}
            <div className="bg-green-50 rounded-lg p-4">
              <h4 className="font-medium text-green-700 mb-2">
                Current Medications
              </h4>
              {appointment.patient.currentMedications &&
              appointment.patient.currentMedications.length > 0 ? (
                <ul className="space-y-1">
                  {appointment.patient.currentMedications.map(
                    (medication, index) => (
                      <li key={index} className="text-sm text-green-600">
                        • {medication}
                      </li>
                    )
                  )}
                </ul>
              ) : (
                <p className="text-sm text-green-500 italic">
                  No current medications
                </p>
              )}
            </div>
          </div>
        </div>

        {/* Description */}
        {appointment.description && (
          <div className="bg-gray-50 rounded-lg p-4">
            <h4 className="font-medium text-gray-700 mb-2">
              Appointment Description
            </h4>
            <p className="text-sm text-gray-600">{appointment.description}</p>
          </div>
        )}

        {/* Notes (for completed appointments) */}
        {appointment.notes && (
          <div className="bg-green-50 rounded-lg p-4">
            <h4 className="font-medium text-green-700 mb-2">Notes</h4>
            <p className="text-sm text-green-600">{appointment.notes}</p>
          </div>
        )}
      </div>
    </Modal>
  );
};

export { TodayAppointmentDetailsModal };
