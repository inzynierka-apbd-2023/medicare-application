import FullCalendar, { EventSourceFunc } from "@fullcalendar/react";
import dayGridPlugin from "@fullcalendar/daygrid";
import timeGridPlugin from "@fullcalendar/timegrid";
import { gapi } from "gapi-script";
import { useGoogleAuth } from "../../hooks/useGoogleAuth";

export default function GoogleCalendarScheduler() {
  const { inited, isSignedIn, signIn, signOut } = useGoogleAuth();

  if (!inited) {
    return (
      <div className="w-full h-full flex items-center justify-center">
        Loading calendar…
      </div>
    );
  }

  if (!isSignedIn) {
    return (
      <div className="w-full h-full flex items-center justify-center">
        <button
          onClick={signIn}
          className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
        >
          Sign in to Google Calendar
        </button>
      </div>
    );
  }

  const fetchPrivateEvents: EventSourceFunc = async (
    fetchInfo,
    successCallback,
    failureCallback
  ) => {
    try {
      const response = await gapi.client.calendar.events.list({
        calendarId: "primary",
        timeMin: fetchInfo.startStr,
        timeMax: fetchInfo.endStr,
        singleEvents: true,
        orderBy: "startTime",
        maxResults: 2500,
      });

      const items = (response.result.items || []).map((evt) => ({
        title: evt.summary,
        start: evt.start?.dateTime || evt.start?.date,
        end: evt.end?.dateTime || evt.end?.date,
      }));
      successCallback(items);
    } catch (error) {
      failureCallback(error as Error);
    }
  };

  return (
    <div className="flex flex-col w-full h-full">
      <div className="flex-1">
        <FullCalendar
          plugins={[dayGridPlugin, timeGridPlugin]} // include both
          initialView="timeGridDay" // default to daily view
          headerToolbar={{
            // show view-switch buttons
            left: "prev,next today",
            center: "title",
            right: "timeGridDay,timeGridWeek,dayGridMonth",
          }}
          buttonText={{
            // friendly labels
            timeGridDay: "Day",
            timeGridWeek: "Week",
            dayGridMonth: "Month",
          }}
          eventSources={[fetchPrivateEvents]}
          height="100%"
          slotMinTime="06:00:00"
          slotMaxTime="22:00:00"
        />
      </div>
    </div>
  );
}
