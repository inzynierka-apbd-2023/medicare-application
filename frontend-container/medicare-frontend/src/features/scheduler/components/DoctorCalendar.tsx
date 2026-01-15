/**
 * DoctorCalendar Component
 * Modular calendar component for doctor appointment scheduling
 */

import React, { useCallback, useRef } from "react";
import type {
  DateSelectArg,
  DatesSetArg,
  EventClickArg,
  EventContentArg,
} from "@fullcalendar/core";
import dayGridPlugin from "@fullcalendar/daygrid";
import FullCalendar from "@fullcalendar/react";
import timeGridPlugin from "@fullcalendar/timegrid";
import { MapPin, Phone, User, Video } from "lucide-react";

import type { Appointment } from "../types";
import { getStatusColors } from "../utils/statusColors";

export interface DoctorCalendarEvent {
  id: string;
  title: string;
  start: string;
  end: string;
  backgroundColor: string;
  borderColor: string;
  textColor: string;
  extendedProps: {
    appointment: Appointment;
    patientName: string;
    patientPhone: string;
    status: string;
    appointmentType: string;
    service?: string;
    room?: string;
    description?: string;
  };
}

interface DoctorCalendarProps {
  appointments: Appointment[];
  currentView: "dayGridMonth" | "timeGridWeek" | "timeGridDay";
  onEventClick: (appointment: Appointment) => void;
  onDateSelect?: (selectInfo: DateSelectArg) => void;
  onDatesSet?: (dateInfo: DatesSetArg) => void;
  isLoading?: boolean;
}

export const DoctorCalendar: React.FC<DoctorCalendarProps> = ({
  appointments,
  currentView,
  onEventClick,
  onDateSelect,
  onDatesSet,
  isLoading = false,
}) => {
  const calendarRef = useRef<FullCalendar>(null);

  // Convert appointments to calendar events
  const calendarEvents: DoctorCalendarEvent[] = appointments.map(
    (appointment) => {
      const colors = getStatusColors(appointment.status?.name || "pending");
      const patientName = appointment.patient
        ? `${appointment.patient.firstName} ${appointment.patient.lastName}`
        : "Unknown Patient";

      return {
        id: appointment.id,
        title: patientName,
        start: appointment.day,
        end: appointment.day,
        backgroundColor: colors.bg,
        borderColor: colors.border,
        textColor: "#FFFFFF",
        extendedProps: {
          appointment,
          patientName,
          patientPhone: appointment.patient?.phone || "No phone",
          status: appointment.status?.name || "Unknown",
          appointmentType: appointment.appointmentType,
          service: "General Consultation",
          room: appointment.room || "",
          description: appointment.description || "",
        },
      };
    }
  );

  const handleEventClick = useCallback(
    (eventInfo: EventClickArg) => {
      const appointment = eventInfo.event.extendedProps
        .appointment as Appointment;
      onEventClick(appointment);
    },
    [onEventClick]
  );

  const handleEventContent = useCallback((eventInfo: EventContentArg) => {
    const { patientName, appointmentType, status } =
      eventInfo.event.extendedProps;

    const getTypeIcon = (type: string) => {
      switch (type) {
        case "virtual":
          return <Video size={12} className="inline mr-1" />;
        case "phone":
          return <Phone size={12} className="inline mr-1" />;
        default:
          return <MapPin size={12} className="inline mr-1" />;
      }
    };

    return (
      <div className="p-1 text-xs overflow-hidden">
        <div className="font-medium truncate flex items-center">
          <User size={12} className="mr-1 flex-shrink-0" />
          {patientName}
        </div>
        <div className="flex items-center mt-1 opacity-90">
          {getTypeIcon(appointmentType)}
          <span className="truncate">{status}</span>
        </div>
      </div>
    );
  }, []);

  // Expose calendar API for parent component
  const getCalendarApi = useCallback(() => {
    return calendarRef.current?.getApi();
  }, []);

  React.useEffect(() => {
    if (calendarRef.current) {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      (calendarRef.current as any).getCalendarApi = getCalendarApi;
    }
  }, [getCalendarApi]);

  return (
    <div className={`relative ${isLoading ? "opacity-50" : ""}`}>
      <FullCalendar
        ref={calendarRef}
        plugins={[dayGridPlugin, timeGridPlugin]}
        initialView={currentView}
        headerToolbar={{
          left: "prev,next today",
          center: "title",
          right: "",
        }}
        events={calendarEvents}
        eventClick={handleEventClick}
        selectable={true}
        selectMirror={true}
        dayMaxEvents={true}
        weekends={true}
        {...(onDateSelect && { select: onDateSelect })}
        {...(onDatesSet && { datesSet: onDatesSet })}
        eventContent={handleEventContent}
        height="600px"
        eventDisplay="block"
        dayHeaderFormat={{ weekday: "short" }}
        slotMinTime="07:00:00"
        slotMaxTime="19:00:00"
        allDaySlot={false}
        businessHours={{
          daysOfWeek: [1, 2, 3, 4, 5],
          startTime: "08:00",
          endTime: "17:00",
        }}
        slotDuration="00:30:00"
        snapDuration="00:15:00"
        eventTimeFormat={{
          hour: "numeric",
          minute: "2-digit",
          omitZeroMinute: false,
          meridiem: "short",
        }}
        eventClassNames="cursor-pointer hover:opacity-80 transition-opacity"
        dayCellClassNames="hover:bg-gray-50"
        nowIndicator={true}
        eventConstraint="businessHours"
        selectConstraint="businessHours"
        eventOverlap={false}
        slotEventOverlap={false}
      />
    </div>
  );
};

export default DoctorCalendar;
