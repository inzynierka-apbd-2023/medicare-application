import { useState } from "react";
import { useNavigate } from "react-router-dom";
import Header from "../../layout/Header";
import GoogleCalendarScheduler from "../Scheduler/GoogleCalendar";
import { X } from "lucide-react";

export default function PatientDashboard() {
  const navigate = useNavigate();

  const [showNotifications, setShowNotifications] = useState(false);
  const [closing, setClosing] = useState(false);

  const notifications = [
    "Your appointment with Dr. Alice Heart is tomorrow at 10:00 AM.",
    "Lab results from your blood test are available.",
    "Reminder: Teleconsultation on May 20, 2025 at 3:00 PM.",
    "Prescription #456 has been renewed.",
    "New message from Dr. Bob Vessel regarding your test.",
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
          Welcome, Patient
        </h1>
        <div className="flex flex-col md:flex-row md:space-x-6 space-y-6 md:space-y-0">
          {/* Left Column */}
          <div className="w-full md:w-3/4 space-y-6">
            <div className="bg-white rounded-2xl shadow-md p-6 h-[600px] flex flex-col">
              <h2 className="text-xl font-semibold text-blue-600 mb-4">
                Your Schedule
              </h2>
              <div className="flex-1 bg-blue-50 rounded-lg p-4 h-full">
                <GoogleCalendarScheduler />
              </div>
            </div>
            <div className="bg-white rounded-2xl shadow-md p-6">
              <h3 className="text-lg font-semibold text-blue-600 mb-2">
                Quick Actions
              </h3>
              <button
                onClick={() => navigate("/scheduler")}
                className="px-4 py-2 bg-blue-100 text-blue-700 rounded-lg hover:bg-blue-200 transition duration-150"
              >
                Book New Appointment
              </button>
            </div>
          </div>
          {/* Right Column */}
          <div className="w-full md:w-1/4 flex flex-col items-center space-y-6">
            {/* Notifications Section */}
            <div className="bg-white rounded-2xl shadow-md p-6 w-full flex flex-col items-center">
              <h2 className="text-xl font-semibold text-blue-600 mb-4 text-center">
                Notifications
              </h2>
              <ul className="space-y-2 list-disc list-inside text-left w-full">
                <li className="text-sm text-gray-600">
                  Appointment Reminder: May 14, 2025 at 10:00 AM with Dr. Alice
                  Heart
                </li>
                <li className="text-sm text-gray-600">
                  Lab Results Available: Cholesterol Panel
                </li>
                <li className="text-sm text-gray-600">
                  New Message: Follow-up from Dr. Bob Vessel
                </li>
              </ul>
              <button
                onClick={openModal}
                className="mt-4 w-full px-4 py-2 bg-blue-100 text-blue-700 rounded-lg hover:bg-blue-200 transition duration-150"
              >
                View All Notifications
              </button>
            </div>
            {/* Recent Documents Section */}
            <div className="bg-white rounded-2xl shadow-md p-6 w-full flex flex-col items-center">
              <h2 className="text-xl font-semibold text-blue-600 mb-2 text-center">
                Recent Documents
              </h2>
              <ul className="list-disc list-inside text-left space-y-2 text-sm text-gray-700 w-full">
                <li>Prescription #456 issued on May 10, 2025</li>
                <li>Referral to Cardiologist on April 22, 2025</li>
                <li>Blood Test Results on March 15, 2025</li>
              </ul>
              <button
                onClick={() => navigate("/documents")}
                className="mt-4 w-full px-4 py-2 bg-blue-100 text-blue-700 rounded-lg hover:bg-blue-200 transition duration-150"
              >
                View All Documents
              </button>
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
