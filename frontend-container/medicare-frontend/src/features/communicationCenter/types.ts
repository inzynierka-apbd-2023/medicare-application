// Communication Center Types
export interface CommunicationMessage {
  id: string;
  type: "email" | "sms" | "in-app" | "phone-call";
  subject?: string;
  content: string;
  senderId: string;
  senderName: string;
  senderRole: "receptionist" | "doctor" | "patient" | "system";
  recipientId: string;
  recipientName: string;
  recipientRole: "receptionist" | "doctor" | "patient";
  status: "draft" | "sent" | "delivered" | "read" | "failed";
  priority: "low" | "normal" | "high" | "urgent";
  tags?: string[];
  attachments?: MessageAttachment[];
  relatedAppointmentId?: string;
  relatedPatientId?: string;
  createdAt: string;
  sentAt?: string;
  deliveredAt?: string;
  readAt?: string;
}

export interface MessageAttachment {
  id: string;
  fileName: string;
  fileSize: number;
  fileType: string;
  url: string;
}

export interface MessageTemplate {
  id: string;
  name: string;
  subject?: string;
  content: string;
  type: "email" | "sms" | "in-app";
  category: "appointment-reminder" | "follow-up" | "welcome" | "general";
  variables: string[]; // Available variables like {patientName}, {appointmentDate}
  isActive: boolean;
  createdBy: string;
  createdAt: string;
}

export interface BulkMessage {
  id: string;
  templateId: string;
  template?: MessageTemplate;
  recipientFilters: {
    role?: "patient" | "doctor";
    doctorId?: string;
    appointmentDateFrom?: string;
    appointmentDateTo?: string;
    patientStatus?: string;
  };
  totalRecipients: number;
  sentCount: number;
  deliveredCount: number;
  failedCount: number;
  status: "draft" | "sending" | "completed" | "failed";
  scheduledFor?: string;
  createdBy: string;
  createdAt: string;
  completedAt?: string;
}

export interface CommunicationStats {
  totalMessages: number;
  todayMessages: number;
  pendingMessages: number;
  failedMessages: number;
  deliveryRate: number; // percentage
  responseRate: number; // percentage
}

export interface CommunicationFilters {
  type?: CommunicationMessage["type"];
  status?: CommunicationMessage["status"];
  priority?: CommunicationMessage["priority"];
  senderRole?: CommunicationMessage["senderRole"];
  recipientRole?: CommunicationMessage["recipientRole"];
  dateFrom?: string;
  dateTo?: string;
  searchTerm?: string;
  patientId?: string;
  appointmentId?: string;
}

export interface CommunicationCenterData {
  stats: CommunicationStats;
  recentMessages: CommunicationMessage[];
  templates: MessageTemplate[];
  bulkMessages: BulkMessage[];
}

export interface CommunicationCenterPageProps {
  className?: string;
}
