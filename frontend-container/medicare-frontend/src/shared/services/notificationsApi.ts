import { toastMessages } from "../toast/toastMessages";

import { api } from "./api";

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
  getForRecipient: async (
    recipientUserId: string,
    unreadOnly = false
  ): Promise<DashboardNotification[]> => {
    const data = await api.get<BackendNotificationDto[]>("/notifications", {
      params: {
        recipientUserId,
        unreadOnly,
        page: 1,
        pageSize: 20,
        _ts: Date.now(),
      },
    });

    return (data || []).map((n) => {
      let ts = n.creationDate;
      if (typeof ts === "string" && ts.endsWith("Z")) ts = ts.slice(0, -1);
      const cleanMsg = (n.description || "Notification").replace(
        /(\d{2}:\d{2}:\d{2})Z\b/,
        "$1"
      );
      return {
        id: n.id,
        message: cleanMsg,
        type: typeToUi(n.type),
        timestamp: ts,
        read: n.isRead,
      } as const;
    });
  },

  markAsRead: async (notificationId: string): Promise<boolean> => {
    await api.post(`/notifications/${notificationId}/read`, {}, undefined, {
      showToastOnSuccess: true,
      successMessage: toastMessages.notifications.markReadSuccess,
    });

    if (
      typeof window !== "undefined" &&
      typeof window.dispatchEvent === "function"
    ) {
      window.dispatchEvent(
        new CustomEvent("notifications:updated", {
          detail: { kind: "read", id: notificationId },
        })
      );
    }

    return true;
  },
};
