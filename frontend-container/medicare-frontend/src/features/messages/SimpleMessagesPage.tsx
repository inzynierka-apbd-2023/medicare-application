import React from "react";
import { useAuth } from "@shared/auth/AuthContext";

import Header from "../../layout/Header";
import { DashboardLayout } from "../dashboard/shared/components";

import { MessagesPage } from "./MessagesPage";

const TestMessagesPage: React.FC = () => {
  const { user } = useAuth();

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
            userType={user.role.toLowerCase() as "patient" | "doctor"}
          />
        </div>
      </DashboardLayout>
    </div>
  );
};

export default TestMessagesPage;
