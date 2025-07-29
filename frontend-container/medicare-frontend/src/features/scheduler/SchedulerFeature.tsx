import React from "react";
import { Scheduler } from "./components";
import { useScheduler } from "../../shared/hooks/useScheduler";
import { LoadingOverlay, ErrorDisplay } from "../../shared/components";
import type { SchedulerProps } from "./types";

export const SchedulerFeature: React.FC<SchedulerProps> = ({
  onAppointmentBook,
  onEventSelect,
}) => {
  const {
    services,
    specializations,
    doctors,
    timeSlots,
    selectedService,
    selectedSpecialization,
    selectedDoctor,
    isLoading,
    error,
    handleServiceChange,
    handleSpecializationChange,
    handleDoctorChange,
    bookAppointment,
  } = useScheduler();

  const handleAppointmentBook = async (booking: any) => {
    try {
      const result = await bookAppointment(booking);

      if (onAppointmentBook) {
        onAppointmentBook(booking);
      }

      // Show success message
      alert(
        `Appointment booked successfully! Appointment ID: ${result.appointmentId}`
      );
    } catch (err) {
      // Error is already handled by the hook
      console.error("Failed to book appointment:", err);
    }
  };

  if (error) {
    return <ErrorDisplay message={error} />;
  }

  return (
    <LoadingOverlay isLoading={isLoading}>
      <Scheduler
        services={services}
        specializations={specializations}
        doctors={doctors}
        timeSlots={timeSlots}
        selectedService={selectedService}
        selectedSpecialization={selectedSpecialization}
        selectedDoctor={selectedDoctor}
        onServiceChange={handleServiceChange}
        onSpecializationChange={handleSpecializationChange}
        onDoctorChange={handleDoctorChange}
        onAppointmentBook={handleAppointmentBook}
        onEventSelect={onEventSelect}
      />
    </LoadingOverlay>
  );
};
