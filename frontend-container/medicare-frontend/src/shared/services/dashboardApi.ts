import { api } from "./api";

// Types for dashboard data
export interface Notification {
  id: string;
  message: string;
  type?: "info" | "warning" | "success" | "error";
  timestamp?: string;
  read?: boolean;
}

export interface Document {
  id: string;
  title: string;
  date: string;
  type?: string;
  size?: string;
}

export interface QuickStat {
  label: string;
  value: number;
  change?: string;
  trend?: "up" | "down" | "stable";
}

export interface PatientMessage {
  id: number;
  patient: string;
  text: string;
  timestamp?: string;
  unread?: boolean;
}

// Owner Dashboard Specific Types
export interface BusinessMetric {
  label: string;
  value: number | string;
  change?: string;
  trend?: "up" | "down" | "stable";
  period?: string;
  icon?: string;
}

export interface RevenueData {
  period: string;
  revenue: number;
  appointments: number;
  avgRevenue: number;
}

export interface StaffProductivity {
  doctorId: string;
  doctorName: string;
  specialization: string;
  appointmentsToday: number;
  appointmentsThisWeek: number;
  appointmentsThisMonth: number;
  averageRating: number;
  totalRevenue: number;
  status: "active" | "on-break" | "offline";
}

export interface FacilityOverview {
  totalPatients: number;
  activePatients: number;
  newPatientsThisMonth: number;
  totalDoctors: number;
  activeDoctors: number;
  totalAppointments: number;
  completedAppointments: number;
  cancelledAppointments: number;
  noShowAppointments: number;
  monthlyRevenue: number;
  yearlyRevenue: number;
}

// Admin Dashboard Specific Types
export interface UserManagementStats {
  totalUsers: number;
  activeUsers: number;
  newUsersThisMonth: number;
  pendingApprovals: number;
  suspendedUsers: number;
  lastLoginActivity: string;
}

export interface UserAccount {
  userId: string;
  username: string;
  email: string;
  role: "Patient" | "Doctor" | "Owner" | "Admin" | "PatientServiceEmployee";
  status: "active" | "pending" | "suspended" | "inactive";
  createdDate: string;
  lastLogin: string;
  profileComplete: boolean;
}

export interface SystemActivity {
  activityId: string;
  timestamp: string;
  userRole: string;
  action: string;
  description: string;
  ipAddress?: string;
  success: boolean;
}

export interface SecurityMetric {
  label: string;
  value: number | string;
  status: "good" | "warning" | "critical";
  description: string;
  icon?: string;
}

export interface AppointmentMetric {
  label: string;
  value: number;
  change?: string;
  trend?: "up" | "down" | "stable";
  period?: string;
  icon?: string;
}

export interface RevenueChartData {
  date: string;
  revenue: number;
  appointments: number;
}

export interface AppointmentsBySpecializationData {
  specialization: string;
  appointments: number;
  revenue: number;
  color: string;
}

export interface DoctorProductivityData {
  doctorName: string;
  specialization: string;
  completedAppointments: number;
  totalRevenue: number;
  completionRate: number;
}

export interface DoctorPerformance {
  doctorId: string;
  doctorName: string;
  specialization: string;
  totalAppointments: number;
  completedAppointments: number;
  cancelledAppointments: number;
  noShowAppointments: number;
  completionRate: number;
  averageRevenue: number;
  patientSatisfaction: number;
  busyHours: string[];
}

interface BackendDocument {
  id: string;
  type: number;
  createdAt?: string;
  notes?: string;
  patientId?: string;
}

const getDocumentType = (typeInt: number): string => {
  switch (typeInt) {
    case 1:
      return "VisitCard";
    case 2:
      return "Prescription";
    case 3:
      return "Referral";
    case 4:
      return "Sick_Leave";
    case 5:
      return "Lab_Results";
    default:
      return "Other";
  }
};

export const patientDashboardApi = {
  getRecentDocuments: async (userId: string): Promise<Document[]> => {
    if (!userId) throw new Error("User ID required");

    const params = { patientId: userId };
    const rawDocs = await api.get<BackendDocument[]>("/documents", { params });

    const items = Array.isArray(rawDocs) ? rawDocs : [];

    const mapped: Document[] = items.map((d) => {
      const typeStr = getDocumentType(d.type);
      return {
        id: String(d.id),
        title: d.notes || typeStr || "Untitled",
        date: d.createdAt
          ? new Date(d.createdAt).toLocaleDateString()
          : new Date().toLocaleDateString(),
        type: typeStr,
        size: "100 KB",
      };
    });

    return mapped;
  },

  markNotificationAsRead: async (notificationId: string): Promise<boolean> => {
    await api.post(`/notifications/${notificationId}/read`, {}, undefined, {
      showToastOnSuccess: false,
    });

    if (typeof window !== "undefined") {
      window.dispatchEvent(
        new CustomEvent("notifications:updated", {
          detail: { kind: "read", id: notificationId },
        })
      );
    }

    return true;
  },
};
