import React, { useEffect, useRef } from "react";
import { EmptyState, Loading } from "@shared/components";
import { MessageCircle } from "lucide-react";

import type { MessageListProps } from "../types";

import { MessageBubble } from "./MessageBubble";

export const MessageList: React.FC<MessageListProps> = ({
  messages,
  currentUserId,
  isLoading = false,
  onMarkAsRead,
}) => {
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Automatycznie przewiń do najnowszej wiadomości
  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  if (isLoading && messages.length === 0) {
    return (
      <div className="flex-1 flex items-center justify-center bg-gray-50">
        <Loading size="lg" text="Loading messages..." />
      </div>
    );
  }

  if (messages.length === 0) {
    return (
      <div className="flex-1 flex items-center justify-center bg-gray-50">
        <EmptyState
          icon={<MessageCircle className="h-16 w-16 text-gray-400" />}
          title="No messages yet"
          description="Start the conversation by sending your first message"
        />
      </div>
    );
  }

  // Grupuj wiadomości według dnia
  const groupedMessages = messages.reduce(
    (groups, message) => {
      const messageDate = new Date(message.timestamp);
      const dateKey = messageDate.toDateString();

      if (!groups[dateKey]) {
        groups[dateKey] = [];
      }

      groups[dateKey].push(message);
      return groups;
    },
    {} as Record<string, typeof messages>
  );

  const formatDateHeader = (dateString: string) => {
    const date = new Date(dateString);
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);

    if (date.toDateString() === today.toDateString()) {
      return "Today";
    } else if (date.toDateString() === yesterday.toDateString()) {
      return "Yesterday";
    } else {
      return date.toLocaleDateString([], {
        weekday: "long",
        year: "numeric",
        month: "long",
        day: "numeric",
      });
    }
  };

  return (
    <div className="flex-1 overflow-y-auto bg-gray-50 p-4">
      <div className="max-w-4xl mx-auto">
        {Object.entries(groupedMessages).map(([dateKey, dayMessages]) => (
          <div key={dateKey}>
            {/* Date separator */}
            <div className="flex items-center justify-center my-6">
              <div className="bg-gray-200 text-gray-600 text-xs px-3 py-1 rounded-full">
                {formatDateHeader(dateKey)}
              </div>
            </div>

            {/* Messages for this day */}
            {dayMessages.map((message) => (
              <MessageBubble
                key={message.id}
                message={message}
                isOwnMessage={message.senderId === currentUserId}
                {...(onMarkAsRead && { onMarkAsRead })}
              />
            ))}
          </div>
        ))}

        {/* Loading indicator for new messages */}
        {isLoading && messages.length > 0 && (
          <div className="flex justify-center py-2">
            <div className="bg-white rounded-full px-3 py-1 shadow-sm">
              <Loading size="sm" />
            </div>
          </div>
        )}

        {/* Scroll anchor */}
        <div ref={messagesEndRef} />
      </div>
    </div>
  );
};
