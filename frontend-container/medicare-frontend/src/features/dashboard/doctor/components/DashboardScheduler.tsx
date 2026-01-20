import React, { useEffect, useState } from "react";
import { Calendar, Clock, FileText, Phone, User } from "lucide-react";

import { useAuth } from "../../../../shared/auth/AuthContext";
import { Card } from "../../../../shared/components";
import { DoctorScheduleModal } from "../../../scheduler/components/DoctorScheduleModal";
import type { VisitNoteData } from "../../../scheduler/components/VisitNoteModal";
import { VisitNoteModal } from "../../../scheduler/components/VisitNoteModal";
import { useDoctorSchedule } from "../../../scheduler/hooks/useDoctorSchedule";
import type { DoctorScheduleEvent } from "../../../scheduler/types/doctorScheduler";

interface DashboardSchedulerProps {
  doctorId?: string;
}

export const DashboardScheduler: React.FC<DashboardSchedulerProps> = ({
  doctorId,
}) => {
  const { user } = useAuth();
  const actualDoctorId = doctorId || user?.id || "mock-doctor-id";

  const [selectedAppointment, setSelectedAppointment] =
    useState<DoctorScheduleEvent | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isVisitNoteModalOpen, setIsVisitNoteModalOpen] = useState(false);
  const [visitNoteMode, setVisitNoteMode] = useState<"create" | "view">("create");
  const [visitNoteData, setVisitNoteData] = useState<VisitNoteData | null>(null);
  const [visitNoteLoading, setVisitNoteLoading] = useState(false);

  const {
    todaysAppointments,
    isLoading,
    error,
    markAppointmentCompleted,
    markAppointmentNoShow,
    addAppointmentNotes,
    getVisitNoteForAppointment,
    createVisitNote,
  } = useDoctorSchedule({
    doctorId: actualDoctorId,
    autoRefresh: true,
    refreshInterval: 60000, // Refresh every minute
  });

  const handleAppointmentClick = (appointment: DoctorScheduleEvent) => {
    setSelectedAppointment(appointment);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedAppointment(null);
  };

  useEffect(() => {
    const appointmentId = selectedAppointment?.id;
    if (!appointmentId) return;

    let isActive = true;

    const checkExistingVisitNote = async () => {
      const existingNote = await getVisitNoteForAppointment(appointmentId);
      if (!isActive) return;

      if (existingNote) {
        setVisitNoteMode("view");
        setVisitNoteData(existingNote);
        setSelectedAppointment((prev) =>
          prev?.id === appointmentId
            ? {
                ...prev,
                hasVisitNote: true,
                ...(existingNote.documentId
                  ? { visitNoteDocumentId: existingNote.documentId }
                  : {}),
              }
            : prev
        );
      } else {
        setVisitNoteMode("create");
        setVisitNoteData(null);
        setSelectedAppointment((prev) =>
          prev?.id === appointmentId ? { ...prev, hasVisitNote: false } : prev
        );
      }
    };

    void checkExistingVisitNote();

    return () => {
      isActive = false;
    };
  }, [getVisitNoteForAppointment, selectedAppointment?.id]);

  const handleOpenVisitNote = async (appointment: DoctorScheduleEvent) => {
    setIsModalOpen(false);
    setVisitNoteLoading(true);

    const existingNote = await getVisitNoteForAppointment(appointment.id);

    if (existingNote) {
      setVisitNoteMode("view");
      setVisitNoteData(existingNote);
      setSelectedAppointment((prev) =>
        prev ? { ...prev, hasVisitNote: true } : null
      );
    } else {
      setVisitNoteMode("create");
      setVisitNoteData(null);
    }

    setVisitNoteLoading(false);
    setIsVisitNoteModalOpen(true);
  };

  const handleCloseVisitNoteModal = () => {
    setIsVisitNoteModalOpen(false);
    if (selectedAppointment) {
      setIsModalOpen(true);
    }
  };

  const handleSaveVisitNote = async (data: VisitNoteData): Promise<void> => {
    if (!selectedAppointment) return;

    if (visitNoteMode !== "create") return;

    const result = await createVisitNote(selectedAppointment, data);
    if (result.success) {
      setSelectedAppointment((prev) =>
        prev ? { ...prev, hasVisitNote: true } : null
      );
      setVisitNoteMode("view");
      if (result.documentId) {
        setVisitNoteData({
          documentId: result.documentId,
          symptoms: data.symptoms,
          findings: data.findings,
          diagnosis: data.diagnosis,
          treatmentPlan: data.treatmentPlan,
          recommendations: data.recommendations,
          vitalSignsJson: data.vitalSignsJson,
          followUpDate: data.followUpDate,
        });
      }
    }
  };

  const handleMarkCompleted = async (
    appointmentId: string
  ): Promise<boolean> => {
    const success = await markAppointmentCompleted(appointmentId);
    if (success) {
      handleCloseModal();
    }
    return success;
  };

  const handleMarkNoShow = async (appointmentId: string): Promise<boolean> => {
    const success = await markAppointmentNoShow(appointmentId);
    if (success) {
      handleCloseModal();
    }
    return success;
  };

  const handleAddNotes = async (
    appointmentId: string,
    notes: string
  ): Promise<boolean> => {
    return await addAppointmentNotes(appointmentId, notes);
  };

  const formatTime = (time: string) => {
    const [hours, minutes] = time.split(":");
    const hour = parseInt(hours);
    const ampm = hour >= 12 ? "PM" : "AM";
    const displayHour = hour % 12 || 12;
    return `${displayHour}:${minutes} ${ampm}`;
  };

  const getTimeStatus = (appointment: DoctorScheduleEvent) => {
    // Parse date and time as local time (not UTC)
    const [year, month, day] = appointment.date.split("-").map(Number);
    const [hours, minutes] = appointment.time.split(":").map(Number);
    const appointmentTime = new Date(year, month - 1, day, hours, minutes);
    const endTime = new Date(
      appointmentTime.getTime() + appointment.duration * 60 * 1000
    );
    const now = new Date();

    if (appointment.status === "completed") return "completed";
    if (appointment.status === "no-show") return "no-show";
    if (appointment.status === "cancelled") return "cancelled";

    if (now >= appointmentTime && now <= endTime) return "current";
    if (now > endTime) return "overdue";
    return "upcoming";
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case "completed":
        return "bg-green-50 border-green-200 text-green-800";
      case "no-show":
        return "bg-red-50 border-red-200 text-red-800";
      case "cancelled":
        return "bg-gray-50 border-gray-200 text-gray-800";
      case "current":
        return "bg-orange-50 border-orange-200 text-orange-800";
      case "overdue":
        return "bg-red-50 border-red-200 text-red-800";
      default:
        return "bg-blue-50 border-blue-200 text-blue-800";
    }
  };

  const sortedAppointments = todaysAppointments.sort((a, b) =>
    a.time.localeCompare(b.time)
  );

  if (error) {
    return (
      <Card variant="medical">
        <div className="p-6 text-center text-red-600">
          <p>Error loading schedule: {error}</p>
        </div>
      </Card>
    );
  }

  return (
    <>
      <Card variant="medical">
        <div className="p-6">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-xl font-semibold text-blue-700 flex items-center">
              <Calendar className="w-5 h-5 mr-2" />
              Today's Schedule
            </h3>
            <div className="text-sm text-gray-600">
              {new Date().toLocaleDateString("en-US", {
                weekday: "long",
                year: "numeric",
                month: "long",
                day: "numeric",
              })}
            </div>
          </div>

          {isLoading && todaysAppointments.length === 0 ? (
            <div className="text-center py-8 text-gray-500">
              <Clock className="w-8 h-8 mx-auto mb-2 animate-spin" />
              <p>Loading today's schedule...</p>
            </div>
          ) : sortedAppointments.length === 0 ? (
            <div className="text-center py-8 text-gray-500">
              <Calendar className="w-12 h-12 mx-auto mb-4 opacity-50" />
              <p className="text-lg font-medium">No appointments today</p>
              <p className="text-sm">Enjoy your free day!</p>
            </div>
          ) : (
            <div className="space-y-3">
              {sortedAppointments.map((appointment) => {
                const timeStatus = getTimeStatus(appointment);
                const statusColorClass = getStatusColor(timeStatus);

                return (
                  <div
                    key={appointment.id}
                    onClick={() => handleAppointmentClick(appointment)}
                    className={`relative flex items-center p-4 rounded-lg border-2 cursor-pointer transition-all hover:shadow-md ${statusColorClass}`}
                  >
                    {/* Time indicator */}
                    <div className="flex-shrink-0 w-20 text-center">
                      <div className="text-lg font-bold">
                        {formatTime(appointment.time)}
                      </div>
                      <div className="text-xs opacity-75">
                        {appointment.duration}min
                      </div>
                    </div>

                    {/* Appointment details */}
                    <div className="flex-1 ml-4">
                      <div className="flex items-start justify-between">
                        <div className="flex-1">
                          <div className="flex items-center space-x-2 mb-1">
                            <User className="w-4 h-4" />
                            <span className="font-semibold text-gray-900">
                              {appointment.patientName}
                            </span>
                            <span className="text-sm text-gray-600">
                              ({appointment.patientAge}y)
                            </span>
                          </div>

                          <div className="flex items-center space-x-2 mb-1">
                            <FileText className="w-4 h-4" />
                            <span className="text-sm font-medium">
                              {appointment.appointmentType}
                            </span>
                          </div>

                          {appointment.chiefComplaint && (
                            <div className="text-sm text-gray-700 mt-1">
                              <span className="font-medium">
                                Chief Complaint:
                              </span>{" "}
                              {appointment.chiefComplaint}
                            </div>
                          )}

                          <div className="flex items-center space-x-2 mt-1">
                            <Phone className="w-3 h-3" />
                            <span className="text-xs text-gray-600">
                              {appointment.patientPhone}
                            </span>
                          </div>
                        </div>

                        {/* Status badge */}
                        <div className="flex-shrink-0 ml-4">
                          {timeStatus === "current" && (
                            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-orange-100 text-orange-800 animate-pulse">
                              • Current
                            </span>
                          )}
                          {timeStatus === "overdue" &&
                            appointment.status === "scheduled" && (
                              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
                                Overdue
                              </span>
                            )}
                          {appointment.status === "completed" && (
                            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                              ✓ Completed
                            </span>
                          )}
                          {appointment.status === "no-show" && (
                            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
                              No Show
                            </span>
                          )}
                        </div>
                      </div>
                    </div>

                    {/* Click indicator */}
                    <div className="flex-shrink-0 ml-2 text-gray-400">
                      <svg
                        className="w-4 h-4"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          strokeWidth={2}
                          d="M9 5l7 7-7 7"
                        />
                      </svg>
                    </div>
                  </div>
                );
              })}
            </div>
          )}

          {/* Summary stats */}
          {sortedAppointments.length > 0 && (
            <div className="mt-6 pt-4 border-t border-gray-200">
              <div className="grid grid-cols-4 gap-4 text-center text-sm">
                <div>
                  <div className="font-semibold text-gray-900">
                    {sortedAppointments.length}
                  </div>
                  <div className="text-gray-600">Total</div>
                </div>
                <div>
                  <div className="font-semibold text-green-600">
                    {
                      sortedAppointments.filter(
                        (apt) => apt.status === "completed"
                      ).length
                    }
                  </div>
                  <div className="text-gray-600">Completed</div>
                </div>
                <div>
                  <div className="font-semibold text-blue-600">
                    {
                      sortedAppointments.filter(
                        (apt) => apt.status === "scheduled"
                      ).length
                    }
                  </div>
                  <div className="text-gray-600">Scheduled</div>
                </div>
                <div>
                  <div className="font-semibold text-red-600">
                    {
                      sortedAppointments.filter(
                        (apt) => apt.status === "no-show"
                      ).length
                    }
                  </div>
                  <div className="text-gray-600">No-Show</div>
                </div>
              </div>
            </div>
          )}
        </div>
      </Card>

      {/* Appointment Details Modal */}
      <DoctorScheduleModal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        appointment={selectedAppointment}
        onMarkCompleted={handleMarkCompleted}
        onMarkNoShow={handleMarkNoShow}
        onAddNotes={handleAddNotes}
        onOpenVisitNote={handleOpenVisitNote}
      />

      {visitNoteLoading ? (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white p-4 rounded-lg">Loading visit note...</div>
        </div>
      ) : (
        <VisitNoteModal
          isOpen={isVisitNoteModalOpen}
          onClose={handleCloseVisitNoteModal}
          onSave={handleSaveVisitNote}
          appointmentId={selectedAppointment?.id || ""}
          patientName={selectedAppointment?.patientName || "Patient"}
          appointmentDate={
            selectedAppointment
              ? `${selectedAppointment.date}T${selectedAppointment.time}`
              : new Date().toISOString()
          }
          isEditMode={false}
          isReadOnly={visitNoteMode === "view"}
          existingVisitNote={visitNoteData}
        />
      )}
    </>
  );
};
