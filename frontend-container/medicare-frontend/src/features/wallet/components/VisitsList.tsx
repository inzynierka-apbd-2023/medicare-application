import React from "react";
import { AppointmentCard } from "./VisitCard";
import type { WalletAppointment } from "../types";

interface AppointmentsListProps {
  appointments: WalletAppointment[];
  onPayAppointment: (appointmentId: string) => void;
  payingAppointmentId?: string | null;
}

export const AppointmentsList: React.FC<AppointmentsListProps> = ({
  appointments,
  onPayAppointment,
  payingAppointmentId,
}) => {
  return (
    <div>
      <div className="mb-2 flex items-center gap-2">
        <span className="font-semibold text-gray-800">Unpaid Appointments</span>
        <span className="bg-gray-200 text-gray-700 rounded-full px-2 py-0.5 text-xs font-medium">
          {appointments.length}
        </span>
      </div>
      {appointments.length === 0 ? (
        <div className="text-gray-500 text-sm">All appointments are paid!</div>
      ) : (
        <div className="flex flex-col gap-4">
          {appointments.map((appointment) => (
            <AppointmentCard
              key={appointment.id}
              appointment={appointment}
              onPay={onPayAppointment}
              isPaying={payingAppointmentId === appointment.id}
            />
          ))}
        </div>
      )}
    </div>
  );
};
