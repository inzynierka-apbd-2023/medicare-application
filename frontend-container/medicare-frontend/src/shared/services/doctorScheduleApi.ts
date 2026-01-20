import type { DoctorScheduleEvent } from "@features/scheduler/types/doctorScheduler";
import { toastMessages } from "@shared/toast/toastMessages";

import { api } from "./api";

export interface DoctorScheduleResponse {
  schedule: DoctorScheduleEvent[];
  totalCount: number;
}

interface StatusUpdateRequest {
  status: "completed" | "no-show";
}

interface NotesUpdateRequest {
  notes: string;
}

const SCHEDULE_BASE_URL = "/appointment/doctor-schedule";

const doctorScheduleApi = {
  getDoctorSchedule: async (
    doctorId: string,
    startDate?: string,
    endDate?: string
  ): Promise<DoctorScheduleEvent[]> => {
    const startStr = startDate || "";
    const endStr = endDate || "";
    const url = `${SCHEDULE_BASE_URL}/${doctorId}?startDate=${encodeURIComponent(startStr)}&endDate=${encodeURIComponent(endStr)}`;

    const responseData = await api.get<DoctorScheduleResponse>(url);
    return responseData.schedule || [];
  },

  getTodaysAppointments: async (
    doctorId: string
  ): Promise<DoctorScheduleEvent[]> => {
    const url = `${SCHEDULE_BASE_URL}/${doctorId}/today`;
    const responseData = await api.get<DoctorScheduleResponse>(url);
    return responseData.schedule;
  },

  getAppointmentDetails: async (
    appointmentId: string
  ): Promise<DoctorScheduleEvent> => {
    const url = `${SCHEDULE_BASE_URL}/appointment/${appointmentId}`;
    return api.get<DoctorScheduleEvent>(url);
  },

  markAppointmentCompleted: async (appointmentId: string): Promise<boolean> => {
    const url = `${SCHEDULE_BASE_URL}/appointment/${appointmentId}/status`;
    const body: StatusUpdateRequest = { status: "completed" };
    await api.put<void>(url, body, undefined, {
      showToastOnSuccess: true,
      successMessage: toastMessages.doctorSchedule.markCompletedSuccess,
    });
    return true;
  },

  markAppointmentNoShow: async (appointmentId: string): Promise<boolean> => {
    const url = `${SCHEDULE_BASE_URL}/appointment/${appointmentId}/status`;
    const body: StatusUpdateRequest = { status: "no-show" };
    await api.put<void>(url, body, undefined, {
      showToastOnSuccess: true,
      successMessage: toastMessages.doctorSchedule.markNoShowSuccess,
    });
    return true;
  },

  addAppointmentNotes: async (
    appointmentId: string,
    notes: string
  ): Promise<boolean> => {
    const url = `${SCHEDULE_BASE_URL}/appointment/${appointmentId}/notes`;
    const body: NotesUpdateRequest = { notes };
    await api.put<void>(url, body, undefined, {
      showToastOnSuccess: true,
      successMessage: toastMessages.doctorSchedule.addNotesSuccess,
    });
    return true;
  },
};

export default doctorScheduleApi;
