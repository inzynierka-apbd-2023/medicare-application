import type { DoctorScheduleEvent } from "../../features/scheduler/types/doctorScheduler";
import { ApiResponse, createErrorResponse } from "./api";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:8080";

export interface DoctorScheduleResponse {
  schedule: DoctorScheduleEvent[];
  totalCount: number;
}

class DoctorScheduleApiService {
  private getAuthHeaders() {
    const token = localStorage.getItem("authToken") || sessionStorage.getItem("authToken");
    return {
      "Content-Type": "application/json",
      ...(token && { Authorization: `Bearer ${token}` }),
    };
  }

  async getDoctorSchedule(
    doctorId: string,
    startDate?: string,
    endDate?: string,
    status?: string
  ): Promise<ApiResponse<DoctorScheduleEvent[]>> {
    try {
      const params = new URLSearchParams();
      if (startDate) params.append("startDate", startDate);
      if (endDate) params.append("endDate", endDate);
      if (status) params.append("status", status);

      const response = await fetch(
        `${API_BASE_URL}/api/appointment/doctor-schedule/${doctorId}?${params}`,
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
      console.error("Failed to fetch doctor schedule:", error);
      return createErrorResponse("Failed to fetch doctor schedule");
    }
  }

  async getTodaysAppointments(
    doctorId: string
  ): Promise<ApiResponse<DoctorScheduleEvent[]>> {
    try {
      const response = await fetch(
        `${API_BASE_URL}/api/appointment/doctor-schedule/${doctorId}/today`,
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
        `${API_BASE_URL}/api/appointment/doctor-schedule/appointment/${appointmentId}`,
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
        `${API_BASE_URL}/api/appointment/doctor-schedule/appointment/${appointmentId}/status`,
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
        `${API_BASE_URL}/api/appointment/doctor-schedule/appointment/${appointmentId}/status`,
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
        `${API_BASE_URL}/api/appointment/doctor-schedule/appointment/${appointmentId}/notes`,
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
