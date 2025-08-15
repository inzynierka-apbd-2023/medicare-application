import { useCallback, useEffect, useState } from "react";

import DoctorScheduleApiService from "../../../shared/services/doctorScheduleApi";
import type {
  DoctorCalendarEvent,
  DoctorScheduleEvent,
} from "../types/doctorScheduler";

interface UseDoctorScheduleProps {
  doctorId?: string;
  autoRefresh?: boolean;
  refreshInterval?: number; // in milliseconds
}

interface UseDoctorScheduleReturn {
  schedule: DoctorScheduleEvent[];
  calendarEvents: DoctorCalendarEvent[];
  selectedAppointment: DoctorScheduleEvent | null;
  todaysAppointments: DoctorScheduleEvent[];
  isLoading: boolean;
  error: string | null;
  refreshSchedule: () => Promise<void>;
  selectAppointment: (appointment: DoctorScheduleEvent | null) => void;
  markAppointmentCompleted: (appointmentId: string) => Promise<boolean>;
  markAppointmentNoShow: (appointmentId: string) => Promise<boolean>;
  addAppointmentNotes: (
    appointmentId: string,
    notes: string
  ) => Promise<boolean>;
  getAppointmentDetails: (
    appointmentId: string
  ) => Promise<DoctorScheduleEvent | null>;
}

export const useDoctorSchedule = ({
  doctorId = "current-doctor-id",
  autoRefresh = false,
  refreshInterval = 30000, // 30 seconds
}: UseDoctorScheduleProps = {}): UseDoctorScheduleReturn => {
  const [schedule, setSchedule] = useState<DoctorScheduleEvent[]>([]);
  const [selectedAppointment, setSelectedAppointment] =
    useState<DoctorScheduleEvent | null>(null);
  const [todaysAppointments, setTodaysAppointments] = useState<
    DoctorScheduleEvent[]
  >([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Convert schedule to calendar events
  const calendarEvents: DoctorCalendarEvent[] = schedule.map((appointment) => {
    const appointmentDate = new Date(`${appointment.date}T${appointment.time}`);
    const endDate = new Date(
      appointmentDate.getTime() + appointment.duration * 60 * 1000
    );

    // Determine time status
    const now = new Date();
    const isToday = appointmentDate.toDateString() === now.toDateString();
    let timeStatus:
      | "upcoming"
      | "current"
      | "overdue"
      | "completed"
      | "no-show" = "upcoming";

    if (appointment.status === "completed") {
      timeStatus = "completed";
    } else if (appointment.status === "no-show") {
      timeStatus = "no-show";
    } else if (isToday) {
      if (now >= appointmentDate && now <= endDate) {
        timeStatus = "current";
      } else if (now > endDate) {
        timeStatus = "overdue";
      }
    }

    // Determine color based on status
    let color = "#3B82F6"; // Blue for scheduled
    if (timeStatus === "completed")
      color = "#10B981"; // Green
    else if (timeStatus === "no-show")
      color = "#EF4444"; // Red
    else if (timeStatus === "current")
      color = "#F59E0B"; // Orange
    else if (timeStatus === "overdue") color = "#DC2626"; // Dark red

    return {
      id: appointment.id,
      title: `${appointment.patientName} - ${appointment.appointmentType}`,
      start: appointmentDate.toISOString(),
      end: endDate.toISOString(),
      color,
      extendedProps: {
        appointment,
        timeStatus,
      },
    };
  });

  const refreshSchedule = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      // Get the current week's schedule
      const startOfWeek = new Date();
      startOfWeek.setDate(startOfWeek.getDate() - startOfWeek.getDay());
      const endOfWeek = new Date(startOfWeek);
      endOfWeek.setDate(endOfWeek.getDate() + 6);

      const scheduleResponse = await DoctorScheduleApiService.getDoctorSchedule(
        doctorId,
        startOfWeek.toISOString().split("T")[0],
        endOfWeek.toISOString().split("T")[0]
      );

      const todaysResponse =
        await DoctorScheduleApiService.getTodaysAppointments(doctorId);

      if (scheduleResponse.success && todaysResponse.success) {
        setSchedule(scheduleResponse.data);
        setTodaysAppointments(todaysResponse.data);
      } else {
        setError(
          scheduleResponse.error ||
            todaysResponse.error ||
            "Failed to fetch schedule"
        );
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "An unexpected error occurred"
      );
    } finally {
      setIsLoading(false);
    }
  }, [doctorId]);

  const selectAppointment = useCallback(
    (appointment: DoctorScheduleEvent | null) => {
      setSelectedAppointment(appointment);
    },
    []
  );

  const markAppointmentCompleted = useCallback(
    async (appointmentId: string): Promise<boolean> => {
      try {
        const response =
          await DoctorScheduleApiService.markAppointmentCompleted(
            appointmentId
          );
        if (response.success) {
          await refreshSchedule();
          return true;
        }
        setError(response.error || "Failed to mark appointment as completed");
        return false;
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "An unexpected error occurred"
        );
        return false;
      }
    },
    [refreshSchedule]
  );

  const markAppointmentNoShow = useCallback(
    async (appointmentId: string): Promise<boolean> => {
      try {
        const response =
          await DoctorScheduleApiService.markAppointmentNoShow(appointmentId);
        if (response.success) {
          await refreshSchedule();
          return true;
        }
        setError(response.error || "Failed to mark appointment as no-show");
        return false;
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "An unexpected error occurred"
        );
        return false;
      }
    },
    [refreshSchedule]
  );

  const addAppointmentNotes = useCallback(
    async (appointmentId: string, notes: string): Promise<boolean> => {
      try {
        const response = await DoctorScheduleApiService.addAppointmentNotes(
          appointmentId,
          notes
        );
        if (response.success) {
          await refreshSchedule();
          return true;
        }
        setError(response.error || "Failed to add appointment notes");
        return false;
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "An unexpected error occurred"
        );
        return false;
      }
    },
    [refreshSchedule]
  );

  const getAppointmentDetails = useCallback(
    async (appointmentId: string): Promise<DoctorScheduleEvent | null> => {
      try {
        const response =
          await DoctorScheduleApiService.getAppointmentDetails(appointmentId);
        if (response.success) {
          return response.data;
        }
        setError(response.error || "Failed to fetch appointment details");
        return null;
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "An unexpected error occurred"
        );
        return null;
      }
    },
    []
  );

  // Auto-refresh functionality
  useEffect(() => {
    refreshSchedule();
  }, [refreshSchedule]);

  useEffect(() => {
    if (autoRefresh) {
      const interval = setInterval(refreshSchedule, refreshInterval);
      return () => clearInterval(interval);
    }
    return undefined;
  }, [autoRefresh, refreshInterval, refreshSchedule]);

  return {
    schedule,
    calendarEvents,
    selectedAppointment,
    todaysAppointments,
    isLoading,
    error,
    refreshSchedule,
    selectAppointment,
    markAppointmentCompleted,
    markAppointmentNoShow,
    addAppointmentNotes,
    getAppointmentDetails,
  };
};
