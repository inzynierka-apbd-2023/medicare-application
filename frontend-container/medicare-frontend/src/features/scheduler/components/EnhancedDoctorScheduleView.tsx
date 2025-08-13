/**
 * Enhanced DoctorScheduleView with Patient Record Integration
 *
 * This component integrates the doctor's schedule view with patient management,
 * allowing doctors to click on appointments to view patient records.
 */

import React, { useCallback, useRef, useState } from "react";
import dayGridPlugin from "@fullcalendar/daygrid";
import FullCalendar from "@fullcalendar/react";
import timeGridPlugin from "@fullcalendar/timegrid";
import { Calendar, Clock, FileText, User, X } from "lucide-react";

import Header from "../../../layout/Header";
import { Button, Card, LoadingOverlay } from "../../../shared/components";
import PatientManagementView from "../../patients/components/PatientManagementView";
import type { DoctorCalendarEvent } from "../types";

// Modal Component for Patient Details
interface PatientDetailModalProps {
  isOpen: boolean;
  onClose: () => void;
  patientId: string | null;
}

const PatientDetailModal: React.FC<PatientDetailModalProps> = ({
  isOpen,
  onClose,
  patientId,
}) => {
  if (!isOpen || !patientId) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg max-w-6xl w-full max-h-[90vh] overflow-hidden">
        <div className="flex items-center justify-between p-4 border-b">
          <h2 className="text-xl font-bold text-gray-900">Patient Record</h2>
          <Button variant="outline" size="sm" onClick={onClose}>
            <X size={16} />
          </Button>
        </div>
        <div className="overflow-y-auto max-h-[calc(90vh-80px)]">
          <PatientManagementView selectedPatientId={patientId} />
        </div>
      </div>
    </div>
  );
};

// Quick Patient Info Card
interface QuickPatientInfoProps {
  patientId: string;
  patientName: string;
  appointmentTime: string;
  appointmentType: string;
  status: string;
  onViewFull: () => void;
}

const QuickPatientInfo: React.FC<QuickPatientInfoProps> = ({
  patientName,
  appointmentTime,
  appointmentType,
  status,
  onViewFull,
}) => {
  return (
    <Card className="mb-4">
      <div className="p-4">
        <div className="flex items-center justify-between mb-3">
          <h3 className="text-lg font-semibold text-gray-900 flex items-center">
            <User size={20} className="mr-2" />
            {patientName}
          </h3>
          <div className="flex gap-2">
            <Button variant="outline" size="sm" onClick={onViewFull}>
              <FileText size={16} className="mr-1" />
              Full Record
            </Button>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4 text-sm">
          <div>
            <span className="text-gray-600">Time:</span>
            <span className="ml-2 font-medium">{appointmentTime}</span>
          </div>
          <div>
            <span className="text-gray-600">Type:</span>
            <span className="ml-2 font-medium">{appointmentType}</span>
          </div>
          <div>
            <span className="text-gray-600">Status:</span>
            <span
              className={`ml-2 px-2 py-1 rounded-full text-xs font-medium ${
                status === "confirmed"
                  ? "bg-green-100 text-green-800"
                  : status === "pending"
                    ? "bg-yellow-100 text-yellow-800"
                    : "bg-red-100 text-red-800"
              }`}
            >
              {status.charAt(0).toUpperCase() + status.slice(1)}
            </span>
          </div>
        </div>
      </div>
    </Card>
  );
};

export const EnhancedDoctorScheduleView: React.FC = () => {
  const calendarRef = useRef<FullCalendar>(null);
  const [selectedPatientId, setSelectedPatientId] = useState<string | null>(
    null
  );
  const [selectedAppointment, setSelectedAppointment] =
    useState<DoctorCalendarEvent | null>(null);
  const [showPatientModal, setShowPatientModal] = useState(false);
  const [isLoading, _setIsLoading] = useState(false);

  // Generate mock appointments with patient data
  const generateMockAppointments = (): DoctorCalendarEvent[] => {
    const appointments: DoctorCalendarEvent[] = [];
    const patientNames = [
      "Jan Kowalski",
      "Anna Nowak",
      "Piotr Wiśniewski",
      "Maria Wójcik",
      "Tomasz Kowalczyk",
      "Katarzyna Kamińska",
      "Michał Lewandowski",
      "Agnieszka Zielińska",
      "Krzysztof Szymański",
      "Magdalena Woźniak",
    ];

    const appointmentTypes = [
      "Regular Checkup",
      "Follow-up",
      "Consultation",
      "Emergency",
      "Diagnostic",
      "Treatment",
      "Vaccination",
      "Physical Therapy",
    ];

    const statuses = ["confirmed", "pending", "cancelled"];

    // Generate appointments for the next 7 days
    for (let day = 0; day < 7; day++) {
      const numAppointments = Math.floor(Math.random() * 6) + 2; // 2-7 appointments per day

      for (let i = 0; i < numAppointments; i++) {
        const appointmentDate = new Date();
        appointmentDate.setDate(appointmentDate.getDate() + day);
        appointmentDate.setHours(
          8 + Math.floor(Math.random() * 10),
          Math.random() < 0.5 ? 0 : 30
        );

        const endDate = new Date(appointmentDate);
        endDate.setMinutes(
          endDate.getMinutes() + (Math.random() < 0.7 ? 30 : 60)
        );

        const patientName =
          patientNames[Math.floor(Math.random() * patientNames.length)];
        const appointmentType =
          appointmentTypes[Math.floor(Math.random() * appointmentTypes.length)];
        const status = statuses[Math.floor(Math.random() * statuses.length)];

        appointments.push({
          id: `appointment-${day}-${i}`,
          title: `${patientName} - ${appointmentType}`,
          start: appointmentDate.toISOString(),
          end: endDate.toISOString(),
          backgroundColor:
            status === "confirmed"
              ? "#10B981"
              : status === "pending"
                ? "#F59E0B"
                : "#EF4444",
          borderColor:
            status === "confirmed"
              ? "#059669"
              : status === "pending"
                ? "#D97706"
                : "#DC2626",
          extendedProps: {
            patientId: `patient-${Math.floor(Math.random() * 50) + 1}`,
            patientName,
            appointmentType,
            status,
            duration: endDate.getTime() - appointmentDate.getTime(),
            notes: `Appointment with ${patientName} for ${appointmentType.toLowerCase()}`,
          },
        });
      }
    }

    return appointments.sort(
      (a, b) => new Date(a.start).getTime() - new Date(b.start).getTime()
    );
  };

  const [events] = useState<DoctorCalendarEvent[]>(generateMockAppointments());

  const handleViewChange = useCallback((view: string) => {
    const calendarApi = calendarRef.current?.getApi();
    if (calendarApi) {
      calendarApi.changeView(view);
    }
  }, []);

  const handleEventClick = useCallback(
    (clickInfo: import("@fullcalendar/core").EventClickArg) => {
      const event = clickInfo.event;
      const appointment: DoctorCalendarEvent = {
        id: event.id,
        title: event.title,
        start: event.start?.toISOString() || "",
        end: event.end?.toISOString() || event.start?.toISOString() || "",
        backgroundColor: event.backgroundColor || "",
        borderColor: event.borderColor || "",
        extendedProps: {
          patientId: event.extendedProps.patientId || "",
          patientName: event.extendedProps.patientName || "",
          appointmentType: event.extendedProps.appointmentType || "",
          status: event.extendedProps.status || "",
          duration: event.extendedProps.duration || 0,
          notes: event.extendedProps.notes,
        },
      };

      setSelectedAppointment(appointment);
      setSelectedPatientId(event.extendedProps.patientId);
    },
    []
  );

  const handleViewPatientRecord = useCallback(() => {
    if (selectedPatientId) {
      setShowPatientModal(true);
    }
  }, [selectedPatientId]);

  const handleCloseModal = useCallback(() => {
    setShowPatientModal(false);
  }, []);

  const navigation = [
    { label: "Day", view: "timeGridDay" },
    { label: "Week", view: "timeGridWeek" },
    { label: "Month", view: "dayGridMonth" },
  ];

  return (
    <div className="min-h-screen bg-gray-100 pt-16">
      <Header />

      <LoadingOverlay isLoading={isLoading}>
        <div className="max-w-7xl mx-auto px-4 py-8">
          {/* Header */}
          <div className="flex flex-col md:flex-row md:justify-between md:items-center mb-6">
            <div>
              <h1 className="text-2xl font-bold text-gray-900 flex items-center">
                <Calendar className="mr-2" />
                Doctor Schedule - Patient Timeline
              </h1>
              <p className="text-gray-600 mt-1">
                View your appointment schedule and access patient records
              </p>
            </div>

            <div className="flex gap-2 mt-4 md:mt-0">
              {navigation.map((nav) => (
                <Button
                  key={nav.view}
                  variant="outline"
                  size="sm"
                  onClick={() => handleViewChange(nav.view)}
                >
                  {nav.label}
                </Button>
              ))}
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
            {/* Calendar */}
            <div className="lg:col-span-3">
              <Card>
                <div className="p-6">
                  <FullCalendar
                    ref={calendarRef}
                    plugins={[dayGridPlugin, timeGridPlugin]}
                    initialView="timeGridWeek"
                    headerToolbar={{
                      left: "prev,next today",
                      center: "title",
                      right: "",
                    }}
                    events={events}
                    editable={false}
                    selectable={true}
                    selectMirror={true}
                    dayMaxEvents={true}
                    weekends={true}
                    eventClick={handleEventClick}
                    height="600px"
                    slotMinTime="07:00:00"
                    slotMaxTime="19:00:00"
                    businessHours={{
                      daysOfWeek: [1, 2, 3, 4, 5],
                      startTime: "08:00",
                      endTime: "17:00",
                    }}
                    eventTimeFormat={{
                      hour: "numeric",
                      minute: "2-digit",
                      meridiem: false,
                    }}
                  />
                </div>
              </Card>
            </div>

            {/* Patient Info Sidebar */}
            <div className="lg:col-span-1">
              <div className="space-y-4">
                {/* Selected Appointment Info */}
                {selectedAppointment ? (
                  <QuickPatientInfo
                    patientId={selectedAppointment.extendedProps.patientId}
                    patientName={selectedAppointment.extendedProps.patientName}
                    appointmentTime={new Date(
                      selectedAppointment.start
                    ).toLocaleString()}
                    appointmentType={
                      selectedAppointment.extendedProps.appointmentType
                    }
                    status={selectedAppointment.extendedProps.status}
                    onViewFull={handleViewPatientRecord}
                  />
                ) : (
                  <Card>
                    <div className="p-4 text-center text-gray-500">
                      <Calendar size={48} className="mx-auto mb-4 opacity-50" />
                      <p>Click on an appointment to view patient details</p>
                    </div>
                  </Card>
                )}

                {/* Today's Appointments Summary */}
                <Card>
                  <div className="p-4">
                    <h3 className="text-lg font-semibold mb-3 flex items-center">
                      <Clock size={20} className="mr-2" />
                      Today's Summary
                    </h3>
                    <div className="space-y-2 text-sm">
                      <div className="flex justify-between">
                        <span className="text-gray-600">
                          Total Appointments:
                        </span>
                        <span className="font-medium">
                          {
                            events.filter((event) => {
                              const eventDate = new Date(event.start);
                              const today = new Date();
                              return (
                                eventDate.toDateString() ===
                                today.toDateString()
                              );
                            }).length
                          }
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Confirmed:</span>
                        <span className="font-medium text-green-600">
                          {
                            events.filter((event) => {
                              const eventDate = new Date(event.start);
                              const today = new Date();
                              return (
                                eventDate.toDateString() ===
                                  today.toDateString() &&
                                event.extendedProps.status === "confirmed"
                              );
                            }).length
                          }
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Pending:</span>
                        <span className="font-medium text-yellow-600">
                          {
                            events.filter((event) => {
                              const eventDate = new Date(event.start);
                              const today = new Date();
                              return (
                                eventDate.toDateString() ===
                                  today.toDateString() &&
                                event.extendedProps.status === "pending"
                              );
                            }).length
                          }
                        </span>
                      </div>
                    </div>
                  </div>
                </Card>

                {/* Quick Actions */}
                <Card>
                  <div className="p-4">
                    <h3 className="text-lg font-semibold mb-3">
                      Quick Actions
                    </h3>
                    <div className="space-y-2">
                      <Button
                        variant="outline"
                        className="w-full justify-start"
                        size="sm"
                      >
                        <User size={16} className="mr-2" />
                        View All Patients
                      </Button>
                      <Button
                        variant="outline"
                        className="w-full justify-start"
                        size="sm"
                      >
                        <FileText size={16} className="mr-2" />
                        Medical Records
                      </Button>
                      <Button
                        variant="outline"
                        className="w-full justify-start"
                        size="sm"
                      >
                        <Clock size={16} className="mr-2" />
                        Schedule Appointment
                      </Button>
                    </div>
                  </div>
                </Card>
              </div>
            </div>
          </div>
        </div>
      </LoadingOverlay>

      {/* Patient Detail Modal */}
      <PatientDetailModal
        isOpen={showPatientModal}
        onClose={handleCloseModal}
        patientId={selectedPatientId}
      />
    </div>
  );
};

export default EnhancedDoctorScheduleView;
