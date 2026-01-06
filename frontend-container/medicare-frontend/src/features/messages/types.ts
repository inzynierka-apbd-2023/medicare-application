export interface User {
  id: string;
  name: string;
  role: "patient" | "doctor";
  email: string;
  avatar?: string;
  specialty?: string; // For doctors
}

export interface Message {
  id: string;
  conversationId: string;
  senderId: string;
  senderName: string;
  senderType: "patient" | "doctor";
  receiverId: string;
  receiverName: string;
  receiverType: "patient" | "doctor";
  content: string;
  timestamp: string;
  isRead: boolean;
  attachments?: MessageAttachment[];
}

export interface MessageAttachment {
  id: string;
  name: string;
  url: string;
  size: number;
  type: string;
}

export interface Conversation {
  id: string;
  participantId: string;
  participantName: string;
  participantType: "patient" | "doctor";
  participants: User[]; // Full participant objects for UI
  lastMessage?: Message;
  unreadCount: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface MessagesPageProps {
  userId?: string;
  userType?: "patient" | "doctor";
  conversationId?: string;
  recipientId?: string; // For starting/opening chat with specific user
}

export interface ConversationListProps {
  conversations: Conversation[];
  selectedConversationId: string | undefined;
  onSelectConversation: (conversationId: string) => void;
  currentUserId: string;
}

export interface MessageListProps {
  messages: Message[];
  currentUserId: string;
  isLoading?: boolean;
  onMarkAsRead?: (messageId: string) => void;
}

export interface MessageInputProps {
  onSendMessage: (content: string) => void;
  isLoading?: boolean;
  placeholder?: string;
}

export interface MessageBubbleProps {
  message: Message;
  isOwnMessage: boolean;
  onMarkAsRead?: (messageId: string) => void;
}

export interface ConversationItemProps {
  conversation: Conversation;
  isSelected: boolean;
  onClick: (conversationId: string) => void;
}

export interface NewMessageModalProps {
  isOpen: boolean;
  onClose: () => void;
  onStartConversation: (
    recipientId: string,
    recipientName: string,
    initialMessage: string
  ) => void;
  availableDoctors: User[];
  isLoading?: boolean;
  preSelectedRecipientId?: string;
}

export interface MessagesState {
  conversations: Conversation[];
  messages: Record<string, Message[]>;
  selectedConversationId?: string;
  isLoading: boolean;
  error: string | null;
}
