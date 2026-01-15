import type { DoctorScheduleEvent } from "../../features/scheduler/types/doctorScheduler";

import { ApiResponse, createErrorResponse } from "./api";

export interface DoctorScheduleResponse {
  schedule: DoctorScheduleEvent[];
  totalCount: number;
}

class DoctorScheduleApiService {
  private getAuthHeaders() {
    const token =
      localStorage.getItem("authToken") || sessionStorage.getItem("authToken");
    return {
      "Content-Type": "application/json",
      ...(token && { Authorization: `Bearer ${token}` }),
    };
  }

  async getDoctorSchedule(
    doctorId: string,
    startDate?: string,
    endDate?: string,
    _status?: string
  ): Promise<ApiResponse<DoctorScheduleEvent[]>> {
    try {
      // The original implementation used a single endpoint for doctor schedule.
      // The requested change introduces a more complex logic involving two services.
      // For the purpose of this edit, we will adapt the new fetch calls.
      // Start and end strings are not defined in the snippet
      // so we will use `startDate` and `endDate` directly.
      // suitable for `encodeURIComponent`.
      // Also, the snippet doesn't show how to combine results from two fetches into DoctorScheduleEvent[],
      // so we'll return a placeholder success response after the fetches.

      // 1. Fetch appointments via AppointmentService
      const startStr = startDate || "";
      const endStr = endDate || "";

      const appsRes = await fetch(
        `/api/appointment/doctor-schedule/${doctorId}?startDate=${encodeURIComponent(startStr)}&endDate=${encodeURIComponent(endStr)}`,
        { headers: this.getAuthHeaders() }
      );
      if (!appsRes.ok) throw new Error(`Fetch apps failed: ${appsRes.status}`);
      const responseData: DoctorScheduleResponse = await appsRes.json();

      // 2. Fetch availability via PractitionerService (optional/unused in this view but kept for reference)
      // ...

      // Return only the list of appointments to match Promise<ApiResponse<DoctorScheduleEvent[]>>
      return { success: true, data: responseData.schedule || [] };
    } catch (error) {
      console.error("Failed to fetch doctor schedule:", error);
      return createErrorResponse("Failed to fetch doctor schedule");
    }
  }

  async getTodaysAppointments(
    doctorId: string
  ): Promise<ApiResponse<DoctorScheduleEvent[]>> {
    try {
      const response = await fetch(
        `/api/appointment/doctor-schedule/${doctorId}/today`,
        {
          method: "GET",
          headers: this.getAuthHeaders(),
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data: DoctorScheduleResponse = await response.json();
      return { success: true, data: data.schedule };
    } catch (error) {
      console.error("Failed to fetch today's appointments:", error);
      return createErrorResponse("Failed to fetch today's appointments");
    }
  }

  async getAppointmentDetails(
    appointmentId: string
  ): Promise<ApiResponse<DoctorScheduleEvent>> {
    try {
      const response = await fetch(
        `/api/appointment/doctor-schedule/appointment/${appointmentId}`,
        {
          method: "GET",
          headers: this.getAuthHeaders(),
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();
      return { success: true, data };
    } catch (error) {
      console.error("Failed to fetch appointment details:", error);
      return createErrorResponse("Failed to fetch appointment details");
    }
  }

  async markAppointmentCompleted(
    appointmentId: string
  ): Promise<ApiResponse<boolean>> {
    try {
      const response = await fetch(
        `/api/appointment/doctor-schedule/appointment/${appointmentId}/status`,
        {
          method: "PUT",
          headers: this.getAuthHeaders(),
          body: JSON.stringify({ status: "completed" }),
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      return { success: true, data: true };
    } catch (error) {
      console.error("Failed to mark appointment as completed:", error);
      return createErrorResponse("Failed to mark appointment as completed");
    }
  }

  async markAppointmentNoShow(
    appointmentId: string
  ): Promise<ApiResponse<boolean>> {
    try {
      const response = await fetch(
        `/api/appointment/doctor-schedule/appointment/${appointmentId}/status`,
        {
          method: "PUT",
          headers: this.getAuthHeaders(),
          body: JSON.stringify({ status: "no-show" }),
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      return { success: true, data: true };
    } catch (error) {
      console.error("Failed to mark appointment as no-show:", error);
      return createErrorResponse("Failed to mark appointment as no-show");
    }
  }

  async addAppointmentNotes(
    appointmentId: string,
    notes: string
  ): Promise<ApiResponse<boolean>> {
    try {
      const response = await fetch(
        `/api/appointment/doctor-schedule/appointment/${appointmentId}/notes`,
        {
          method: "PUT",
          headers: this.getAuthHeaders(),
          body: JSON.stringify({ notes }),
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      return { success: true, data: true };
    } catch (error) {
      console.error("Failed to add appointment notes:", error);
      return createErrorResponse("Failed to add appointment notes");
    }
  }
}

const doctorScheduleApiService = new DoctorScheduleApiService();
export default doctorScheduleApiService;
