import React, { useState } from "react";
import Header from "../Header";
import { X } from "lucide-react";

export default function PatientDashboard() {
  const [showNotifications, setShowNotifications] = useState(false);
  const [closing, setClosing] = useState(false);

  const notifications = [
    "Notification 1",
    "Notification 2",
    "Notification 3",
    "Notification 4",
    "Notification 5",
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
    <div className="min-h-screen w-screen bg-gray-100 overflow-x-hidden">
      <Header />
      <main className="pt-24 px-8 pb-10">
        <h1 className="text-3xl font-bold text-blue-700 mb-8">Welcome, Patient</h1>
        <div className="flex space-x-6">
          <div className="w-3/4 space-y-6">
            <div className="bg-white rounded-2xl shadow-md p-6 h-[600px] flex flex-col">
              <h2 className="text-xl font-semibold text-blue-600 mb-4">Your Schedule</h2>
              <div className="flex-1 bg-blue-50 rounded-lg flex items-center justify-center text-blue-300">
                Scheduler Placeholder
              </div>
            </div>
            <div className="bg-white rounded-2xl shadow-md p-6">
              <h3 className="text-lg font-semibold text-blue-600 mb-2">Quick Actions</h3>
              <button className="px-4 py-2 bg-blue-100 text-blue-700 rounded-lg hover:bg-blue-200 transition duration-150">
                Book New Appointment
              </button>
            </div>
          </div>
          <div className="w-1/4 space-y-6">
            <div className="bg-white rounded-2xl shadow-md p-6">
              <h2 className="text-xl font-semibold text-blue-600 mb-4">Notifications</h2>
              <ul className="space-y-2 list-disc list-inside text-left">
                <li className="text-sm text-gray-600">Notification 1</li>
                <li className="text-sm text-gray-600">Notification 2</li>
                <li className="text-sm text-gray-600">Notification 3</li>
              </ul>
              <button
                onClick={openModal}
                className="mt-4 w-full px-4 py-2 bg-blue-100 text-blue-700 rounded-lg hover:bg-blue-200 transition duration-150"
              >
                View All Notifications
              </button>
            </div>
            <div className="bg-white rounded-2xl shadow-md p-6">
              <h2 className="text-xl font-semibold text-blue-600 mb-2">Recent Documents</h2>
              <p className="text-sm text-gray-500">You have no new documents.</p>
            </div>
            <div className="bg-white rounded-2xl shadow-md p-6">
              <h2 className="text-xl font-semibold text-blue-600 mb-2">Health Tips</h2>
              <p className="text-sm text-gray-500">Stay hydrated and take regular breaks.</p>
            </div>
          </div>
        </div>
      </main>

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
              <h2 className="text-3xl font-semibold text-blue-600 mb-4">All Notifications</h2>
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
