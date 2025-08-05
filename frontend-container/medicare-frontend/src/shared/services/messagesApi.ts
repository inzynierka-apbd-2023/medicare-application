import type { Conversation, Message } from "../../features/messages/types";

import { ApiResponse, createErrorResponse, createMockResponse } from "./api";

// Mock data dla wiadomości
const mockMessages: Message[] = [
  {
    id: "msg_1",
    conversationId: "conv_patient_1_doctor_1",
    senderId: "patient_1",
    senderName: "John Doe",
    senderType: "patient",
    receiverId: "doc_alice_heart",
    receiverName: "Dr. Alice Heart",
    receiverType: "doctor",
    content:
      "Hello Doctor, I have a question about my recent blood test results. When will they be available?",
    timestamp: "2025-08-04T10:30:00Z",
    isRead: true,
  },
  {
    id: "msg_2",
    conversationId: "conv_patient_1_doctor_1",
    senderId: "doc_alice_heart",
    senderName: "Dr. Alice Heart",
    senderType: "doctor",
    receiverId: "patient_1",
    receiverName: "John Doe",
    receiverType: "patient",
    content:
      "Hello John, your blood test results should be available by tomorrow afternoon. I'll review them and send you a detailed explanation.",
    timestamp: "2025-08-04T11:15:00Z",
    isRead: true,
  },
  {
    id: "msg_3",
    conversationId: "conv_patient_1_doctor_1",
    senderId: "patient_1",
    senderName: "John Doe",
    senderType: "patient",
    receiverId: "doc_alice_heart",
    receiverName: "Dr. Alice Heart",
    receiverType: "doctor",
    content:
      "Thank you for the quick response. I also wanted to ask about the medication dosage - should I continue taking 20mg daily?",
    timestamp: "2025-08-04T14:20:00Z",
    isRead: false,
  },
  {
    id: "msg_4",
    conversationId: "conv_patient_2_doctor_2",
    senderId: "patient_2",
    senderName: "Maria Smith",
    senderType: "patient",
    receiverId: "doc_bob_vessel",
    receiverName: "Dr. Bob Vessel",
    receiverType: "doctor",
    content:
      "Hi Dr. Vessel, I'm experiencing some side effects from the new medication. Should I be concerned?",
    timestamp: "2025-08-04T09:00:00Z",
    isRead: true,
  },
  {
    id: "msg_5",
    conversationId: "conv_patient_2_doctor_2",
    senderId: "doc_bob_vessel",
    senderName: "Dr. Bob Vessel",
    senderType: "doctor",
    receiverId: "patient_2",
    receiverName: "Maria Smith",
    receiverType: "patient",
    content:
      "Hi Maria, please describe the side effects you're experiencing. We may need to adjust the dosage or switch to an alternative medication.",
    timestamp: "2025-08-04T09:45:00Z",
    isRead: true,
  },
  {
    id: "msg_6",
    conversationId: "conv_patient_2_doctor_2",
    senderId: "patient_2",
    senderName: "Maria Smith",
    senderType: "patient",
    receiverId: "doc_bob_vessel",
    receiverName: "Dr. Bob Vessel",
    receiverType: "doctor",
    content:
      "I've been feeling nauseous and dizzy, especially in the mornings. This started about 3 days after I began taking the medication.",
    timestamp: "2025-08-04T16:30:00Z",
    isRead: false,
  },
  {
    id: "msg_7",
    conversationId: "conv_patient_3_doctor_1",
    senderId: "patient_3",
    senderName: "Adam Nowak",
    senderType: "patient",
    receiverId: "doc_alice_heart",
    receiverName: "Dr. Alice Heart",
    receiverType: "doctor",
    content:
      "Doctor, I uploaded my recent blood pressure readings as you requested. Please let me know if you need anything else.",
    timestamp: "2025-08-04T08:15:00Z",
    isRead: true,
  },
  {
    id: "msg_8",
    conversationId: "conv_patient_3_doctor_1",
    senderId: "doc_alice_heart",
    senderName: "Dr. Alice Heart",
    senderType: "doctor",
    receiverId: "patient_3",
    receiverName: "Adam Nowak",
    receiverType: "patient",
    content:
      "Thank you Adam. The readings look good overall. Continue with your current medication and keep monitoring twice daily.",
    timestamp: "2025-08-04T12:00:00Z",
    isRead: true,
  },
];

// Konstruujemy konwersacje po zdefiniowaniu wiadomości
const createMockConversations = (): Conversation[] => {
  const msg3 = mockMessages.find((m) => m.id === "msg_3");
  const msg6 = mockMessages.find((m) => m.id === "msg_6");
  const msg8 = mockMessages.find((m) => m.id === "msg_8");

  return [
    {
      id: "conv_patient_1_doctor_1",
      participantId: "doc_alice_heart",
      participantName: "Dr. Alice Heart",
      participantType: "doctor",
      participants: [
        {
          id: "doc_alice_heart",
          name: "Dr. Alice Heart",
          role: "doctor",
          email: "alice.heart@clinic.com",
          specialty: "General Medicine",
        },
      ],
      ...(msg3 && { lastMessage: msg3 }),
      unreadCount: 1,
      isActive: true,
      createdAt: "2025-08-04T10:30:00Z",
      updatedAt: "2025-08-04T14:20:00Z",
    },
    {
      id: "conv_patient_2_doctor_2",
      participantId: "doc_bob_vessel",
      participantName: "Dr. Bob Vessel",
      participantType: "doctor",
      participants: [
        {
          id: "doc_bob_vessel",
          name: "Dr. Bob Vessel",
          role: "doctor",
          email: "bob.vessel@clinic.com",
          specialty: "General Medicine",
        },
      ],
      ...(msg6 && { lastMessage: msg6 }),
      unreadCount: 1,
      isActive: true,
      createdAt: "2025-08-04T09:00:00Z",
      updatedAt: "2025-08-04T16:30:00Z",
    },
    {
      id: "conv_patient_3_doctor_1",
      participantId: "doc_alice_heart",
      participantName: "Dr. Alice Heart",
      participantType: "doctor",
      participants: [
        {
          id: "doc_alice_heart",
          name: "Dr. Alice Heart",
          role: "doctor",
          email: "alice.heart@clinic.com",
          specialty: "General Medicine",
        },
      ],
      ...(msg8 && { lastMessage: msg8 }),
      unreadCount: 0,
      isActive: true,
      createdAt: "2025-08-04T08:15:00Z",
      updatedAt: "2025-08-04T12:00:00Z",
    },
  ];
};

const mockConversations = createMockConversations();

// API dla możliwych odbiorców wiadomości
const mockAvailableDoctors = [
  {
    id: "doc_alice_heart",
    name: "Dr. Alice Heart",
    type: "doctor" as const,
    specialization: "General Medicine",
  },
  {
    id: "doc_bob_vessel",
    name: "Dr. Bob Vessel",
    type: "doctor" as const,
    specialization: "General Medicine",
  },
  {
    id: "doc_carol_serum",
    name: "Dr. Carol Serum",
    type: "doctor" as const,
    specialization: "Laboratory Medicine",
  },
];

export const messagesApi = {
  /**
   * Pobierz wszystkie konwersacje dla użytkownika
   */
  getConversations: async (
    userId: string,
    userType: "patient" | "doctor"
  ): Promise<ApiResponse<Conversation[]>> => {
    try {
      await new Promise((resolve) => setTimeout(resolve, 500));

      // Filtruj konwersacje na podstawie typu użytkownika
      const filteredConversations = mockConversations.filter((conv) => {
        if (userType === "patient") {
          // Dla pacjenta pokazuj konwersacje gdzie jest uczestnikiem
          return mockMessages.some(
            (msg) =>
              msg.conversationId === conv.id &&
              ((msg.senderId === userId && msg.senderType === "patient") ||
                (msg.receiverId === userId && msg.receiverType === "patient"))
          );
        } else {
          // Dla lekarza pokazuj konwersacje gdzie jest uczestnikiem
          return mockMessages.some(
            (msg) =>
              msg.conversationId === conv.id &&
              ((msg.senderId === userId && msg.senderType === "doctor") ||
                (msg.receiverId === userId && msg.receiverType === "doctor"))
          );
        }
      });

      return createMockResponse(filteredConversations);
    } catch (_error) {
      return createErrorResponse("Failed to fetch conversations");
    }
  },

  /**
   * Pobierz wiadomości dla konkretnej konwersacji
   */
  getMessages: async (
    conversationId: string
  ): Promise<ApiResponse<Message[]>> => {
    try {
      await new Promise((resolve) => setTimeout(resolve, 300));

      const conversationMessages = mockMessages.filter(
        (msg) => msg.conversationId === conversationId
      );

      return createMockResponse(conversationMessages);
    } catch (_error) {
      return createErrorResponse("Failed to fetch messages");
    }
  },

  /**
   * Wyślij nową wiadomość
   */
  sendMessage: async (
    conversationId: string,
    senderId: string,
    senderName: string,
    senderType: "patient" | "doctor",
    receiverId: string,
    receiverName: string,
    receiverType: "patient" | "doctor",
    content: string
  ): Promise<ApiResponse<Message>> => {
    try {
      await new Promise((resolve) => setTimeout(resolve, 600));

      const newMessage: Message = {
        id: `msg_${Date.now()}`,
        conversationId,
        senderId,
        senderName,
        senderType,
        receiverId,
        receiverName,
        receiverType,
        content,
        timestamp: new Date().toISOString(),
        isRead: false,
      };

      // Dodaj do mock data
      mockMessages.push(newMessage);

      // Zaktualizuj konwersację
      const conversation = mockConversations.find(
        (c) => c.id === conversationId
      );
      if (conversation) {
        conversation.lastMessage = newMessage;
        conversation.updatedAt = newMessage.timestamp;
        if (senderType !== conversation.participantType) {
          conversation.unreadCount += 1;
        }
      }

      return createMockResponse(newMessage);
    } catch (_error) {
      return createErrorResponse("Failed to send message");
    }
  },

  /**
   * Rozpocznij nową konwersację
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
      await new Promise((resolve) => setTimeout(resolve, 800));

      const conversationId = `conv_${senderType}_${senderId}_${receiverType}_${receiverId}`;

      // Sprawdź czy konwersacja już istnieje
      const existingConversation = mockConversations.find(
        (c) => c.id === conversationId
      );
      if (existingConversation) {
        return createErrorResponse("Conversation already exists");
      }

      const newMessage: Message = {
        id: `msg_${Date.now()}`,
        conversationId,
        senderId,
        senderName,
        senderType,
        receiverId,
        receiverName,
        receiverType,
        content: initialMessage,
        timestamp: new Date().toISOString(),
        isRead: false,
      };

      const newConversation: Conversation = {
        id: conversationId,
        participantId: receiverId,
        participantName: receiverName,
        participantType: receiverType,
        participants: [
          {
            id: receiverId,
            name: receiverName,
            role: receiverType,
            email: `${receiverId}@clinic.com`,
            ...(receiverType === "doctor" && { specialty: "General Medicine" }),
          },
        ],
        lastMessage: newMessage,
        unreadCount: 1,
        isActive: true,
        createdAt: newMessage.timestamp,
        updatedAt: newMessage.timestamp,
      };

      // Dodaj do mock data
      mockMessages.push(newMessage);
      mockConversations.push(newConversation);

      return createMockResponse({
        conversation: newConversation,
        message: newMessage,
      });
    } catch (_error) {
      return createErrorResponse("Failed to start conversation");
    }
  },

  /**
   * Oznacz wiadomość jako przeczytaną
   */
  markMessageAsRead: async (
    messageId: string
  ): Promise<ApiResponse<boolean>> => {
    try {
      await new Promise((resolve) => setTimeout(resolve, 200));

      const message = mockMessages.find((m) => m.id === messageId);
      if (message) {
        message.isRead = true;

        // Zaktualizuj licznik nieprzeczytanych w konwersacji
        const conversation = mockConversations.find(
          (c) => c.id === message.conversationId
        );
        if (conversation && conversation.unreadCount > 0) {
          conversation.unreadCount -= 1;
        }
      }

      return createMockResponse(true);
    } catch (_error) {
      return createErrorResponse("Failed to mark message as read");
    }
  },

  /**
   * Pobierz dostępnych odbiorców wiadomości
   */
  getAvailableRecipients: async (
    userType: "patient" | "doctor"
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
      await new Promise((resolve) => setTimeout(resolve, 400));

      if (userType === "patient") {
        // Pacjenci mogą pisać do lekarzy
        return createMockResponse(mockAvailableDoctors);
      } else {
        // Lekarze mogą pisać do pacjentów (w rzeczywistej aplikacji byłby to API call)
        const mockPatients = [
          { id: "patient_1", name: "John Doe", type: "patient" as const },
          { id: "patient_2", name: "Maria Smith", type: "patient" as const },
          { id: "patient_3", name: "Adam Nowak", type: "patient" as const },
        ];
        return createMockResponse(mockPatients);
      }
    } catch (_error) {
      return createErrorResponse("Failed to fetch available recipients");
    }
  },
};
