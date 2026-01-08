import React, { useState } from "react";
import { Button, Input, Modal } from "@shared/components";

import type { NewMessageModalProps, User } from "../types";

export const NewMessageModal: React.FC<NewMessageModalProps> = ({
  isOpen,
  onClose,
  onStartConversation,
  availableRecipients,
  isLoading = false,
  preSelectedRecipientId,
  userRole = "patient", // Current user's role
}) => {
  const [selectedRecipient, setSelectedRecipient] = useState<User | null>(
    () => {
      if (preSelectedRecipientId) {
        return (
          availableRecipients.find((d) => d.id === preSelectedRecipientId) ||
          null
        );
      }
      return null;
    }
  );

  // Update selected recipient when preSelectedRecipientId or availableRecipients changes
  React.useEffect(() => {
    if (preSelectedRecipientId && isOpen) {
      const found = availableRecipients.find(
        (d) => d.id === preSelectedRecipientId
      );
      if (found) setSelectedRecipient(found);
    }
  }, [preSelectedRecipientId, availableRecipients, isOpen]);

  const [message, setMessage] = useState("");
  const [searchQuery, setSearchQuery] = useState("");

  const filteredRecipients = availableRecipients.filter(
    (recipient) =>
      recipient.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (recipient.specialty &&
        recipient.specialty.toLowerCase().includes(searchQuery.toLowerCase()))
  );

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedRecipient && message.trim()) {
      onStartConversation(
        selectedRecipient.id,
        selectedRecipient.name,
        message.trim(),
        selectedRecipient.role // Pass recipient's role
      );
      handleClose();
    }
  };

  const handleClose = () => {
    setSelectedRecipient(null);
    setMessage("");
    setSearchQuery("");
    onClose();
  };

  const isFormValid = selectedRecipient && message.trim().length > 0;

  // Determine what to call the recipient
  let targetLabel = "Recipient";
  if (userRole === "doctor") targetLabel = "Patient";
  else if (userRole === "patient") targetLabel = "Doctor";
  else if (userRole === "receptionist") targetLabel = "Recipient";

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="New Message" size="lg">
      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Recipient Selection */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Select {targetLabel}
          </label>

          {/* Search Input */}
          <Input
            type="text"
            placeholder={`Search by name${userRole === "patient" ? " or specialty" : ""}...`}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="mb-3"
          />

          {/* Recipient List */}
          <div className="max-h-48 overflow-y-auto border border-gray-200 rounded-lg">
            {filteredRecipients.length === 0 ? (
              <div className="p-4 text-center text-gray-500">
                {searchQuery
                  ? "No users found matching your search"
                  : `No ${targetLabel.toLowerCase()}s available`}
              </div>
            ) : (
              filteredRecipients.map((recipient) => (
                <div
                  key={recipient.id}
                  onClick={() => setSelectedRecipient(recipient)}
                  className={`p-3 cursor-pointer border-b border-gray-100 last:border-b-0 hover:bg-gray-50 transition-colors ${
                    selectedRecipient?.id === recipient.id
                      ? "bg-blue-50 border-blue-200"
                      : ""
                  }`}
                >
                  <div className="flex items-center space-x-3">
                    {/* Avatar */}
                    <div className="w-10 h-10 bg-gradient-to-br from-blue-500 to-blue-600 rounded-full flex items-center justify-center text-white font-semibold text-sm">
                      {recipient.name
                        .split(" ")
                        .map((n: string) => n[0])
                        .join("")
                        .substring(0, 2)}
                    </div>

                    {/* Info */}
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <h4 className="font-medium text-gray-900">
                          {recipient.name}
                        </h4>
                        {/* Show role badge for receptionists */}
                        {userRole === "receptionist" && (
                          <span
                            className={`text-xs px-2 py-0.5 rounded-full ${
                              recipient.role === "doctor"
                                ? "bg-purple-100 text-purple-700"
                                : "bg-green-100 text-green-700"
                            }`}
                          >
                            {recipient.role === "doctor" ? "Doctor" : "Patient"}
                          </span>
                        )}
                      </div>
                      {recipient.specialty && (
                        <p className="text-sm text-gray-600">
                          {recipient.specialty}
                        </p>
                      )}
                    </div>

                    {/* Selection indicator */}
                    {selectedRecipient?.id === recipient.id && (
                      <div className="w-4 h-4 bg-blue-500 rounded-full flex items-center justify-center">
                        <svg
                          className="w-2 h-2 text-white"
                          fill="currentColor"
                          viewBox="0 0 8 8"
                        >
                          <path d="M6.564.75l-3.59 3.612-1.538-1.55L0 4.26l2.974 2.99L8 2.193z" />
                        </svg>
                      </div>
                    )}
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        {/* Message Input */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Message
          </label>
          <textarea
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            placeholder="Type your message here..."
            rows={4}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 resize-none"
            required
          />
          <p className="text-xs text-gray-500 mt-1">
            {message.length}/500 characters
          </p>
        </div>

        {/* Selected Recipient Summary */}
        {selectedRecipient && (
          <div className="bg-blue-50 p-3 rounded-lg">
            <h4 className="text-sm font-medium text-blue-900 mb-1">
              Sending message to:
            </h4>
            <div className="flex items-center space-x-2">
              <div className="w-6 h-6 bg-blue-500 rounded-full flex items-center justify-center text-white text-xs font-semibold">
                {selectedRecipient.name
                  .split(" ")
                  .map((n: string) => n[0])
                  .join("")
                  .substring(0, 2)}
              </div>
              <span className="text-sm text-blue-800">
                {selectedRecipient.name}
                {selectedRecipient.specialty &&
                  ` • ${selectedRecipient.specialty}`}
              </span>
            </div>
          </div>
        )}

        {/* Action Buttons */}
        <div className="flex justify-end space-x-3 pt-4 border-t border-gray-200">
          <Button
            type="button"
            variant="secondary"
            onClick={handleClose}
            disabled={isLoading}
          >
            Cancel
          </Button>
          <Button
            type="submit"
            variant="primary"
            disabled={!isFormValid || isLoading}
            loading={isLoading}
          >
            Send Message
          </Button>
        </div>
      </form>
    </Modal>
  );
};
