import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { MessageCircle } from "lucide-react";

import Header from "../../../layout/Header";
import { useAuth } from "../../../shared/auth/AuthContext";
import { Card, Modal } from "../../../shared/components";
import doctorDashboardApi, {
  DoctorQuickStat,
} from "../../../shared/services/doctorDashboardApi";
import { messagesApi } from "../../../shared/services/messagesApi";
import { notificationsApi } from "../../../shared/services/notificationsApi";
import {
  DashboardCard,
  DashboardLayout,
  type Notification,
  NotificationsList,
} from "../shared/components";

import { DashboardScheduler } from "./components";

interface PatientMessage {
  id: string;
  patient: string;
  text: string;
}

export default function DoctorDashboard() {
  const navigate = useNavigate();
  const [showNotifications, setShowNotifications] = useState(false);
  const [quickStats, setQuickStats] = useState<DoctorQuickStat[]>([]);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [recentMessages, setRecentMessages] = useState<PatientMessage[]>([]);
  const [loading, setLoading] = useState(true);
  const [messagesLoading, setMessagesLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { user } = useAuth();
  const doctorLastName =
    (user?.lastName || "").trim() || user?.username || "Doctor";

  const [doctorId, setDoctorId] = useState<string>();

  useEffect(() => {
    const initDashboard = async () => {
      if (!user?.id) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        // 1. Resolve Doctor ID from User ID
        let doctorProfile;
        try {
          doctorProfile = await doctorDashboardApi.getDoctorByUserId(user.id);
        } catch (error: unknown) {
          const axiosErr = error as { response?: { status?: number } };
          if (axiosErr?.response?.status !== 404) {
            throw error;
          }
          // 404 means profile doesn't exist, proceed to registration logic
        }

        if (doctorProfile?.id) {
          const realDoctorId = doctorProfile.id;
          console.log(
            `[DoctorDashboard] user.id=${user.id}, realDoctorId=${realDoctorId}`
          );
          setDoctorId(realDoctorId);
          // Load stats, notifications, and messages in parallel
          const [statsData, notifData, msgRes] = await Promise.all([
            doctorDashboardApi
              .getQuickStats(realDoctorId)
              .catch(() => [] as DoctorQuickStat[]),
            notificationsApi.getForRecipient(user.id, false).catch((err) => {
              console.error("Failed to load notifications", err);
              return [] as Notification[];
            }),
            messagesApi.getConversations(user.id, "doctor"),
          ]);

          setQuickStats(statsData);
          setNotifications(notifData);

          if (msgRes) {
            const unreadConversations = msgRes
              .filter((conv) => conv.unreadCount > 0)
              .slice(0, 5);

            const messages: PatientMessage[] = unreadConversations.map(
              (conv) => ({
                id: conv.participantId,
                patient: conv.participantName || "Unknown Patient",
                text: conv.lastMessage?.content || "New message",
              })
            );
            setRecentMessages(messages);
          }
          setMessagesLoading(false);
        } else {
          try {
            const newProfile = await doctorDashboardApi.registerDoctor(user.id);
            if (newProfile?.id) {
              const newDoctorId = newProfile.id;
              setDoctorId(newDoctorId);
              const [statsDataResult, notifDataResult, msgRes] =
                await Promise.all([
                  doctorDashboardApi
                    .getQuickStats(newDoctorId)
                    .catch(() => [] as DoctorQuickStat[]),
                  notificationsApi
                    .getForRecipient(user.id, false)
                    .catch(() => [] as Notification[]),
                  messagesApi.getConversations(user.id, "doctor"),
                ]);

              setQuickStats(statsDataResult);
              setNotifications(notifDataResult); // Directly set data, error caught by catch

              if (msgRes) {
                const unreadConversations = msgRes
                  .filter((conv) => conv.unreadCount > 0)
                  .slice(0, 5);
                const messages: PatientMessage[] = unreadConversations.map(
                  (conv) => ({
                    id: conv.participantId,
                    patient: conv.participantName || "Unknown Patient",
                    text: conv.lastMessage?.content || "New message",
                  })
                );
                setRecentMessages(messages);
              }
              setMessagesLoading(false);
            }
          } catch (_regError) {
            setError(
              "Doctor profile not found and auto-creation failed. Please contact support."
            );
          }
        }
      } catch (err) {
        setError("Failed to initialize dashboard");
        console.error("Error initializing dashboard:", err);
      } finally {
        setLoading(false);
      }
    };

    initDashboard();
  }, [user?.id]);

  const handleMarkNotificationAsRead = async (notificationId: string) => {
    try {
      await notificationsApi.markAsRead(notificationId);
      setNotifications((prev) =>
        prev.map((notif) =>
          notif.id === notificationId ? { ...notif, read: true } : notif
        )
      );
    } catch (err) {
      console.error("Failed to mark notification as read:", err);
    }
  };

  const handleViewAllNotifications = () => {
    setShowNotifications(true);
  };

  const handleCloseNotifications = () => {
    setShowNotifications(false);
  };

  const handlePatientList = () => {
    navigate("/patient-list");
  };

  const handleTodaysAppointments = () => {
    navigate("/todays-appointments");
  };

  const handleMedicalRecords = () => {
    navigate("/medical-records");
  };

  const handleFullSchedule = () => {
    navigate("/doctor-scheduler");
  };

  const handleMessagePatient = (patientId: string) => {
    navigate(`/messages?patientId=${patientId}`);
  };

  return (
    <div className="min-h-screen bg-gray-100 overflow-x-hidden">
      <Header />
      <DashboardLayout title={`Welcome, Dr. ${doctorLastName}`}>
        <div className="flex flex-col md:flex-row md:space-x-6 space-y-6 md:space-y-0">
          {/* Left Column - Schedule and Recent Messages */}
          <div className="w-full md:w-3/4 space-y-6">
            {/* Today's Schedule - Embedded Scheduler */}
            {doctorId && <DashboardScheduler doctorId={doctorId} />}

            <Card variant="medical" padding="md">
              <h3 className="text-lg font-semibold text-blue-600 mb-2">
                Unread Messages from Patients
              </h3>
              {messagesLoading ? (
                <div className="w-full flex justify-center items-center py-4">
                  <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600"></div>
                </div>
              ) : recentMessages.length === 0 ? (
                <p className="text-gray-500 text-sm py-4">
                  No unread messages.
                </p>
              ) : (
                <ul className="space-y-2 w-full">
                  {recentMessages.map((msg) => (
                    <li
                      key={msg.id}
                      className="flex items-center text-sm text-gray-700"
                    >
                      <span className="font-medium">{msg.patient}: </span>
                      <span className="ml-1 flex-1">{msg.text}</span>
                      <button
                        title={`Message ${msg.patient}`}
                        className="ml-3 p-1 rounded-lg bg-blue-100 hover:bg-blue-200 text-blue-700 transition"
                        onClick={() => handleMessagePatient(msg.id)}
                      >
                        <MessageCircle size={16} />
                      </button>
                    </li>
                  ))}
                </ul>
              )}
            </Card>
          </div>

          {/* Right Column - Stats, Notifications, and Quick Access */}
          <div className="w-full md:w-1/4 flex flex-col items-center space-y-6">
            <DashboardCard title="Quick Stats">
              {loading ? (
                <div className="w-full flex justify-center items-center py-8">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                </div>
              ) : error ? (
                <div className="w-full text-center py-4 text-red-600">
                  {error}
                </div>
              ) : (
                <ul className="w-full grid grid-cols-2 gap-4">
                  {quickStats.map((stat, idx) => (
                    <li
                      key={idx}
                      className="bg-blue-50 rounded-xl px-2 py-3 text-center"
                    >
                      <span className="block text-2xl font-bold text-blue-700">
                        {stat.value}
                      </span>
                      <span className="block text-xs text-gray-600">
                        {stat.label}
                      </span>
                    </li>
                  ))}
                </ul>
              )}
            </DashboardCard>

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
              title="Quick Access"
              action={{
                label: "Patient List",
                onClick: handlePatientList,
                variant: "primary",
              }}
            >
              <ul className="space-y-2 w-full">
                <li>
                  <button
                    onClick={handleTodaysAppointments}
                    className="w-full text-left px-4 py-2 bg-green-50 rounded-lg hover:bg-green-100 text-green-700 font-medium transition"
                  >
                    Today's Appointments
                  </button>
                </li>
                <li>
                  <button
                    onClick={handleFullSchedule}
                    className="w-full text-left px-4 py-2 bg-purple-50 rounded-lg hover:bg-purple-100 text-purple-700 font-medium transition"
                  >
                    Full Schedule Calendar
                  </button>
                </li>
                <li>
                  <button
                    onClick={handleMedicalRecords}
                    className="w-full text-left px-4 py-2 bg-blue-50 rounded-lg hover:bg-blue-100 text-blue-700 font-medium transition"
                  >
                    Medical Records
                  </button>
                </li>
                <li>
                  <button
                    onClick={() => navigate("/prescriptions-management")}
                    className="w-full text-left px-4 py-2 bg-blue-50 rounded-lg hover:bg-blue-100 text-blue-700 font-medium transition"
                  >
                    Prescriptions
                  </button>
                </li>
                <li>
                  <button
                    onClick={() => navigate("/lab-results-review")}
                    className="w-full text-left px-4 py-2 bg-blue-50 rounded-lg hover:bg-blue-100 text-blue-700 font-medium transition"
                  >
                    Lab Results
                  </button>
                </li>
              </ul>
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
            onNotificationClick={handleMarkNotificationAsRead}
          />
        </div>
      </Modal>
    </div>
  );
}
