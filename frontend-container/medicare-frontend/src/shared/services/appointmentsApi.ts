import type { Appointment } from "../../features/appointments/types";
import type { Service } from "../../features/scheduler/types";

import { type ApiResponse, createErrorResponse } from "./api";
import { apiClient as api } from "./apiClient";
import { schedulerApi } from "./schedulerApi";

// Interface for backend appointment row
interface BackendAppointmentRow {
  id: string;
  scheduledAt: string;
  doctorId?: string;
  notes?: string;
  status?: string;
  isPaid?: boolean;
  IsPaid?: boolean;
  serviceId?: string;
  ServiceId?: string;
}

// Interface for doctor directory response
interface DoctorDirectoryRow {
  DoctorId?: string;
  doctorId?: string;
  UserId?: string;
  userId?: string;
  FirstName?: string;
  firstName?: string;
  LastName?: string;
  lastName?: string;
}

// Map backend AppointmentService entity -> Appointments page Appointment type
const toUiAppointment = async (
  row: BackendAppointmentRow,
  services: Service[] = []
): Promise<Appointment> => {
  const start = new Date(row.scheduledAt);
  // Store ISO date to ensure UI parsing with new Date(date) works reliably
  const date = start.toISOString();
  const time = start.toLocaleTimeString([], {
    hour: "2-digit",
    minute: "2-digit",
  });

  // Resolve doctor name via Practitioner directory or fallback to user profile
  const doctorId = String(row.doctorId ?? "").toLowerCase();
  let doctorName = "Unknown Doctor";
  let doctorUserId = "";
  try {
    const list = await api.get("/practitioner/doctors");
    const rows = Array.isArray(list.data)
      ? (list.data as DoctorDirectoryRow[])
      : [];
    const d = rows.find(
      (r) =>
        String(r.DoctorId ?? r.doctorId ?? "").toLowerCase() === doctorId ||
        String(r.UserId ?? r.userId ?? "").toLowerCase() === doctorId
    );
    if (d) {
      const first = String(d.FirstName ?? d.firstName ?? "");
      const last = String(d.LastName ?? d.lastName ?? "");
      const name = `${first} ${last}`.trim();
      if (name) {
        doctorName = name;
      } else {
        // Doctor found but no name in directory - get userId for fallback lookup
        doctorUserId = String(d.UserId ?? d.userId ?? "");
      }
    }

    // Fallback: if still unknown, try fetching user profile directly
    if (doctorName === "Unknown Doctor" && (doctorUserId || doctorId)) {
      const userIdToFetch = doctorUserId || doctorId;
      try {
        const u = await api.get(`/users/${userIdToFetch}`);
        const first = String(u.data?.firstName ?? u.data?.FirstName ?? "");
        const last = String(u.data?.lastName ?? u.data?.LastName ?? "");
        const name = `${first} ${last}`.trim();
        if (name) doctorName = name;
      } catch {
        // ignore user fetch failure
      }
    }
  } catch {
    // ignore
  }

  const statusRaw = String(row.status ?? "Scheduled");
  const now = new Date();
  const isPast = start.getTime() < now.getTime();
  let status: Appointment["status"] = "upcoming";
  if (statusRaw.toLowerCase() === "cancelled") status = "cancelled";
  else if (isPast) status = "past";

  return {
    id: String(row.id),
    date,
    time,
    doctor: doctorName,
    specialization: "General",
    description: row.notes || "",
    status,
    paymentStatus: row.isPaid || row.IsPaid ? "paid" : "not_paid",
    total: row.isPaid || row.IsPaid ? 0 : 300,
    serviceName:
      services.find((s) => s.id === (row.serviceId || row.ServiceId))?.name ||
      "General Consultation",
  };
};

export const appointmentsApi = {
  // Fetch all appointments for the given patient (current user)
  getAppointmentsForPatient: async (
    patientId: string
  ): Promise<ApiResponse<Appointment[]>> => {
    try {
      const resp = await api.get(
        `/appointment/appointments/patient/${patientId}`
      );
      const items = Array.isArray(resp.data) ? resp.data : [];

      // Fetch services to map definitions
      const servicesRes = await schedulerApi.getServices();
      const services = servicesRes.success ? servicesRes.data : [];

      const mapped = await Promise.all(
        items.map((item) => toUiAppointment(item, services))
      );
      return { data: mapped, success: true };
    } catch (error) {
      console.error("Failed to fetch appointments", error);
      return createErrorResponse("Failed to fetch appointments");
    }
  },

  // Cancel an appointment via AppointmentService
  cancelAppointment: async (id: string): Promise<ApiResponse<Appointment>> => {
    try {
      await api.put(`/appointment/appointments/${id}/status`, {
        status: "Cancelled",
      });
      // Return minimal shape; caller updates local state
      return {
        data: {
          id,
          date: "",
          time: "",
          doctor: "",
          status: "cancelled",
          paymentStatus: "not_paid",
          total: 0,
        },
        success: true,
      } as ApiResponse<Appointment>;
    } catch (error) {
      console.error("Failed to cancel appointment", error);
      return createErrorResponse("Failed to cancel appointment");
    }
  },

  // Placeholder payment status update (no real backend yet)
  updatePaymentStatus: async (
    _id: string,
    paymentData: { paymentStatus: "paid" | "not_paid" }
  ): Promise<ApiResponse<Appointment>> => {
    try {
      // No real billing linkage on this view; return success to allow UI state updates
      return {
        data: {
          id: _id,
          date: "",
          time: "",
          doctor: "",
          status: "upcoming",
          paymentStatus: paymentData.paymentStatus,
          total: 0,
        } as Appointment,
        success: true,
      } as ApiResponse<Appointment>;
    } catch (error) {
      console.error("Failed to update payment status", error);
      return createErrorResponse("Failed to update payment status");
    }
  },
};
