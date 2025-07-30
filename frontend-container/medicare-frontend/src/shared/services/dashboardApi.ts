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

const MOCK_DOCTOR_NOTIFICATIONS: Notification[] = [
  {
    id: "1",
    message: "Appointment with John Doe at 10:30 AM today.",
    type: "warning",
    timestamp: "2025-05-14T09:00:00Z",
    read: false,
  },
  {
    id: "2",
    message: "Lab result for Maria Smith is now available.",
    type: "success",
    timestamp: "2025-05-13T14:00:00Z",
    read: false,
  },
  {
    id: "3",
    message: "Patient Adam Nowak sent a new message.",
    type: "info",
    timestamp: "2025-05-13T11:30:00Z",
    read: true,
  },
  {
    id: "4",
    message: "Follow-up reminder: 2 patients need summary reports.",
    type: "warning",
    timestamp: "2025-05-12T16:00:00Z",
    read: false,
  },
];

const MOCK_DOCTOR_STATS: QuickStat[] = [
  { label: "Patients Today", value: 7, change: "+2", trend: "up" },
  { label: "Total Patients", value: 234, change: "+12", trend: "up" },
  { label: "Visits this Month", value: 49, change: "+5", trend: "up" },
  { label: "Unread Messages", value: 3, change: "-1", trend: "down" },
];

const MOCK_PATIENT_MESSAGES: PatientMessage[] = [
  {
    id: 2,
    patient: "Maria Smith",
    text: "Can I move my appointment to Friday?",
    timestamp: "2025-05-13T15:30:00Z",
    unread: true,
  },
  {
    id: 3,
    patient: "Adam Nowak",
    text: "Uploaded my recent blood test results.",
    timestamp: "2025-05-13T10:15:00Z",
    unread: true,
  },
  {
    id: 1,
    patient: "John Doe",
    text: "Thank you for the prescription.",
    timestamp: "2025-05-12T14:20:00Z",
    unread: false,
  },
];

// API functions for Patient Dashboard
export const patientDashboardApi = {
  getNotifications: (): Promise<ApiResponse<Notification[]>> => {
    return createMockResponse(MOCK_PATIENT_NOTIFICATIONS, 300);
  },

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

// API functions for Doctor Dashboard
export const doctorDashboardApi = {
  getNotifications: (): Promise<ApiResponse<Notification[]>> => {
    return createMockResponse(MOCK_DOCTOR_NOTIFICATIONS, 300);
  },

  getQuickStats: (): Promise<ApiResponse<QuickStat[]>> => {
    return createMockResponse(MOCK_DOCTOR_STATS, 250);
  },

  getPatientMessages: (): Promise<ApiResponse<PatientMessage[]>> => {
    return createMockResponse(MOCK_PATIENT_MESSAGES, 200);
  },

  markNotificationAsRead: (
    notificationId: string
  ): Promise<ApiResponse<boolean>> => {
    const notification = MOCK_DOCTOR_NOTIFICATIONS.find(
      (n) => n.id === notificationId
    );
    if (notification) {
      notification.read = true;
    }
    return createMockResponse(true, 100);
  },
};
