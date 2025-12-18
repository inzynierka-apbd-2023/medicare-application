import { ApiResponse, createErrorResponse } from "./api";

export interface DoctorQuickStat {
  label: string;
  value: number;
  change?: string;
  trend?: string;
}

export interface DoctorQuickStatsResponse {
  stats: DoctorQuickStat[];
}

export interface DoctorProfile {
  id: string;
  userId: string;
  firstName?: string;
  lastName?: string;
  [key: string]: unknown;
}

class DoctorDashboardApiService {
  private getAuthHeaders() {
    const token =
      localStorage.getItem("authToken") || sessionStorage.getItem("authToken");
    return {
      "Content-Type": "application/json",
      ...(token && { Authorization: `Bearer ${token}` }),
    };
  }

  async getQuickStats(
    doctorId: string
  ): Promise<ApiResponse<DoctorQuickStat[]>> {
    try {
      const response = await fetch(
        `/api/appointment/doctor-dashboard/${doctorId}/quick-stats`,
        {
          method: "GET",
          headers: this.getAuthHeaders(),
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data: DoctorQuickStatsResponse = await response.json();
      return { success: true, data: data.stats };
    } catch (error) {
      console.error("Failed to fetch doctor quick stats:", error);
      return createErrorResponse("Failed to fetch doctor quick stats");
    }
  }

  async getDoctorByUserId(userId: string): Promise<ApiResponse<DoctorProfile>> {
    try {
      const response = await fetch(
        `/api/practitioner/doctors/by-user/${userId}`,
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
      console.error("Failed to fetch doctor profile:", error);
      return createErrorResponse("Failed to fetch doctor profile");
    }
  }

  async registerDoctor(userId: string): Promise<ApiResponse<DoctorProfile>> {
    try {
      const response = await fetch(`/api/practitioner/doctors`, {
        method: "POST",
        headers: this.getAuthHeaders(),
        body: JSON.stringify({ userId, bio: "Auto-generated profile" }),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();
      return { success: true, data };
    } catch (error) {
      console.error("Failed to register doctor profile:", error);
      return createErrorResponse("Failed to register doctor profile");
    }
  }
}

const doctorDashboardApiService = new DoctorDashboardApiService();
export default doctorDashboardApiService;
