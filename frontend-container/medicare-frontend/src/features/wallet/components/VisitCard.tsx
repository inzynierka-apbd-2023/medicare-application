import React from "react";
import { Calendar, User } from "lucide-react";
import { Button, Card, Badge } from "../../../shared/components";
import type { AppointmentCardProps } from "../types";

export const AppointmentCard: React.FC<AppointmentCardProps> = ({
  appointment,
  onPay,
  isPaying = false,
}) => {
  const getPaymentBadge = () => {
    if (appointment.paymentStatus === "paid") {
      return <Badge variant="paid">Paid</Badge>;
    }
    return <Badge variant="unpaid">Payment Pending</Badge>;
  };

  return (
    <Card
      variant="medical"
      padding="lg"
      className="transition-all hover:shadow-xl border border-gray-200 bg-gray-50"
    >
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-6">
        {/* Appointment Info */}
        <div className="flex-1 space-y-4">
          <div className="flex flex-col sm:flex-row sm:items-center gap-4">
            <div className="space-y-2">
              <div className="flex items-center gap-2 text-lg font-semibold text-gray-800">
                <Calendar size={18} />
                {new Date(appointment.date).toLocaleDateString()}
                <span className="text-gray-600 font-normal text-base">
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
          <div className="flex items-center gap-4">
            <span className="text-sm text-gray-700">
              Total cost:{" "}
              <span className="font-semibold text-gray-900">
                {appointment.total} PLN
              </span>
            </span>
            {getPaymentBadge()}
          </div>
        </div>

        {/* Action Button */}
        <div className="flex gap-3 self-end md:self-center">
          <Button
            variant={isPaying ? "gray" : "success"}
            disabled={isPaying || appointment.paymentStatus === "paid"}
            onClick={() => onPay(appointment.id)}
            className="min-w-32 shadow-md"
          >
            {isPaying
              ? "Processing..."
              : appointment.paymentStatus === "paid"
                ? "Paid"
                : `Pay ${appointment.total} PLN`}
          </Button>
        </div>
      </div>
    </Card>
  );
};
