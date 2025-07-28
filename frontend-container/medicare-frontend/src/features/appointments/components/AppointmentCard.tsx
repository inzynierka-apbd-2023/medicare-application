import React from "react";
import { useNavigate } from "react-router-dom";
import {
  CreditCard,
  CheckCircle,
  XCircle,
  Info,
  Calendar,
  User,
  FileText,
} from "lucide-react";
import { Card, Button, Badge } from "../../../shared/components";
import type { AppointmentCardProps } from "../types";

export const AppointmentCard: React.FC<AppointmentCardProps> = ({
  appointment,
  onDetails,
  onPayment,
  onCancel,
  isUpcoming = false,
}) => {
  const navigate = useNavigate();

  const getStatusBadge = () => {
    if (appointment.status === "upcoming") {
      return (
        <Badge variant="success" icon={<CheckCircle size={14} />}>
          Upcoming
        </Badge>
      );
    }
    if (appointment.status === "cancelled") {
      return (
        <Badge variant="error" icon={<XCircle size={14} />}>
          Cancelled
        </Badge>
      );
    }
    return (
      <Badge variant="default" icon={<Info size={14} />}>
        {appointment.status}
      </Badge>
    );
  };

  const getPaymentBadge = () => {
    if (appointment.paymentStatus === "paid") {
      return (
        <Badge variant="paid" icon={<CreditCard size={14} />}>
          Paid
        </Badge>
      );
    }
    return (
      <Badge variant="unpaid" icon={<CreditCard size={14} />}>
        Unpaid
      </Badge>
    );
  };

  const handlePaymentClick = () => {
    if (onPayment) {
      onPayment(appointment.id);
    } else {
      navigate("/user/wallet");
    }
  };

  const renderUpcomingActions = () => (
    <>
      <Button variant="primary" onClick={() => onDetails(appointment)}>
        Details
      </Button>
      <Button
        variant={appointment.paymentStatus === "paid" ? "gray" : "success"}
        onClick={handlePaymentClick}
        disabled={appointment.paymentStatus === "paid"}
      >
        {appointment.paymentStatus === "paid" ? "Paid" : "Pay Now"}
      </Button>
    </>
  );

  const renderPastActions = () => (
    <>
      <Button
        variant="emerald"
        leftIcon={<FileText size={16} />}
        onClick={() => navigate(`/documents?appointmentId=${appointment.id}`)}
      >
        Documents
      </Button>
      <Button variant="secondary" onClick={() => onDetails(appointment)}>
        Details
      </Button>
    </>
  );

  return (
    <Card
      variant={isUpcoming ? "medical" : "default"}
      padding="lg"
      className={`transition-all hover:shadow-lg ${!isUpcoming ? "opacity-70" : ""}`}
    >
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-6">
        {/* Appointment Info */}
        <div className="flex-1 space-y-4">
          <div className="flex flex-col sm:flex-row sm:items-center gap-4">
            <div className="space-y-2">
              <div className="flex items-center gap-2 text-lg font-semibold text-blue-700">
                <Calendar size={18} />
                {new Date(appointment.date).toLocaleDateString()}
                <span className="text-gray-500 font-normal text-base">
                  {appointment.time}
                </span>
              </div>
              <div className="flex items-center gap-2 text-gray-800">
                <User size={16} />
                <span className="font-medium">Dr. {appointment.doctor}</span>
              </div>
              {appointment.specialization && (
                <div className="text-sm text-gray-600">
                  Specialization: {appointment.specialization}
                </div>
              )}
            </div>
          </div>

          {/* Payment Info */}
          <div className="flex items-center gap-2">
            <span className="text-sm text-gray-600">
              Total cost: {appointment.total} PLN
            </span>
          </div>

          {/* Status Badges */}
          <div className="flex gap-2">
            {getStatusBadge()}
            {getPaymentBadge()}
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex gap-3 self-end md:self-center">
          {isUpcoming ? renderUpcomingActions() : renderPastActions()}
        </div>
      </div>
    </Card>
  );
};
