import { useMemo } from "react";
import type { Appointment } from "@features/scheduler/types";
import { getStatusColors } from "@features/scheduler/utils/statusColors";
import type { EventContentArg } from "@fullcalendar/core";
import dayGridPlugin from "@fullcalendar/daygrid";
import FullCalendar from "@fullcalendar/react";
import timeGridPlugin from "@fullcalendar/timegrid";
import { Card } from "@shared/components";
import { Calendar as CalendarIcon, Clock } from "lucide-react";

import "./DashboardScheduleView.css";

// Define local interface for calendar events to avoid type conflicts
interface DashboardCalendarEvent {
  id: string;
  title: string;
  start: string;
  end: string;
  backgroundColor: string;
  borderColor: string;
  extendedProps: {
    appointment: Appointment;
    doctorName: string;
    specialty: string;
    type: string;
    status: string;
    room?: string;
    description?: string;
  };
}

interface DashboardScheduleViewProps {
  appointments: Appointment[];
  className?: string;
}

const formatEventContent = (eventInfo: EventContentArg) => {
  const appointment = eventInfo.event.extendedProps.appointment as Appointment;

  return (
    <div className="p-1 text-xs overflow-hidden">
      <div className="font-semibold truncate">
        {appointment.doctor ? `Dr. ${appointment.doctor.lastName}` : "Doctor"}
      </div>
      <div className="text-gray-600 truncate">
        {appointment.appointmentType}
      </div>
    </div>
  );
};

export default function DashboardScheduleView({
  appointments,
  className = "",
}: DashboardScheduleViewProps) {
  const calendarEvents: DashboardCalendarEvent[] = useMemo(() => {
    return appointments
      .filter((apt) => apt.status?.name !== "Cancelled")
      .map((appointment) => {
        const startDate = new Date(appointment.day);
        const [hours, minutes] = appointment.timeSlot?.startDateTime
          ?.split("T")[1]
          ?.split(":") || ["09", "00"];
        startDate.setHours(parseInt(hours), parseInt(minutes));

        const endDate = new Date(startDate);
        endDate.setMinutes(endDate.getMinutes() + appointment.durationMinutes);

        const statusColors = getStatusColors(appointment.status?.name);
        const backgroundColor = statusColors.bg;
        const borderColor = statusColors.border;

        return {
          id: appointment.id,
          title: appointment.doctor
            ? `Dr. ${appointment.doctor.lastName}`
            : "Appointment",
          start: startDate.toISOString(),
          end: endDate.toISOString(),
          backgroundColor,
          borderColor,
          extendedProps: {
            appointment,
            doctorName: appointment.doctor
              ? `${appointment.doctor.firstName} ${appointment.doctor.lastName}`
              : "Unknown Doctor",
            specialty:
              appointment.doctor?.specializations[0]?.name || "General",
            type: appointment.appointmentType,
            status: appointment.status?.name || "Scheduled",
            ...(appointment.room && { room: appointment.room }),
            ...(appointment.description && {
              description: appointment.description,
            }),
          },
        };
      });
  }, [appointments]);

  const upcomingCount = useMemo(() => {
    const now = new Date();
    return appointments.filter(
      (apt) => new Date(apt.day) >= now && apt.status?.name !== "Cancelled"
    ).length;
  }, [appointments]);

  return (
    <Card variant="medical" padding="md" className={className}>
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold text-blue-600 flex items-center gap-2">
          <CalendarIcon className="w-5 h-5" />
          Your Schedule
        </h3>
        <div className="flex items-center gap-4 text-sm text-gray-600">
          <div className="flex items-center gap-1">
            <Clock className="w-4 h-4" />
            <span>{upcomingCount} upcoming</span>
          </div>
        </div>
      </div>

      {appointments.length === 0 ? (
        <div className="text-center py-12 text-gray-500">
          <CalendarIcon className="w-16 h-16 mx-auto mb-4 text-gray-300" />
          <p className="text-lg mb-2">No appointments scheduled</p>
          <p className="text-sm">
            Your appointments will appear here when scheduled.
          </p>
        </div>
      ) : (
        <div className="calendar-container dashboard-calendar">
          <FullCalendar
            plugins={[dayGridPlugin, timeGridPlugin]}
            initialView="dayGridMonth"
            headerToolbar={{
              left: "prev,next today",
              center: "title",
              right: "dayGridMonth,timeGridWeek",
            }}
            events={calendarEvents}
            eventContent={formatEventContent}
            height="auto"
            eventDisplay="block"
            dayMaxEvents={3}
            moreLinkClick="popover"
            eventMouseEnter={(info) => {
              const appointment = info.event.extendedProps
                .appointment as Appointment;
              info.el.title = `${info.event.title}\n${appointment.appointmentType} appointment\nTime: ${new Date(info.event.start!).toLocaleTimeString()}\nDuration: ${appointment.durationMinutes} minutes`;
            }}
            eventClick={(info) => {
              // Read-only mode - just show tooltip or do nothing
              info.jsEvent.preventDefault();
            }}
            selectable={false}
            selectMirror={false}
            dayPopoverFormat={{
              month: "long",
              day: "numeric",
              year: "numeric",
            }}
            eventTimeFormat={{
              hour: "numeric",
              minute: "2-digit",
              meridiem: "short",
            }}
            slotLabelFormat={{
              hour: "numeric",
              minute: "2-digit",
              meridiem: "short",
            }}
            allDaySlot={false}
            slotMinTime="08:00:00"
            slotMaxTime="17:00:00"
            businessHours={{
              daysOfWeek: [1, 2, 3, 4, 5],
              startTime: "08:00",
              endTime: "17:00",
            }}
            weekends={true}
            nowIndicator={true}
          />
        </div>
      )}

      {/* Legend */}
      <div className="mt-4 flex flex-wrap gap-4 text-xs">
        <div className="flex items-center gap-1">
          <div className="w-3 h-3 bg-blue-500 rounded"></div>
          <span>In-person</span>
        </div>
        <div className="flex items-center gap-1">
          <div className="w-3 h-3 bg-green-500 rounded"></div>
          <span>Virtual</span>
        </div>
        <div className="flex items-center gap-1">
          <div className="w-3 h-3 bg-amber-500 rounded"></div>
          <span>Phone</span>
        </div>
        <div className="flex items-center gap-1">
          <div className="w-3 h-3 bg-gray-500 rounded"></div>
          <span>Completed</span>
        </div>
      </div>
    </Card>
  );
}
