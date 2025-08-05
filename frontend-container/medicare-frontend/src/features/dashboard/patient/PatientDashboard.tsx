import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import Header from "../../../layout/Header";
import {
  ErrorDisplay,
  LoadingOverlay,
  Modal,
} from "../../../shared/components";
import { useLoadingService } from "../../../shared/hooks/useLoadingService";
import { patientDashboardApi } from "../../../shared/services/dashboardApi";
import {
  DashboardCard,
  DashboardLayout,
  type Document,
  DocumentsList,
  type Notification,
  NotificationsList,
  ScheduleCard,
} from "../shared/components";

import { QuickActionsCard, UpcomingAppointmentsCard } from "./components";

export default function PatientDashboard() {
  const navigate = useNavigate();
  const [showNotifications, setShowNotifications] = useState(false);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [documents, setDocuments] = useState<Document[]>([]);

  const { isLoading, error, clearError, executeInitialLoad, executeQuietly } =
    useLoadingService();

  // Mock data for appointments
  const mockAppointments = [
    {
      id: "1",
      doctorName: "Smith",
      specialty: "Cardiology",
      date: new Date(Date.now() + 24 * 60 * 60 * 1000),
      time: "10:00 AM",
      type: "in-person" as const,
      location: "Room 205, Main Building",
      status: "upcoming" as const,
    },
    {
      id: "2",
      doctorName: "Johnson",
      specialty: "General Medicine",
      date: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000),
      time: "2:30 PM",
      type: "phone" as const,
      status: "upcoming" as const,
    },
  ];

  // Fetch dashboard data on component mount
  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        // Fetch notifications and documents in parallel
        const [notificationsResponse, documentsResponse] = await Promise.all([
          patientDashboardApi.getNotifications(),
          patientDashboardApi.getDocuments(),
        ]);

        if (notificationsResponse.success) {
          setNotifications(notificationsResponse.data);
        } else {
          throw new Error(
            notificationsResponse.error || "Failed to fetch notifications"
          );
        }

        if (documentsResponse.success) {
          setDocuments(documentsResponse.data);
        } else {
          throw new Error(
            documentsResponse.error || "Failed to fetch documents"
          );
        }
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to load dashboard data";
        throw new Error(errorMessage);
      }
    };

    executeInitialLoad(fetchDashboardData);
  }, [executeInitialLoad]);

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
    navigate("/documents?filter=medical-records");
  };

  const handleViewMessages = () => {
    navigate("/messages");
  };

  const handleViewMedications = () => {
    navigate("/documents?filter=prescriptions");
  };

  const handleViewBilling = () => {
    navigate("/user/wallet");
  };

  const handleManageProfile = () => {
    navigate("/profile");
  };

  const handleMarkNotificationAsRead = async (notificationId: string) => {
    try {
      // Use executeQuietly to avoid showing loading state for this quick operation
      await executeQuietly(async () => {
        const response =
          await patientDashboardApi.markNotificationAsRead(notificationId);
        if (response.success) {
          setNotifications((prev) =>
            prev.map((notif) =>
              notif.id === notificationId ? { ...notif, read: true } : notif
            )
          );
        } else {
          throw new Error("Failed to mark notification as read");
        }
      });
    } catch (err) {
      console.error("Failed to mark notification as read:", err);
    }
  };

  return (
    <div className="min-h-screen bg-gray-100 overflow-x-hidden">
      <Header />
      <LoadingOverlay isLoading={isLoading} message="Loading dashboard...">
        <DashboardLayout title="Welcome, Patient">
          {error ? (
            <div className="flex items-center justify-center h-64">
              <ErrorDisplay
                message={error}
                onRetry={clearError}
                className="max-w-md"
              />
            </div>
          ) : (
            <div className="flex flex-col md:flex-row md:space-x-6 space-y-6 md:space-y-0">
              {/* Left Column - Schedule, Appointments and Quick Actions */}
              <div className="w-full md:w-3/4 space-y-6">
                <ScheduleCard title="Your Schedule">
                  <div className="p-4 text-center text-gray-500">
                    Calendar functionality will be implemented soon
                  </div>
                </ScheduleCard>

                {/* Centered Appointments and Quick Actions */}
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                  <UpcomingAppointmentsCard
                    appointments={mockAppointments}
                    onBookNew={handleBookAppointment}
                    onViewAll={() => navigate("/appointments")}
                  />

                  <QuickActionsCard
                    onBookAppointment={handleBookAppointment}
                    onViewMessages={handleViewMessages}
                    onViewDocuments={handleViewDocuments}
                    onViewMedications={handleViewMedications}
                    onViewBilling={handleViewBilling}
                    onManageProfile={handleManageProfile}
                  />
                </div>
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
                  <NotificationsList
                    notifications={notifications}
                    maxVisible={3}
                    onNotificationClick={handleMarkNotificationAsRead}
                  />
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
          )}
        </DashboardLayout>
      </LoadingOverlay>

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
            onNotificationClick={handleMarkNotificationAsRead}
          />
        </div>
      </Modal>
    </div>
  );
}
