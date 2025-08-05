import React, { useEffect, useState } from "react";
import { Button } from "@shared/components";

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

  // Mock available doctors - in real app this would come from API
  useEffect(() => {
    if (userType === "patient") {
      setAvailableDoctors([
        {
          id: "doc1",
          name: "Dr. Anna Kowalska",
          role: "doctor",
          email: "anna.kowalska@clinic.com",
          specialty: "Cardiologist",
        },
        {
          id: "doc2",
          name: "Dr. Piotr Nowak",
          role: "doctor",
          email: "piotr.nowak@clinic.com",
          specialty: "Dermatologist",
        },
        {
          id: "doc3",
          name: "Dr. Maria Wiśniewska",
          role: "doctor",
          email: "maria.wisniewska@clinic.com",
          specialty: "Neurologist",
        },
      ]);
    }
  }, [userType]);

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
    initialMessage: string
  ) => {
    try {
      const conversationId = await createConversation(
        recipientId,
        initialMessage
      );
      selectConversation(conversationId);
      setIsNewMessageModalOpen(false);
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
                        {conversation.participantType === "doctor" &&
                          " • Online"}
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
      />
    </div>
  );
};
