import React from "react";
import Header from "../../layout/Header";
import { SchedulerFeature } from "./SchedulerFeature";

export const SchedulerPage: React.FC = () => {
  const handleAppointmentBook = (booking: any) => {
    console.log("Appointment booked:", booking);
    // Here you can add additional logic like navigation, notifications, etc.
  };

  const handleEventSelect = (event: any) => {
    console.log("Event selected:", event);
    // Handle event selection logic
  };

  return (
    <div className="min-h-screen bg-gray-100 overflow-x-hidden">
      <Header />

      <div className="container mx-auto max-w-[160rem] px-4 sm:px-6 lg:px-8">
        <main className="pt-24 pb-10">
          <h1 className="text-3xl font-bold text-blue-700 mb-8">
            Schedule Appointment
          </h1>

          <div className="flex justify-center">
            <div className="w-full max-w-6xl bg-white rounded-2xl shadow-md p-6">
              <SchedulerFeature
                onAppointmentBook={handleAppointmentBook}
                onEventSelect={handleEventSelect}
              />
            </div>
          </div>
        </main>
      </div>
    </div>
  );
};
