import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { MessageCircle } from "lucide-react";

import Header from "../../../layout/Header";
import { Card, Modal } from "../../../shared/components";
import {
  DashboardCard,
  DashboardLayout,
  type Notification,
  NotificationsList,
  ScheduleCard,
} from "../shared/components";

interface QuickStat {
  label: string;
  value: number;
}

interface PatientMessage {
  id: number;
  patient: string;
  text: string;
}

export default function DoctorDashboard() {
  const navigate = useNavigate();
  const [showNotifications, setShowNotifications] = useState(false);

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

  const quickStats: QuickStat[] = [
    { label: "Patients Today", value: 7 },
    { label: "Total Patients", value: 234 },
    { label: "Visits this Month", value: 49 },
    { label: "Unread Messages", value: 3 },
  ];

  const handleViewAllNotifications = () => {
    setShowNotifications(true);
  };

  const handleCloseNotifications = () => {
    setShowNotifications(false);
  };

  const handlePatientList = () => {
    navigate("/patientlist");
  };

  const handleTodaysAppointments = () => {
    navigate("/doctor-schedule");
  };

  const handleEnhancedSchedule = () => {
    navigate("/enhanced-doctor-schedule");
  };

  const handlePatientManagement = () => {
    navigate("/patient-management");
  };

  const handleMedicalRecords = () => {
    navigate("/medical-records");
  };

  const handleMessagePatient = (patientId: number) => {
    navigate(`/messages?patientId=${patientId}`);
  };

  return (
    <div className="min-h-screen bg-gray-100 overflow-x-hidden">
      <Header />
      <DashboardLayout title="Welcome, Dr. Heart">
        <div className="flex flex-col md:flex-row md:space-x-6 space-y-6 md:space-y-0">
          {/* Left Column - Schedule and Recent Messages */}
          <div className="w-full md:w-3/4 space-y-6">
            <ScheduleCard title="Your Schedule">
              <div className="space-y-4">
                <div className="flex items-center justify-between p-4 bg-blue-50 rounded-lg">
                  <div>
                    <p className="text-sm text-blue-600 font-medium">
                      Today's Appointments
                    </p>
                    <p className="text-2xl font-bold text-blue-700">
                      6 appointments
                    </p>
                    <p className="text-xs text-blue-500">
                      3 remaining, 2 completed, 1 no-show
                    </p>
                  </div>
                  <button
                    onClick={handleTodaysAppointments}
                    className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition text-sm font-medium"
                  >
                    View Schedule
                  </button>
                </div>
                <div className="p-4 text-center">
                  <button
                    onClick={handleTodaysAppointments}
                    className="text-blue-600 hover:text-blue-800 underline font-medium"
                  >
                    Open Full Schedule Timeline →
                  </button>
                </div>
              </div>
            </ScheduleCard>

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
                    My Schedule & Timeline
                  </button>
                </li>
                <li>
                  <button
                    onClick={handleEnhancedSchedule}
                    className="w-full text-left px-4 py-2 bg-purple-50 rounded-lg hover:bg-purple-100 text-purple-700 font-medium transition"
                  >
                    Enhanced Schedule & Patient Records
                  </button>
                </li>
                <li>
                  <button
                    onClick={handlePatientManagement}
                    className="w-full text-left px-4 py-2 bg-blue-50 rounded-lg hover:bg-blue-100 text-blue-700 font-medium transition"
                  >
                    Patient Management
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
