import React from "react";
import { ServiceSelector } from "./ServiceSelector";
import { SpecializationSelector } from "./SpecializationSelector";
import { DoctorSelector } from "./DoctorSelector";
import type { ScheduleFiltersProps } from "../types";

export const ScheduleFilters: React.FC<ScheduleFiltersProps> = ({
  services,
  specializations,
  doctors,
  selectedService,
  selectedSpecialization,
  selectedDoctor,
  onServiceChange,
  onSpecializationChange,
  onDoctorChange,
}) => {
  const handleServiceChange = (serviceId: string) => {
    onServiceChange(serviceId);
    // Reset other selections when service changes
    if (serviceId) {
      onSpecializationChange("");
      onDoctorChange("");
    }
  };

  const handleSpecializationChange = (specializationId: string) => {
    onSpecializationChange(specializationId);
    // Reset service and doctor when specialization changes
    if (specializationId) {
      onServiceChange("");
      onDoctorChange("");
    }
  };

  const handleDoctorChange = (doctorId: string) => {
    onDoctorChange(doctorId);
    // Reset service and specialization when doctor changes
    if (doctorId) {
      onServiceChange("");
      onSpecializationChange("");
    }
  };

  return (
    <div className="flex flex-col md:flex-row md:space-x-4 space-y-4 md:space-y-0 mb-6">
      <ServiceSelector
        services={services}
        selectedService={selectedService}
        onServiceChange={handleServiceChange}
        disabled={!!selectedDoctor}
      />

      <SpecializationSelector
        specializations={specializations}
        selectedSpecialization={selectedSpecialization}
        onSpecializationChange={handleSpecializationChange}
        disabled={!!selectedDoctor}
      />

      <DoctorSelector
        doctors={doctors}
        selectedDoctor={selectedDoctor}
        onDoctorChange={handleDoctorChange}
      />
    </div>
  );
};
