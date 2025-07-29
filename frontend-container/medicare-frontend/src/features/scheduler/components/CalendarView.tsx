import React, { useState } from "react";
import { Calendar, Clock, User } from "lucide-react";
import { Button } from "../../../shared/components";
import type { CalendarViewProps, TimeSlot } from "../types";

export const CalendarView: React.FC<CalendarViewProps> = ({
  events,
  timeSlots,
  onTimeSlotSelect,
  onEventSelect,
  selectedDoctor,
}) => {
  const [selectedTimeSlot, setSelectedTimeSlot] = useState<TimeSlot | null>(
    null
  );

  const handleTimeSlotClick = (timeSlot: TimeSlot) => {
    if (timeSlot.isAvailable) {
      setSelectedTimeSlot(timeSlot);
      onTimeSlotSelect(timeSlot);
    }
  };

  const formatTime = (date: Date) => {
    return date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
  };

  const formatDate = (date: Date) => {
    return date.toLocaleDateString([], {
      weekday: "short",
      month: "short",
      day: "numeric",
    });
  };

  // Group time slots by date
  const groupedTimeSlots = timeSlots.reduce(
    (groups, slot) => {
      const dateKey = slot.start.toDateString();
      if (!groups[dateKey]) {
        groups[dateKey] = [];
      }
      groups[dateKey].push(slot);
      return groups;
    },
    {} as Record<string, TimeSlot[]>
  );

  if (!selectedDoctor && timeSlots.length === 0) {
    return (
      <div className="h-[600px] bg-gray-50 rounded-lg flex flex-col items-center justify-center text-gray-500">
        <Calendar size={64} className="mb-4" />
        <p className="text-lg font-medium">
          Please select a doctor to view available time slots
        </p>
        <p className="text-sm">
          Choose a service, specialization, or doctor to get started
        </p>
      </div>
    );
  }

  if (timeSlots.length === 0) {
    return (
      <div className="h-[600px] bg-gray-50 rounded-lg flex flex-col items-center justify-center text-gray-500">
        <Clock size={64} className="mb-4" />
        <p className="text-lg font-medium">No available time slots</p>
        <p className="text-sm">
          The selected doctor has no available appointments
        </p>
      </div>
    );
  }

  return (
    <div className="h-[600px] bg-white rounded-lg border border-gray-200 overflow-hidden">
      <div className="h-full overflow-y-auto p-4">
        <div className="mb-4 flex items-center gap-2 text-blue-600">
          <Calendar size={20} />
          <h3 className="font-semibold">Available Time Slots</h3>
          {selectedDoctor && (
            <span className="text-sm text-gray-500">for selected doctor</span>
          )}
        </div>

        <div className="space-y-6">
          {Object.entries(groupedTimeSlots).map(([dateKey, slots]) => (
            <div key={dateKey} className="border-b border-gray-100 pb-4">
              <h4 className="font-medium text-gray-700 mb-3">
                {formatDate(new Date(dateKey))}
              </h4>

              <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-2">
                {slots.map((slot) => (
                  <button
                    key={slot.id}
                    onClick={() => handleTimeSlotClick(slot)}
                    disabled={!slot.isAvailable}
                    className={`
                      p-3 rounded-lg border text-sm font-medium transition-colors
                      ${
                        slot.isAvailable
                          ? selectedTimeSlot?.id === slot.id
                            ? "bg-blue-600 text-white border-blue-600"
                            : "bg-white text-blue-600 border-blue-200 hover:bg-blue-50"
                          : "bg-gray-100 text-gray-400 border-gray-200 cursor-not-allowed"
                      }
                    `}
                  >
                    <div className="flex items-center justify-center gap-1">
                      <Clock size={14} />
                      {formatTime(slot.start)}
                    </div>
                  </button>
                ))}
              </div>
            </div>
          ))}
        </div>

        {selectedTimeSlot && (
          <div className="mt-6 p-4 bg-blue-50 rounded-lg border border-blue-200">
            <div className="flex items-center gap-2 mb-2">
              <Clock size={16} className="text-blue-600" />
              <span className="font-medium text-blue-900">
                Selected Time Slot
              </span>
            </div>
            <p className="text-sm text-blue-700">
              {formatDate(selectedTimeSlot.start)} at{" "}
              {formatTime(selectedTimeSlot.start)} -{" "}
              {formatTime(selectedTimeSlot.end)}
            </p>
            <Button
              variant="primary"
              className="mt-3"
              onClick={() => {
                // This will be handled by the parent component
                console.log("Book appointment for:", selectedTimeSlot);
              }}
            >
              Book This Appointment
            </Button>
          </div>
        )}
      </div>
    </div>
  );
};
