import React from "react";
import type {
  PatientAction,
  PatientActionButtonProps,
  PatientActionConfig,
} from "@features/userTypes/types";
import {
  Calendar,
  ClipboardList,
  FileText,
  MessageCircle,
  PlusCircle,
} from "lucide-react";

const actionConfigs: Record<PatientAction, PatientActionConfig> = {
  appointments: {
    icon: <Calendar size={16} />,
    title: "View appointments",
    colorClass: "bg-blue-100 hover:bg-blue-200 text-blue-700",
    route: "/my-appointments",
  },
  "medical-records": {
    icon: <FileText size={16} />,
    title: "View medical records",
    colorClass: "bg-green-100 hover:bg-green-200 text-green-700",
    route: "/medical-records",
  },
  prescription: {
    icon: <PlusCircle size={16} />,
    title: "Write new prescription",
    colorClass: "bg-purple-100 hover:bg-purple-200 text-purple-700",
    route: "/prescriptions/new",
  },
  message: {
    icon: <MessageCircle size={16} />,
    title: "Send message",
    colorClass: "bg-yellow-100 hover:bg-yellow-200 text-yellow-700",
    route: "/messages",
  },
  notes: {
    icon: <ClipboardList size={16} />,
    title: "See/add notes",
    colorClass: "bg-gray-200 hover:bg-gray-300 text-gray-800",
    route: "/notes",
  },
};

export const PatientActionButton: React.FC<PatientActionButtonProps> = ({
  action,
  onClick,
}) => {
  const config = actionConfigs[action];

  return (
    <button
      title={config.title}
      className={`p-2 rounded-lg transition ${config.colorClass}`}
      onClick={onClick}
    >
      {config.icon}
    </button>
  );
};
