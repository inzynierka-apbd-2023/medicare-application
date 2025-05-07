import React from "react";
import Header from "../Header";

export default function PatientDashboard() {
  return (
    <div className="min-h-screen w-screen bg-gray-100 overflow-x-hidden">
      <main className="pt-24 px-8 pb-10">
        <h1 className="text-3xl font-bold text-blue-700 mb-8">Welcome, Patient</h1>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {/* Upcoming Appointments */}
          <section className="bg-white rounded-2xl shadow-md p-6">
            <h2 className="text-xl font-semibold text-blue-600 mb-2">Upcoming Appointments</h2>
            <p className="text-sm text-gray-500">Here you’ll see your upcoming visits and options to manage them.</p>
          </section>

          {/* Medical History */}
          <section className="bg-white rounded-2xl shadow-md p-6">
            <h2 className="text-xl font-semibold text-blue-600 mb-2">Medical History</h2>
            <p className="text-sm text-gray-500">List of past visits, prescriptions, and referrals will appear here.</p>
          </section>

          {/* Available Specializations */}
          <section className="bg-white rounded-2xl shadow-md p-6">
            <h2 className="text-xl font-semibold text-blue-600 mb-2">Available Doctors & Specializations</h2>
            <p className="text-sm text-gray-500">Browse and search doctors by specialization.</p>
          </section>

          {/* e-Consultations */}
          <section className="bg-white rounded-2xl shadow-md p-6">
            <h2 className="text-xl font-semibold text-blue-600 mb-2">e-Consultations</h2>
            <p className="text-sm text-gray-500">You can start or review previous e-consultations here.</p>
          </section>

          {/* Account Overview */}
          <section className="bg-white rounded-2xl shadow-md p-6">
            <h2 className="text-xl font-semibold text-blue-600 mb-2">Account Overview</h2>
            <p className="text-sm text-gray-500">Your personal details and preferences.</p>
          </section>

          {/* Notifications */}
          <section className="bg-white rounded-2xl shadow-md p-6">
            <h2 className="text-xl font-semibold text-blue-600 mb-2">Notifications & Reminders</h2>
            <p className="text-sm text-gray-500">Reminders about upcoming visits, unread messages, or follow-ups.</p>
          </section>
        </div>
      </main>
    </div>
  );
}
