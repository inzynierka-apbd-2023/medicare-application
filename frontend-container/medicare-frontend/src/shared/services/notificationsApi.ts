import { apiClient as api } from "./apiClient";
import type { ApiResponse } from "./api";

export interface BackendNotificationDto {
  id: string;
  recipientUserId: string;
  description?: string;
  type: number;
  creationDate: string;
  isRead: boolean;
  actionUrl?: string;
}

export interface DashboardNotification {
  id: string;
  message: string;
  type?: "info" | "warning" | "success" | "error";
  timestamp?: string;
  read?: boolean;
}

type NotificationUiType = "info" | "warning" | "success" | "error";
const typeToUi = (t: number): NotificationUiType => {
  switch (t) {
    case 1:
      return "info"; // Appointment reminder
    case 2:
      return "success"; // e.g., lab available
    case 3:
      return "warning";
    case 4:
      return "error";
    default:
      return "info";
  }
};

export const notificationsApi = {
  async getForRecipient(recipientUserId: string, unreadOnly = false): Promise<ApiResponse<DashboardNotification[]>> {
    try {
      const res = await api.get<BackendNotificationDto[]>("/notifications", {
        params: { recipientUserId, unreadOnly, page: 1, pageSize: 20, _ts: Date.now() },
      });
      const items = (res.data || []).map((n) => {
        // Strip trailing 'Z' if present and format a short time if UI needs it later
        let ts = n.creationDate;
        if (typeof ts === "string" && ts.endsWith("Z")) ts = ts.slice(0, -1);
        // Clean up legacy messages that embed a UTC 'Z' time suffix
        const cleanMsg = (n.description || "Notification").replace(/(\d{2}:\d{2}:\d{2})Z\b/, "$1");
        return {
          id: n.id,
          message: cleanMsg,
          type: typeToUi(n.type),
          timestamp: ts,
          read: n.isRead,
        } as const;
      });
      return { data: items, success: true };
    } catch (err) {
      return { data: [], success: false, error: err instanceof Error ? err.message : "Failed to fetch notifications" };
    }
  },

  async markAsRead(notificationId: string): Promise<ApiResponse<boolean>> {
    try {
      await api.post(`/notifications/${notificationId}/read`);
      // Broadcast a lightweight global event so UI (e.g., Header badge) can refresh immediately
      try {
        if (typeof window !== "undefined" && typeof window.dispatchEvent === "function") {
          window.dispatchEvent(
            new CustomEvent("notifications:updated", { detail: { kind: "read", id: notificationId } })
          );
        }
      } catch { /* best-effort */ }
      return { data: true, success: true };
    } catch (err) {
      return { data: false as any, success: false, error: err instanceof Error ? err.message : "Failed to mark read" };
    }
  },
};
