import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Appointments } from "./Appointments";
import { AppointmentsDetailsModal } from "./components/AppointmentsDetailsModal";
import { useAppointments } from "../../shared/hooks/useAppointments";
import { Loading, ErrorDisplay } from "../../shared/components";
import Header from "../../layout/Header";
import type { Appointment } from "./types";

const AppointmentsPage: React.FC = () => {
  const navigate = useNavigate();
  const [selectedAppointment, setSelectedAppointment] =
    useState<Appointment | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const {
    appointments,
    loading,
    error,
    refetch,
    updatePayment,
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
            />
          </div>
        </div>
      </div>

      <AppointmentsDetailsModal
        isOpen={isModalOpen}
        appointment={selectedAppointment}
        onClose={handleCloseModal}
      />
    </div>
  );
};

export { AppointmentsPage };
