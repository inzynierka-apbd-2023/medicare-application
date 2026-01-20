import React, { useCallback, useState } from "react";
import { useDoctors, usePatients } from "@features/receptionistScheduler/hooks";
import type {
  AppointmentFilters,
  Doctor,
  Patient,
} from "@features/receptionistScheduler/types";
import { Button, Input, SearchInput } from "@shared/components";
import { Filter, User, X } from "lucide-react";

interface SchedulerFiltersProps {
  filters: AppointmentFilters;
  onFiltersChange: (filters: Partial<AppointmentFilters>) => void;
  onClearFilters: () => void;
}

export const SchedulerFilters: React.FC<SchedulerFiltersProps> = ({
  filters,
  onFiltersChange,
  onClearFilters,
}) => {
  const { doctors } = useDoctors();
  const { searchPatients } = usePatients();
  const [showFilters, setShowFilters] = useState(false);

  // Patient search state
  const [patientSearchTerm, setPatientSearchTerm] = useState("");
  const [patientSearchResults, setPatientSearchResults] = useState<Patient[]>(
    []
  );
  const [showPatientDropdown, setShowPatientDropdown] = useState(false);
  const [selectedPatient, setSelectedPatient] = useState<Patient | null>(null);

  // Patient search handler
  const handlePatientSearch = useCallback(
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      const query = e.target.value;
      setPatientSearchTerm(query);

      if (query.trim().length > 2) {
        try {
          const results = await searchPatients(query);
          setPatientSearchResults(results);
          setShowPatientDropdown(true);
        } catch (error) {
          console.error("Error searching patients:", error);
          setPatientSearchResults([]);
          setShowPatientDropdown(false);
        }
      } else {
        setPatientSearchResults([]);
        setShowPatientDropdown(false);
      }
    },
    [searchPatients]
  );

  const handlePatientSelect = useCallback(
    (patient: Patient) => {
      setSelectedPatient(patient);
      setPatientSearchTerm(`${patient.firstName} ${patient.lastName}`);
      setShowPatientDropdown(false);

      // Update the filters with patient name to filter the scheduler
      onFiltersChange({
        patientName: `${patient.firstName} ${patient.lastName}`,
      });
    },
    [onFiltersChange]
  );

  const clearPatientFilter = useCallback(() => {
    setSelectedPatient(null);
    setPatientSearchTerm("");
    setPatientSearchResults([]);
    setShowPatientDropdown(false);

    // Remove patientName from filters entirely instead of setting to empty string
    const { patientName: _patientName, ...otherFilters } = filters;
    onFiltersChange(otherFilters);
  }, [filters, onFiltersChange]);

  const resetLocalPatientState = useCallback(() => {
    setSelectedPatient(null);
    setPatientSearchTerm("");
    setPatientSearchResults([]);
    setShowPatientDropdown(false);
  }, []);

  const handleDoctorChange = useCallback(
    (e: React.ChangeEvent<HTMLSelectElement>) => {
      const value = e.target.value;
      const { doctorId: _doctorId, ...otherFilters } = filters;
      if (value) {
        onFiltersChange({ ...otherFilters, doctorId: value });
      } else {
        onFiltersChange(otherFilters);
      }
    },
    [filters, onFiltersChange]
  );

  const handleStatusChange = useCallback(
    (e: React.ChangeEvent<HTMLSelectElement>) => {
      const value = e.target.value;
      const { status: _status, ...otherFilters } = filters;
      if (value) {
        onFiltersChange({ ...otherFilters, status: value });
      } else {
        onFiltersChange(otherFilters);
      }
    },
    [filters, onFiltersChange]
  );

  const handleAppointmentCategoryChange = useCallback(
    (e: React.ChangeEvent<HTMLSelectElement>) => {
      const value = e.target.value;
      const { appointmentCategory: _appointmentCategory, ...otherFilters } =
        filters;
      if (value) {
        onFiltersChange({ ...otherFilters, appointmentCategory: value });
      } else {
        onFiltersChange(otherFilters);
      }
    },
    [filters, onFiltersChange]
  );

  const handleAppointmentTypeChange = useCallback(
    (e: React.ChangeEvent<HTMLSelectElement>) => {
      const value = e.target.value;
      const { appointmentType: _appointmentType, ...otherFilters } = filters;
      if (value) {
        onFiltersChange({ ...otherFilters, appointmentType: value });
      } else {
        onFiltersChange(otherFilters);
      }
    },
    [filters, onFiltersChange]
  );

  const handleDateRangeChange = useCallback(
    (field: "start" | "end", value: string) => {
      const currentRange = filters.dateRange || { start: "", end: "" };
      onFiltersChange({
        dateRange: {
          ...currentRange,
          [field]: value,
        },
      });
    },
    [filters.dateRange, onFiltersChange]
  );

  const hasActiveFilters = !!(
    filters.patientName ||
    selectedPatient ||
    filters.doctorId ||
    filters.status ||
    filters.appointmentType ||
    filters.appointmentCategory ||
    filters.dateRange?.start ||
    filters.dateRange?.end
  );

  return (
    <div className="bg-white rounded-lg border p-4 mb-6">
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center space-x-4">
          <div className="flex items-center">
            <Filter size={20} className="mr-2 text-gray-600" />
            <h3 className="text-lg font-medium text-gray-900">Filters</h3>
          </div>
          {hasActiveFilters && (
            <Button
              variant="outline"
              size="sm"
              onClick={() => {
                onClearFilters();
                resetLocalPatientState();
              }}
              className="flex items-center"
            >
              <X size={16} className="mr-1" />
              Clear All
            </Button>
          )}
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={() => setShowFilters(!showFilters)}
        >
          {showFilters ? "Hide Filters" : "Show Filters"}
        </Button>
      </div>

      {/* Patient Search */}
      <div className="mb-4">
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Search Patient
        </label>
        <div className="relative">
          <SearchInput
            value={patientSearchTerm}
            onChange={handlePatientSearch}
            placeholder="Search by patient name, email, or MRN..."
            className="w-full"
          />

          {/* Clear button when patient is selected */}
          {selectedPatient && (
            <button
              onClick={clearPatientFilter}
              className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600"
            >
              <X size={16} />
            </button>
          )}

          {/* Patient dropdown */}
          {showPatientDropdown && patientSearchResults.length > 0 && (
            <div className="absolute z-20 w-full mt-1 bg-white border rounded-lg shadow-lg max-h-60 overflow-y-auto">
              {patientSearchResults.map((patient) => (
                <button
                  key={patient.id}
                  type="button"
                  onClick={() => handlePatientSelect(patient)}
                  className="w-full p-3 text-left hover:bg-gray-50 border-b border-gray-100 last:border-b-0"
                >
                  <div className="flex items-center">
                    <User size={16} className="mr-2 text-gray-500" />
                    <div>
                      <div className="font-medium">
                        {patient.firstName} {patient.lastName}
                      </div>
                      <div className="text-sm text-gray-600">
                        {patient.email} • {patient.medicalRecordNumber}
                      </div>
                    </div>
                  </div>
                </button>
              ))}
            </div>
          )}

          {/* Show selected patient */}
          {selectedPatient && (
            <div className="mt-2 p-2 bg-blue-50 border border-blue-200 rounded-lg">
              <div className="flex items-center justify-between">
                <div className="flex items-center">
                  <User size={16} className="mr-2 text-blue-600" />
                  <div>
                    <div className="font-medium text-blue-900">
                      {selectedPatient.firstName} {selectedPatient.lastName}
                    </div>
                    <div className="text-sm text-blue-700">
                      Filtering appointments for this patient
                    </div>
                  </div>
                </div>
                <button
                  onClick={clearPatientFilter}
                  className="text-blue-600 hover:text-blue-800"
                >
                  <X size={16} />
                </button>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Advanced Filters */}
      {showFilters && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4 pt-4 border-t">
          {/* Doctor Filter */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Doctor
            </label>
            <select
              value={filters.doctorId || ""}
              onChange={handleDoctorChange}
              className="w-full p-2 border rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">All Doctors</option>
              {doctors.map((doctor: Doctor) => (
                <option key={doctor.id} value={doctor.id}>
                  Dr. {doctor.firstName} {doctor.lastName}
                </option>
              ))}
            </select>
          </div>

          {/* Status Filter */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Status
            </label>
            <select
              value={filters.status || ""}
              onChange={handleStatusChange}
              className="w-full p-2 border rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">All Statuses</option>
              <option value="status-1">Scheduled</option>
              <option value="status-2">Confirmed</option>
              <option value="status-3">Completed</option>
              <option value="status-4">Cancelled</option>
              <option value="status-5">No-Show</option>
              <option value="status-6">Rescheduled</option>
            </select>
          </div>

          {/* Appointment Type Filter */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Type
            </label>
            <select
              value={filters.appointmentType || ""}
              onChange={handleAppointmentTypeChange}
              className="w-full p-2 border rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">All Types</option>
              <option value="in-person">In-Person</option>
              <option value="video-call">Video Call</option>
              <option value="phone">Phone</option>
            </select>
          </div>

          {/* Appointment Category Filter */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Category
            </label>
            <select
              value={filters.appointmentCategory || ""}
              onChange={handleAppointmentCategoryChange}
              className="w-full p-2 border rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">All Categories</option>
              <option value="consultation">Consultation</option>
              <option value="emergency">Emergency</option>
              <option value="follow-up">Follow-up</option>
              <option value="procedure">Procedure</option>
              <option value="surgery">Surgery</option>
              <option value="check-up">Check-up</option>
              <option value="vaccination">Vaccination</option>
            </select>
          </div>

          {/* Date Range */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Date Range
            </label>
            <div className="space-y-2">
              <Input
                type="date"
                value={filters.dateRange?.start || ""}
                onChange={(e) => handleDateRangeChange("start", e.target.value)}
                placeholder="Start date"
                className="text-sm"
              />
              <Input
                type="date"
                value={filters.dateRange?.end || ""}
                onChange={(e) => handleDateRangeChange("end", e.target.value)}
                placeholder="End date"
                className="text-sm"
              />
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
