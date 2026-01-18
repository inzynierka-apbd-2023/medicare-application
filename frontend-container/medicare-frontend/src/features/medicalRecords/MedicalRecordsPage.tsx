import React, { useEffect, useState } from "react";
import { useParams, useSearchParams } from "react-router-dom";
import {
  Calendar,
  FileText,
  Heart,
  Phone,
  Pill,
  Search,
  Shield,
  User,
} from "lucide-react";

import Header from "../../layout/Header";
import {
  Badge,
  Button,
  Card,
  EmptyState,
  ErrorDisplay,
  LoadingOverlay,
  SearchInput,
} from "../../shared/components";

import { MedicalRecordDetailModal } from "./components/MedicalRecordDetailModal";
import { PatientSearchResults } from "./components/PatientSearchResults";
import { useMedicalRecords } from "./hooks/useMedicalRecords";
import type { MedicalRecordSection, MedicalRecordsPageProps } from "./types";

export const MedicalRecordsPage: React.FC<MedicalRecordsPageProps> = ({
  patientId: propPatientId,
}) => {
  // Support both route params (/medical-records/:patientId) and query params (?patientId=...)
  const { patientId: routePatientId } = useParams<{ patientId?: string }>();
  const [searchParams] = useSearchParams();
  const queryPatientId = searchParams.get("patientId");
  const patientId =
    propPatientId || routePatientId || queryPatientId || undefined;

  const {
    records,
    selectedRecord,
    isLoading,
    error,
    searchPatient,
    selectPatient,
  } = useMedicalRecords(patientId);

  // Initialize search term from URL patientId if present
  const [searchTerm, setSearchTerm] = useState(patientId || "");
  const [selectedSection, setSelectedSection] =
    useState<MedicalRecordSection>("overview");
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);

  const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
    const query = event.target.value;
    setSearchTerm(query);
    if (query.trim()) {
      searchPatient(query);
    }
  };

  const handlePatientSelect = async (patientId: string) => {
    await selectPatient(patientId);
  };

  const handleViewSection = (section: MedicalRecordSection) => {
    setSelectedSection(section);
    setIsDetailModalOpen(true);
  };

  const handleCloseDetailModal = () => {
    setIsDetailModalOpen(false);
  };

  // Auto-search if patientId is provided
  useEffect(() => {
    if (patientId && !selectedRecord && !isLoading && !error) {
      selectPatient(patientId);
    }
  }, [patientId, selectedRecord, selectPatient, isLoading, error]); // Only re-run if these change

  // Search term initialized with patientId from URL
  // User can freely modify the search bar after initial page load

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const getStatusBadgeVariant = (status: string) => {
    switch (status.toLowerCase()) {
      case "active":
        return "success";
      case "resolved":
        return "default";
      case "chronic":
        return "warning";
      case "critical":
        return "error";
      default:
        return "default";
    }
  };

  // Only show error page for actual errors, not "not found" which is normal search behavior
  const isActualError =
    error && !error.toLowerCase().includes("no patient found");

  if (isActualError && !selectedRecord) {
    return (
      <div className="min-h-screen bg-gray-100 pt-16">
        <Header />
        <div className="max-w-6xl mx-auto px-4 py-8">
          <h1 className="text-3xl font-bold text-blue-700 mb-6">
            Patient Medical Records
          </h1>
          <ErrorDisplay
            message={error}
            onRetry={() => window.location.reload()}
          />
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 pt-16">
      <Header />

      <LoadingOverlay
        isLoading={isLoading && !selectedRecord}
        message="Loading medical records..."
      >
        <div className="max-w-6xl mx-auto px-4 py-8">
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-blue-700 mb-4">
              Patient Medical Records
            </h1>
            <p className="text-gray-600">
              Access comprehensive medical history and patient information
            </p>
          </div>

          {/* Patient Search */}
          <Card variant="medical" className="mb-6">
            <div className="space-y-4">
              <h2 className="text-xl font-semibold text-blue-600 flex items-center">
                <Search className="w-5 h-5 mr-2" />
                Search Patient
              </h2>
              <SearchInput
                placeholder="Search by patient name, medical record number, or patient ID..."
                value={searchTerm}
                onChange={handleSearch}
                className="w-full"
              />
              {searchTerm && !selectedRecord && (
                <PatientSearchResults
                  records={records}
                  searchTerm={searchTerm}
                  onSelectPatient={handlePatientSelect}
                />
              )}
            </div>
          </Card>

          {/* Selected Patient Medical Record */}
          {selectedRecord ? (
            <div className="space-y-6">
              {/* Patient Header */}
              <Card variant="medical">
                <div className="flex items-center justify-between">
                  <div className="flex items-center space-x-4">
                    <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center">
                      <User className="w-8 h-8 text-blue-600" />
                    </div>
                    <div>
                      <h2 className="text-2xl font-bold text-gray-900">
                        {selectedRecord.name}
                      </h2>
                      <p className="text-gray-600">
                        MRN: {selectedRecord.medicalRecordNumber}
                      </p>
                      <p className="text-sm text-gray-500">
                        DOB: {formatDate(selectedRecord.dateOfBirth)} •
                        {selectedRecord.gender} • Blood Type:{" "}
                        {selectedRecord.bloodType || "Unknown"}
                      </p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-sm text-gray-500">Last Updated</p>
                    <p className="font-medium text-gray-900">
                      {formatDate(selectedRecord.lastUpdated)}
                    </p>
                  </div>
                </div>
              </Card>

              {/* Quick Stats */}
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <Card variant="medical" className="text-center">
                  <Heart className="w-8 h-8 text-red-500 mx-auto mb-2" />
                  <h3 className="font-semibold text-gray-900">Conditions</h3>
                  <p className="text-2xl font-bold text-red-600">
                    {selectedRecord.medicalConditions.length}
                  </p>
                  <p className="text-sm text-gray-500">
                    {
                      selectedRecord.medicalConditions.filter(
                        (c) => c.status === "Active"
                      ).length
                    }{" "}
                    active
                  </p>
                </Card>

                <Card variant="medical" className="text-center">
                  <Pill className="w-8 h-8 text-green-500 mx-auto mb-2" />
                  <h3 className="font-semibold text-gray-900">Medications</h3>
                  <p className="text-2xl font-bold text-green-600">
                    {selectedRecord.currentMedications.length}
                  </p>
                  <p className="text-sm text-gray-500">Current prescriptions</p>
                </Card>

                <Card variant="medical" className="text-center">
                  <Calendar className="w-8 h-8 text-purple-500 mx-auto mb-2" />
                  <h3 className="font-semibold text-gray-900">Visits</h3>
                  <p className="text-2xl font-bold text-purple-600">
                    {selectedRecord.visits.length}
                  </p>
                  <p className="text-sm text-gray-500">Total visits</p>
                </Card>
              </div>

              {/* Additional Information Cards */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <Card variant="medical" className="text-center">
                  <Phone className="w-8 h-8 text-blue-500 mx-auto mb-2" />
                  <h3 className="font-semibold text-gray-900">
                    Emergency Contacts
                  </h3>
                  <p className="text-2xl font-bold text-blue-600">
                    {selectedRecord.emergencyContacts.length}
                  </p>
                  <p className="text-sm text-gray-500">
                    {
                      selectedRecord.emergencyContacts.filter(
                        (c) => c.isPrimary
                      ).length
                    }{" "}
                    primary
                  </p>
                </Card>

                <Card variant="medical" className="text-center">
                  <Shield className="w-8 h-8 text-indigo-500 mx-auto mb-2" />
                  <h3 className="font-semibold text-gray-900">Insurance</h3>
                  <p className="text-2xl font-bold text-indigo-600">
                    {selectedRecord.insurance.length}
                  </p>
                  <p className="text-sm text-gray-500">
                    {selectedRecord.insurance.filter((i) => i.isPrimary).length}{" "}
                    primary policy
                  </p>
                </Card>
              </div>

              {/* Medical Information Sections */}
              <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Active Medical Conditions */}
                <Card variant="medical">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold text-blue-600 flex items-center">
                      <Heart className="w-5 h-5 mr-2" />
                      Active Conditions
                    </h3>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleViewSection("conditions")}
                    >
                      View All
                    </Button>
                  </div>
                  <div className="space-y-3">
                    {selectedRecord.medicalConditions
                      .filter((condition) => condition.status === "Active")
                      .slice(0, 3)
                      .map((condition) => (
                        <div
                          key={condition.id}
                          className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                        >
                          <div>
                            <p className="font-medium text-gray-900">
                              {condition.name}
                            </p>
                            <p className="text-sm text-gray-600">
                              Diagnosed: {formatDate(condition.diagnosedDate)}
                            </p>
                          </div>
                          <Badge
                            variant={getStatusBadgeVariant(condition.severity)}
                          >
                            {condition.severity}
                          </Badge>
                        </div>
                      ))}
                    {selectedRecord.medicalConditions.filter(
                      (c) => c.status === "Active"
                    ).length === 0 && (
                      <p className="text-gray-500 italic">
                        No active conditions
                      </p>
                    )}
                  </div>
                </Card>

                {/* Current Medications */}
                <Card variant="medical">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold text-blue-600 flex items-center">
                      <Pill className="w-5 h-5 mr-2" />
                      Current Medications
                    </h3>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleViewSection("medications")}
                    >
                      View All
                    </Button>
                  </div>
                  <div className="space-y-3">
                    {selectedRecord.currentMedications
                      .slice(0, 3)
                      .map((medication) => (
                        <div
                          key={medication.id}
                          className="p-3 bg-gray-50 rounded-lg"
                        >
                          <div className="flex items-center justify-between">
                            <p className="font-medium text-gray-900">
                              {medication.name}
                            </p>
                            <Badge variant="success">{medication.status}</Badge>
                          </div>
                          <p className="text-sm text-gray-600">
                            {medication.dosage} • {medication.frequency}
                          </p>
                          <p className="text-xs text-gray-500">
                            {medication.instructions}
                          </p>
                        </div>
                      ))}
                    {selectedRecord.currentMedications.length === 0 && (
                      <p className="text-gray-500 italic">
                        No current medications
                      </p>
                    )}
                  </div>
                </Card>

                {/* Recent Visits */}
                <Card variant="medical">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold text-blue-600 flex items-center">
                      <Calendar className="w-5 h-5 mr-2" />
                      Recent Visits
                    </h3>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleViewSection("visits")}
                    >
                      View All
                    </Button>
                  </div>
                  <div className="space-y-3">
                    {selectedRecord.visits.slice(0, 3).map((visit) => (
                      <div key={visit.id} className="p-3 bg-gray-50 rounded-lg">
                        <div className="flex items-center justify-between">
                          <p className="font-medium text-gray-900">
                            {visit.doctorName}
                          </p>
                          <p className="text-sm text-gray-500">
                            {formatDate(visit.date)}
                          </p>
                        </div>
                        <p className="text-sm text-gray-600">
                          {visit.specialty}
                        </p>
                        <p className="text-sm text-gray-700 mt-1">
                          {visit.chiefComplaint}
                        </p>
                        {visit.vitalSigns && (
                          <div className="mt-2 pt-2 border-t border-gray-200">
                            <p className="text-xs text-gray-600">
                              Vitals:
                              {visit.vitalSigns.bloodPressureSystolic &&
                                visit.vitalSigns.bloodPressureDiastolic && (
                                  <span className="ml-1">
                                    BP {visit.vitalSigns.bloodPressureSystolic}/
                                    {visit.vitalSigns.bloodPressureDiastolic}
                                  </span>
                                )}
                              {visit.vitalSigns.heartRate && (
                                <span className="ml-2">
                                  HR {visit.vitalSigns.heartRate}
                                </span>
                              )}
                              {visit.vitalSigns.temperature && (
                                <span className="ml-2">
                                  Temp {visit.vitalSigns.temperature}°F
                                </span>
                              )}
                            </p>
                          </div>
                        )}
                      </div>
                    ))}
                    {selectedRecord.visits.length === 0 && (
                      <p className="text-gray-500 italic">No visit history</p>
                    )}
                  </div>
                </Card>
              </div>

              {/* Contact & Insurance Info */}
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Emergency Contacts */}
                <Card variant="medical">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold text-blue-600 flex items-center">
                      <Phone className="w-5 h-5 mr-2" />
                      Emergency Contacts
                    </h3>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleViewSection("contacts")}
                    >
                      View All
                    </Button>
                  </div>
                  <div className="space-y-3">
                    {selectedRecord.emergencyContacts
                      .slice(0, 2)
                      .map((contact) => (
                        <div
                          key={contact.id}
                          className="p-3 bg-gray-50 rounded-lg"
                        >
                          <div className="flex items-center justify-between">
                            <p className="font-medium text-gray-900">
                              {contact.name}
                            </p>
                            {contact.isPrimary && (
                              <Badge variant="info">Primary</Badge>
                            )}
                          </div>
                          <p className="text-sm text-gray-600">
                            {contact.relationship}
                          </p>
                          <p className="text-sm text-gray-700">
                            {contact.phone}
                          </p>
                        </div>
                      ))}
                  </div>
                </Card>

                {/* Insurance Information */}
                <Card variant="medical">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold text-blue-600 flex items-center">
                      <Shield className="w-5 h-5 mr-2" />
                      Insurance
                    </h3>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleViewSection("insurance")}
                    >
                      View All
                    </Button>
                  </div>
                  <div className="space-y-3">
                    {selectedRecord.insurance.slice(0, 2).map((insurance) => (
                      <div
                        key={insurance.id}
                        className="p-3 bg-gray-50 rounded-lg"
                      >
                        <div className="flex items-center justify-between">
                          <p className="font-medium text-gray-900">
                            {insurance.provider}
                          </p>
                          {insurance.isPrimary && (
                            <Badge variant="info">Primary</Badge>
                          )}
                        </div>
                        <p className="text-sm text-gray-600">
                          Policy: {insurance.policyNumber}
                        </p>
                        <p className="text-sm text-gray-700">
                          Valid from: {formatDate(insurance.validFrom)}
                          {insurance.validTo && (
                            <> • Valid to: {formatDate(insurance.validTo)}</>
                          )}
                        </p>
                      </div>
                    ))}
                  </div>
                </Card>
              </div>
            </div>
          ) : (
            !isLoading &&
            !searchTerm && (
              <EmptyState
                icon={<FileText className="w-12 h-12 text-gray-400" />}
                title="No Patient Selected"
                description="Search for a patient using the search box above to view their medical records."
              />
            )
          )}
        </div>
      </LoadingOverlay>

      {/* Detail Modal */}
      {selectedRecord && (
        <MedicalRecordDetailModal
          isOpen={isDetailModalOpen}
          onClose={handleCloseDetailModal}
          record={selectedRecord}
          section={selectedSection}
        />
      )}
    </div>
  );
};
