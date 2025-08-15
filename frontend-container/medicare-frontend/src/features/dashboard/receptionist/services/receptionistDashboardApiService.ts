import { apiClient as api } from "../../../../shared/services/apiClient";
import type {
  DoctorAvailability,
  QuickAppointment,
  ReceptionistDashboardData,
  ReceptionistDashboardStats,
} from "../types";

// Mock data for development
const generateMockStats = (): ReceptionistDashboardStats => ({
  totalAppointments: 45,
  todayAppointments: 12,
  totalDoctors: 8,
  availableDoctors: 5,
});

const generateMockTodayAppointments = (): QuickAppointment[] => [
  {
    id: "apt-1",
    patientName: "Sarah Johnson",
    doctorName: "Dr. Emily Chen",
    time: "09:00",
    type: "in-person",
    status: "completed",
    room: "Room 101",
  },
  {
    id: "apt-2",
    patientName: "Michael Davis",
    doctorName: "Dr. Robert Martinez",
    time: "09:30",
    type: "video-call",
    status: "in-progress",
  },
  {
    id: "apt-3",
    patientName: "Jennifer Wilson",
    doctorName: "Dr. Lisa Thompson",
    time: "10:00",
    type: "in-person",
    status: "waiting",
    room: "Room 102",
  },
  {
    id: "apt-4",
    patientName: "David Brown",
    doctorName: "Dr. James Wilson",
    time: "10:30",
    type: "phone",
    status: "waiting",
  },
  {
    id: "apt-5",
    patientName: "Amanda Garcia",
    doctorName: "Dr. Maria Garcia",
    time: "11:00",
    type: "in-person",
    status: "waiting",
    room: "Room 103",
  },
];

const generateMockDoctorAvailability = (): DoctorAvailability[] => [
  {
    id: "doc-1",
    name: "Dr. Emily Chen",
    specialization: "Cardiology",
    status: "available",
    totalAppointments: 6,
    completedToday: 2,
    nextAvailable: "11:30",
  },
  {
    id: "doc-2",
    name: "Dr. Robert Martinez",
    specialization: "Dermatology",
    status: "busy",
    currentPatient: "Michael Davis",
    totalAppointments: 5,
    completedToday: 1,
    nextAvailable: "10:15",
  },
  {
    id: "doc-3",
    name: "Dr. Lisa Thompson",
    specialization: "Internal Medicine",
    status: "busy",
    currentPatient: "Jennifer Wilson",
    totalAppointments: 7,
    completedToday: 2,
    nextAvailable: "10:30",
  },
  {
    id: "doc-4",
    name: "Dr. James Wilson",
    specialization: "Pediatrics",
    status: "available",
    totalAppointments: 4,
    completedToday: 1,
    nextAvailable: "10:30",
  },
  {
    id: "doc-5",
    name: "Dr. Maria Garcia",
    specialization: "Orthopedics",
    status: "available",
    totalAppointments: 3,
    completedToday: 0,
    nextAvailable: "11:00",
  },
];

// Configuration flag to enable/disable mock mode
const USE_MOCK_DATA = true;

export class ReceptionistDashboardApiService {
  // Helper function to simulate API delay
  private static delay(ms: number = 300): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }

  /**
   * Get dashboard data for receptionist
   */
  static async getDashboardData(): Promise<ReceptionistDashboardData> {
    await this.delay();

    if (USE_MOCK_DATA) {
      return {
        stats: generateMockStats(),
        todayAppointments: generateMockTodayAppointments(),
        doctorAvailability: generateMockDoctorAvailability(),
      };
    }

    try {
      const response = await api.get("/receptionist/dashboard");
      return response.data;
    } catch (error) {
      console.error("Error fetching dashboard data:", error);
      throw new Error("Failed to fetch dashboard data");
    }
  }

  /**
   * Get real-time updates for dashboard
   */
  static async getRealtimeUpdates(): Promise<
    Partial<ReceptionistDashboardData>
  > {
    await this.delay(100);

    if (USE_MOCK_DATA) {
      // Return partial updates (in real app, this might come from WebSocket)
      return {
        stats: generateMockStats(),
        doctorAvailability: generateMockDoctorAvailability(),
      };
    }

    try {
      const response = await api.get("/receptionist/dashboard/updates");
      return response.data;
    } catch (error) {
      console.error("Error fetching realtime updates:", error);
      return {};
    }
  }
}
