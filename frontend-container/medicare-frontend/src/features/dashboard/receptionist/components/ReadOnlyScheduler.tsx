import React, { useCallback, useState } from "react";
import type { EventClickArg } from "@fullcalendar/core";
import dayGridPlugin from "@fullcalendar/daygrid";
import FullCalendar from "@fullcalendar/react";
import timeGridPlugin from "@fullcalendar/timegrid";
import { Calendar, Stethoscope, User } from "lucide-react";

import { Card, Input, LoadingOverlay } from "../../../../shared/components";
import { useReadOnlyScheduler } from "../hooks/useReadOnlyScheduler";
import type { CalendarEvent, Doctor, Patient } from "../types";

interface ReadOnlySchedulerProps {
  className?: string;
}

export const ReadOnlyScheduler: React.FC<ReadOnlySchedulerProps> = ({
  className = "",
}) => {
  const [patientSearchTerm, setPatientSearchTerm] = useState("");
  const [doctorSearchTerm, setDoctorSearchTerm] = useState("");
  const [showPatientDropdown, setShowPatientDropdown] = useState(false);
  const [showDoctorDropdown, setShowDoctorDropdown] = useState(false);
  const [selectedPatient, setSelectedPatient] = useState<Patient | null>(null);
  const [selectedDoctor, setSelectedDoctor] = useState<Doctor | null>(null);

  const {
    calendarEvents,
    patientSearchResults,
    doctorSearchResults,
    isLoading,
    searchPatients,
    searchDoctors,
  } = useReadOnlyScheduler({
    patientFilter: selectedPatient?.id,
    doctorFilter: selectedDoctor?.id,
  });

  // Patient search handler
  const handlePatientSearch = useCallback(
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      const query = e.target.value;
      setPatientSearchTerm(query);

      if (query.trim().length >= 3) {
        try {
          await searchPatients(query);
          setShowPatientDropdown(true);
        } catch (error) {
          console.error("Error searching patients:", error);
          setShowPatientDropdown(false);
        }
      } else {
        setShowPatientDropdown(false);
      }
    },
    [searchPatients]
  );

  // Doctor search handler
  const handleDoctorSearch = useCallback(
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      const query = e.target.value;
      setDoctorSearchTerm(query);

      if (query.trim().length >= 3) {
        try {
          await searchDoctors(query);
          setShowDoctorDropdown(true);
        } catch (error) {
          console.error("Error searching doctors:", error);
          setShowDoctorDropdown(false);
        }
      } else {
        setShowDoctorDropdown(false);
      }
    },
    [searchDoctors]
  );

  const handlePatientSelect = useCallback((patient: Patient) => {
    setSelectedPatient(patient);
    setPatientSearchTerm(`${patient.firstName} ${patient.lastName}`);
    setShowPatientDropdown(false);
  }, []);

  const handleDoctorSelect = useCallback((doctor: Doctor) => {
    setSelectedDoctor(doctor);
    setDoctorSearchTerm(`${doctor.firstName} ${doctor.lastName}`);
    setShowDoctorDropdown(false);
  }, []);

  const handleClearPatientSearch = useCallback(() => {
    setSelectedPatient(null);
    setPatientSearchTerm("");
    setShowPatientDropdown(false);
  }, []);

  const handleClearDoctorSearch = useCallback(() => {
    setSelectedDoctor(null);
    setDoctorSearchTerm("");
    setShowDoctorDropdown(false);
  }, []);

  const handleEventClick = useCallback((clickInfo: EventClickArg) => {
    // Read-only mode - just show event details in console for now
    console.log("Appointment clicked:", clickInfo.event.title);
  }, []);

  const filteredEvents = calendarEvents.filter((event: CalendarEvent) => {
    if (
      selectedPatient &&
      !event.title.includes(
        `${selectedPatient.firstName} ${selectedPatient.lastName}`
      )
    ) {
      return false;
    }
    if (
      selectedDoctor &&
      !event.title.includes(
        `${selectedDoctor.firstName} ${selectedDoctor.lastName}`
      )
    ) {
      return false;
    }
    return true;
  });

  return (
    <Card className={`p-6 ${className}`}>
      <div className="flex items-center gap-2 mb-6">
        <Calendar className="h-5 w-5 text-blue-600" />
        <h3 className="text-lg font-semibold text-gray-900">
          Schedule Overview
        </h3>
      </div>

      {/* Search Filters */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
        {/* Patient Search */}
        <div className="relative">
          <div className="relative">
            <User className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
            <Input
              type="text"
              placeholder="Search patients (min 3 chars)..."
              value={patientSearchTerm}
              onChange={handlePatientSearch}
              className="pl-10"
            />
            {selectedPatient && (
              <button
                onClick={handleClearPatientSearch}
                className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600"
              >
                ×
              </button>
            )}
          </div>

          {/* Patient Dropdown */}
          {showPatientDropdown && patientSearchResults.length > 0 && (
            <div className="absolute z-10 mt-1 w-full bg-white border border-gray-300 rounded-md shadow-lg max-h-60 overflow-auto">
              {patientSearchResults.map((patient: Patient) => (
                <button
                  key={patient.id}
                  onClick={() => handlePatientSelect(patient)}
                  className="w-full px-4 py-2 text-left hover:bg-gray-50 focus:bg-gray-50"
                >
                  <div className="font-medium">
                    {patient.firstName} {patient.lastName}
                  </div>
                  <div className="text-sm text-gray-500">{patient.email}</div>
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Doctor Search */}
        <div className="relative">
          <div className="relative">
            <Stethoscope className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
            <Input
              type="text"
              placeholder="Search doctors (min 3 chars)..."
              value={doctorSearchTerm}
              onChange={handleDoctorSearch}
              className="pl-10"
            />
            {selectedDoctor && (
              <button
                onClick={handleClearDoctorSearch}
                className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600"
              >
                ×
              </button>
            )}
          </div>

          {/* Doctor Dropdown */}
          {showDoctorDropdown && doctorSearchResults.length > 0 && (
            <div className="absolute z-10 mt-1 w-full bg-white border border-gray-300 rounded-md shadow-lg max-h-60 overflow-auto">
              {doctorSearchResults.map((doctor: Doctor) => (
                <button
                  key={doctor.id}
                  onClick={() => handleDoctorSelect(doctor)}
                  className="w-full px-4 py-2 text-left hover:bg-gray-50 focus:bg-gray-50"
                >
                  <div className="font-medium">
                    Dr. {doctor.firstName} {doctor.lastName}
                  </div>
                  <div className="text-sm text-gray-500">
                    {doctor.specialization}
                  </div>
                </button>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Active Filters Display */}
      {(selectedPatient || selectedDoctor) && (
        <div className="flex flex-wrap gap-2 mb-4">
          {selectedPatient && (
            <span className="inline-flex items-center px-3 py-1 rounded-full text-sm bg-blue-100 text-blue-800">
              Patient: {selectedPatient.firstName} {selectedPatient.lastName}
              <button
                onClick={handleClearPatientSearch}
                className="ml-2 text-blue-600 hover:text-blue-800"
              >
                ×
              </button>
            </span>
          )}
          {selectedDoctor && (
            <span className="inline-flex items-center px-3 py-1 rounded-full text-sm bg-green-100 text-green-800">
              Doctor: Dr. {selectedDoctor.firstName} {selectedDoctor.lastName}
              <button
                onClick={handleClearDoctorSearch}
                className="ml-2 text-green-600 hover:text-green-800"
              >
                ×
              </button>
            </span>
          )}
        </div>
      )}

      {/* Calendar */}
      <div className="relative">
        {isLoading && (
          <LoadingOverlay isLoading={isLoading}>
            <div />
          </LoadingOverlay>
        )}
        <FullCalendar
          plugins={[dayGridPlugin, timeGridPlugin]}
          initialView="timeGridWeek"
          headerToolbar={{
            left: "prev,next today",
            center: "title",
            right: "dayGridMonth,timeGridWeek,timeGridDay",
          }}
          height="600px"
          events={filteredEvents}
          eventClick={handleEventClick}
          editable={false}
          selectable={false}
          eventDisplay="block"
          dayMaxEvents={true}
          eventBackgroundColor="#3B82F6"
          eventBorderColor="#3B82F6"
          eventTextColor="white"
          slotMinTime="07:00:00"
          slotMaxTime="19:00:00"
          allDaySlot={false}
          slotDuration="00:30:00"
          businessHours={{
            daysOfWeek: [1, 2, 3, 4, 5],
            startTime: "08:00",
            endTime: "18:00",
          }}
        />
      </div>
    </Card>
  );
};
