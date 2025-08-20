import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { MessageCircle } from "lucide-react";

import Header from "../../../layout/Header";
import { Card, Modal } from "../../../shared/components";
import { useAuth } from "../../../shared/auth/AuthContext";
import doctorDashboardApiService, { DoctorQuickStat } from "../../../shared/services/doctorDashboardApi";
import {
  DashboardCard,
  DashboardLayout,
  type Notification,
  NotificationsList,
} from "../shared/components";

import { DashboardScheduler } from "./components";

interface PatientMessage {
  id: number;
  patient: string;
  text: string;
}

export default function DoctorDashboard() {
  const navigate = useNavigate();
  const [showNotifications, setShowNotifications] = useState(false);
  const [quickStats, setQuickStats] = useState<DoctorQuickStat[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { user } = useAuth();
  const doctorLastName = (user?.lastName || "").trim() || user?.username || "Doctor";

  useEffect(() => {
    const loadQuickStats = async () => {
      if (!user?.id) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        const response = await doctorDashboardApiService.getQuickStats(user.id);
        if (response.success) {
          setQuickStats(response.data);
        } else {
          setError("Failed to load quick stats");
        }
      } catch (err) {
        setError("Failed to load quick stats");
        console.error("Error loading quick stats:", err);
      } finally {
        setLoading(false);
      }
    };

    loadQuickStats();
  }, [user?.id]);

  // Sample data - in real app this would come from API/props
  const notifications: Notification[] = [
    {
      id: "1",
      message: "Appointment with John Doe at 10:30 AM today.",
    },
    {
      id: "2",
      message: "Lab result for Maria Smith is now available.",
    },
    {
      id: "3",
      message: "Patient Adam Nowak sent a new message.",
    },
    {
      id: "4",
      message: "Follow-up reminder: 2 patients need summary reports.",
    },
  ];

  const recentMessages: PatientMessage[] = [
    {
      id: 2,
      patient: "Maria Smith",
      text: "Can I move my appointment to Friday?",
    },
    {
      id: 3,
      patient: "Adam Nowak",
      text: "Uploaded my recent blood test results.",
    },
    {
      id: 1,
      patient: "John Doe",
      text: "Thank you for the prescription.",
    },
  ];

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

  const handleMessagePatient = (patientId: number) => {
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
            <DashboardScheduler />

            <Card variant="medical" padding="md">
              <h3 className="text-lg font-semibold text-blue-600 mb-2">
                Recent Messages from Patients
              </h3>
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
              <NotificationsList notifications={notifications} maxVisible={3} />
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
                  <button className="w-full text-left px-4 py-2 bg-blue-50 rounded-lg hover:bg-blue-100 text-blue-700 font-medium transition">
                    Prescriptions
                  </button>
                </li>
                <li>
                  <button className="w-full text-left px-4 py-2 bg-blue-50 rounded-lg hover:bg-blue-100 text-blue-700 font-medium transition">
                    Reports
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
          />
        </div>
      </Modal>
    </div>
  );
}
