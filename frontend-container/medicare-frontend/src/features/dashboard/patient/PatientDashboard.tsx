import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { X } from "lucide-react";
import Header from "../../../layout/Header";
import GoogleCalendarScheduler from "../../../pages/Scheduler/GoogleCalendar";
import {
  DashboardLayout,
  DashboardCard,
  ScheduleCard,
  NotificationsList,
  DocumentsList,
  type Notification,
  type Document,
} from "../shared/components";
import { Button, Modal, Card } from "../../../shared/components";

export default function PatientDashboard() {
  const navigate = useNavigate();
  const [showNotifications, setShowNotifications] = useState(false);

  // Sample data - in real app this would come from API/props
  const notifications: Notification[] = [
    {
      id: "1",
      message:
        "Appointment Reminder: May 14, 2025 at 10:00 AM with Dr. Alice Heart",
    },
    {
      id: "2",
      message: "Lab Results Available: Cholesterol Panel",
    },
    {
      id: "3",
      message: "New Message: Follow-up from Dr. Bob Vessel",
    },
    {
      id: "4",
      message: "Your appointment with Dr. Alice Heart is tomorrow at 10:00 AM.",
    },
    {
      id: "5",
      message: "Lab results from your blood test are available.",
    },
    {
      id: "6",
      message: "Reminder: Teleconsultation on May 20, 2025 at 3:00 PM.",
    },
    {
      id: "7",
      message: "Prescription #456 has been renewed.",
    },
    {
      id: "8",
      message: "New message from Dr. Bob Vessel regarding your test.",
    },
  ];

  const documents: Document[] = [
    {
      id: "1",
      title: "Prescription #456 issued",
      date: "May 10, 2025",
    },
    {
      id: "2",
      title: "Referral to Cardiologist",
      date: "April 22, 2025",
    },
    {
      id: "3",
      title: "Blood Test Results",
      date: "March 15, 2025",
    },
  ];

  const handleViewAllNotifications = () => {
    setShowNotifications(true);
  };

  const handleCloseNotifications = () => {
    setShowNotifications(false);
  };

  const handleBookAppointment = () => {
    navigate("/scheduler");
  };

  const handleViewDocuments = () => {
    navigate("/documents");
  };

  return (
    <div className="min-h-screen bg-gray-100 overflow-x-hidden">
      <Header />
      <DashboardLayout title="Welcome, Patient">
        <div className="flex flex-col md:flex-row md:space-x-6 space-y-6 md:space-y-0">
          {/* Left Column - Schedule and Quick Actions */}
          <div className="w-full md:w-3/4 space-y-6">
            <ScheduleCard title="Your Schedule">
              <GoogleCalendarScheduler />
            </ScheduleCard>

            <Card variant="medical" padding="md">
              <h3 className="text-lg font-semibold text-blue-600 mb-2">
                Quick Actions
              </h3>
              <button
                onClick={handleBookAppointment}
                className="px-4 py-2 bg-blue-100 text-blue-700 rounded-lg hover:bg-blue-200 transition duration-150"
              >
                Book New Appointment
              </button>
            </Card>
          </div>

          {/* Right Column - Notifications and Documents */}
          <div className="w-full md:w-1/4 flex flex-col items-center space-y-6">
            <DashboardCard
              title="Notifications"
              action={{
                label: "View All Notifications",
                onClick: handleViewAllNotifications,
                variant: "outline",
              }}
            >
              <NotificationsList notifications={notifications} maxVisible={3} />
            </DashboardCard>

            <DashboardCard
              title="Recent Documents"
              titleClassName="mb-2"
              action={{
                label: "View All Documents",
                onClick: handleViewDocuments,
                variant: "outline",
              }}
            >
              <DocumentsList documents={documents} maxVisible={3} />
            </DashboardCard>
          </div>
        </div>
      </DashboardLayout>

      {/* Notifications Modal */}
      <Modal
        isOpen={showNotifications}
        onClose={handleCloseNotifications}
        title="All Notifications"
        size="lg"
      >
        <div className="max-h-80 overflow-y-auto">
          <NotificationsList
            notifications={notifications}
            maxVisible={notifications.length}
            className="space-y-3"
          />
        </div>
      </Modal>
    </div>
  );
}
