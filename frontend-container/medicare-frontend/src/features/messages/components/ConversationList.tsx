import React from "react";
import { Badge, Card, EmptyState } from "@shared/components";
import { Users } from "lucide-react";

import type { Conversation, ConversationListProps } from "../types";

export const ConversationList: React.FC<ConversationListProps> = ({
  conversations,
  selectedConversationId,
  onSelectConversation,
  currentUserId,
}) => {
  const formatLastMessageTime = (timestamp: string) => {
    const messageDate = new Date(timestamp);
    const now = new Date();
    const diffInMinutes = Math.floor(
      (now.getTime() - messageDate.getTime()) / (1000 * 60)
    );

    if (diffInMinutes < 60) {
      return diffInMinutes === 0 ? "now" : `${diffInMinutes}m`;
    }

    const diffInHours = Math.floor(diffInMinutes / 60);
    if (diffInHours < 24) {
      return `${diffInHours}h`;
    }

    const diffInDays = Math.floor(diffInHours / 24);
    if (diffInDays < 7) {
      return `${diffInDays}d`;
    }

    return messageDate.toLocaleDateString([], {
      month: "short",
      day: "numeric",
    });
  };

  const getOtherParticipant = (conversation: Conversation) => {
    // Use existing participant info from conversation
    return {
      id: conversation.participantId,
      name: conversation.participantName,
      role: conversation.participantType,
    };
  };

  const truncateMessage = (text: string, maxLength: number = 50) => {
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + "...";
  };

  if (conversations.length === 0) {
    return (
      <div className="h-full flex items-center justify-center p-4">
        <EmptyState
          icon={<Users className="h-12 w-12 text-gray-400" />}
          title="No conversations"
          description="Your conversations will appear here"
        />
      </div>
    );
  }

  return (
    <div className="h-full overflow-y-auto">
      <div className="p-3">
        <h2 className="text-lg font-semibold text-gray-800 mb-4">Messages</h2>

        <div className="space-y-1">
          {conversations.map((conversation) => {
            const otherParticipant = getOtherParticipant(conversation);
            const lastMessage = conversation.lastMessage;
            const isSelected = conversation.id === selectedConversationId;
            const unreadCount = conversation.unreadCount || 0;

            return (
              <Card
                key={conversation.id}
                onClick={() => onSelectConversation(conversation.id)}
                className={`p-3 cursor-pointer transition-all duration-200 hover:shadow-md ${
                  isSelected
                    ? "ring-2 ring-blue-500 bg-blue-50"
                    : "hover:bg-gray-50"
                } ${unreadCount > 0 ? "bg-blue-25" : ""}`}
              >
                <div className="flex items-center space-x-3">
                  {/* Avatar */}
                  <div className="flex-shrink-0">
                    <div className="w-12 h-12 bg-gradient-to-br from-blue-500 to-blue-600 rounded-full flex items-center justify-center text-white font-semibold text-lg">
                      {otherParticipant
                        ? `${otherParticipant.name
                            .split(" ")
                            .map((n) => n[0])
                            .join("")
                            .substring(0, 2)}`
                        : "??"}
                    </div>
                  </div>

                  {/* Content */}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between mb-1">
                      <h3
                        className={`text-sm font-medium truncate ${
                          unreadCount > 0 ? "text-gray-900" : "text-gray-700"
                        }`}
                      >
                        {otherParticipant?.name || "Unknown User"}
                      </h3>

                      <div className="flex items-center space-x-2 flex-shrink-0">
                        {unreadCount > 0 && (
                          <Badge variant="info" size="sm">
                            {unreadCount}
                          </Badge>
                        )}
                        {lastMessage && (
                          <span className="text-xs text-gray-500">
                            {formatLastMessageTime(lastMessage.timestamp)}
                          </span>
                        )}
                      </div>
                    </div>

                    {/* Last message preview */}
                    {lastMessage && (
                      <div className="flex items-center space-x-1">
                        {lastMessage.senderId === currentUserId && (
                          <span className="text-gray-400 text-xs">You:</span>
                        )}
                        <p
                          className={`text-xs truncate ${
                            unreadCount > 0
                              ? "text-gray-800 font-medium"
                              : "text-gray-500"
                          }`}
                        >
                          {lastMessage.attachments &&
                          lastMessage.attachments.length > 0 ? (
                            <span className="flex items-center">
                              📎 {lastMessage.attachments.length} attachment
                              {lastMessage.attachments.length > 1 ? "s" : ""}
                              {lastMessage.content &&
                                ` • ${truncateMessage(lastMessage.content, 30)}`}
                            </span>
                          ) : (
                            truncateMessage(lastMessage.content)
                          )}
                        </p>
                      </div>
                    )}

                    {/* Doctor specialty - removed since not available in current data structure */}
                  </div>
                </div>
              </Card>
            );
          })}
        </div>
      </div>
    </div>
  );
};
