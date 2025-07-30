import React from "react";

import { AppointmentList } from "./components/AppointmentList";
import type { AppointmentListProps } from "./types";

export const Appointments: React.FC<AppointmentListProps> = ({
  appointments,
  onDetails,
  onPayment,
  onCancel,
}) => {
  return (
    <AppointmentList
      appointments={appointments}
      onDetails={onDetails}
      {...(onPayment && { onPayment })}
      {...(onCancel && { onCancel })}
    />
  );
};
