import { useCallback, useEffect, useState } from "react";

import doctorScheduleApi from "../../../shared/services/doctorScheduleApi";
import type {
  DoctorCalendarEvent,
  DoctorScheduleEvent,
} from "../types/doctorScheduler";

interface UseDoctorScheduleProps {
  doctorId?: string;
  autoRefresh?: boolean;
  refreshInterval?: number;
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
  doctorId,
  autoRefresh = false,
  refreshInterval = 30000,
}: UseDoctorScheduleProps = {}): UseDoctorScheduleReturn => {
  const [schedule, setSchedule] = useState<DoctorScheduleEvent[]>([]);
  const [selectedAppointment, setSelectedAppointment] =
    useState<DoctorScheduleEvent | null>(null);
  const [todaysAppointments, setTodaysAppointments] = useState<
    DoctorScheduleEvent[]
  >([]);
  const [isLoading, setIsLoading] = useState(
    !!doctorId &&
      doctorId !== "current-doctor-id" &&
      doctorId !== "mock-doctor-id"
  );
  const [error, setError] = useState<string | null>(null);

  const calendarEvents: DoctorCalendarEvent[] = schedule.map((appointment) => {
    // Parse date and time as local time (not UTC)
    // appointment.date is "YYYY-MM-DD", appointment.time is "HH:MM"
    const [year, month, day] = appointment.date.split("-").map(Number);
    const [hours, minutes] = appointment.time.split(":").map(Number);
    const appointmentDate = new Date(year, month - 1, day, hours, minutes);
    const endDate = new Date(
      appointmentDate.getTime() + appointment.duration * 60 * 1000
    );

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

    let color = "#3B82F6";
    if (timeStatus === "completed") color = "#10B981";
    else if (timeStatus === "no-show") color = "#EF4444";
    else if (timeStatus === "current") color = "#F59E0B";
    else if (timeStatus === "overdue") color = "#DC2626";

    // Format as local date-time string (YYYY-MM-DDTHH:mm:ss) to avoid timezone shifts
    const formatLocalDateTime = (d: Date) => {
      const pad = (n: number) => n.toString().padStart(2, "0");
      return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
    };

    return {
      id: appointment.id,
      title: `${appointment.patientName} - ${appointment.appointmentType}`,
      start: formatLocalDateTime(appointmentDate),
      end: formatLocalDateTime(endDate),
      color,
      extendedProps: {
        appointment,
        timeStatus,
      },
    };
  });

  const refreshSchedule = useCallback(async () => {
    if (!doctorId) {
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const now = new Date();
      const startRange = new Date(now.getFullYear(), now.getMonth(), 1);
      startRange.setDate(startRange.getDate() - 7);

      const endRange = new Date(now.getFullYear(), now.getMonth() + 1, 0);
      endRange.setDate(endRange.getDate() + 7);

      const [scheduleData, todaysData] = await Promise.all([
        doctorScheduleApi.getDoctorSchedule(
          doctorId,
          startRange.toISOString().split("T")[0],
          endRange.toISOString().split("T")[0]
        ),
        doctorScheduleApi.getTodaysAppointments(doctorId),
      ]);

      setSchedule(scheduleData);
      setTodaysAppointments(todaysData);
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
        await doctorScheduleApi.markAppointmentCompleted(appointmentId);
        await refreshSchedule();
        return true;
      } catch {
        return false;
      }
    },
    [refreshSchedule]
  );

  const markAppointmentNoShow = useCallback(
    async (appointmentId: string): Promise<boolean> => {
      try {
        await doctorScheduleApi.markAppointmentNoShow(appointmentId);
        await refreshSchedule();
        return true;
      } catch {
        return false;
      }
    },
    [refreshSchedule]
  );

  const addAppointmentNotes = useCallback(
    async (appointmentId: string, notes: string): Promise<boolean> => {
      try {
        await doctorScheduleApi.addAppointmentNotes(appointmentId, notes);
        await refreshSchedule();
        return true;
      } catch {
        return false;
      }
    },
    [refreshSchedule]
  );

  const getAppointmentDetails = useCallback(
    async (appointmentId: string): Promise<DoctorScheduleEvent | null> => {
      try {
        return await doctorScheduleApi.getAppointmentDetails(appointmentId);
      } catch {
        return null;
      }
    },
    []
  );

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
