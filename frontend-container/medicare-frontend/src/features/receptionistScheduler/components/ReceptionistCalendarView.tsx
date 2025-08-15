import React, { useCallback } from "react";
import type { EventClickArg } from "@fullcalendar/core";
import dayGridPlugin from "@fullcalendar/daygrid";
import FullCalendar from "@fullcalendar/react";
import timeGridPlugin from "@fullcalendar/timegrid";
import { Calendar, Clock, MapPin, Phone, User, Video } from "lucide-react";

import { Card } from "../../../shared/components";
import type { CalendarEvent, ReceptionistAppointment } from "../types";

interface ReceptionistCalendarViewProps {
  events: CalendarEvent[];
  onEventClick: (appointment: ReceptionistAppointment) => void;
  onDateSelect: (date: string, time?: string) => void;
  isLoading?: boolean;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const formatEventContent = (eventInfo: any) => {
  const appointment = eventInfo.event.extendedProps
    .appointment as ReceptionistAppointment;
  const appointmentType = appointment.appointmentType;

  const getTypeIcon = () => {
    switch (appointmentType) {
      case "video-call":
        return <Video size={12} className="inline mr-1" />;
      case "phone":
        return <Phone size={12} className="inline mr-1" />;
      case "in-person":
        return <MapPin size={12} className="inline mr-1" />;
      default:
        return <User size={12} className="inline mr-1" />;
    }
  };

  return (
    <div className="text-xs p-1">
      <div className="font-medium truncate">
        {getTypeIcon()}
        {eventInfo.event.title}
      </div>
      {appointment.room && (
        <div className="text-gray-600 truncate">{appointment.room}</div>
      )}
    </div>
  );
};

export const ReceptionistCalendarView: React.FC<
  ReceptionistCalendarViewProps
> = ({
  events,
  onEventClick,
  onDateSelect: _onDateSelect,
  isLoading = false,
}) => {
  const handleEventClick = useCallback(
    (info: EventClickArg) => {
      const appointment = info.event.extendedProps
        .appointment as ReceptionistAppointment;
      onEventClick(appointment);
    },
    [onEventClick]
  );

  if (isLoading) {
    return (
      <Card>
        <div className="p-6 text-center">
          <Clock className="w-8 h-8 mx-auto mb-4 animate-spin text-blue-500" />
          <p className="text-gray-600">Loading schedule...</p>
        </div>
      </Card>
    );
  }

  return (
    <Card>
      <div className="p-6">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-semibold text-blue-700 flex items-center">
            <Calendar className="w-5 h-5 mr-2" />
            Appointment Schedule
          </h2>
          <div className="flex items-center space-x-4 text-sm text-gray-600">
            <div className="flex items-center">
              <MapPin size={14} className="mr-1" />
              <span>In-Person</span>
            </div>
            <div className="flex items-center">
              <Video size={14} className="mr-1" />
              <span>Video Call</span>
            </div>
            <div className="flex items-center">
              <Phone size={14} className="mr-1" />
              <span>Phone</span>
            </div>
          </div>
        </div>

        {events.length === 0 ? (
          <div className="text-center py-12 text-gray-500">
            <Calendar className="w-16 h-16 mx-auto mb-4 text-gray-300" />
            <p className="text-lg mb-2">No appointments scheduled</p>
            <p className="text-sm">
              Click on a date to schedule a new appointment.
            </p>
          </div>
        ) : (
          <div className="calendar-container">
            <FullCalendar
              plugins={[dayGridPlugin, timeGridPlugin]}
              initialView="timeGridWeek"
              headerToolbar={{
                left: "prev,next today",
                center: "title",
                right: "dayGridMonth,timeGridWeek,timeGridDay",
              }}
              events={events}
              eventContent={formatEventContent}
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
              slotLabelFormat={{
                hour: "numeric",
                minute: "2-digit",
                meridiem: false,
              }}
              allDaySlot={false}
              nowIndicator={true}
              // eslint-disable-next-line @typescript-eslint/no-explicit-any
              eventMouseEnter={(info: any) => {
                const appointment = info.event.extendedProps
                  .appointment as ReceptionistAppointment;
                const patientName = info.event.extendedProps.patientName;
                const doctorName = info.event.extendedProps.doctorName;
                const status = info.event.extendedProps.status;

                info.el.title = `${patientName}\nDoctor: ${doctorName}\nType: ${appointment.appointmentType}\nStatus: ${status}\nDuration: ${appointment.duration} minutes${appointment.room ? `\nRoom: ${appointment.room}` : ""}`;
              }}
              dayMaxEvents={3}
              moreLinkClick="popover"
            />
          </div>
        )}
      </div>
    </Card>
  );
};
