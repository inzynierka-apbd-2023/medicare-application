import React, { useMemo } from "react";

import type {
  Doctor,
  SchedulerProps,
  Service,
  Specialization,
  TimeSlot,
} from "../types";

import { CalendarView, ScheduleFilters } from "./";

interface SchedulerComponentProps extends SchedulerProps {
  services: Service[];
  specializations: Specialization[];
  doctors: Doctor[];
  timeSlots: TimeSlot[];
  selectedService: string;
  selectedSpecialization: string;
  selectedDoctor: string;
  onServiceChange: (serviceId: string) => void;
  onSpecializationChange: (specializationId: string) => void;
  onDoctorChange: (doctorId: string) => void;
}

export const Scheduler: React.FC<SchedulerComponentProps> = ({
  services,
  specializations,
  doctors,
  timeSlots,
  selectedService,
  selectedSpecialization,
  selectedDoctor,
  onServiceChange,
  onSpecializationChange,
  onDoctorChange,
  onAppointmentBook,
  onEventSelect,
}) => {
  // Filter data based on selections
  const filteredData = useMemo(() => {
    let filteredServices = services;
    let filteredSpecializations = specializations;
    let filteredDoctors = doctors;

    if (selectedService) {
      const service = services.find((s) => s.id === selectedService);
      if (service) {
        filteredSpecializations = specializations.filter(
          (spec) => spec.id === service.specializationId
        );
        filteredDoctors = doctors.filter((doctor) =>
          service.doctorIds.includes(doctor.id)
        );
      }
    } else if (selectedSpecialization) {
      const specialization = specializations.find(
        (s) => s.id === selectedSpecialization
      );
      if (specialization) {
        filteredServices = services.filter((service) =>
          specialization.serviceIds.includes(service.id)
        );
        filteredDoctors = doctors.filter((doctor) =>
          specialization.doctorIds.includes(doctor.id)
        );
      }
    } else if (selectedDoctor) {
      // When a doctor is selected, show all their services and specializations
      filteredServices = services.filter((service) =>
        service.doctorIds.includes(selectedDoctor)
      );
      filteredSpecializations = specializations.filter((spec) =>
        spec.doctorIds.includes(selectedDoctor)
      );
    }

    return {
      services: filteredServices,
      specializations: filteredSpecializations,
      doctors: filteredDoctors,
    };
  }, [
    services,
    specializations,
    doctors,
    selectedService,
    selectedSpecialization,
    selectedDoctor,
  ]);

  // Filter time slots based on selected doctor
  const filteredTimeSlots = useMemo(() => {
    if (!selectedDoctor) return [];
    return timeSlots.filter((slot) => slot.doctorId === selectedDoctor);
  }, [timeSlots, selectedDoctor]);

  const handleTimeSlotSelect = (timeSlot: TimeSlot) => {
    if (onAppointmentBook) {
      const booking = {
        serviceId: selectedService,
        specializationId: selectedSpecialization,
        doctorId: selectedDoctor,
        timeSlot,
      };
      onAppointmentBook(booking);
    }
  };

  return (
    <div className="space-y-6">
      <ScheduleFilters
        services={filteredData.services}
        specializations={filteredData.specializations}
        doctors={filteredData.doctors}
        selectedService={selectedService}
        selectedSpecialization={selectedSpecialization}
        selectedDoctor={selectedDoctor}
        onServiceChange={onServiceChange}
        onSpecializationChange={onSpecializationChange}
        onDoctorChange={onDoctorChange}
      />

      <CalendarView
        events={[]} // Will be populated when Microsoft Graph integration is added
        timeSlots={filteredTimeSlots}
        onTimeSlotSelect={handleTimeSlotSelect}
        {...(onEventSelect && { onEventSelect })}
        selectedDoctor={selectedDoctor}
      />
    </div>
  );
};
