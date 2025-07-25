interface Notification {
  id: string;
  message: string;
  type?: "info" | "warning" | "success" | "error";
  timestamp?: string;
  read?: boolean;
}

interface NotificationsListProps {
  notifications: Notification[];
  maxVisible?: number;
  className?: string;
  onNotificationClick?: (notificationId: string) => void;
}

export function NotificationsList({
  notifications,
  maxVisible = 3,
  className = "",
  onNotificationClick,
}: NotificationsListProps) {
  const visibleNotifications = notifications.slice(0, maxVisible);

  const handleNotificationClick = (notification: Notification) => {
    if (onNotificationClick && !notification.read) {
      onNotificationClick(notification.id);
    }
  };

  const getNotificationStyles = (notification: Notification) => {
    const baseStyles = "text-sm cursor-pointer transition-colors duration-150";
    const readStyles = notification.read
      ? "text-gray-500"
      : "text-gray-600 font-medium";

    return `${baseStyles} ${readStyles}`;
  };

  if (visibleNotifications.length === 0) {
    return (
      <div className={`text-gray-500 text-sm text-center ${className}`}>
        No notifications
      </div>
    );
  }

  return (
    <ul
      className={`space-y-2 list-disc list-inside text-left w-full ${className}`}
    >
      {visibleNotifications.map((notification) => (
        <li
          key={notification.id}
          className={getNotificationStyles(notification)}
          onClick={() => handleNotificationClick(notification)}
        >
          {notification.message}
          {!notification.read && (
            <span className="ml-2 inline-block w-2 h-2 bg-blue-500 rounded-full"></span>
          )}
        </li>
      ))}
    </ul>
  );
}

export type { Notification, NotificationsListProps };
