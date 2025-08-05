import React, { useState } from "react";
import { Button, Modal } from "@shared/components";

import type { Appointment } from "../types";

import { StarRating } from "./StarRating";

interface DoctorRatingModalProps {
  appointment: Appointment | null;
  isOpen: boolean;
  onClose: () => void;
  onSubmitRating: (
    appointmentId: string,
    rating: number,
    comment?: string
  ) => void;
}

export const DoctorRatingModal: React.FC<DoctorRatingModalProps> = ({
  appointment,
  isOpen,
  onClose,
  onSubmitRating,
}) => {
  const [rating, setRating] = useState(appointment?.doctorRating?.rating || 0);
  const [comment, setComment] = useState(
    appointment?.doctorRating?.comment || ""
  );

  const handleSubmit = () => {
    if (appointment && rating > 0) {
      onSubmitRating(appointment.id, rating, comment.trim() || undefined);
      onClose();
    }
  };

  const handleClose = () => {
    setRating(appointment?.doctorRating?.rating || 0);
    setComment(appointment?.doctorRating?.comment || "");
    onClose();
  };

  if (!appointment) return null;

  const isEditing = !!appointment.doctorRating;

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title={isEditing ? "Edit Doctor Rating" : "Rate Doctor"}
      size="md"
    >
      <div className="space-y-4">
        {/* Doctor Info */}
        <div className="bg-gray-50 rounded-lg p-3">
          <h3 className="font-medium text-gray-900 mb-1">
            Dr. {appointment.doctor}
          </h3>
          <div className="text-sm text-gray-600">
            <div>Visit: {new Date(appointment.date).toLocaleDateString()}</div>
            {appointment.specialization && (
              <div>Specialization: {appointment.specialization}</div>
            )}
          </div>
        </div>

        {/* Rating */}
        <div className="space-y-2">
          <label className="block text-sm font-medium text-gray-700">
            Rating (required)
          </label>
          <div className="flex items-center gap-3">
            <StarRating rating={rating} onRatingChange={setRating} size={28} />
            {rating > 0 && (
              <span className="text-sm text-gray-600">{rating}/5</span>
            )}
          </div>
        </div>

        {/* Comment */}
        <div className="space-y-2">
          <label className="block text-sm font-medium text-gray-700">
            Comment (optional)
          </label>
          <textarea
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            placeholder="Share your opinion about the doctor..."
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
            rows={3}
            maxLength={300}
          />
          <div className="text-right text-xs text-gray-500">
            {comment.length}/300
          </div>
        </div>

        {/* Actions */}
        <div className="flex justify-end gap-3 pt-3 border-t border-gray-200">
          <Button variant="secondary" onClick={handleClose}>
            Cancel
          </Button>
          <Button
            variant="primary"
            onClick={handleSubmit}
            disabled={rating === 0}
          >
            {isEditing ? "Update Rating" : "Submit Rating"}
          </Button>
        </div>
      </div>
    </Modal>
  );
};
