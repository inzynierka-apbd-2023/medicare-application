import React from "react";
import {
  Calendar,
  CreditCard,
  FileText,
  MapPin,
  MessageSquare,
  Pill,
  Settings,
  Stethoscope,
} from "lucide-react";

import { Card } from "../../../../shared/components";

interface QuickAction {
  id: string;
  title: string;
  description: string;
  icon: React.ReactNode;
  action: () => void;
  color: string;
  urgent?: boolean;
}

interface QuickActionsCardProps {
  onBookAppointment?: () => void;
  onViewMessages?: () => void;
  onViewDocuments?: () => void;
  onViewMedications?: () => void;
  onViewBilling?: () => void;
  onManageProfile?: () => void;
}

export default function QuickActionsCard({
  onBookAppointment,
  onViewMessages,
  onViewDocuments,
  onViewMedications,
  onViewBilling,
  onManageProfile,
}: QuickActionsCardProps) {
  const quickActions: QuickAction[] = [
    {
      id: "book-appointment",
      title: "Book Appointment",
      description: "Schedule with a doctor",
      icon: <Calendar className="w-5 h-5" />,
      action: onBookAppointment || (() => {}),
      color: "bg-blue-50 hover:bg-blue-100 text-blue-600 border-blue-200",
    },
    {
      id: "messages",
      title: "Messages",
      description: "Chat with your doctor",
      icon: <MessageSquare className="w-5 h-5" />,
      action: onViewMessages || (() => {}),
      color:
        "bg-purple-50 hover:bg-purple-100 text-purple-600 border-purple-200",
    },
    {
      id: "prescriptions",
      title: "Prescriptions",
      description: "View medications",
      icon: <Pill className="w-5 h-5" />,
      action: onViewMedications || (() => {}),
      color:
        "bg-orange-50 hover:bg-orange-100 text-orange-600 border-orange-200",
    },
    {
      id: "documents",
      title: "Medical Records",
      description: "Access your files",
      icon: <FileText className="w-5 h-5" />,
      action: onViewDocuments || (() => {}),
      color:
        "bg-indigo-50 hover:bg-indigo-100 text-indigo-600 border-indigo-200",
    },
    {
      id: "billing",
      title: "Billing & Subscription",
      description: "Manage payments",
      icon: <CreditCard className="w-5 h-5" />,
      action: onViewBilling || (() => {}),
      color: "bg-teal-50 hover:bg-teal-100 text-teal-600 border-teal-200",
    },
    {
      id: "profile",
      title: "Profile Settings",
      description: "Update your info",
      icon: <Settings className="w-5 h-5" />,
      action: onManageProfile || (() => {}),
      color: "bg-gray-50 hover:bg-gray-100 text-gray-600 border-gray-200",
    },
  ];

  return (
    <Card variant="medical" padding="md">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold text-blue-600 flex items-center gap-2">
          <Stethoscope className="w-5 h-5" />
          Quick Actions
        </h3>
      </div>

      {/* Regular Actions Grid */}
      <div className="grid grid-cols-2 gap-3">
        {quickActions.map((action) => (
          <button
            key={action.id}
            onClick={action.action}
            className={`p-3 rounded-lg border transition-all duration-200 ${action.color} hover:shadow-md group`}
          >
            <div className="flex flex-col items-center text-center gap-2">
              <div className="flex-shrink-0 group-hover:scale-110 transition-transform duration-200">
                {action.icon}
              </div>
              <div>
                <h4 className="font-medium text-sm">{action.title}</h4>
                <p className="text-xs opacity-80 leading-tight">
                  {action.description}
                </p>
              </div>
            </div>
          </button>
        ))}
      </div>

      {/* Additional Info */}
      <div className="mt-4 p-3 bg-blue-50 rounded-lg border border-blue-200">
        <div className="flex items-start gap-2">
          <MapPin className="w-4 h-4 text-blue-600 mt-0.5 flex-shrink-0" />
          <div className="text-sm text-blue-800">
            <p className="font-medium mb-1">Visit Our Clinic</p>
            <p className="text-xs text-blue-600">
              123 Medical Center Drive, Health City
            </p>
            <p className="text-xs text-blue-600">
              Open: Mon-Fri 8AM-6PM, Sat 9AM-2PM
            </p>
          </div>
        </div>
      </div>
    </Card>
  );
}
