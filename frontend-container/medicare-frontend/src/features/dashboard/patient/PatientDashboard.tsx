import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
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
import {
  Modal,
  Card,
  LoadingOverlay,
  ErrorDisplay,
} from "../../../shared/components";
import { patientDashboardApi } from "../../../shared/services/dashboardApi";
import { useLoadingService } from "../../../shared/hooks/useLoadingService";

export default function PatientDashboard() {
  const navigate = useNavigate();
  const [showNotifications, setShowNotifications] = useState(false);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [documents, setDocuments] = useState<Document[]>([]);

  const { isLoading, error, clearError, executeInitialLoad, executeQuietly } =
    useLoadingService();

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
    navigate("/documents");
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
