import React from "react";

import { DoctorSchedulerPage } from "../scheduler/DoctorSchedulerPage";
import { SchedulerPage } from "../scheduler/SchedulerPage";

interface ContextualSchedulerProps {
  userType?: "patient" | "doctor" | "owner";
}

export const ContextualScheduler: React.FC<ContextualSchedulerProps> = ({
  userType = "patient",
}) => {
  // In a real app, this would come from auth context
  // For now, we'll use a simple check based on the current route or user type

  if (userType === "doctor") {
    return <DoctorSchedulerPage isReadOnly={true} />;
  }

  // Default to patient scheduler
  return <SchedulerPage />;
};
