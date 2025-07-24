interface Notification {
  id: string;
  message: string;
  type?: "info" | "warning" | "success" | "error";
  timestamp?: Date;
}

interface NotificationsListProps {
  notifications: Notification[];
  maxVisible?: number;
  className?: string;
}

export function NotificationsList({
  notifications,
  maxVisible = 3,
  className = "",
}: NotificationsListProps) {
  const visibleNotifications = notifications.slice(0, maxVisible);

  return (
    <ul
      className={`space-y-2 list-disc list-inside text-left w-full ${className}`}
    >
      {visibleNotifications.map((notification) => (
        <li key={notification.id} className="text-sm text-gray-600">
          {notification.message}
        </li>
      ))}
    </ul>
  );
}

export type { Notification, NotificationsListProps };
