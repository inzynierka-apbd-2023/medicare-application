import type { Appointment } from "@features/appointments/types";
import type { Service } from "@features/scheduler/types";
import { toastMessages } from "@shared/toast/toastMessages";

import { api, type ApiResponse, handleApiCall } from "./api";

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

const toUiAppointment = (
  row: BackendAppointmentRow,
  services: Service[],
  doctorDirectory: DoctorDirectoryRow[]
): Appointment => {
  const start = new Date(row.scheduledAt);
  const date = start.toISOString();
  const time = start.toLocaleTimeString([], {
    hour: "2-digit",
    minute: "2-digit",
  });

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

  const statusRaw = String(row.status ?? "Scheduled");
  const now = new Date();
  const isPast = start.getTime() < now.getTime();
  let status: Appointment["status"] = "upcoming";
  if (statusRaw.toLowerCase() === "cancelled") status = "cancelled";
  else if (isPast) status = "past";

  const serviceId = row.serviceId || row.ServiceId;
  const service = services.find((s) => s.id === serviceId);

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
    serviceName: service?.name || "General Consultation",
  };
};

export const appointmentsApi = {
  getAppointmentsForPatient: async (
    patientId: string
  ): Promise<ApiResponse<Appointment[]>> => {
    return handleApiCall<Appointment[]>(
      async () => {
        const [resp, servicesResp, docResp] = await Promise.allSettled([
          api.get<BackendAppointmentRow[]>(
            `/appointment/appointments/patient/${patientId}`,
            undefined,
            { showToastOnError: false }
          ),
          api.get<Service[]>("/practitioner/catalog/services", undefined, {
            showToastOnError: false,
          }),
          api.get<DoctorDirectoryRow[]>("/practitioner/doctors", undefined, {
            showToastOnError: false,
          }),
        ]);

        if (resp.status === "rejected") {
          throw new Error(
            String(
              resp.reason?.message || toastMessages.appointments.fetchError
            )
          );
        }

        const items = Array.isArray(resp.value) ? resp.value : [];
        const services =
          servicesResp.status === "fulfilled" &&
          Array.isArray(servicesResp.value)
            ? servicesResp.value
            : [];
        const doctorDirectory =
          docResp.status === "fulfilled" && Array.isArray(docResp.value)
            ? docResp.value
            : [];

        return items.map((item) =>
          toUiAppointment(item, services, doctorDirectory)
        );
      },
      {
        showToastOnSuccess: false,
        showToastOnError: true,
      }
    );
  },

  cancelAppointment: async (id: string): Promise<ApiResponse<Appointment>> => {
    return handleApiCall<Appointment>(
      async () => {
        await api.put(
          `/appointment/appointments/${id}/status`,
          { status: "Cancelled" },
          undefined,
          { showToastOnSuccess: false }
        );

        return {
          id,
          date: "",
          time: "",
          doctor: "",
          status: "cancelled",
          paymentStatus: "not_paid",
          total: 0,
          specialization: "",
          description: "",
          serviceName: "",
        } as Appointment;
      },
      {
        showToastOnSuccess: true,
        successMessage: toastMessages.appointments.cancelSuccess,
        showToastOnError: true,
      }
    );
  },

  updatePaymentStatus: async (
    _id: string,
    paymentData: { paymentStatus: "paid" | "not_paid" }
  ): Promise<ApiResponse<Appointment>> => {
    return handleApiCall<Appointment>(
      async () => {
        await api.put(
          `/appointment/appointments/${_id}/payment`,
          paymentData,
          undefined,
          { showToastOnSuccess: false }
        );

        return {
          id: _id,
          date: "",
          time: "",
          doctor: "",
          status: "upcoming",
          paymentStatus: paymentData.paymentStatus,
          total: 0,
          specialization: "",
          description: "",
          serviceName: "",
        } as Appointment;
      },
      {
        showToastOnSuccess: true,
        successMessage: toastMessages.appointments.paymentUpdateSuccess,
        showToastOnError: true,
      }
    );
  },

  rateAppointment: async (
    id: string,
    rating: number,
    description?: string
  ): Promise<ApiResponse<void>> => {
    return handleApiCall<void>(
      async () => {
        await api.post(
          `/appointment/appointments/${id}/rate`,
          { rating, description },
          undefined,
          { showToastOnSuccess: false }
        );
      },
      {
        showToastOnSuccess: true,
        successMessage: toastMessages.appointments.rateSuccess,
        showToastOnError: true,
      }
    );
  },
};
