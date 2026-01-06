import { useCallback, useEffect, useState } from "react";

import { useAuth } from "../../../shared/auth/AuthContext";
import DoctorScheduleApiService from "../../../shared/services/doctorScheduleApi";
import type { DoctorScheduleEvent } from "../../scheduler/types/doctorScheduler";
import type { TodayAppointment } from "../types";

interface UseTodaysAppointmentsReturn {
  appointments: TodayAppointment[];
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
  markAsCompleted: (id: string) => Promise<boolean>;
  markAsNoShow: (id: string) => Promise<boolean>;
}

// Mock data for today's appointments
// Map API response to TodayAppointment
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

      const response = await DoctorScheduleApiService.getTodaysAppointments(
        user.id
      );

      if (response.success && response.data) {
        // Map backend events to frontend appointments
        // response.data is DoctorScheduleEvent[]
        const mappedAppointments = response.data.map(mapToTodayAppointment);
        setAppointments(mappedAppointments);
      } else {
        throw new Error(response.error || "Failed to fetch appointments");
      }
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
      const response =
        await DoctorScheduleApiService.markAppointmentCompleted(id);

      if (response.success) {
        setAppointments((prev) =>
          prev.map((apt) =>
            apt.id === id ? { ...apt, status: "completed" as const } : apt
          )
        );
        return true;
      }
      return false;
    } catch (err) {
      console.error("Failed to mark appointment as completed:", err);
      return false;
    }
  };

  const markAsNoShow = async (id: string): Promise<boolean> => {
    try {
      const response = await DoctorScheduleApiService.markAppointmentNoShow(id);

      if (response.success) {
        setAppointments((prev) =>
          prev.map((apt) =>
            apt.id === id ? { ...apt, status: "no-show" as const } : apt
          )
        );
        return true;
      }
      return false;
    } catch (err) {
      console.error("Failed to mark appointment as no-show:", err);
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
