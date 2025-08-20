import { ApiResponse, createErrorResponse } from "./api";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:8080";

export interface DoctorQuickStat {
  label: string;
  value: number;
  change?: string;
  trend?: string;
}

export interface DoctorQuickStatsResponse {
  stats: DoctorQuickStat[];
}

class DoctorDashboardApiService {
  private getAuthHeaders() {
    const token = localStorage.getItem("authToken") || sessionStorage.getItem("authToken");
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
        `${API_BASE_URL}/api/appointment/doctor-dashboard/${doctorId}/quick-stats`,
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
}

const doctorDashboardApiService = new DoctorDashboardApiService();
export default doctorDashboardApiService;
