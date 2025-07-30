import React from "react";

import type { AppointmentSectionProps } from "../types";

import { AppointmentCard } from "./AppointmentCard";

export const AppointmentSection: React.FC<AppointmentSectionProps> = ({
  title,
  appointments,
  onDetails,
  onPayment,
  onCancel,
  isUpcoming = false,
  emptyMessage,
}) => (
  <div className="space-y-6">
    <h3 className="text-2xl font-bold text-blue-700">{title}</h3>
    {appointments.length === 0 ? (
      <div className="text-center text-gray-500 py-8">{emptyMessage}</div>
    ) : (
      <div className="space-y-4">
        {appointments.map((appointment) => (
          <AppointmentCard
            key={appointment.id}
            appointment={appointment}
            onDetails={onDetails}
            {...(onPayment && { onPayment })}
            {...(onCancel && { onCancel })}
            isUpcoming={isUpcoming}
          />
        ))}
      </div>
    )}
  </div>
);
