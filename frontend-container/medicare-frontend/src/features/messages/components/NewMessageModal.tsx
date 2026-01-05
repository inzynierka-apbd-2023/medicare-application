import React, { useState } from "react";
import { Button, Input, Modal } from "@shared/components";

import type { NewMessageModalProps, User } from "../types";

export const NewMessageModal: React.FC<NewMessageModalProps> = ({
  isOpen,
  onClose,
  onStartConversation,
  availableDoctors,
  isLoading = false,
}) => {
  const [selectedDoctor, setSelectedDoctor] = useState<User | null>(null);
  const [message, setMessage] = useState("");
  const [searchQuery, setSearchQuery] = useState("");

  const filteredDoctors = availableDoctors.filter(
    (doctor) =>
      doctor.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (doctor.specialty &&
        doctor.specialty.toLowerCase().includes(searchQuery.toLowerCase()))
  );

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedDoctor && message.trim()) {
      onStartConversation(
        selectedDoctor.id,
        selectedDoctor.name,
        message.trim()
      );
      handleClose();
    }
  };

  const handleClose = () => {
    setSelectedDoctor(null);
    setMessage("");
    setSearchQuery("");
    onClose();
  };

  const isFormValid = selectedDoctor && message.trim().length > 0;

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="New Message" size="lg">
      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Doctor Selection */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Select Doctor
          </label>

          {/* Search Input */}
          <Input
            type="text"
            placeholder="Search by name or specialty..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="mb-3"
          />

          {/* Doctor List */}
          <div className="max-h-48 overflow-y-auto border border-gray-200 rounded-lg">
            {filteredDoctors.length === 0 ? (
              <div className="p-4 text-center text-gray-500">
                {searchQuery
                  ? "No doctors found matching your search"
                  : "No doctors available"}
              </div>
            ) : (
              filteredDoctors.map((doctor) => (
                <div
                  key={doctor.id}
                  onClick={() => setSelectedDoctor(doctor)}
                  className={`p-3 cursor-pointer border-b border-gray-100 last:border-b-0 hover:bg-gray-50 transition-colors ${
                    selectedDoctor?.id === doctor.id
                      ? "bg-blue-50 border-blue-200"
                      : ""
                  }`}
                >
                  <div className="flex items-center space-x-3">
                    {/* Avatar */}
                    <div className="w-10 h-10 bg-gradient-to-br from-blue-500 to-blue-600 rounded-full flex items-center justify-center text-white font-semibold text-sm">
                      {doctor.name
                        .split(" ")
                        .map((n: string) => n[0])
                        .join("")
                        .substring(0, 2)}
                    </div>

                    {/* Info */}
                    <div className="flex-1">
                      <h4 className="font-medium text-gray-900">
                        {doctor.name}
                      </h4>
                      {doctor.specialty && (
                        <p className="text-sm text-gray-600">
                          {doctor.specialty}
                        </p>
                      )}
                    </div>

                    {/* Selection indicator */}
                    {selectedDoctor?.id === doctor.id && (
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

        {/* Selected Doctor Summary */}
        {selectedDoctor && (
          <div className="bg-blue-50 p-3 rounded-lg">
            <h4 className="text-sm font-medium text-blue-900 mb-1">
              Sending message to:
            </h4>
            <div className="flex items-center space-x-2">
              <div className="w-6 h-6 bg-blue-500 rounded-full flex items-center justify-center text-white text-xs font-semibold">
                {selectedDoctor.name
                  .split(" ")
                  .map((n: string) => n[0])
                  .join("")
                  .substring(0, 2)}
              </div>
              <span className="text-sm text-blue-800">
                {selectedDoctor.name}
                {selectedDoctor.specialty && ` • ${selectedDoctor.specialty}`}
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
