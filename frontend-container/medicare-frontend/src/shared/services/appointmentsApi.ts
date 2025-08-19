import type { Appointment } from "../../features/appointments/types";

import { type ApiResponse, createErrorResponse } from "./api";
import { apiClient as api } from "./apiClient";

// Map backend AppointmentService entity -> Appointments page Appointment type
const toUiAppointment = async (row: any): Promise<Appointment> => {
  const start = new Date(row.scheduledAt);
  // Store ISO date to ensure UI parsing with new Date(date) works reliably
  const date = start.toISOString();
  const time = start.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });

  // Resolve doctor name via Practitioner directory or fallback to user profile
  const doctorId = String(row.doctorId ?? "");
  let doctorName = "Unknown Doctor";
  try {
    const list = await api.get("/practitioner/doctors");
    const rows = Array.isArray(list.data) ? list.data : [];
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const d = rows.find(
      (r: any) =>
        String(r.DoctorId ?? r.doctorId) === doctorId ||
        String(r.UserId ?? r.userId) === doctorId
    );
    if (d) {
      const first = String(d.FirstName ?? d.firstName ?? "");
      const last = String(d.LastName ?? d.lastName ?? "");
      const name = `${first} ${last}`.trim();
      if (name) doctorName = name;
    } else if (doctorId) {
      // Fallback to practitioner entity then user profile
      try {
        const dr = await api.get(`/practitioner/doctors/${doctorId}`);
        const userId = String(dr.data?.userId ?? dr.data?.UserId ?? "");
        if (userId) {
          try {
            const u = await api.get(`/users/${userId}`);
            const first = String(u.data?.firstName ?? u.data?.FirstName ?? "");
            const last = String(u.data?.lastName ?? u.data?.LastName ?? "");
            const name = `${first} ${last}`.trim();
            if (name) doctorName = name;
          } catch {
            // ignore user fetch failure
          }
        } else {
          // If no userId in practitioner entity, attempt direct user fetch by doctorId
          try {
            const u = await api.get(`/users/${doctorId}`);
            const first = String(u.data?.firstName ?? u.data?.FirstName ?? "");
            const last = String(u.data?.lastName ?? u.data?.LastName ?? "");
            const name = `${first} ${last}`.trim();
            if (name) doctorName = name;
          } catch {
            // ignore
          }
        }
      } catch {
        // Practitioner doctor not found; try treating doctorId as userId directly
        try {
          const u = await api.get(`/users/${doctorId}`);
          const first = String(u.data?.firstName ?? u.data?.FirstName ?? "");
          const last = String(u.data?.lastName ?? u.data?.LastName ?? "");
          const name = `${first} ${last}`.trim();
          if (name) doctorName = name;
        } catch {
          // ignore
        }
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
    paymentStatus: "not_paid",
    total: 0,
  };
};

export const appointmentsApi = {
  // Fetch all appointments for the given patient (current user)
  getAppointmentsForPatient: async (
    patientId: string
  ): Promise<ApiResponse<Appointment[]>> => {
    try {
      const resp = await api.get(`/appointment/appointments/patient/${patientId}`);
      const items = Array.isArray(resp.data) ? resp.data : [];
      const mapped = await Promise.all(items.map(toUiAppointment));
      return { data: mapped, success: true };
    } catch (error) {
      console.error("Failed to fetch appointments", error);
      return createErrorResponse("Failed to fetch appointments");
    }
  },

  // Cancel an appointment via AppointmentService
  cancelAppointment: async (
    id: string
  ): Promise<ApiResponse<Appointment>> => {
    try {
      await api.put(`/appointment/appointments/${id}/status`, { status: "Cancelled" });
      // Return minimal shape; caller updates local state
      return { data: { id, date: "", time: "", doctor: "", status: "cancelled", paymentStatus: "not_paid", total: 0 }, success: true } as ApiResponse<Appointment>;
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
