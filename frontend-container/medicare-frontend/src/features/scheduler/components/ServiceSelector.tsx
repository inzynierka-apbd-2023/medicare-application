import React from "react";
import type { ServiceSelectorProps } from "../types";

export const ServiceSelector: React.FC<ServiceSelectorProps> = ({
  services,
  selectedService,
  onServiceChange,
  disabled = false,
}) => {
  return (
    <select
      value={selectedService}
      onChange={(e) => onServiceChange(e.target.value)}
      disabled={disabled}
      className="flex-1 p-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed"
    >
      <option value="">Select Service</option>
      {services.map((service) => (
        <option key={service.id} value={service.id}>
          {service.name}
        </option>
      ))}
    </select>
  );
};
