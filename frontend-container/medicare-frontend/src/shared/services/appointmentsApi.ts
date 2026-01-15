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
  services: Service[],
  doctorDirectory: DoctorDirectoryRow[]
): Promise<Appointment> => {
  const start = new Date(row.scheduledAt);
  const date = start.toISOString();
  const time = start.toLocaleTimeString([], {
    hour: "2-digit",
    minute: "2-digit",
  });

  // Resolve doctor name via passed directory or fallback to user profile
  const doctorId = String(row.doctorId ?? "").toLowerCase();
  let doctorName = "Unknown Doctor";

  const d = doctorDirectory.find(
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
    }
  }

  // Fallback: This N+1 is harder to remove completely without bulk user fetch,
  // but usually directory covers active doctors.
  // We will skip the extra per-user fetch to solve the performance issue reported.
  // If really needed, we'd need a bulk user fetch endpoint.

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

      // Fetch doctor directory ONCE
      let doctorDirectory: DoctorDirectoryRow[] = [];
      try {
        const docResp = await api.get("/practitioner/doctors");
        if (Array.isArray(docResp.data)) {
          doctorDirectory = docResp.data as DoctorDirectoryRow[];
        }
      } catch (e) {
        console.warn("Failed to fetch doctor directory", e);
      }

      const mapped = await Promise.all(
        items.map((item) => toUiAppointment(item, services, doctorDirectory))
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
  // Submit a doctor rating
  rateAppointment: async (
    id: string,
    rating: number,
    description?: string
  ): Promise<ApiResponse<void>> => {
    try {
      await api.post(`/appointment/appointments/${id}/rate`, {
        rating,
        description,
      });
      return { success: true, data: undefined };
    } catch (error) {
      console.error("Failed to submit rating", error);
      return createErrorResponse("Failed to submit rating");
    }
  },
};
