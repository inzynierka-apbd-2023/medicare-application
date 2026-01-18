import React, { useEffect, useRef, useState } from "react";
import { Button } from "@shared/components";
import { messagesApi } from "@shared/services/messagesApi";
import { patientsApi } from "@shared/services/patientsApi";

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
  const [availableRecipients, setAvailableRecipients] = useState<User[]>([]);

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

  // Load available recipients (doctors or patients)
  useEffect(() => {
    const fetchRecipients = async () => {
      if (userId) {
        try {
          const recipients: User[] = [];

          // 1. Fetch from Messaging Service (Contacts)
          const availableRecipientsList =
            await messagesApi.getAvailableRecipients(userType, userId);

          availableRecipientsList.forEach((d) => {
            recipients.push({
              id: d.id,
              name: d.name,
              role: d.type,
              specialty: d.specialization || "General",
              email: "", // API doesn't return email, placeholder
            } as User);
          });

          // 2. If Doctor, also fetch Patients from Patient Service (to ensure all patients are visible)
          if (userType === "doctor") {
            const patientsRes = await patientsApi.getPatients(userId);
            if (patientsRes.success) {
              patientsRes.data.forEach((p) => {
                // Check if already in list
                if (!recipients.find((r) => r.id === p.id)) {
                  recipients.push({
                    id: p.id,
                    name: p.name,
                    role: "patient",
                    email: p.email || "",
                  } as User);
                }
              });
            }
          }

          // 3. If Receptionist, fetch ALL doctors and ALL patients
          if (userType === "receptionist") {
            // Fetch all doctors from PractitionerService
            try {
              const doctorsRes = await import(
                "@shared/services/apiClient"
              ).then((m) =>
                m.apiClient.get<
                  Array<{
                    doctorId: string;
                    firstName: string;
                    lastName: string;
                    specializations?: string;
                  }>
                >("/practitioner/doctors", { params: { isActive: true } })
              );
              if (doctorsRes.data) {
                doctorsRes.data.forEach((d) => {
                  if (!recipients.find((r) => r.id === d.doctorId)) {
                    recipients.push({
                      id: d.doctorId,
                      name:
                        `${d.firstName || ""} ${d.lastName || ""}`.trim() ||
                        "Unknown Doctor",
                      role: "doctor",
                      specialty: d.specializations || "General",
                      email: "",
                    } as User);
                  }
                });
              }
            } catch (e) {
              console.error("Failed to fetch doctors for receptionist", e);
            }

            // Fetch all patients from PatientService
            try {
              const patientsRes = await import(
                "@shared/services/apiClient"
              ).then((m) =>
                m.apiClient.get<{
                  items: Array<{
                    patientId: string;
                    firstName?: string;
                    lastName?: string;
                    email?: string;
                  }>;
                }>("/patient/patients", { params: { pageSize: 100 } })
              );
              if (patientsRes.data?.items) {
                patientsRes.data.items.forEach((p) => {
                  if (!recipients.find((r) => r.id === p.patientId)) {
                    recipients.push({
                      id: p.patientId,
                      name:
                        `${p.firstName || ""} ${p.lastName || ""}`.trim() ||
                        "Unknown Patient",
                      role: "patient",
                      email: p.email || "",
                    } as User);
                  }
                });
              }
            } catch (e) {
              console.error("Failed to fetch patients for receptionist", e);
            }
          }

          setAvailableRecipients(recipients);
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
    initialMessage: string,
    recipientRole?: "patient" | "doctor" | "receptionist"
  ) => {
    try {
      // Check if conversation already exists with this user
      const existingConv = conversations.find(
        (c) => c.participantId === recipientId
      );

      if (existingConv) {
        // Reuse existing conversation
        await sendMessage(existingConv.id, initialMessage);
        selectConversation(existingConv.id);
        setIsNewMessageModalOpen(false);
        console.log(
          `[MessagesPage] Reused existing conversation ${existingConv.id}`
        );
      } else {
        // Create new conversation
        const conversationId = await createConversation(
          recipientId,
          recipientName,
          initialMessage,
          recipientRole
        );
        selectConversation(conversationId);
        setIsNewMessageModalOpen(false);
      }
    } catch (e) {
      console.error("Failed to start conversation", e);
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
            <Button
              variant="primary"
              size="sm"
              onClick={() => setIsNewMessageModalOpen(true)}
              className="flex items-center space-x-1"
            >
              <span>+</span>
              <span>New</span>
            </Button>
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
              {conversations.length === 0 && (
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
        availableRecipients={availableRecipients}
        isLoading={isLoading}
        {...(recipientId ? { preSelectedRecipientId: recipientId } : {})}
        userRole={userType}
      />
    </div>
  );
};
