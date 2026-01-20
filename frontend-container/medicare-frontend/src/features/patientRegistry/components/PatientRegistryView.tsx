import React, { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { usePatientRegistry } from "@features/patientRegistry/hooks/usePatientRegistry";
import type { PatientRegistryInfo } from "@features/patientRegistry/types";
import Header from "@layout/Header";
import type { TableColumn } from "@shared/components";
import {
  Badge,
  Button,
  Card,
  ErrorDisplay,
  LoadingOverlay,
  SearchInput,
  Table,
} from "@shared/components";
import { useDebounce } from "@shared/hooks";
import { Calendar, Edit, Eye, Search } from "lucide-react";

import { PatientDetailsModal } from "./PatientDetailsModal";

export const PatientRegistryView: React.FC = () => {
  const navigate = useNavigate();
  const [currentPage, setCurrentPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedPatient, setSelectedPatient] =
    useState<PatientRegistryInfo | null>(null);
  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);

  // Debounce search term to avoid excessive API calls
  const debouncedSearchTerm = useDebounce(searchTerm, 500);

  // Memoize filters to prevent unnecessary re-renders
  const filters = useMemo(() => {
    return debouncedSearchTerm
      ? { searchTerm: debouncedSearchTerm }
      : undefined;
  }, [debouncedSearchTerm]);

  const {
    patients,
    doctors,
    totalCount,
    totalPages,
    isLoading,
    error,
    updatePatient,
    clearError,
  } = usePatientRegistry({
    page: currentPage,
    limit: 10,
    ...(filters && { filters }),
  });

  const handleSearch = (value: string) => {
    // Only update if the value actually changed
    if (value !== searchTerm) {
      setSearchTerm(value);
      setCurrentPage(1);
    }
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handleViewPatient = (patient: PatientRegistryInfo) => {
    setSelectedPatient(patient);
    setIsEditMode(false);
    setIsDetailsModalOpen(true);
  };

  const handleEditPatient = (patient: PatientRegistryInfo) => {
    setSelectedPatient(patient);
    setIsEditMode(true);
    setIsDetailsModalOpen(true);
  };

  const handleUpdatePatient = async (
    patientData: Partial<PatientRegistryInfo>
  ) => {
    if (selectedPatient) {
      const result = await updatePatient(selectedPatient.id!, patientData);
      if (result) {
        setIsDetailsModalOpen(false);
        setSelectedPatient(null);
        setIsEditMode(false);
      }
    }
  };

  const handleBookAppointment = (patient: PatientRegistryInfo) => {
    if (patient.id) {
      navigate(
        `/receptionist-scheduler?openBooking=true&patientId=${patient.id}`
      );
    }
  };

  const columns: TableColumn<PatientRegistryInfo>[] = [
    {
      key: "medicalRecordNumber",
      title: "MRN",
      render: (_, patient) => (
        <span className="font-mono text-sm text-gray-600">
          {patient.medicalRecordNumber}
        </span>
      ),
    },
    {
      key: "fullName",
      title: "Patient Name",
      render: (_, patient) => (
        <div>
          <div className="font-medium text-gray-900">
            {patient.firstName} {patient.lastName}
          </div>
          <div className="text-sm text-gray-500">{patient.email}</div>
        </div>
      ),
    },
    {
      key: "phone",
      title: "Phone",
      render: (_, patient) => (
        <span className="text-sm text-gray-900">{patient.phone}</span>
      ),
    },
    {
      key: "dateOfBirth",
      title: "Date of Birth",
      render: (_, patient) => {
        const birthDate = new Date(patient.dateOfBirth);
        const today = new Date();
        const age = today.getFullYear() - birthDate.getFullYear();
        return (
          <div>
            <div className="text-sm text-gray-900">
              {birthDate.toLocaleDateString()}
            </div>
            <div className="text-xs text-gray-500">{age} years old</div>
          </div>
        );
      },
    },
    {
      key: "bloodType",
      title: "Blood Type",
      render: (_, patient) =>
        patient.bloodType ? (
          <Badge variant="info">{patient.bloodType}</Badge>
        ) : (
          <span className="text-gray-400">-</span>
        ),
    },
    {
      key: "isActive",
      title: "Status",
      render: (_, patient) => (
        <Badge variant={patient.isActive ? "success" : "error"}>
          {patient.isActive ? "Active" : "Inactive"}
        </Badge>
      ),
    },
    {
      key: "actions",
      title: "Actions",
      render: (_, patient) => (
        <div className="flex space-x-2">
          <Button
            variant="secondary"
            size="sm"
            onClick={() => handleBookAppointment(patient)}
            title="Book Appointment"
            className="text-blue-600 hovered:text-blue-700"
          >
            <Calendar size={16} />
          </Button>
          <Button
            variant="secondary"
            size="sm"
            onClick={() => handleViewPatient(patient)}
            title="View Patient Details"
          >
            <Eye size={16} />
          </Button>
          <Button
            variant="secondary"
            size="sm"
            onClick={() => handleEditPatient(patient)}
            title="Edit Patient Information"
          >
            <Edit size={16} />
          </Button>
        </div>
      ),
    },
  ];

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100 pt-16">
        <Header />
        <div className="max-w-7xl mx-auto px-4 py-8">
          <ErrorDisplay message={error} onRetry={clearError} />
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 pt-16">
      <Header />
      <LoadingOverlay isLoading={isLoading} message="Loading patients...">
        <div className="max-w-7xl mx-auto px-4 py-8">
          {/* Header */}
          <div className="mb-8">
            <div className="flex justify-between items-center">
              <div>
                <h1 className="text-3xl font-bold text-blue-700">
                  Patient Registry
                </h1>
                <p className="text-lg text-gray-600 mt-2">
                  View and manage patient information
                </p>
              </div>
            </div>
          </div>

          {/* Search and Filters */}
          <Card className="mb-6">
            <div className="flex items-center space-x-4">
              <div className="flex-1">
                <SearchInput
                  placeholder="Search by name, email, phone, or MRN..."
                  onSearch={handleSearch}
                  debounceMs={800}
                  className="w-full"
                />
              </div>
              <Button
                variant="secondary"
                className="flex items-center space-x-2"
              >
                <Search size={16} />
                <span>Advanced Filters</span>
              </Button>
            </div>
          </Card>

          {/* Results Summary */}
          <div className="mb-4">
            <p className="text-sm text-gray-600">
              Showing {patients.length} of {totalCount} patients
            </p>
          </div>

          {/* Patients Table */}
          <Card>
            <Table
              data={patients}
              columns={columns}
              emptyText="No patients found"
            />
          </Card>

          {/* Pagination Controls */}
          {totalPages > 1 && (
            <div className="mt-4 flex justify-center">
              <div className="flex space-x-2">
                <Button
                  variant="secondary"
                  onClick={() => handlePageChange(currentPage - 1)}
                  disabled={currentPage <= 1}
                >
                  Previous
                </Button>
                <span className="flex items-center px-4 py-2 text-sm text-gray-700">
                  Page {currentPage} of {totalPages}
                </span>
                <Button
                  variant="secondary"
                  onClick={() => handlePageChange(currentPage + 1)}
                  disabled={currentPage >= totalPages}
                >
                  Next
                </Button>
              </div>
            </div>
          )}
        </div>
      </LoadingOverlay>

      {/* Patient Details Modal */}
      {selectedPatient && (
        <PatientDetailsModal
          isOpen={isDetailsModalOpen}
          onClose={() => {
            setIsDetailsModalOpen(false);
            setSelectedPatient(null);
            setIsEditMode(false);
          }}
          patient={selectedPatient}
          doctors={doctors}
          isEditMode={isEditMode}
          onUpdate={handleUpdatePatient}
        />
      )}
    </div>
  );
};
