import React, { useEffect, useRef, useState } from "react";
import { Button } from "@shared/components";
import { messagesApi } from "@shared/services/messagesApi";

import { useMessages } from "./hooks/useMessages";
import {
  ConversationList,
  MessageInput,
  MessageList,
  NewMessageModal,
} from "./components";
import type { MessagesPageProps, User } from "./types";

export const MessagesPage: React.FC<MessagesPageProps> = ({
  userId,
  userType = "patient",
  conversationId,
  recipientId,
}) => {
  const {
    conversations,
    messages,
    selectedConversationId,
    isLoading,
    error,
    selectConversation,
    sendMessage,
    markAsRead,
    createConversation,
  } = useMessages(userId, userType);

  const [isNewMessageModalOpen, setIsNewMessageModalOpen] = useState(false);
  const [availableDoctors, setAvailableDoctors] = useState<User[]>([]);

  // Track if we've handled the initial recipientId to prevent loop/re-opening
  const hasHandledRecipientRef = useRef(false);

  // Auto-select conversation OR open new message modal for recipient
  useEffect(() => {
    // Only proceed if we have a recipientId, haven't handled it yet, and conversations are loaded
    if (
      recipientId &&
      !isLoading &&
      !hasHandledRecipientRef.current &&
      conversations
    ) {
      // Check if conversation exists with this participant
      const existingConv = conversations.find(
        (c) => c.participantId === recipientId
      );

      if (existingConv) {
        if (selectedConversationId !== existingConv.id) {
          selectConversation(existingConv.id);
          console.log(
            `[MessagesPage] Selected existing conversation with ${recipientId}`
          );
        }
      } else {
        // Open new message modal with pre-selected recipient
        setIsNewMessageModalOpen(true);
        console.log(
          `[MessagesPage] Opening new message modal for ${recipientId}`
        );
      }

      hasHandledRecipientRef.current = true;
    }
  }, [
    recipientId,
    conversations,
    isLoading,
    selectedConversationId,
    selectConversation,
  ]);

  // Load available doctors/recipients
  useEffect(() => {
    const fetchRecipients = async () => {
      if (userId) {
        try {
          const res = await messagesApi.getAvailableRecipients(
            userType,
            userId
          );
          if (res.success) {
            setAvailableDoctors(
              res.data.map(
                (d) =>
                  ({
                    id: d.id,
                    name: d.name,
                    role: d.type,
                    specialty: d.specialization || "General",
                  }) as User
              )
            );
          }
        } catch (e) {
          console.error("Failed to load recipients", e);
        }
      }
    };

    fetchRecipients();
  }, [userId, userType]);

  // Auto-select conversation if provided in props
  useEffect(() => {
    if (conversationId && !selectedConversationId) {
      selectConversation(conversationId);
    }
  }, [conversationId, selectedConversationId, selectConversation]);

  const handleSendMessage = async (content: string) => {
    if (selectedConversationId) {
      await sendMessage(selectedConversationId, content);
    }
  };

  const handleStartConversation = async (
    recipientId: string,
    recipientName: string,
    initialMessage: string
  ) => {
    try {
      const conversationId = await createConversation(
        recipientId,
        recipientName,
        initialMessage
      );
      selectConversation(conversationId);
      setIsNewMessageModalOpen(false);

      // Refresh available doctors list to exclude the one we just messaged
      if (userId) {
        const res = await messagesApi.getAvailableRecipients(userType, userId);
        if (res.success) {
          setAvailableDoctors(
            res.data.map(
              (d) =>
                ({
                  id: d.id,
                  name: d.name,
                  role: d.type,
                  specialty: d.specialization || "General",
                }) as User
            )
          );
        }
      }
    } catch (error) {
      console.error("Failed to start conversation:", error);
    }
  };

  const selectedMessages = selectedConversationId
    ? messages[selectedConversationId] || []
    : [];

  if (error) {
    return (
      <div className="h-full flex items-center justify-center">
        <div className="text-center">
          <div className="text-red-500 text-4xl mb-4">⚠️</div>
          <h3 className="text-lg font-semibold text-gray-700 mb-2">
            Failed to load messages
          </h3>
          <p className="text-gray-500 mb-4">{error}</p>
          <Button variant="primary" onClick={() => window.location.reload()}>
            Try Again
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="h-full flex bg-white rounded-lg shadow-sm overflow-hidden">
      {/* Sidebar - Conversations List */}
      <div className="w-80 border-r border-gray-200 flex flex-col">
        {/* Header */}
        <div className="p-4 border-b border-gray-200">
          <div className="flex items-center justify-between mb-3">
            <h1 className="text-xl font-semibold text-gray-800">Messages</h1>
            {userType === "patient" && (
              <Button
                variant="primary"
                size="sm"
                onClick={() => setIsNewMessageModalOpen(true)}
                className="flex items-center space-x-1"
              >
                <span>+</span>
                <span>New</span>
              </Button>
            )}
          </div>

          {conversations.length > 0 && (
            <p className="text-sm text-gray-500">
              {conversations.length} conversation
              {conversations.length !== 1 ? "s" : ""}
            </p>
          )}
        </div>

        {/* Conversations */}
        <div className="flex-1 overflow-hidden">
          <ConversationList
            conversations={conversations}
            selectedConversationId={selectedConversationId}
            onSelectConversation={selectConversation}
            currentUserId={userId || ""}
          />
        </div>
      </div>

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col">
        {selectedConversationId ? (
          <>
            {/* Chat Header */}
            <div className="p-4 border-b border-gray-200 bg-white">
              {(() => {
                const conversation = conversations.find(
                  (c) => c.id === selectedConversationId
                );
                return conversation ? (
                  <div className="flex items-center space-x-3">
                    <div className="w-10 h-10 bg-gradient-to-br from-blue-500 to-blue-600 rounded-full flex items-center justify-center text-white font-semibold text-sm">
                      {conversation.participantName
                        .split(" ")
                        .map((n: string) => n[0])
                        .join("")
                        .substring(0, 2)}
                    </div>
                    <div>
                      <h2 className="font-semibold text-gray-800">
                        {conversation.participantName}
                      </h2>
                      <p className="text-sm text-gray-500 capitalize">
                        {conversation.participantType}
                      </p>
                    </div>
                  </div>
                ) : null;
              })()}
            </div>

            {/* Messages */}
            <MessageList
              messages={selectedMessages}
              currentUserId={userId || ""}
              isLoading={isLoading}
              onMarkAsRead={markAsRead}
            />

            {/* Message Input */}
            <div className="border-t border-gray-200 bg-white">
              <MessageInput
                onSendMessage={handleSendMessage}
                isLoading={isLoading}
                placeholder="Type your message..."
              />
            </div>
          </>
        ) : (
          /* Empty State */
          <div className="flex-1 flex items-center justify-center bg-gray-50">
            <div className="text-center">
              <div className="text-6xl mb-4">💬</div>
              <h3 className="text-lg font-semibold text-gray-700 mb-2">
                Select a conversation
              </h3>
              <p className="text-gray-500 mb-4">
                Choose a conversation from the sidebar to start messaging
              </p>
              {userType === "patient" && conversations.length === 0 && (
                <Button
                  variant="primary"
                  onClick={() => setIsNewMessageModalOpen(true)}
                >
                  Start New Conversation
                </Button>
              )}
            </div>
          </div>
        )}
      </div>

      {/* New Message Modal */}
      <NewMessageModal
        isOpen={isNewMessageModalOpen}
        onClose={() => setIsNewMessageModalOpen(false)}
        onStartConversation={handleStartConversation}
        availableDoctors={availableDoctors}
        isLoading={isLoading}
        {...(recipientId ? { preSelectedRecipientId: recipientId } : {})}
      />
    </div>
  );
};
