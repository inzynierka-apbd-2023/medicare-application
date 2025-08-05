import React from "react";
import { MessagesPage } from "@features/messages";

import { DashboardLayout } from "./features/dashboard/shared/components";
import Header from "./layout/Header";

const TestMessagesPage: React.FC = () => {
  return (
    <div className="min-h-screen bg-gray-100 overflow-x-hidden">
      <Header />
      <DashboardLayout title="Messages">
        <div
          className="bg-white rounded-lg shadow-lg"
          style={{ height: "80vh" }}
        >
          <MessagesPage userId="patient_1" userType="patient" />
        </div>
      </DashboardLayout>
    </div>
  );
};

export default TestMessagesPage;
