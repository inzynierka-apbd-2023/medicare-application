import type { Conversation, Message } from "../../features/messages/types";

import { ApiResponse, createErrorResponse } from "./api";
import { apiClient as api } from "./apiClient";

export const messagesApi = {
  /**
   * Get all conversations for a user
   */
  getConversations: async (
    userId: string,
    _userType: "patient" | "doctor"
  ): Promise<ApiResponse<Conversation[]>> => {
    try {
      const res = await api.get(`/messaging/messages/conversations/${userId}`);

      const conversations: Conversation[] = res.data.map(
        (dto: {
          id: string;
          participantId: string;
          participantName: string;
          participantType: string;
          lastMessageContent: string;
          updatedAt: string;
          unreadCount: number;
        }) => ({
          id: dto.id || dto.participantId, // Use conversation ID from backend
          participantId: dto.participantId,
          participantName: dto.participantName || "Unknown User",
          participantType: dto.participantType || "unknown",
          participants: [],
          lastMessage: {
            id: "latest",
            content: dto.lastMessageContent || "",
            timestamp: dto.updatedAt,
            isRead: dto.unreadCount === 0,
            conversationId: dto.participantId,
            senderId: "",
            senderName: "",
            senderType: "patient",
            receiverId: "",
            receiverName: "",
            receiverType: "doctor",
          } as Message,
          unreadCount: dto.unreadCount,
          isActive: true,
          createdAt: dto.updatedAt,
          updatedAt: dto.updatedAt,
        })
      );

      return { data: conversations, success: true };
    } catch (error) {
      console.error("Failed to fetch conversations", error);
      return createErrorResponse("Failed to fetch conversations");
    }
  },

  /**
   * Get messages for a specific conversation (between current user and other user)
   */
  getMessages: async (
    conversationId: string,
    currentUserId?: string
  ): Promise<ApiResponse<Message[]>> => {
    try {
      if (!currentUserId)
        throw new Error("Current User ID is required to fetch messages");

      // conversationId is treated as the Other User ID
      const otherUserId = conversationId;
      const res = await api.get(
        `/messaging/messages/conversation/${currentUserId}/${otherUserId}`
      );

      const messages: Message[] = res.data.map(
        (m: {
          id: string;
          senderId: string;
          recipientId: string;
          content: string;
          sentAt: string;
          isRead: boolean;
        }) => ({
          id: m.id,
          conversationId: otherUserId,
          senderId: m.senderId,
          senderName: m.senderId === currentUserId ? "Me" : "Other", // Placeholder
          senderType: "unknown",
          receiverId: m.recipientId,
          receiverName: m.recipientId === currentUserId ? "Me" : "Other",
          receiverType: "unknown",
          content: m.content,
          timestamp: m.sentAt,
          isRead: m.isRead,
        })
      );

      return { data: messages, success: true };
    } catch (error) {
      console.error("Failed to fetch messages", error);
      return createErrorResponse("Failed to fetch messages");
    }
  },

  /**
   * Send a new message
   */
  sendMessage: async (
    conversationId: string,
    senderId: string,
    senderName: string,
    senderType: "patient" | "doctor",
    // receiverId is not directly used in API call if conversationId is used as recipient
    _receiverId: string,
    receiverName: string,
    receiverType: "patient" | "doctor",
    content: string
  ): Promise<ApiResponse<Message>> => {
    try {
      // conversationId is treated as RecipientId (if it's an existing conversation)
      // or we use receiverId explicitly.
      // The Hook calls this with conversationId.
      const realRecipientId = conversationId;

      const payload = {
        senderId,
        recipientId: realRecipientId,
        subject: "Message",
        content,
        messageType: "General",
        priority: "Normal",
        senderName,
        recipientName: receiverName,
      };

      const res = await api.post("/messaging/messages", payload);
      const m = res.data;

      const newMessage: Message = {
        id: m.id,
        conversationId: realRecipientId,
        senderId: m.senderId,
        senderName: senderName,
        senderType: senderType,
        receiverId: m.recipientId,
        receiverName: receiverName,
        receiverType: receiverType,
        content: m.content,
        timestamp: m.sentAt,
        isRead: m.isRead,
      };

      return { data: newMessage, success: true };
    } catch (error) {
      console.error("Failed to send message", error);
      return createErrorResponse("Failed to send message");
    }
  },

  /**
   * Start a new conversation (Send first message)
   */
  startConversation: async (
    senderId: string,
    senderName: string,
    senderType: "patient" | "doctor",
    receiverId: string,
    receiverName: string,
    receiverType: "patient" | "doctor",
    initialMessage: string
  ): Promise<ApiResponse<{ conversation: Conversation; message: Message }>> => {
    try {
      // Just send the message. Conversation ID will be receiverId.
      const msgResp = await messagesApi.sendMessage(
        receiverId,
        senderId,
        senderName,
        senderType,
        receiverId,
        receiverName,
        receiverType,
        initialMessage
      );

      if (!msgResp.success || !msgResp.data) throw new Error(msgResp.error);

      const message = msgResp.data;
      const conversation: Conversation = {
        id: receiverId,
        participantId: receiverId,
        participantName: receiverName,
        participantType: receiverType,
        participants: [],
        lastMessage: message,
        unreadCount: 0,
        isActive: true,
        createdAt: message.timestamp,
        updatedAt: message.timestamp,
      };

      return { data: { conversation, message }, success: true };
    } catch (error) {
      console.error("Failed to start conversation", error);
      return createErrorResponse("Failed to start conversation");
    }
  },

  /**
   * Mark message as read
   */
  markMessageAsRead: async (
    messageId: string,
    userId: string
  ): Promise<ApiResponse<boolean>> => {
    try {
      await api.put(`/messaging/messages/${messageId}/read`, { userId });
      return { data: true, success: true };
    } catch (error) {
      console.error("Failed to mark message as read", error);
      return { data: false, success: false, error: "Failed to mark as read" };
    }
  },

  /**
   * Get available recipients from the backend.
   * Uses the MessagingService's local PatientDoctorContacts table,
   * which is populated via RabbitMQ events when appointments are created.
   */
  getAvailableRecipients: async (
    userRole: "patient" | "doctor",
    currentUserId?: string
  ): Promise<
    ApiResponse<
      Array<{
        id: string;
        name: string;
        type: "patient" | "doctor";
        specialization?: string;
      }>
    >
  > => {
    try {
      if (!currentUserId) {
        console.warn("getAvailableRecipients: currentUserId is required");
        return { data: [], success: true };
      }

      // Call the new backend endpoint
      const res = await api.get(
        `/messaging/messages/recipients/${currentUserId}?userRole=${userRole}`
      );

      const recipients = Array.isArray(res.data) ? res.data : [];

      return {
        data: recipients.map(
          (r: {
            id: string;
            name: string;
            type: string;
            specialization?: string;
          }) => ({
            id: r.id,
            name: r.name,
            type: r.type as "patient" | "doctor",
            specialization: r.specialization || "General",
          })
        ),
        success: true,
      };
    } catch (e) {
      console.error("Failed to fetch recipients", e);
      return { data: [], success: false, error: "Failed to fetch recipients" };
    }
  },
};
