import { ApiResponse, createMockResponse } from "./api";

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

// Appointment Analytics Types
export interface AppointmentMetric {
  label: string;
  value: number;
  change?: string;
  trend?: "up" | "down" | "stable";
  period?: string;
  icon?: string;
}

// Basic Chart Data Interfaces
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

// Mock data storage
const MOCK_PATIENT_NOTIFICATIONS: Notification[] = [
  {
    id: "1",
    message:
      "Appointment Reminder: May 14, 2025 at 10:00 AM with Dr. Alice Heart",
    type: "info",
    timestamp: "2025-05-13T10:00:00Z",
    read: false,
  },
  {
    id: "2",
    message: "Lab Results Available: Cholesterol Panel",
    type: "success",
    timestamp: "2025-05-12T14:30:00Z",
    read: false,
  },
  {
    id: "3",
    message: "New Message: Follow-up from Dr. Bob Vessel",
    type: "info",
    timestamp: "2025-05-11T09:15:00Z",
    read: true,
  },
  {
    id: "4",
    message: "Your appointment with Dr. Alice Heart is tomorrow at 10:00 AM.",
    type: "warning",
    timestamp: "2025-05-13T08:00:00Z",
    read: false,
  },
  {
    id: "5",
    message: "Lab results from your blood test are available.",
    type: "success",
    timestamp: "2025-05-10T16:45:00Z",
    read: true,
  },
  {
    id: "6",
    message: "Reminder: Teleconsultation on May 20, 2025 at 3:00 PM.",
    type: "info",
    timestamp: "2025-05-09T11:00:00Z",
    read: false,
  },
  {
    id: "7",
    message: "Prescription #456 has been renewed.",
    type: "success",
    timestamp: "2025-05-08T13:20:00Z",
    read: true,
  },
  {
    id: "8",
    message: "New message from Dr. Bob Vessel regarding your test.",
    type: "info",
    timestamp: "2025-05-07T15:30:00Z",
    read: false,
  },
];

const MOCK_PATIENT_DOCUMENTS: Document[] = [
  {
    id: "1",
    title: "Prescription #456 issued",
    date: "May 10, 2025",
    type: "prescription",
    size: "125 KB",
  },
  {
    id: "2",
    title: "Referral to Cardiologist",
    date: "April 22, 2025",
    type: "referral",
    size: "89 KB",
  },
  {
    id: "3",
    title: "Blood Test Results",
    date: "March 15, 2025",
    type: "lab_result",
    size: "203 KB",
  },
];

// API functions for Patient Dashboard
export const patientDashboardApi = {
  getDocuments: (): Promise<ApiResponse<Document[]>> => {
    return createMockResponse(MOCK_PATIENT_DOCUMENTS, 200);
  },

  markNotificationAsRead: (
    notificationId: string
  ): Promise<ApiResponse<boolean>> => {
    const notification = MOCK_PATIENT_NOTIFICATIONS.find(
      (n) => n.id === notificationId
    );
    if (notification) {
      notification.read = true;
    }
    return createMockResponse(true, 100);
  },
};

// Mock data for Owner Dashboard
const MOCK_OWNER_NOTIFICATIONS: Notification[] = [
  {
    id: "o1",
    message: "Monthly revenue increased by 12% compared to last month",
    type: "success",
    timestamp: "2025-01-10T09:00:00Z",
    read: false,
  },
  {
    id: "o2",
    message: "Dr. Smith has reached 95% patient satisfaction rate",
    type: "success",
    timestamp: "2025-01-10T08:30:00Z",
    read: false,
  },
  {
    id: "o3",
    message: "New doctor application received - Dr. Maria Garcia",
    type: "info",
    timestamp: "2025-01-09T16:45:00Z",
    read: true,
  },
  {
    id: "o4",
    message: "Facility capacity at 85% this week",
    type: "warning",
    timestamp: "2025-01-09T10:15:00Z",
    read: false,
  },
  {
    id: "o5",
    message: "Weekly report generated successfully",
    type: "info",
    timestamp: "2025-01-08T23:59:00Z",
    read: true,
  },
];

const MOCK_BUSINESS_METRICS: BusinessMetric[] = [
  {
    label: "Total Revenue",
    value: "€284,500",
    change: "+12.5%",
    trend: "up",
    period: "This Month",
    icon: "euro",
  },
  {
    label: "Total Patients",
    value: 1247,
    change: "+8.2%",
    trend: "up",
    period: "All Time",
    icon: "users",
  },
  {
    label: "Active Doctors",
    value: 18,
    change: "+2",
    trend: "up",
    period: "Current",
    icon: "userCheck",
  },
  {
    label: "Monthly Appointments",
    value: 2156,
    change: "+15.3%",
    trend: "up",
    period: "This Month",
    icon: "calendar",
  },
  {
    label: "Facility Utilization",
    value: "85%",
    change: "+5%",
    trend: "up",
    period: "This Week",
    icon: "building",
  },
  {
    label: "Patient Satisfaction",
    value: "4.8/5",
    change: "+0.2",
    trend: "up",
    period: "Overall",
    icon: "star",
  },
];

const MOCK_REVENUE_DATA: RevenueData[] = [
  {
    period: "January",
    revenue: 284500,
    appointments: 2156,
    avgRevenue: 131.95,
  },
  {
    period: "December",
    revenue: 252300,
    appointments: 1987,
    avgRevenue: 126.99,
  },
  {
    period: "November",
    revenue: 267800,
    appointments: 2089,
    avgRevenue: 128.15,
  },
  {
    period: "October",
    revenue: 245600,
    appointments: 1956,
    avgRevenue: 125.51,
  },
  {
    period: "September",
    revenue: 258900,
    appointments: 2034,
    avgRevenue: 127.26,
  },
  { period: "August", revenue: 271200, appointments: 2134, avgRevenue: 127.08 },
];

const MOCK_STAFF_PRODUCTIVITY: StaffProductivity[] = [
  {
    doctorId: "doc1",
    doctorName: "Dr. Alice Heart",
    specialization: "Cardiology",
    appointmentsToday: 8,
    appointmentsThisWeek: 42,
    appointmentsThisMonth: 187,
    averageRating: 4.9,
    totalRevenue: 23450,
    status: "active",
  },
  {
    doctorId: "doc2",
    doctorName: "Dr. Bob Vessel",
    specialization: "Neurology",
    appointmentsToday: 6,
    appointmentsThisWeek: 38,
    appointmentsThisMonth: 156,
    averageRating: 4.7,
    totalRevenue: 19800,
    status: "active",
  },
  {
    doctorId: "doc3",
    doctorName: "Dr. Carol Bones",
    specialization: "Orthopedics",
    appointmentsToday: 7,
    appointmentsThisWeek: 35,
    appointmentsThisMonth: 142,
    averageRating: 4.8,
    totalRevenue: 18600,
    status: "on-break",
  },
  {
    doctorId: "doc4",
    doctorName: "Dr. David Skin",
    specialization: "Dermatology",
    appointmentsToday: 9,
    appointmentsThisWeek: 45,
    appointmentsThisMonth: 201,
    averageRating: 4.6,
    totalRevenue: 21200,
    status: "active",
  },
  {
    doctorId: "doc5",
    doctorName: "Dr. Eve Mind",
    specialization: "Psychiatry",
    appointmentsToday: 5,
    appointmentsThisWeek: 28,
    appointmentsThisMonth: 124,
    averageRating: 4.9,
    totalRevenue: 16800,
    status: "active",
  },
];

const MOCK_FACILITY_OVERVIEW: FacilityOverview = {
  totalPatients: 1247,
  activePatients: 1189,
  newPatientsThisMonth: 102,
  totalDoctors: 18,
  activeDoctors: 16,
  totalAppointments: 2156,
  completedAppointments: 1987,
  cancelledAppointments: 89,
  noShowAppointments: 80,
  monthlyRevenue: 284500,
  yearlyRevenue: 3104800,
};

// API functions for Owner Dashboard
export const ownerDashboardApi = {
  getNotifications: (): Promise<ApiResponse<Notification[]>> => {
    return createMockResponse(MOCK_OWNER_NOTIFICATIONS, 300);
  },

  getBusinessMetrics: (): Promise<ApiResponse<BusinessMetric[]>> => {
    return createMockResponse(MOCK_BUSINESS_METRICS, 250);
  },

  getRevenueData: (): Promise<ApiResponse<RevenueData[]>> => {
    return createMockResponse(MOCK_REVENUE_DATA, 200);
  },

  getStaffProductivity: (): Promise<ApiResponse<StaffProductivity[]>> => {
    return createMockResponse(MOCK_STAFF_PRODUCTIVITY, 300);
  },

  getFacilityOverview: (): Promise<ApiResponse<FacilityOverview>> => {
    return createMockResponse(MOCK_FACILITY_OVERVIEW, 150);
  },

  markNotificationAsRead: (
    notificationId: string
  ): Promise<ApiResponse<boolean>> => {
    const notification = MOCK_OWNER_NOTIFICATIONS.find(
      (n) => n.id === notificationId
    );
    if (notification) {
      notification.read = true;
    }
    return createMockResponse(true, 100);
  },
};

// Mock data for Admin Dashboard
const MOCK_USER_MANAGEMENT_STATS: UserManagementStats = {
  totalUsers: 1583,
  activeUsers: 1489,
  newUsersThisMonth: 87,
  pendingApprovals: 23,
  suspendedUsers: 12,
  lastLoginActivity: "2 minutes ago",
};

const MOCK_USER_ACCOUNTS: UserAccount[] = [
  {
    userId: "usr001",
    username: "john.doe.patient",
    email: "john.doe@email.com",
    role: "Patient",
    status: "active",
    createdDate: "2024-12-15",
    lastLogin: "2025-01-10T14:30:00Z",
    profileComplete: true,
  },
  {
    userId: "usr002",
    username: "dr.alice.heart",
    email: "alice.heart@medicare.com",
    role: "Doctor",
    status: "active",
    createdDate: "2024-10-08",
    lastLogin: "2025-01-10T09:15:00Z",
    profileComplete: true,
  },
  {
    userId: "usr003",
    username: "maria.smith.patient",
    email: "maria.smith@email.com",
    role: "Patient",
    status: "pending",
    createdDate: "2025-01-09",
    lastLogin: "Never",
    profileComplete: false,
  },
  {
    userId: "usr004",
    username: "admin.system",
    email: "admin@medicare.com",
    role: "Admin",
    status: "active",
    createdDate: "2024-01-01",
    lastLogin: "2025-01-10T15:45:00Z",
    profileComplete: true,
  },
  {
    userId: "usr005",
    username: "dr.bob.vessel",
    email: "bob.vessel@medicare.com",
    role: "Doctor",
    status: "suspended",
    createdDate: "2024-08-22",
    lastLogin: "2025-01-05T11:20:00Z",
    profileComplete: true,
  },
  {
    userId: "usr006",
    username: "owner.main",
    email: "owner@medicare.com",
    role: "Owner",
    status: "active",
    createdDate: "2024-01-01",
    lastLogin: "2025-01-10T16:00:00Z",
    profileComplete: true,
  },
];

const MOCK_SYSTEM_ACTIVITIES: SystemActivity[] = [
  {
    activityId: "act001",
    timestamp: "2025-01-10T16:30:00Z",
    userRole: "Doctor",
    action: "Login",
    description: "Dr. Alice Heart logged in successfully",
    ipAddress: "192.168.1.105",
    success: true,
  },
  {
    activityId: "act002",
    timestamp: "2025-01-10T16:25:00Z",
    userRole: "Patient",
    action: "Profile Update",
    description: "John Doe updated contact information",
    ipAddress: "192.168.1.203",
    success: true,
  },
  {
    activityId: "act003",
    timestamp: "2025-01-10T16:20:00Z",
    userRole: "Admin",
    action: "User Suspension",
    description: "User dr.bob.vessel suspended by admin",
    ipAddress: "192.168.1.10",
    success: true,
  },
  {
    activityId: "act004",
    timestamp: "2025-01-10T16:15:00Z",
    userRole: "Patient",
    action: "Failed Login",
    description: "Failed login attempt for maria.smith.patient",
    ipAddress: "192.168.1.156",
    success: false,
  },
  {
    activityId: "act005",
    timestamp: "2025-01-10T16:10:00Z",
    userRole: "Owner",
    action: "System Access",
    description: "Owner accessed financial reports",
    ipAddress: "192.168.1.5",
    success: true,
  },
];

const MOCK_SECURITY_METRICS: SecurityMetric[] = [
  {
    label: "Failed Login Attempts",
    value: 12,
    status: "warning",
    description: "In the last 24 hours",
    icon: "shield",
  },
  {
    label: "Active Sessions",
    value: 234,
    status: "good",
    description: "Currently logged in users",
    icon: "users",
  },
  {
    label: "Password Expiry Alerts",
    value: 8,
    status: "warning",
    description: "Users with passwords expiring soon",
    icon: "key",
  },
  {
    label: "System Uptime",
    value: "99.9%",
    status: "good",
    description: "Last 30 days availability",
    icon: "server",
  },
  {
    label: "Suspicious Activities",
    value: 2,
    status: "critical",
    description: "Flagged for review",
    icon: "alertTriangle",
  },
  {
    label: "Data Backup Status",
    value: "Complete",
    status: "good",
    description: "Last backup 2 hours ago",
    icon: "database",
  },
];

// API functions for Admin Dashboard
export const adminDashboardApi = {
  getUserManagementStats: (): Promise<ApiResponse<UserManagementStats>> => {
    return createMockResponse(MOCK_USER_MANAGEMENT_STATS, 200);
  },

  getUserAccounts: (): Promise<ApiResponse<UserAccount[]>> => {
    return createMockResponse(MOCK_USER_ACCOUNTS, 300);
  },

  getSystemActivities: (): Promise<ApiResponse<SystemActivity[]>> => {
    return createMockResponse(MOCK_SYSTEM_ACTIVITIES, 250);
  },

  getSecurityMetrics: (): Promise<ApiResponse<SecurityMetric[]>> => {
    return createMockResponse(MOCK_SECURITY_METRICS, 200);
  },

  approveUser: (userId: string): Promise<ApiResponse<boolean>> => {
    const user = MOCK_USER_ACCOUNTS.find((u) => u.userId === userId);
    if (user) {
      user.status = "active";
      user.profileComplete = true;
    }
    return createMockResponse(true, 150);
  },

  suspendUser: (userId: string): Promise<ApiResponse<boolean>> => {
    const user = MOCK_USER_ACCOUNTS.find((u) => u.userId === userId);
    if (user) {
      user.status = "suspended";
    }
    return createMockResponse(true, 150);
  },

  activateUser: (userId: string): Promise<ApiResponse<boolean>> => {
    const user = MOCK_USER_ACCOUNTS.find((u) => u.userId === userId);
    if (user) {
      user.status = "active";
    }
    return createMockResponse(true, 150);
  },

  deleteUser: (userId: string): Promise<ApiResponse<boolean>> => {
    const userIndex = MOCK_USER_ACCOUNTS.findIndex((u) => u.userId === userId);
    if (userIndex !== -1) {
      MOCK_USER_ACCOUNTS.splice(userIndex, 1);
    }
    return createMockResponse(true, 200);
  },

  createUser: (
    userData: Omit<UserAccount, "userId" | "createdDate" | "lastLogin">
  ): Promise<ApiResponse<UserAccount>> => {
    const newUser: UserAccount = {
      ...userData,
      userId: `usr${(Math.random() * 1000).toString().padStart(3, "0")}`,
      createdDate: new Date().toISOString().split("T")[0],
      lastLogin: "Never",
    };
    MOCK_USER_ACCOUNTS.push(newUser);
    return createMockResponse(newUser, 250);
  },

  updateUser: (
    userId: string,
    userData: Partial<UserAccount>
  ): Promise<ApiResponse<UserAccount>> => {
    const user = MOCK_USER_ACCOUNTS.find((u) => u.userId === userId);
    if (user) {
      Object.assign(user, userData);
      return createMockResponse(user, 200);
    }
    throw new Error("User not found");
  },

  getUserById: (userId: string): Promise<ApiResponse<UserAccount>> => {
    const user = MOCK_USER_ACCOUNTS.find((u) => u.userId === userId);
    if (user) {
      return createMockResponse(user, 150);
    }
    throw new Error("User not found");
  },
};

// Mock data for Appointment Analytics
const MOCK_APPOINTMENT_METRICS: AppointmentMetric[] = [
  {
    label: "Total Appointments",
    value: 2156,
    change: "+15.3%",
    trend: "up",
    period: "This Month",
    icon: "calendar",
  },
  {
    label: "Completion Rate",
    value: 92.1,
    change: "+2.4%",
    trend: "up",
    period: "This Month",
    icon: "checkCircle",
  },
  {
    label: "Average Wait Time",
    value: 14,
    change: "-8%",
    trend: "down",
    period: "Minutes",
    icon: "clock",
  },
  {
    label: "Cancellation Rate",
    value: 4.1,
    change: "-1.2%",
    trend: "down",
    period: "This Month",
    icon: "xCircle",
  },
  {
    label: "No-Show Rate",
    value: 3.7,
    change: "+0.5%",
    trend: "up",
    period: "This Month",
    icon: "userX",
  },
  {
    label: "Revenue per Appointment",
    value: 132,
    change: "+7.8%",
    trend: "up",
    period: "EUR Average",
    icon: "euro",
  },
];

// Mock data for new chart types based on database schema
const MOCK_REVENUE_CHART_DATA: RevenueChartData[] = [
  { date: "2025-01-01", revenue: 22680, appointments: 172 },
  { date: "2025-01-02", revenue: 24156, appointments: 183 },
  { date: "2025-01-03", revenue: 21384, appointments: 162 },
  { date: "2025-01-04", revenue: 24948, appointments: 189 },
  { date: "2025-01-05", revenue: 19140, appointments: 145 },
  { date: "2025-01-06", revenue: 26664, appointments: 202 },
  { date: "2025-01-07", revenue: 22836, appointments: 173 },
  { date: "2025-01-08", revenue: 25320, appointments: 192 },
  { date: "2025-01-09", revenue: 23450, appointments: 178 },
  { date: "2025-01-10", revenue: 27890, appointments: 211 },
  { date: "2025-01-11", revenue: 21560, appointments: 163 },
  { date: "2025-01-12", revenue: 28440, appointments: 216 },
  { date: "2025-01-13", revenue: 24780, appointments: 188 },
  { date: "2025-01-14", revenue: 26120, appointments: 198 },
];

const MOCK_APPOINTMENTS_BY_SPECIALIZATION: AppointmentsBySpecializationData[] =
  [
    {
      specialization: "Cardiology",
      appointments: 487,
      revenue: 90195,
      color: "#3B82F6",
    },
    {
      specialization: "Neurology",
      appointments: 356,
      revenue: 70488,
      color: "#10B981",
    },
    {
      specialization: "Orthopedics",
      appointments: 342,
      revenue: 56430,
      color: "#F59E0B",
    },
    {
      specialization: "Dermatology",
      appointments: 601,
      revenue: 85342,
      color: "#EF4444",
    },
    {
      specialization: "Psychiatry",
      appointments: 324,
      revenue: 56700,
      color: "#8B5CF6",
    },
    {
      specialization: "Pediatrics",
      appointments: 428,
      revenue: 51360,
      color: "#06B6D4",
    },
    {
      specialization: "Oncology",
      appointments: 267,
      revenue: 66675,
      color: "#F97316",
    },
    {
      specialization: "General Practice",
      appointments: 892,
      revenue: 107040,
      color: "#84CC16",
    },
  ];

const MOCK_DOCTOR_PRODUCTIVITY: DoctorProductivityData[] = [
  {
    doctorName: "Dr. Alice Heart",
    specialization: "Cardiology",
    completedAppointments: 178,
    totalRevenue: 32930,
    completionRate: 95.2,
  },
  {
    doctorName: "Dr. Bob Vessel",
    specialization: "Neurology",
    completedAppointments: 142,
    totalRevenue: 28116,
    completionRate: 91.0,
  },
  {
    doctorName: "Dr. Carol Bones",
    specialization: "Orthopedics",
    completedAppointments: 131,
    totalRevenue: 21615,
    completionRate: 92.3,
  },
  {
    doctorName: "Dr. David Skin",
    specialization: "Dermatology",
    completedAppointments: 186,
    totalRevenue: 26412,
    completionRate: 92.5,
  },
  {
    doctorName: "Dr. Emma Mind",
    specialization: "Psychiatry",
    completedAppointments: 124,
    totalRevenue: 21700,
    completionRate: 88.6,
  },
  {
    doctorName: "Dr. Frank Child",
    specialization: "Pediatrics",
    completedAppointments: 156,
    totalRevenue: 18720,
    completionRate: 94.5,
  },
  {
    doctorName: "Dr. Grace Cancer",
    specialization: "Oncology",
    completedAppointments: 98,
    totalRevenue: 24500,
    completionRate: 89.1,
  },
  {
    doctorName: "Dr. Henry General",
    specialization: "General Practice",
    completedAppointments: 234,
    totalRevenue: 28080,
    completionRate: 96.3,
  },
];


// API functions for Appointment Analytics
export const appointmentAnalyticsApi = {
  getAppointmentMetrics: (): Promise<ApiResponse<AppointmentMetric[]>> => {
    return createMockResponse(MOCK_APPOINTMENT_METRICS, 200);
  },

  getRevenueChartData: (): Promise<ApiResponse<RevenueChartData[]>> => {
    return createMockResponse(MOCK_REVENUE_CHART_DATA, 250);
  },

  getAppointmentsBySpecialization: (): Promise<
    ApiResponse<AppointmentsBySpecializationData[]>
  > => {
    return createMockResponse(MOCK_APPOINTMENTS_BY_SPECIALIZATION, 200);
  },

  getDoctorProductivity: (): Promise<ApiResponse<DoctorProductivityData[]>> => {
    return createMockResponse(MOCK_DOCTOR_PRODUCTIVITY, 300);
  },

  // getDoctorPerformance removed: now provided by dedicated practitioner service endpoint.
};
