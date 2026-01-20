import type { Conversation, Message } from "@features/messages/types";
import { toastMessages } from "@shared/toast/toastMessages";

import { api } from "./api";

export const messagesApi = {
  getConversations: async (
    userId: string,
    _userType: "patient" | "doctor" | "receptionist"
  ): Promise<Conversation[]> => {
    const res = await api.get<
      Array<{
        id: string;
        participantId: string;
        participantName: string;
        participantType: string;
        lastMessageContent: string;
        updatedAt: string;
        unreadCount: number;
      }>
    >(`/messaging/messages/conversations/${userId}`, undefined, {
      showToastOnError: true,
      showToastOnSuccess: false,
    });

    return res.map((dto) => ({
      id: dto.id || dto.participantId,
      participantId: dto.participantId,
      participantName: dto.participantName || "Unknown User",
      participantType:
        (dto.participantType as "patient" | "doctor" | "receptionist") ||
        "patient",
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
    }));
  },

  getMessages: async (
    conversationId: string,
    currentUserId?: string
  ): Promise<Message[]> => {
    if (!currentUserId)
      throw new Error("Current User ID is required to fetch messages");

    const otherUserId = conversationId;
    const res = await api.get<
      Array<{
        id: string;
        senderId: string;
        recipientId: string;
        content: string;
        sentAt: string;
        isRead: boolean;
      }>
    >(
      `/messaging/messages/conversation/${currentUserId}/${otherUserId}`,
      undefined,
      {
        showToastOnError: true,
        showToastOnSuccess: false,
      }
    );

    return res.map((m) => ({
      id: m.id,
      conversationId: otherUserId,
      senderId: m.senderId,
      senderName: m.senderId === currentUserId ? "Me" : "Other",
      senderType: "patient",
      receiverId: m.recipientId,
      receiverName: m.recipientId === currentUserId ? "Me" : "Other",
      receiverType: "doctor",
      content: m.content,
      timestamp: m.sentAt,
      isRead: m.isRead,
    }));
  },

  sendMessage: async (
    conversationId: string,
    senderId: string,
    senderName: string,
    senderType: "patient" | "doctor" | "receptionist",
    receiverName: string,
    receiverType: "patient" | "doctor" | "receptionist",
    content: string
  ): Promise<Message> => {
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

    const m = await api.post<{
      id: string;
      senderId: string;
      recipientId: string;
      content: string;
      sentAt: string;
      isRead: boolean;
    }>("/messaging/messages", payload, undefined, {
      showToastOnError: true,
      showToastOnSuccess: true,
      successMessage: toastMessages.messages.sendMessageSuccess,
    });

    return {
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
  },

  startConversation: async (
    senderId: string,
    senderName: string,
    senderType: "patient" | "doctor" | "receptionist",
    receiverId: string,
    receiverName: string,
    receiverType: "patient" | "doctor" | "receptionist",
    initialMessage: string
  ): Promise<{ conversation: Conversation; message: Message }> => {
    const message = await messagesApi.sendMessage(
      receiverId,
      senderId,
      senderName,
      senderType,
      receiverName,
      receiverType,
      initialMessage
    );

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

    return { conversation, message };
  },

  markMessageAsRead: async (
    messageId: string,
    userId: string
  ): Promise<boolean> => {
    await api.put(
      `/messaging/messages/${messageId}/read`,
      { userId },
      undefined,
      {
        showToastOnError: true,
        showToastOnSuccess: false,
      }
    );
    return true;
  },

  getAvailableRecipients: async (
    userRole: "patient" | "doctor" | "receptionist",
    currentUserId?: string
  ): Promise<
    Array<{
      id: string;
      name: string;
      type: "patient" | "doctor";
      specialization: string;
    }>
  > => {
    if (!currentUserId) {
      return [];
    }

    const res = await api.get<
      Array<{
        id: string;
        name: string;
        type: string;
        specialization?: string;
      }>
    >(
      `/messaging/messages/recipients/${currentUserId}?userRole=${userRole}`,
      undefined,
      {
        showToastOnError: true,
        showToastOnSuccess: false,
      }
    );

    const recipients = Array.isArray(res) ? res : [];

    return recipients.map((r) => ({
      id: r.id,
      name: r.name,
      type: (r.type as "patient" | "doctor") || "patient",
      specialization: r.specialization || "General",
    }));
  },
};
