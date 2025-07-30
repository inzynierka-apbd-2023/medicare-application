import React from "react";
import { CalendarCheck } from "lucide-react";

import type { AppointmentListProps } from "../types";

import { AppointmentSection } from "./AppointmentSection";

export const AppointmentList: React.FC<AppointmentListProps> = ({
  appointments,
  onDetails,
  onPayment,
  onCancel,
}) => {
  const upcoming = appointments.filter((appt) => appt.status === "upcoming");
  const previous = appointments.filter((appt) => appt.status !== "upcoming");

  return (
    <div className="space-y-8">
      <div className="text-center">
        <h2 className="text-4xl font-bold text-blue-700 mb-2 flex items-center justify-center gap-3">
          <CalendarCheck className="text-blue-600" size={42} />
          Your Appointments
        </h2>
        <p className="text-gray-600">
          Manage your upcoming and past appointments
        </p>
      </div>

      <AppointmentSection
        title="Upcoming Appointments"
        appointments={upcoming}
        onDetails={onDetails}
        {...(onPayment && { onPayment })}
        {...(onCancel && { onCancel })}
        isUpcoming={true}
        emptyMessage="You don't have any upcoming appointments."
      />

      <AppointmentSection
        title="Previous Appointments"
        appointments={previous}
        onDetails={onDetails}
        {...(onPayment && { onPayment })}
        {...(onCancel && { onCancel })}
        isUpcoming={false}
        emptyMessage="No previous appointments."
      />
    </div>
  );
};
