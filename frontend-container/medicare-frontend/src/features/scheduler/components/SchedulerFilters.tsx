import React, { ChangeEvent } from "react";
import { Filter, X } from "lucide-react";

import { Button } from "../../../shared/components";
import type {
  Doctor,
  SchedulerFilters,
  Service,
  Specialization,
} from "../types";

interface SchedulerFiltersProps {
  filters: SchedulerFilters;
  onFiltersChange: (filters: Partial<SchedulerFilters>) => void;
  specializations: Specialization[];
  services: Service[];
  doctors: Doctor[];
  isLoading?: boolean;
}

export const SchedulerFiltersComponent: React.FC<SchedulerFiltersProps> = ({
  filters,
  onFiltersChange,
  specializations,
  services,
  doctors,
  isLoading = false,
}) => {
  const handleSpecializationChange = (specializationId: string) => {
    onFiltersChange(
      specializationId ? { specialization: specializationId } : {}
    );
  };

  const handleServiceChange = (serviceId: string) => {
    onFiltersChange(serviceId ? { service: serviceId } : {});
  };

  const handleDoctorChange = (doctorId: string) => {
    onFiltersChange(doctorId ? { doctor: doctorId } : {});
  };

  const handleAppointmentTypeChange = (type: string) => {
    onFiltersChange({
      appointmentType: type as "in-person" | "virtual" | "phone" | "all",
    });
  };

  const handleDateRangeChange = (field: "start" | "end", value: string) => {
    const currentRange = filters.dateRange || { start: "", end: "" };
    onFiltersChange({
      dateRange: {
        ...currentRange,
        [field]: value,
      },
    });
  };

  const handleClearDateRange = () => {
    // Spread to create a new filters object without dateRange
    const { dateRange: _removed, ...rest } = filters;
    void _removed;
    onFiltersChange({
      ...rest,
      appointmentType: rest.appointmentType ?? "all",
    });
  };

  const clearFilters = () => {
    // Create a fresh object with only the required property
    onFiltersChange({ appointmentType: "all" });
  };

  const hasActiveFilters =
    filters.specialization ||
    filters.service ||
    filters.doctor ||
    (filters.appointmentType && filters.appointmentType !== "all") ||
    filters.dateRange;

  // Lists are already filtered by the hook based on current filters
  const filteredServices = services;
  const filteredDoctors = doctors;

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center">
          <Filter className="w-5 h-5 text-gray-600 mr-2" />
          <h3 className="text-lg font-medium text-gray-900">Filters</h3>
        </div>
        {hasActiveFilters && (
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={clearFilters}
            className="text-gray-600 hover:text-gray-800"
          >
            <X className="w-4 h-4 mr-1" />
            Clear All
          </Button>
        )}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4">
        {/* Specialization Filter */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Specialization
          </label>
          <select
            value={filters.specialization || ""}
            onChange={(e: ChangeEvent<HTMLSelectElement>) =>
              handleSpecializationChange(e.target.value)
            }
            disabled={isLoading}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
          >
            <option value="">All Specializations</option>
            {specializations.map((spec) => (
              <option key={spec.id} value={spec.id}>
                {spec.name}
              </option>
            ))}
          </select>
        </div>

        {/* Service Filter */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Service
          </label>
          <select
            value={filters.service || ""}
            onChange={(e: ChangeEvent<HTMLSelectElement>) =>
              handleServiceChange(e.target.value)
            }
            disabled={isLoading}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
          >
            <option value="">All Services</option>
            {filteredServices.map((service) => (
              <option key={service.id} value={service.id}>
                {service.name}
              </option>
            ))}
          </select>
        </div>

        {/* Doctor Filter */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Doctor
          </label>
          <select
            value={filters.doctor || ""}
            onChange={(e: ChangeEvent<HTMLSelectElement>) =>
              handleDoctorChange(e.target.value)
            }
            disabled={isLoading}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
          >
            <option value="">All Doctors</option>
            {filteredDoctors.map((doctor) => (
              <option key={doctor.id} value={doctor.id}>
                Dr. {doctor.firstName} {doctor.lastName}
              </option>
            ))}
          </select>
        </div>

        {/* Appointment Type Filter */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Appointment Type
          </label>
          <select
            value={filters.appointmentType || "all"}
            onChange={(e) => handleAppointmentTypeChange(e.target.value)}
            disabled={isLoading}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
          >
            <option value="all">All Types</option>
            <option value="in-person">In-Person</option>
            <option value="virtual">Virtual</option>
            <option value="phone">Phone</option>
          </select>
        </div>

        {/* Date Range Filter */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Date Range
          </label>
          <div className="space-y-2">
            <input
              type="date"
              value={filters.dateRange?.start?.split("T")[0] || ""}
              onChange={(e) => handleDateRangeChange("start", e.target.value)}
              disabled={isLoading}
              className="w-full px-3 py-1 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
              placeholder="Start date"
            />
            <input
              type="date"
              value={filters.dateRange?.end?.split("T")[0] || ""}
              onChange={(e) => handleDateRangeChange("end", e.target.value)}
              disabled={isLoading}
              min={filters.dateRange?.start?.split("T")[0] || undefined}
              className="w-full px-3 py-1 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
              placeholder="End date"
            />
          </div>
        </div>
      </div>

      {/* Active Filters Display */}
      {hasActiveFilters && (
        <div className="mt-4 pt-4 border-t border-gray-200">
          <div className="flex flex-wrap gap-2">
            {filters.specialization && (
              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                {
                  specializations.find((s) => s.id === filters.specialization)
                    ?.name
                }
                <button
                  onClick={() => handleSpecializationChange("")}
                  className="ml-1.5 text-blue-600 hover:text-blue-800"
                >
                  <X className="w-3 h-3" />
                </button>
              </span>
            )}
            {filters.service && (
              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                {services.find((s) => s.id === filters.service)?.name}
                <button
                  onClick={() => handleServiceChange("")}
                  className="ml-1.5 text-green-600 hover:text-green-800"
                >
                  <X className="w-3 h-3" />
                </button>
              </span>
            )}
            {filters.doctor && (
              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
                Dr. {doctors.find((d) => d.id === filters.doctor)?.firstName}{" "}
                {doctors.find((d) => d.id === filters.doctor)?.lastName}
                <button
                  onClick={() => handleDoctorChange("")}
                  className="ml-1.5 text-purple-600 hover:text-purple-800"
                >
                  <X className="w-3 h-3" />
                </button>
              </span>
            )}
            {filters.appointmentType && filters.appointmentType !== "all" && (
              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-orange-100 text-orange-800">
                {filters.appointmentType}
                <button
                  onClick={() => handleAppointmentTypeChange("all")}
                  className="ml-1.5 text-orange-600 hover:text-orange-800"
                >
                  <X className="w-3 h-3" />
                </button>
              </span>
            )}
            {filters.dateRange &&
              (filters.dateRange.start || filters.dateRange.end) && (
                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                  {filters.dateRange.start}{" "}
                  {filters.dateRange.end && `- ${filters.dateRange.end}`}
                  <button
                    onClick={handleClearDateRange}
                    className="ml-1.5 text-gray-600 hover:text-gray-800"
                  >
                    <X className="w-3 h-3" />
                  </button>
                </span>
              )}
          </div>
        </div>
      )}
    </div>
  );
};

export default SchedulerFiltersComponent;
