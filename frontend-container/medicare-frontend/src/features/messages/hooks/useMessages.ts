import { useCallback, useEffect, useState } from "react";
import { messagesApi } from "@shared/services/messagesApi";

import type { Conversation, Message } from "../types";

export interface UseMessagesReturn {
  conversations: Conversation[];
  messages: Record<string, Message[]>;
  selectedConversationId: string | undefined;
  isLoading: boolean;
  error: string | null;
  selectConversation: (conversationId: string) => void;
  sendMessage: (conversationId: string, content: string) => Promise<void>;
  markAsRead: (messageId: string) => void;
  createConversation: (
    recipientId: string,
    initialMessage: string
  ) => Promise<string>;
}

export const useMessages = (
  userId?: string,
  userType: "patient" | "doctor" = "patient"
): UseMessagesReturn => {
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [messages, setMessages] = useState<Record<string, Message[]>>({});
  const [selectedConversationId, setSelectedConversationId] =
    useState<string>();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Load conversations
  const loadConversations = useCallback(async () => {
    if (!userId) return;

    try {
      setIsLoading(true);
      setError(null);
      const response = await messagesApi.getConversations(userId, userType);
      if (response.success) {
        setConversations(response.data);
      } else {
        setError(response.error || "Failed to load conversations");
      }
    } catch (err) {
      setError("Failed to load conversations");
      console.error("Error loading conversations:", err);
    } finally {
      setIsLoading(false);
    }
  }, [userId, userType]);

  // Load messages for a conversation
  const loadMessages = useCallback(async (conversationId: string) => {
    try {
      setIsLoading(true);
      const response = await messagesApi.getMessages(conversationId);
      if (response.success) {
        setMessages((prev) => ({
          ...prev,
          [conversationId]: response.data,
        }));
      } else {
        setError(response.error || "Failed to load messages");
      }
    } catch (err) {
      setError("Failed to load messages");
      console.error("Error loading messages:", err);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Select conversation
  const selectConversation = useCallback(
    (conversationId: string) => {
      setSelectedConversationId(conversationId);

      // Load messages if not already loaded
      if (!messages[conversationId]) {
        loadMessages(conversationId);
      }
    },
    [messages, loadMessages]
  );

  // Send message
  const sendMessage = useCallback(
    async (
      conversationId: string,
      content: string
      // Note: attachments parameter removed as it's not used in current API
    ) => {
      if (!userId) return;

      try {
        setIsLoading(true);

        // Find conversation to get recipient info
        const conversation = conversations.find((c) => c.id === conversationId);
        if (!conversation) {
          setError("Conversation not found");
          return;
        }

        // Mock user names - in real app this would come from user context/profile
        const senderName =
          userType === "patient" ? "Current Patient" : "Current Doctor";
        const receiverName = conversation.participantName;
        const receiverType = conversation.participantType;
        const receiverId = conversation.participantId;

        const response = await messagesApi.sendMessage(
          conversationId,
          userId,
          senderName,
          userType,
          receiverId,
          receiverName,
          receiverType,
          content
        );

        if (response.success) {
          // Add new message to local state
          setMessages((prev) => ({
            ...prev,
            [conversationId]: [...(prev[conversationId] || []), response.data],
          }));

          // Update conversation with new last message
          setConversations((prev) =>
            prev.map((conv) =>
              conv.id === conversationId
                ? {
                    ...conv,
                    lastMessage: response.data,
                    updatedAt: response.data.timestamp,
                  }
                : conv
            )
          );
        } else {
          setError(response.error || "Failed to send message");
        }
      } catch (err) {
        setError("Failed to send message");
        console.error("Error sending message:", err);
      } finally {
        setIsLoading(false);
      }
    },
    [userId, userType, conversations]
  );

  // Mark message as read - simplified version without API call
  const markAsRead = useCallback(
    async (messageId: string) => {
      try {
        // Since there's no markAsRead API endpoint, just update local state
        setMessages((prev) => {
          const updated = { ...prev };
          Object.keys(updated).forEach((conversationId) => {
            updated[conversationId] = updated[conversationId].map((msg) =>
              msg.id === messageId ? { ...msg, isRead: true } : msg
            );
          });
          return updated;
        });

        // Update unread count in conversations
        setConversations((prev) =>
          prev.map((conv) => {
            const conversationMessages = messages[conv.id] || [];
            const unreadCount = conversationMessages.filter(
              (msg) => !msg.isRead && msg.senderId !== userId
            ).length;
            return { ...conv, unreadCount };
          })
        );
      } catch (err) {
        console.error("Error marking message as read:", err);
      }
    },
    [messages, userId]
  );

  // Create new conversation
  const createConversation = useCallback(
    async (recipientId: string, initialMessage: string): Promise<string> => {
      if (!userId) throw new Error("User ID required");

      try {
        setIsLoading(true);

        // Mock recipient info - in real app this would come from user API
        const senderName =
          userType === "patient" ? "Current Patient" : "Current Doctor";
        const recipientName = "Doctor"; // Would be fetched from user API
        const recipientType: "patient" | "doctor" =
          userType === "patient" ? "doctor" : "patient";

        const response = await messagesApi.startConversation(
          userId,
          senderName,
          userType,
          recipientId,
          recipientName,
          recipientType,
          initialMessage
        );

        if (response.success) {
          const { conversation, message } = response.data;
          setConversations((prev) => [conversation, ...prev]);

          // Add initial message to messages
          setMessages((prev) => ({
            ...prev,
            [conversation.id]: [message],
          }));

          return conversation.id;
        } else {
          throw new Error(response.error || "Failed to create conversation");
        }
      } catch (err) {
        setError("Failed to create conversation");
        console.error("Error creating conversation:", err);
        throw err;
      } finally {
        setIsLoading(false);
      }
    },
    [userId, userType]
  );

  // Load conversations on mount
  useEffect(() => {
    loadConversations();
  }, [loadConversations]);

  return {
    conversations,
    messages,
    selectedConversationId,
    isLoading,
    error,
    selectConversation,
    sendMessage,
    markAsRead,
    createConversation,
  };
};
