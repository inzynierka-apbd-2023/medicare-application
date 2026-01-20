import React, { useEffect, useState } from "react";
import {
  AlertTriangle,
  Calendar,
  CheckCircle,
  Clock,
  Edit,
  FileText,
  Mail,
  Phone,
  Pill,
  PlusCircle,
  Save,
  User,
  XCircle,
} from "lucide-react";

import { Badge, Button, Modal } from "../../../shared/components";
import type { DoctorScheduleModalProps } from "../types/doctorScheduler";

export const DoctorScheduleModal: React.FC<DoctorScheduleModalProps> = ({
  isOpen,
  onClose,
  appointment,
  onMarkCompleted,
  onMarkNoShow,
  onAddNotes,
  onOpenVisitNote,
}) => {
  const [notes, setNotes] = useState(appointment?.notes || "");
  const [isEditingNotes, setIsEditingNotes] = useState(false);

  useEffect(() => {
    setNotes(appointment?.notes || "");
    setIsEditingNotes(false);
  }, [appointment]);

  if (!appointment) return null;

  const handleSaveNotes = async () => {
    if (onAddNotes && notes !== appointment.notes) {
      const success = await onAddNotes(appointment.id, notes);
      if (success) {
        setIsEditingNotes(false);
      }
    } else {
      setIsEditingNotes(false);
    }
  };

  const handleMarkCompleted = async () => {
    if (onMarkCompleted) {
      const success = await onMarkCompleted(appointment.id);
      if (success) {
        onClose();
      }
    }
  };

  const handleMarkNoShow = async () => {
    if (onMarkNoShow) {
      const success = await onMarkNoShow(appointment.id);
      if (success) {
        onClose();
      }
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case "completed":
        return "green";
      case "no-show":
        return "red";
      case "cancelled":
        return "gray";
      default:
        return "blue";
    }
  };

  const formatTime = (time: string) => {
    const [hours, minutes] = time.split(":");
    const hour = parseInt(hours);
    const ampm = hour >= 12 ? "PM" : "AM";
    const displayHour = hour % 12 || 12;
    return `${displayHour}:${minutes} ${ampm}`;
  };

  const appointmentDateTime = new Date(
    `${appointment.date}T${appointment.time}`
  );
  const endTime = new Date(
    appointmentDateTime.getTime() + appointment.duration * 60 * 1000
  );
  const isCompleted = appointment.status === "completed";
  const isNoShow = appointment.status === "no-show";
  const canModifyStatus =
    !isCompleted && !isNoShow && appointment.status !== "cancelled";

  // Check if appointment is in the past (eligible for visit note)
  const now = new Date();
  const isAppointmentPast = now > appointmentDateTime;
  const canCreateOrEditVisitNote =
    isAppointmentPast && appointment.status !== "cancelled";

  const handleOpenVisitNote = () => {
    if (onOpenVisitNote) {
      onOpenVisitNote(appointment);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Appointment Details"
      size="lg"
    >
      <div className="space-y-6">
        {/* Header with patient info and status */}
        <div className="flex items-start justify-between">
          <div className="flex items-start space-x-4">
            <div className="bg-blue-100 p-3 rounded-full">
              <User className="w-6 h-6 text-blue-600" />
            </div>
            <div>
              <h3 className="text-xl font-semibold text-gray-900">
                {appointment.patientName}
              </h3>
              <p className="text-gray-600">Age {appointment.patientAge}</p>
            </div>
          </div>
          <Badge color={getStatusColor(appointment.status)} size="lg">
            {appointment.status.charAt(0).toUpperCase() +
              appointment.status.slice(1)}
          </Badge>
        </div>

        {/* Appointment details */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="space-y-4">
            <div className="flex items-center space-x-3">
              <Calendar className="w-5 h-5 text-gray-500" />
              <div>
                <p className="font-medium text-gray-900">Date & Time</p>
                <p className="text-gray-600">
                  {appointmentDateTime.toLocaleDateString()} at{" "}
                  {formatTime(appointment.time)}
                </p>
              </div>
            </div>

            <div className="flex items-center space-x-3">
              <Clock className="w-5 h-5 text-gray-500" />
              <div>
                <p className="font-medium text-gray-900">Duration</p>
                <p className="text-gray-600">
                  {appointment.duration} minutes (until{" "}
                  {formatTime(endTime.toTimeString().slice(0, 5))})
                </p>
              </div>
            </div>

            <div className="flex items-center space-x-3">
              <FileText className="w-5 h-5 text-gray-500" />
              <div>
                <p className="font-medium text-gray-900">Appointment Type</p>
                <p className="text-gray-600">{appointment.appointmentType}</p>
              </div>
            </div>
          </div>

          <div className="space-y-4">
            <div className="flex items-center space-x-3">
              <Phone className="w-5 h-5 text-gray-500" />
              <div>
                <p className="font-medium text-gray-900">Phone</p>
                <p className="text-gray-600">{appointment.patientPhone}</p>
              </div>
            </div>

            {appointment.patientEmail && (
              <div className="flex items-center space-x-3">
                <Mail className="w-5 h-5 text-gray-500" />
                <div>
                  <p className="font-medium text-gray-900">Email</p>
                  <p className="text-gray-600">{appointment.patientEmail}</p>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Chief Complaint */}
        {appointment.chiefComplaint && (
          <div>
            <h4 className="text-lg font-medium text-gray-900 mb-2">
              Chief Complaint
            </h4>
            <p className="text-gray-700 bg-gray-50 p-3 rounded-lg">
              {appointment.chiefComplaint}
            </p>
          </div>
        )}

        {/* Medical Information */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {appointment.medicalHistory &&
            appointment.medicalHistory.length > 0 && (
              <div>
                <h5 className="font-medium text-gray-900 mb-2 flex items-center">
                  <FileText className="w-4 h-4 mr-1" />
                  Medical History
                </h5>
                <ul className="space-y-1">
                  {appointment.medicalHistory.map((item, index) => (
                    <li
                      key={index}
                      className="text-sm text-gray-600 bg-blue-50 px-2 py-1 rounded"
                    >
                      {item}
                    </li>
                  ))}
                </ul>
              </div>
            )}

          {appointment.allergies && appointment.allergies.length > 0 && (
            <div>
              <h5 className="font-medium text-gray-900 mb-2 flex items-center">
                <AlertTriangle className="w-4 h-4 mr-1 text-red-500" />
                Allergies
              </h5>
              <ul className="space-y-1">
                {appointment.allergies.map((allergy, index) => (
                  <li
                    key={index}
                    className="text-sm text-gray-600 bg-red-50 px-2 py-1 rounded"
                  >
                    {allergy}
                  </li>
                ))}
              </ul>
            </div>
          )}

          {appointment.currentMedications &&
            appointment.currentMedications.length > 0 && (
              <div>
                <h5 className="font-medium text-gray-900 mb-2 flex items-center">
                  <Pill className="w-4 h-4 mr-1" />
                  Current Medications
                </h5>
                <ul className="space-y-1">
                  {appointment.currentMedications.map((medication, index) => (
                    <li
                      key={index}
                      className="text-sm text-gray-600 bg-green-50 px-2 py-1 rounded"
                    >
                      {medication}
                    </li>
                  ))}
                </ul>
              </div>
            )}
        </div>

        {/* Notes Section */}
        <div>
          <div className="flex items-center justify-between mb-2">
            <h4 className="text-lg font-medium text-gray-900">Notes</h4>
            {!isEditingNotes && (
              <Button
                variant="outline"
                size="sm"
                onClick={() => setIsEditingNotes(true)}
              >
                {notes ? "Edit Notes" : "Add Notes"}
              </Button>
            )}
          </div>

          {isEditingNotes ? (
            <div className="space-y-3">
              <textarea
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                rows={4}
                placeholder="Add your notes about this appointment..."
              />
              <div className="flex space-x-2">
                <Button onClick={handleSaveNotes} className="flex items-center">
                  <Save className="w-4 h-4 mr-1" />
                  Save Notes
                </Button>
                <Button
                  variant="outline"
                  onClick={() => {
                    setNotes(appointment.notes || "");
                    setIsEditingNotes(false);
                  }}
                >
                  Cancel
                </Button>
              </div>
            </div>
          ) : (
            <div className="bg-gray-50 p-3 rounded-lg min-h-[100px]">
              {notes ? (
                <p className="text-gray-700 whitespace-pre-wrap">{notes}</p>
              ) : (
                <p className="text-gray-500 italic">No notes added yet</p>
              )}
            </div>
          )}
        </div>

        {/* Action Buttons */}
        {canModifyStatus && (
          <div className="flex space-x-3 pt-4 border-t">
            <Button
              onClick={handleMarkCompleted}
              className="flex items-center bg-green-600 hover:bg-green-700"
            >
              <CheckCircle className="w-4 h-4 mr-2" />
              Mark as Completed
            </Button>
            <Button
              onClick={handleMarkNoShow}
              variant="outline"
              className="flex items-center border-red-300 text-red-600 hover:bg-red-50"
            >
              <XCircle className="w-4 h-4 mr-2" />
              Mark as No-Show
            </Button>
          </div>
        )}

        {/* Visit Note Button - Only show for past appointments */}
        {canCreateOrEditVisitNote && onOpenVisitNote && (
          <div className="pt-4 border-t">
            {appointment.hasVisitNote ? (
              <Button
                onClick={handleOpenVisitNote}
                className="flex items-center w-full justify-center bg-amber-600 hover:bg-amber-700"
              >
                <Edit className="w-4 h-4 mr-2" />
                View Visit Note
              </Button>
            ) : (
              <Button
                onClick={handleOpenVisitNote}
                className="flex items-center w-full justify-center bg-blue-600 hover:bg-blue-700"
              >
                <PlusCircle className="w-4 h-4 mr-2" />
                Generate Visit Note
              </Button>
            )}
          </div>
        )}
      </div>
    </Modal>
  );
};
