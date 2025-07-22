import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { MessageCircle } from "lucide-react";
import Header from "../Header";
import GoogleCalendarScheduler from "../Scheduler/GoogleCalendar";
import { X } from "lucide-react";

export default function DoctorDashboard() {
  const navigate = useNavigate();

  const [showNotifications, setShowNotifications] = useState(false);
  const [closing, setClosing] = useState(false);

  const notifications = [
    "Appointment with John Doe at 10:30 AM today.",
    "Lab result for Maria Smith is now available.",
    "Patient Adam Nowak sent a new message.",
    "Follow-up reminder: 2 patients need summary reports.",
  ];

  const recentMessages = [
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
    { id: 1, patient: "John Doe", text: "Thank you for the prescription." },
  ];

  const quickStats = [
    { label: "Patients Today", value: 7 },
    { label: "Total Patients", value: 234 },
    { label: "Visits this Month", value: 49 },
    { label: "Unread Messages", value: 3 },
  ];

  const openModal = () => {
    setShowNotifications(true);
    setClosing(false);
  };

  const closeModal = () => {
    setClosing(true);
    setTimeout(() => {
      setShowNotifications(false);
      setClosing(false);
    }, 150);
  };

  return (
    <div className="min-h-screen bg-gray-100 overflow-x-hidden">
      <Header />
      <main className="pt-24 px-8 pb-10">
        <h1 className="text-3xl font-bold text-blue-700 mb-8">
          Welcome, Dr. Heart
        </h1>
        <div className="flex flex-col md:flex-row md:space-x-6 space-y-6 md:space-y-0">
          {/* Left Column */}
          <div className="w-full md:w-3/4 space-y-6">
            {/* Scheduler */}
            <div className="bg-white rounded-2xl shadow-md p-6 h-[600px] flex flex-col">
              <h2 className="text-xl font-semibold text-blue-600 mb-4">
                Your Schedule
              </h2>
              <div className="flex-1 bg-blue-50 rounded-lg p-4 h-full">
                <GoogleCalendarScheduler />
              </div>
            </div>
            {/* Recent Messages */}
            <div className="bg-white rounded-2xl shadow-md p-6">
              <h3 className="text-lg font-semibold text-blue-600 mb-2">
                Recent Messages from Patients
              </h3>
              <ul className="space-y-2">
                {recentMessages.map((msg, idx) => (
                  <li
                    key={idx}
                    className="flex items-center text-sm text-gray-700"
                  >
                    <span className="font-medium">{msg.patient}: </span>
                    <span className="ml-1">{msg.text}</span>
                    <button
                      title={`Message ${msg.patient}`}
                      className="ml-3 p-1 rounded-lg bg-blue-100 hover:bg-blue-200 text-blue-700 transition"
                      onClick={() => navigate(`/messages?patientId=${msg.id}`)}
                    >
                      <MessageCircle size={16} />
                    </button>
                  </li>
                ))}
              </ul>
            </div>
          </div>
          {/* Right Column */}
          <div className="w-full md:w-1/4 flex flex-col items-center space-y-6">
            {/* Quick Stats */}
            <div className="bg-white rounded-2xl shadow-md p-6 w-full flex flex-col items-center">
              <h2 className="text-xl font-semibold text-blue-600 mb-4 text-center">
                Quick Stats
              </h2>
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
            </div>
            {/* Notifications */}
            <div className="bg-white rounded-2xl shadow-md p-6 w-full flex flex-col items-center">
              <h2 className="text-xl font-semibold text-blue-600 mb-4 text-center">
                Notifications
              </h2>
              <ul className="space-y-2 list-disc list-inside text-left w-full">
                <li className="text-sm text-gray-600">{notifications[0]}</li>
                <li className="text-sm text-gray-600">{notifications[1]}</li>
                <li className="text-sm text-gray-600">{notifications[2]}</li>
              </ul>
              <button
                onClick={openModal}
                className="mt-4 w-full px-4 py-2 bg-blue-100 text-blue-700 rounded-lg hover:bg-blue-200 transition duration-150"
              >
                View All Notifications
              </button>
            </div>
            {/* Quick Links */}
            <div className="bg-white rounded-2xl shadow-md p-6 w-full flex flex-col items-center">
              <h2 className="text-xl font-semibold text-blue-600 mb-2 text-center">
                Quick Access
              </h2>
              <ul className="space-y-2 w-full">
                <li>
                  <button className="w-full text-left px-4 py-2 bg-blue-50 rounded-lg hover:bg-blue-100 text-blue-700 font-medium transition">
                    Patient List
                  </button>
                </li>
                <li>
                  <button className="w-full text-left px-4 py-2 bg-blue-50 rounded-lg hover:bg-blue-100 text-blue-700 font-medium transition">
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
            </div>
          </div>
        </div>
      </main>
      {/* Notifications Modal Overlay */}
      {showNotifications && (
        <>
          <div
            className={`fixed inset-0 bg-black bg-opacity-50 z-50 transition-opacity duration-150 ease-out ${
              closing ? "opacity-0" : "opacity-100"
            }`}
            onClick={closeModal}
          />
          <div className="fixed inset-0 flex items-center justify-center z-50">
            <div
              className={`${
                closing ? "animate-scale-out" : "animate-scale-in"
              } bg-white rounded-2xl shadow-lg p-6 relative w-full md:w-3/4 lg:w-2/3 xl:w-1/2`}
            >
              <button
                className="absolute top-4 right-4 text-blue-300 hover:text-blue-400 transition duration-150"
                onClick={closeModal}
              >
                <X size={16} />
              </button>
              <h2 className="text-3xl font-semibold text-blue-600 mb-4">
                All Notifications
              </h2>
              <ul className="space-y-3 max-h-80 overflow-y-auto list-disc list-inside text-left">
                {notifications.map((note, idx) => (
                  <li key={idx} className="text-base text-gray-700">
                    {note}
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
