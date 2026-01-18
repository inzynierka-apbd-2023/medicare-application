import { toastMessages } from "../toast/toastMessages";

import { api } from "./api";

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

const doctorDashboardApi = {
  async getQuickStats(doctorId: string): Promise<DoctorQuickStat[]> {
    const response = await api.get<DoctorQuickStatsResponse>(
      `/appointment/doctor-dashboard/${doctorId}/quick-stats`
    );
    return response.stats;
  },

  async getDoctorByUserId(userId: string): Promise<DoctorProfile> {
    return await api.get<DoctorProfile>(
      `/practitioner/doctors/by-user/${userId}`
    );
  },

  async registerDoctor(userId: string): Promise<DoctorProfile> {
    return await api.post<DoctorProfile>(
      `/practitioner/doctors`,
      { userId, bio: "Auto-generated profile" },
      undefined,
      {
        showToastOnSuccess: true,
        successMessage: toastMessages.doctorDashboard.registerSuccess,
      }
    );
  },
};

export default doctorDashboardApi;
