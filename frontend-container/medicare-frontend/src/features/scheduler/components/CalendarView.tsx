import React, { useCallback } from "react";
import type {
  DateSelectArg,
  DatesSetArg,
  EventClickArg,
  EventContentArg,
} from "@fullcalendar/core";
import dayGridPlugin from "@fullcalendar/daygrid";
import FullCalendar from "@fullcalendar/react";
import timeGridPlugin from "@fullcalendar/timegrid";

import type { CalendarEvent } from "../types";

interface CalendarViewProps {
  events: CalendarEvent[];
  onEventClick: (event: CalendarEvent) => void;
  onDateSelect: (date: string) => void;
  onDateRangeChange: (start: string, end: string) => void;
  isLoading?: boolean;
}

export const CalendarView: React.FC<CalendarViewProps> = ({
  events,
  onEventClick,
  onDateSelect,
  onDateRangeChange,
  isLoading = false,
}) => {
  const handleEventClick = useCallback(
    (eventInfo: EventClickArg) => {
      const calendarEvent: CalendarEvent = {
        id: eventInfo.event.id,
        title: eventInfo.event.title,
        start: eventInfo.event.start?.toISOString() || "",
        end:
          eventInfo.event.end?.toISOString() ||
          eventInfo.event.start?.toISOString() ||
          "",
        backgroundColor: eventInfo.event.backgroundColor,
        borderColor: eventInfo.event.borderColor,
        textColor: eventInfo.event.textColor,
        extendedProps: eventInfo.event
          .extendedProps as CalendarEvent["extendedProps"],
      };
      onEventClick(calendarEvent);
    },
    [onEventClick]
  );

  const handleDateSelect = useCallback(
    (selectInfo: DateSelectArg) => {
      onDateSelect(selectInfo.startStr);
    },
    [onDateSelect]
  );

  const handleDatesSet = useCallback(
    (dateInfo: DatesSetArg) => {
      onDateRangeChange(dateInfo.startStr, dateInfo.endStr);
    },
    [onDateRangeChange]
  );

  const handleEventContent = useCallback((eventInfo: EventContentArg) => {
    const { appointment, doctorName, status } = eventInfo.event.extendedProps;

    return (
      <div className="p-1 text-xs">
        <div className="font-medium truncate">{doctorName}</div>
        <div className="text-xs opacity-90 truncate">
          {appointment.appointmentType === "virtual"
            ? "📹"
            : appointment.appointmentType === "phone"
              ? "📞"
              : "🏥"}{" "}
          {status}
        </div>
      </div>
    );
  }, []);

  return (
    <div className="relative">
      {isLoading && (
        <div className="absolute inset-0 bg-white bg-opacity-75 flex items-center justify-center z-10">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
      )}

      <FullCalendar
        plugins={[dayGridPlugin, timeGridPlugin]}
        initialView="dayGridMonth"
        headerToolbar={{
          left: "prev,next today",
          center: "title",
          right: "dayGridMonth,timeGridWeek,timeGridDay",
        }}
        events={events}
        eventClick={handleEventClick}
        selectable={true}
        selectMirror={true}
        dayMaxEvents={true}
        weekends={true}
        select={handleDateSelect}
        datesSet={handleDatesSet}
        eventContent={handleEventContent}
        height="auto"
        eventDisplay="block"
        dayHeaderFormat={{ weekday: "short" }}
        slotMinTime="08:00:00"
        slotMaxTime="17:00:00"
        allDaySlot={false}
        businessHours={{
          daysOfWeek: [1, 2, 3, 4, 5], // Monday to Friday
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

export default CalendarView;
