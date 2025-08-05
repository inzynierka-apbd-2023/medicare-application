import React, { useState } from "react";
import { useNavigate } from "react-router-dom";

import Header from "../../layout/Header";
import { ErrorDisplay, Loading } from "../../shared/components";
import { useAppointments } from "../../shared/hooks/useAppointments";

import { Appointments } from "./Appointments";
import { AppointmentsDetailsModal, DoctorRatingModal } from "./components";
import type { Appointment } from "./types";

const AppointmentsPage: React.FC = () => {
  const navigate = useNavigate();
  const [selectedAppointment, setSelectedAppointment] =
    useState<Appointment | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isRatingModalOpen, setIsRatingModalOpen] = useState(false);
  const [appointmentToRate, setAppointmentToRate] =
    useState<Appointment | null>(null);

  const {
    appointments,
    loading,
    error,
    refetch,
    updatePayment: _updatePayment,
    cancelAppointment,
  } = useAppointments();

  const handleAppointmentDetails = (appointment: Appointment) => {
    setSelectedAppointment(appointment);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedAppointment(null);
  };

  const handlePayment = (appointmentId: string) => {
    // Navigate to wallet with appointment ID for specific payment
    navigate(`/user/wallet?appointmentId=${appointmentId}`);
  };

  const handleCancelAppointment = async (appointmentId: string) => {
    const success = await cancelAppointment(appointmentId);
    if (success) {
      // Show success message or toast notification
      console.log("Appointment cancelled successfully");
    }
  };

  const handleRateDoctor = (appointmentId: string) => {
    const appointment = appointments.find((appt) => appt.id === appointmentId);
    if (appointment) {
      setAppointmentToRate(appointment);
      setIsRatingModalOpen(true);
    }
  };

  const handleSubmitRating = async (
    appointmentId: string,
    rating: number,
    comment?: string
  ) => {
    // Here you would call an API to submit the rating
    // For now, we'll just update the local state
    console.log("Rating submitted:", { appointmentId, rating, comment });

    // Close the modal
    setIsRatingModalOpen(false);
    setAppointmentToRate(null);

    // In a real app, you would refetch appointments to get updated data
    await refetch();
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <div className="pt-20 pb-12 flex items-center justify-center">
          <Loading size="lg" text="Loading appointments..." />
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <div className="pt-20 pb-12">
          <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
            <ErrorDisplay message={error} onRetry={refetch} />
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      <Header />
      <div className="pt-20 pb-12">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="bg-white rounded-2xl shadow-lg p-8">
            <Appointments
              appointments={appointments}
              onDetails={handleAppointmentDetails}
              onPayment={handlePayment}
              onCancel={handleCancelAppointment}
              onRateDoctor={handleRateDoctor}
            />
          </div>
        </div>
      </div>

      <AppointmentsDetailsModal
        isOpen={isModalOpen}
        appointment={selectedAppointment}
        onClose={handleCloseModal}
      />

      <DoctorRatingModal
        appointment={appointmentToRate}
        isOpen={isRatingModalOpen}
        onClose={() => {
          setIsRatingModalOpen(false);
          setAppointmentToRate(null);
        }}
        onSubmitRating={handleSubmitRating}
      />
    </div>
  );
};

export { AppointmentsPage };
