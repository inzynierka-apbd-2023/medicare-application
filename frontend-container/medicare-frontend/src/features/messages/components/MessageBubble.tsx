import React, { useEffect } from "react";
import type { MessageBubbleProps } from "@features/messages/types";
import { Check, CheckCheck, Clock } from "lucide-react";

export const MessageBubble: React.FC<MessageBubbleProps> = ({
  message,
  isOwnMessage,
  onMarkAsRead,
}) => {
  // Automatycznie oznacz jako przeczytane gdy wiadomość zostanie wyświetlona
  useEffect(() => {
    if (!message.isRead && !isOwnMessage && onMarkAsRead) {
      const timer = setTimeout(() => {
        onMarkAsRead(message.id);
      }, 1000); // Oznacz jako przeczytane po 1 sekundzie

      return () => clearTimeout(timer);
    }

    return undefined;
  }, [message.id, message.isRead, isOwnMessage, onMarkAsRead]);

  const formatTime = (timestamp: string) => {
    const date = new Date(timestamp);
    return date.toLocaleTimeString([], {
      hour: "2-digit",
      minute: "2-digit",
      hour12: false,
    });
  };

  const getStatusIcon = () => {
    if (!isOwnMessage) return null;

    if (message.isRead) {
      return <CheckCheck size={16} className="text-blue-500" />;
    }
    return <Check size={16} className="text-gray-400" />;
  };

  return (
    <div
      className={`flex mb-4 ${isOwnMessage ? "justify-end" : "justify-start"}`}
    >
      <div className={`max-w-[70%] ${isOwnMessage ? "order-2" : "order-1"}`}>
        {/* Nazwa nadawcy (tylko dla wiadomości nie własnych) */}
        {!isOwnMessage && (
          <div className="text-xs text-gray-500 mb-1 px-3">
            {message.senderName}
          </div>
        )}

        {/* Bąbelek wiadomości */}
        <div
          className={`
            px-4 py-3 rounded-2xl shadow-sm
            ${
              isOwnMessage
                ? "bg-blue-500 text-white rounded-br-md"
                : "bg-white text-gray-800 border border-gray-200 rounded-bl-md"
            }
          `}
        >
          <p className="text-sm leading-relaxed break-words">
            {message.content}
          </p>

          {/* Attachments (jeśli są) */}
          {message.attachments && message.attachments.length > 0 && (
            <div className="mt-2 space-y-1">
              {message.attachments.map((attachment) => (
                <div
                  key={attachment.id}
                  className={`
                    text-xs p-2 rounded-lg border-dashed border
                    ${
                      isOwnMessage
                        ? "border-blue-300 bg-blue-400/20"
                        : "border-gray-300 bg-gray-50"
                    }
                  `}
                >
                  <div className="flex items-center gap-2">
                    <Clock size={12} />
                    <span>{attachment.name}</span>
                    <span className="text-gray-500">
                      ({(attachment.size / 1024).toFixed(1)} KB)
                    </span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Czas i status */}
        <div
          className={`
          flex items-center gap-1 mt-1 px-3
          ${isOwnMessage ? "justify-end" : "justify-start"}
        `}
        >
          <span className="text-xs text-gray-500">
            {formatTime(message.timestamp)}
          </span>
          {getStatusIcon()}
        </div>
      </div>
    </div>
  );
};
