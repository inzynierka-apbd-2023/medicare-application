import { useEffect, useState } from "react";

import type { Conversation, Message } from "../../features/messages/types";
import { messagesApi } from "../services/messagesApi";

interface UseMessagesResult {
  conversations: Conversation[];
  messages: Message[];
  selectedConversationId: string | undefined;
  isLoading: boolean;
  error: string | null;
  sendMessage: (content: string) => Promise<void>;
  selectConversation: (conversationId: string) => void;
  startNewConversation: (
    recipientId: string,
    recipientName: string,
    recipientType: "patient" | "doctor",
    initialMessage: string
  ) => Promise<void>;
  markMessageAsRead: (messageId: string) => Promise<void>;
  refetch: () => Promise<void>;
}

export const useMessages = (
  userId: string,
  userName: string,
  userType: "patient" | "doctor",
  initialConversationId?: string
): UseMessagesResult => {
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [messages, setMessages] = useState<Message[]>([]);
  const [selectedConversationId, setSelectedConversationId] = useState<
    string | undefined
  >(initialConversationId);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Pobierz konwersacje
  const fetchConversations = async () => {
    try {
      setError(null);
      const response = await messagesApi.getConversations(userId, userType);

      if (response.success) {
        setConversations(response.data);

        // Jeśli nie ma wybranej konwersacji, wybierz pierwszą
        if (!selectedConversationId && response.data.length > 0) {
          setSelectedConversationId(response.data[0].id);
        }
      } else {
        setError(response.error || "Failed to fetch conversations");
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "An unexpected error occurred"
      );
    }
  };

  // Pobierz wiadomości dla wybranej konwersacji
  const fetchMessages = async (conversationId: string) => {
    try {
      setIsLoading(true);
      setError(null);

      const response = await messagesApi.getMessages(conversationId);

      if (response.success) {
        setMessages(response.data);
      } else {
        setError(response.error || "Failed to fetch messages");
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "An unexpected error occurred"
      );
    } finally {
      setIsLoading(false);
    }
  };

  // Wyślij wiadomość
  const sendMessage = async (content: string) => {
    if (!selectedConversationId) {
      setError("No conversation selected");
      return;
    }

    const selectedConversation = conversations.find(
      (c) => c.id === selectedConversationId
    );
    if (!selectedConversation) {
      setError("Selected conversation not found");
      return;
    }

    try {
      setError(null);

      const response = await messagesApi.sendMessage(
        selectedConversationId,
        userId,
        userName,
        userType,
        selectedConversation.participantId,
        selectedConversation.participantName,
        selectedConversation.participantType,
        content
      );

      if (response.success) {
        // Dodaj nową wiadomość do listy
        setMessages((prev) => [...prev, response.data]);

        // Zaktualizuj konwersację
        setConversations((prev) =>
          prev.map((conv) =>
            conv.id === selectedConversationId
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
      setError(err instanceof Error ? err.message : "Failed to send message");
    }
  };

  // Wybierz konwersację
  const selectConversation = (conversationId: string) => {
    setSelectedConversationId(conversationId);
  };

  // Rozpocznij nową konwersację
  const startNewConversation = async (
    recipientId: string,
    recipientName: string,
    recipientType: "patient" | "doctor",
    initialMessage: string
  ) => {
    try {
      setError(null);

      const response = await messagesApi.startConversation(
        userId,
        userName,
        userType,
        recipientId,
        recipientName,
        recipientType,
        initialMessage
      );

      if (response.success) {
        const { conversation, message } = response.data;

        // Dodaj nową konwersację
        setConversations((prev) => [conversation, ...prev]);

        // Wybierz nową konwersację
        setSelectedConversationId(conversation.id);

        // Ustaw wiadomości dla nowej konwersacji
        setMessages([message]);
      } else {
        setError(response.error || "Failed to start conversation");
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to start conversation"
      );
    }
  };

  // Oznacz wiadomość jako przeczytaną
  const markMessageAsRead = async (messageId: string) => {
    try {
      const response = await messagesApi.markMessageAsRead(messageId);

      if (response.success) {
        // Zaktualizuj status wiadomości
        setMessages((prev) =>
          prev.map((msg) =>
            msg.id === messageId ? { ...msg, isRead: true } : msg
          )
        );

        // Zaktualizuj licznik nieprzeczytanych w konwersacji
        const message = messages.find((m) => m.id === messageId);
        if (message) {
          setConversations((prev) =>
            prev.map((conv) =>
              conv.id === message.conversationId && conv.unreadCount > 0
                ? { ...conv, unreadCount: conv.unreadCount - 1 }
                : conv
            )
          );
        }
      }
    } catch (err) {
      console.error("Failed to mark message as read:", err);
    }
  };

  // Odśwież dane
  const refetch = async () => {
    setIsLoading(true);
    await fetchConversations();
    if (selectedConversationId) {
      await fetchMessages(selectedConversationId);
    }
    setIsLoading(false);
  };

  // Pobierz konwersacje przy pierwszym załadowaniu
  useEffect(() => {
    fetchConversations();
  }, [fetchConversations]);

  // Pobierz wiadomości gdy wybrana zostanie konwersacja
  useEffect(() => {
    if (selectedConversationId) {
      fetchMessages(selectedConversationId);
    } else {
      setMessages([]);
      setIsLoading(false);
    }
  }, [selectedConversationId]);

  return {
    conversations,
    messages,
    selectedConversationId,
    isLoading,
    error,
    sendMessage,
    selectConversation,
    startNewConversation,
    markMessageAsRead,
    refetch,
  };
};
