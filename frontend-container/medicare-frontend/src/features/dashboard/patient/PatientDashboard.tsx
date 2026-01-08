import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import Header from "../../../layout/Header";
import { useAuth } from "../../../shared/auth/AuthContext";
import {
  ErrorDisplay,
  LoadingOverlay,
  Modal,
} from "../../../shared/components";
import { useLoadingService } from "../../../shared/hooks/useLoadingService";
import { useSubscription } from "../../../shared/hooks/useSubscription";
import { patientDashboardApi } from "../../../shared/services/dashboardApi";
import { notificationsApi } from "../../../shared/services/notificationsApi";
import { useScheduler } from "../../scheduler/hooks/useScheduler";
import {
  DashboardCard,
  DashboardLayout,
  type Document,
  DocumentsList,
  type Notification,
  NotificationsList,
} from "../shared/components";

import {
  DashboardScheduleView,
  QuickActionsCard,
  UpcomingAppointmentsCard,
} from "./components";

export default function PatientDashboard() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [showNotifications, setShowNotifications] = useState(false);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [documents, setDocuments] = useState<Document[]>([]);
  const subscription = useSubscription();

  const { isLoading, error, clearError, executeInitialLoad } =
    useLoadingService();

  // Use the logged-in user's id as the patient id for scheduler calls
  const currentPatientId = user?.id ?? "";

  // Initialize scheduler for read-only dashboard view
  const { appointments } = useScheduler({
    patientId: currentPatientId,
  });

  // Convert scheduler appointments to dashboard format for compatibility
  const dashboardAppointments = appointments.map((apt) => {
    const cardType: "in-person" | "phone" =
      apt.appointmentType === "in-person" ? "in-person" : "phone";

    // Derive start/end to detect overdue
    const startTime = apt.timeSlot?.startDateTime
      ? new Date(apt.timeSlot.startDateTime)
      : new Date(apt.day);
    const endTime = apt.timeSlot?.endDateTime
      ? new Date(apt.timeSlot.endDateTime as unknown as string)
      : new Date(startTime.getTime() + (apt.durationMinutes || 30) * 60000);
    const isOverdue = endTime.getTime() < Date.now();

    let cardStatus: "upcoming" | "completed" | "cancelled" = "upcoming";
    if (apt.status?.name === "Cancelled") cardStatus = "cancelled";
    else if (apt.status?.name === "Completed" || isOverdue)
      cardStatus = "completed";

    return {
      id: apt.id,
      doctorName: apt.doctor
        ? `${apt.doctor.firstName} ${apt.doctor.lastName}`
        : "Unknown Doctor",
      specialty: apt.doctor?.specializations?.[0]?.name || "General",
      date: startTime,
      time: startTime.toLocaleTimeString([], {
        hour: "2-digit",
        minute: "2-digit",
      }),
      type: cardType,
      status: cardStatus,
    };
  });

  // Fetch dashboard data on component mount
  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        // Fetch notifications and documents in parallel
        const [notificationsResponse, documentsResponse] = await Promise.all([
          notificationsApi.getForRecipient(currentPatientId, false),
          patientDashboardApi.getRecentDocuments(currentPatientId),
        ]);

        if (notificationsResponse.success) {
          setNotifications(notificationsResponse.data as Notification[]);
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
  }, [executeInitialLoad, currentPatientId]);

  const handleViewAllNotifications = () => {
    setShowNotifications(true);
  };

  const handleCloseNotifications = () => {
    setShowNotifications(false);
  };

  const handleBookAppointment = () => {
    navigate("/appointment-scheduler");
  };

  const handleViewDocuments = () => {
    navigate("/my-documents?filter=medical-records");
  };

  const handleViewMessages = () => {
    navigate("/messages");
  };

  const handleViewMedications = () => {
    navigate("/my-documents?filter=prescriptions");
  };

  const handleViewBilling = () => {
    navigate("/user/wallet");
  };

  const handleManageProfile = () => {
    navigate("/user/myprofile");
  };

  const handleMarkNotificationAsRead = async (notificationId: string) => {
    try {
      const response = await notificationsApi.markAsRead(notificationId);
      if (response.success) {
        setNotifications((prev: Notification[]) => {
          const updated = prev.map((notif: Notification) =>
            notif.id === notificationId ? { ...notif, read: true } : notif
          );
          return updated;
        });
        // Best-effort broadcast (Header listens and will refresh unread badge)
        try {
          window.dispatchEvent(
            new CustomEvent("notifications:updated", {
              detail: { kind: "read", id: notificationId },
            })
          );
        } catch {
          // Ignore error
        }
      } else {
        throw new Error("Failed to mark notification as read");
      }
    } catch (err) {
      console.error("Failed to mark notification as read:", err);
    }
  };

  return (
    <div className="min-h-screen bg-gray-100 overflow-x-hidden">
      <Header />
      <LoadingOverlay isLoading={isLoading} message="Loading dashboard...">
        <DashboardLayout
          title={user?.firstName ? `Welcome, ${user.firstName}` : "Welcome"}
        >
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
                <DashboardScheduleView appointments={appointments} />

                {/* Centered Appointments and Quick Actions */}
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                  <UpcomingAppointmentsCard
                    appointments={dashboardAppointments}
                    onBookNew={handleBookAppointment}
                    onViewAll={() => navigate("/my-appointments")}
                  />

                  <QuickActionsCard
                    onBookAppointment={handleBookAppointment}
                    onViewMessages={handleViewMessages}
                    onViewDocuments={handleViewDocuments}
                    onViewMedications={handleViewMedications}
                    onViewBilling={handleViewBilling}
                    onManageProfile={handleManageProfile}
                    features={subscription.features}
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
