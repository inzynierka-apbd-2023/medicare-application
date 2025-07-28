import React from "react";
import {
  Calendar,
  User,
  CreditCard,
  Stethoscope,
  FileText,
} from "lucide-react";
import { Modal, Badge } from "../../../shared/components";
import type { Appointment } from "../types";

interface AppointmentsDetailsModalProps {
  isOpen: boolean;
  appointment: Appointment | null;
  onClose: () => void;
}

const AppointmentsDetailsModal: React.FC<AppointmentsDetailsModalProps> = ({
  isOpen,
  appointment,
  onClose,
}) => {
  if (!appointment) return null;

  const getStatusBadge = () => {
    if (appointment.status === "upcoming") {
      return <Badge variant="success">Upcoming</Badge>;
    }
    if (appointment.status === "cancelled") {
      return <Badge variant="error">Cancelled</Badge>;
    }
    if (appointment.status === "past") {
      return <Badge variant="default">Completed</Badge>;
    }
    return <Badge variant="default">{appointment.status}</Badge>;
  };

  const getPaymentBadge = () => {
    if (appointment.paymentStatus === "paid") {
      return <Badge variant="paid">Fully Paid</Badge>;
    }
    return <Badge variant="unpaid">Unpaid</Badge>;
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Appointment Details"
      size="lg"
    >
      <div className="space-y-6">
        {/* Date and Time */}
        <div className="flex items-center gap-3 p-4 bg-blue-50 rounded-lg">
          <Calendar className="text-blue-600 flex-shrink-0" size={20} />
          <div>
            <div className="font-semibold text-blue-700">
              {new Date(appointment.date).toLocaleDateString("en-US", {
                weekday: "long",
                year: "numeric",
                month: "long",
                day: "numeric",
              })}
            </div>
            <div className="text-blue-600">{appointment.time}</div>
          </div>
        </div>

        {/* Doctor Information */}
        <div className="flex items-center gap-3 p-4 bg-gray-50 rounded-lg">
          <User className="text-gray-600 flex-shrink-0" size={20} />
          <div>
            <div className="font-semibold text-gray-800">
              Dr. {appointment.doctor}
            </div>
            {appointment.specialization && (
              <div className="text-gray-600 text-sm">
                {appointment.specialization}
              </div>
            )}
          </div>
        </div>

        {/* Status and Payment */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2">
            <div className="font-medium text-gray-700">Status</div>
            {getStatusBadge()}
          </div>
          <div className="space-y-2">
            <div className="font-medium text-gray-700">Payment</div>
            {getPaymentBadge()}
          </div>
        </div>

        {/* Payment Details */}
        <div className="flex items-center gap-3 p-4 bg-green-50 rounded-lg">
          <CreditCard className="text-green-600 flex-shrink-0" size={20} />
          <div>
            <div className="font-medium text-gray-800">Payment Information</div>
            <div className="text-sm text-gray-600">
              Total cost:{" "}
              <span className="font-semibold">{appointment.total} PLN</span>
            </div>
            <div className="text-sm text-gray-600">
              Status:{" "}
              <span className="font-semibold">
                {appointment.paymentStatus === "paid"
                  ? "Paid in full"
                  : "Payment pending"}
              </span>
            </div>
          </div>
        </div>

        {/* Visit Purpose */}
        {appointment.description && (
          <div className="space-y-2">
            <div className="flex items-center gap-2 font-medium text-gray-700">
              <FileText size={16} />
              Visit Purpose
            </div>
            <div className="p-4 bg-blue-50 rounded-lg text-gray-700">
              {appointment.description}
            </div>
          </div>
        )}
      </div>
    </Modal>
  );
};

export { AppointmentsDetailsModal };
