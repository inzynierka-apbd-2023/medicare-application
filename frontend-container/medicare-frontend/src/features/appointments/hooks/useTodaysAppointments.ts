import { useCallback, useEffect, useState } from "react";
import type { TodayAppointment } from "@features/appointments/types";
import type { DoctorScheduleEvent } from "@features/scheduler/types/doctorScheduler";
import { useAuth } from "@shared/auth/AuthContext";
import doctorScheduleApi from "@shared/services/doctorScheduleApi";

interface UseTodaysAppointmentsReturn {
  appointments: TodayAppointment[];
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
  markAsCompleted: (id: string) => Promise<boolean>;
  markAsNoShow: (id: string) => Promise<boolean>;
}

const mapToTodayAppointment = (evt: DoctorScheduleEvent): TodayAppointment => {
  return {
    id: evt.id,
    date: evt.date,
    time: evt.time,
    duration: evt.duration,
    patient: {
      id: evt.patientId,
      name: evt.patientName || "Unknown",
      age: evt.patientAge || 0,
      phone: evt.patientPhone || "",
      email: evt.patientEmail || "",
      medicalHistory: evt.medicalHistory || [],
      allergies: evt.allergies || [],
      currentMedications: evt.currentMedications || [],
    },
    appointmentType: evt.appointmentType,
    description: evt.notes || "",
    status: (evt.status === "overdue"
      ? "scheduled"
      : evt.status) as TodayAppointment["status"],
    chiefComplaint: evt.chiefComplaint || "",
    notes: evt.notes || "",
  };
};

export const useTodaysAppointments = (): UseTodaysAppointmentsReturn => {
  const { user } = useAuth();
  const [appointments, setAppointments] = useState<TodayAppointment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchTodaysAppointments = useCallback(async () => {
    if (!user?.id || user.role !== "Doctor") {
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      setError(null);

      const data = await doctorScheduleApi.getTodaysAppointments(user.id);
      const mappedAppointments = data.map(mapToTodayAppointment);
      setAppointments(mappedAppointments);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Failed to fetch today's appointments"
      );
    } finally {
      setLoading(false);
    }
  }, [user?.id, user?.role]);

  const markAsCompleted = async (id: string): Promise<boolean> => {
    try {
      await doctorScheduleApi.markAppointmentCompleted(id);
      setAppointments((prev) =>
        prev.map((apt) =>
          apt.id === id ? { ...apt, status: "completed" as const } : apt
        )
      );
      return true;
    } catch {
      return false;
    }
  };

  const markAsNoShow = async (id: string): Promise<boolean> => {
    try {
      await doctorScheduleApi.markAppointmentNoShow(id);
      setAppointments((prev) =>
        prev.map((apt) =>
          apt.id === id ? { ...apt, status: "no-show" as const } : apt
        )
      );
      return true;
    } catch {
      return false;
    }
  };

  useEffect(() => {
    fetchTodaysAppointments();
  }, [fetchTodaysAppointments]);

  return {
    appointments,
    loading,
    error,
    refetch: fetchTodaysAppointments,
    markAsCompleted,
    markAsNoShow,
  };
};
