import React from "react";
import { useSearchParams } from "react-router-dom";
import { DashboardLayout } from "@features/dashboard/shared/components";
import Header from "@layout/Header";
import { useAuth } from "@shared/auth/AuthContext";

import { MessagesPage } from "./MessagesPage";

const SimpleMessagesPage: React.FC = () => {
  const { user } = useAuth();
  const [searchParams] = useSearchParams();
  const recipientId = searchParams.get("recipientId") || undefined;
  const conversationId = searchParams.get("conversationId") || undefined;

  if (!user) return <div>Loading...</div>;

  return (
    <div className="min-h-screen bg-gray-100 overflow-x-hidden">
      <Header />
      <DashboardLayout title="Messages">
        <div
          className="bg-white rounded-lg shadow-lg"
          style={{ height: "80vh" }}
        >
          <MessagesPage
            userId={user.id}
            userType={
              user.role.toLowerCase() as "patient" | "doctor" | "receptionist"
            }
            {...(recipientId ? { recipientId } : {})}
            {...(conversationId ? { conversationId } : {})}
          />
        </div>
      </DashboardLayout>
    </div>
  );
};

export default SimpleMessagesPage;
