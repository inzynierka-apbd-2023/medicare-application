/**
 * PatientManagementView - Doctor's comprehensive patient management interface
 *
 * Features:
 * - Browse and search all patients
 * - View detailed patient information
 * - Access medical history and records
 * - Manage patient treatments and prescriptions
 * - Add clinical notes and observations
 * - View appointment history
 */

import React, { useCallback, useEffect, useState } from "react";
import {
  Activity,
  AlertCircle,
  Calendar,
  ChevronLeft,
  ChevronRight,
  Clock,
  Edit,
  FileText,
  Filter,
  Heart,
  Mail,
  Phone,
  Pill,
  Plus,
  Search,
  User,
} from "lucide-react";

import Header from "../../../layout/Header";
import { Button, Card, LoadingOverlay } from "../../../shared/components";
import type { Patient } from "../../scheduler/types";

// Extended Patient interface for medical records
interface ExtendedPatient extends Patient {
  lastVisit?: string;
  nextAppointment?: string;
  totalVisits?: number;
  allergies?: string[];
  medications?: string[];
  conditions?: string[];
  emergencyContact?: {
    name: string;
    phone: string;
    relationship: string;
  };
  insurance?: {
    provider: string;
    policyNumber: string;
    groupNumber: string;
  };
}

interface MedicalRecord {
  id: string;
  patientId: string;
  date: string;
  type: "visit" | "prescription" | "test" | "note";
  title: string;
  description: string;
  doctorName: string;
  attachments?: string[];
}

// Mock data generators
const generateMockPatients = (): ExtendedPatient[] => {
  const patients: ExtendedPatient[] = [];

  const firstNames = [
    "Jan",
    "Anna",
    "Piotr",
    "Maria",
    "Tomasz",
    "Katarzyna",
    "Michał",
    "Agnieszka",
    "Krzysztof",
    "Magdalena",
    "Paweł",
    "Joanna",
    "Andrzej",
    "Barbara",
    "Wojciech",
    "Ewa",
  ];
  const lastNames = [
    "Kowalski",
    "Nowak",
    "Wiśniewski",
    "Wójcik",
    "Kowalczyk",
    "Kamińska",
    "Lewandowski",
    "Zielińska",
    "Szymański",
    "Woźniak",
    "Dąbrowski",
    "Kozłowski",
    "Jankowski",
    "Mazur",
    "Król",
    "Witkowski",
  ];
  const conditions = [
    "Hypertension",
    "Diabetes Type 2",
    "Asthma",
    "Arthritis",
    "High Cholesterol",
    "Migraine",
    "Depression",
    "Anxiety",
  ];
  const allergies = [
    "Penicillin",
    "Peanuts",
    "Shellfish",
    "Latex",
    "Dust mites",
    "Pollen",
    "Cats",
    "Dogs",
  ];
  const medications = [
    "Metformin",
    "Lisinopril",
    "Atorvastatin",
    "Omeprazole",
    "Amlodipine",
    "Metoprolol",
    "Losartan",
    "Levothyroxine",
  ];

  for (let i = 1; i <= 50; i++) {
    const firstName = firstNames[Math.floor(Math.random() * firstNames.length)];
    const lastName = lastNames[Math.floor(Math.random() * lastNames.length)];
    const birthYear = 1940 + Math.floor(Math.random() * 60);
    const lastVisitDays = Math.floor(Math.random() * 365);
    const nextAppointmentDays = Math.floor(Math.random() * 30) + 1;

    const nextAppointmentValue =
      Math.random() > 0.3
        ? new Date(Date.now() + nextAppointmentDays * 24 * 60 * 60 * 1000)
            .toISOString()
            .split("T")[0]
        : undefined;

    const patient = {
      id: `patient-${i}`,
      firstName,
      lastName,
      email: `${firstName.toLowerCase()}.${lastName.toLowerCase()}@email.com`,
      phone: `+48 ${Math.floor(Math.random() * 900) + 100}-${Math.floor(Math.random() * 900) + 100}-${Math.floor(Math.random() * 900) + 100}`,
      dateOfBirth: `${birthYear}-${String(Math.floor(Math.random() * 12) + 1).padStart(2, "0")}-${String(Math.floor(Math.random() * 28) + 1).padStart(2, "0")}`,
      gender: Math.random() > 0.5 ? "Male" : "Female",
      medicalRecordNumber: `MRN-${String(i).padStart(6, "0")}`,
      bloodType: ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"][
        Math.floor(Math.random() * 8)
      ],
      height: 150 + Math.floor(Math.random() * 50),
      weight: 50 + Math.floor(Math.random() * 80),
      lastVisit: new Date(Date.now() - lastVisitDays * 24 * 60 * 60 * 1000)
        .toISOString()
        .split("T")[0],
      nextAppointment: nextAppointmentValue,
      totalVisits: Math.floor(Math.random() * 50) + 1,
      allergies:
        Math.random() > 0.5
          ? [allergies[Math.floor(Math.random() * allergies.length)]]
          : undefined,
      medications:
        Math.random() > 0.4
          ? [medications[Math.floor(Math.random() * medications.length)]]
          : undefined,
      conditions:
        Math.random() > 0.3
          ? [conditions[Math.floor(Math.random() * conditions.length)]]
          : undefined,
      emergencyContact: {
        name: `${firstNames[Math.floor(Math.random() * firstNames.length)]} ${lastNames[Math.floor(Math.random() * lastNames.length)]}`,
        phone: `+48 ${Math.floor(Math.random() * 900) + 100}-${Math.floor(Math.random() * 900) + 100}-${Math.floor(Math.random() * 900) + 100}`,
        relationship: ["Spouse", "Parent", "Child", "Sibling", "Friend"][
          Math.floor(Math.random() * 5)
        ],
      },
      insurance: {
        provider: ["NFZ", "PZU", "Allianz", "Warta", "AXA"][
          Math.floor(Math.random() * 5)
        ],
        policyNumber: `POL-${Math.floor(Math.random() * 1000000)}`,
        groupNumber: `GRP-${Math.floor(Math.random() * 10000)}`,
      },
    };

    patients.push(patient as ExtendedPatient);
  }

  return patients.sort((a, b) => a.lastName.localeCompare(b.lastName));
};

const generateMockMedicalRecords = (patientId: string): MedicalRecord[] => {
  const records: MedicalRecord[] = [];
  const recordTypes = ["visit", "prescription", "test", "note"] as const;
  const recordCount = Math.floor(Math.random() * 20) + 5;

  for (let i = 0; i < recordCount; i++) {
    const daysAgo = Math.floor(Math.random() * 730); // Up to 2 years ago
    const type = recordTypes[Math.floor(Math.random() * recordTypes.length)];

    let title = "";
    let description = "";

    switch (type) {
      case "visit":
        title = "Medical Consultation";
        description =
          "Regular checkup and health assessment. Patient reported feeling well overall.";
        break;
      case "prescription":
        title = "Prescription - Medication";
        description =
          "Prescribed medication for ongoing treatment. Patient advised on proper dosage.";
        break;
      case "test":
        title = "Laboratory Test Results";
        description =
          "Blood work and diagnostic tests completed. Results within normal ranges.";
        break;
      case "note":
        title = "Clinical Note";
        description =
          "Follow-up note regarding patient's progress and treatment plan adjustments.";
        break;
    }

    records.push({
      id: `record-${patientId}-${i}`,
      patientId,
      date: new Date(Date.now() - daysAgo * 24 * 60 * 60 * 1000)
        .toISOString()
        .split("T")[0],
      type,
      title,
      description,
      doctorName: "Dr. Heart",
    });
  }

  return records.sort(
    (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()
  );
};

interface PatientManagementViewProps {
  selectedPatientId?: string;
  onPatientSelect?: (patient: ExtendedPatient) => void;
}

export const PatientManagementView: React.FC<PatientManagementViewProps> = ({
  selectedPatientId,
  onPatientSelect,
}) => {
  const [patients, setPatients] = useState<ExtendedPatient[]>([]);
  const [selectedPatient, setSelectedPatient] =
    useState<ExtendedPatient | null>(null);
  const [medicalRecords, setMedicalRecords] = useState<MedicalRecord[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterBy, setFilterBy] = useState<"all" | "recent" | "upcoming">(
    "all"
  );
  const [currentPage, setCurrentPage] = useState(1);
  const [_showAddRecord, setShowAddRecord] = useState(false);
  const [activeTab, setActiveTab] = useState<
    "overview" | "records" | "appointments"
  >("overview");

  const patientsPerPage = 10;

  // Load patients
  useEffect(() => {
    setIsLoading(true);
    const mockPatients = generateMockPatients();
    setPatients(mockPatients);

    // Auto-select patient if provided
    if (selectedPatientId) {
      const patient = mockPatients.find((p) => p.id === selectedPatientId);
      if (patient) {
        setSelectedPatient(patient);
        onPatientSelect?.(patient);
      }
    }

    setIsLoading(false);
  }, [selectedPatientId, onPatientSelect]);

  // Load medical records when patient is selected
  useEffect(() => {
    if (selectedPatient) {
      setIsLoading(true);
      const records = generateMockMedicalRecords(selectedPatient.id);
      setMedicalRecords(records);
      setIsLoading(false);
    }
  }, [selectedPatient]);

  // Filter and search patients
  const filteredPatients = patients.filter((patient) => {
    const matchesSearch =
      patient.firstName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      patient.lastName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      patient.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
      patient.medicalRecordNumber
        ?.toLowerCase()
        .includes(searchTerm.toLowerCase());

    if (!matchesSearch) return false;

    switch (filterBy) {
      case "recent":
        return (
          patient.lastVisit &&
          new Date(patient.lastVisit) >
            new Date(Date.now() - 30 * 24 * 60 * 60 * 1000)
        );
      case "upcoming":
        return (
          patient.nextAppointment &&
          new Date(patient.nextAppointment) > new Date()
        );
      default:
        return true;
    }
  });

  // Pagination
  const totalPages = Math.ceil(filteredPatients.length / patientsPerPage);
  const startIndex = (currentPage - 1) * patientsPerPage;
  const paginatedPatients = filteredPatients.slice(
    startIndex,
    startIndex + patientsPerPage
  );

  const handlePatientSelect = useCallback(
    (patient: ExtendedPatient) => {
      setSelectedPatient(patient);
      setActiveTab("overview");
      onPatientSelect?.(patient);
    },
    [onPatientSelect]
  );

  const handleAddMedicalRecord = useCallback(() => {
    setShowAddRecord(true);
  }, []);

  const getRecordIcon = (type: string) => {
    switch (type) {
      case "visit":
        return <User size={16} className="text-blue-600" />;
      case "prescription":
        return <Pill size={16} className="text-green-600" />;
      case "test":
        return <Activity size={16} className="text-purple-600" />;
      case "note":
        return <FileText size={16} className="text-gray-600" />;
      default:
        return <FileText size={16} className="text-gray-600" />;
    }
  };

  return (
    <div className="min-h-screen bg-gray-100 pt-16">
      <Header />

      <LoadingOverlay isLoading={isLoading}>
        <div className="max-w-7xl mx-auto px-4 py-8">
          {/* Header */}
          <div className="flex flex-col md:flex-row md:justify-between md:items-center mb-6">
            <div>
              <h1 className="text-2xl font-bold text-gray-900 flex items-center">
                <User className="mr-2" />
                Patient Management
              </h1>
              <p className="text-gray-600 mt-1">
                Access and manage all patient records and information
              </p>
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Patients List */}
            <div className="lg:col-span-1">
              <Card>
                <div className="p-4 border-b">
                  <h3 className="text-lg font-semibold mb-4">
                    Patients ({filteredPatients.length})
                  </h3>

                  {/* Search and Filter */}
                  <div className="space-y-3">
                    <div className="relative">
                      <Search
                        className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400"
                        size={16}
                      />
                      <input
                        type="text"
                        placeholder="Search patients..."
                        value={searchTerm}
                        onChange={(e) => {
                          setSearchTerm(e.target.value);
                          setCurrentPage(1);
                        }}
                        className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      />
                    </div>

                    <div className="flex gap-2">
                      <select
                        value={filterBy}
                        onChange={(e) => {
                          setFilterBy(
                            e.target.value as "all" | "recent" | "upcoming"
                          );
                          setCurrentPage(1);
                        }}
                        className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      >
                        <option value="all">All Patients</option>
                        <option value="recent">Recent Visits</option>
                        <option value="upcoming">Upcoming Appointments</option>
                      </select>
                      <Button variant="outline" size="sm">
                        <Filter size={16} />
                      </Button>
                    </div>
                  </div>
                </div>

                <div className="max-h-96 overflow-y-auto">
                  {paginatedPatients.map((patient) => (
                    <div
                      key={patient.id}
                      onClick={() => handlePatientSelect(patient)}
                      className={`p-4 border-b cursor-pointer hover:bg-gray-50 transition-colors ${
                        selectedPatient?.id === patient.id
                          ? "bg-blue-50 border-blue-200"
                          : ""
                      }`}
                    >
                      <div className="flex justify-between items-start">
                        <div className="flex-1">
                          <h4 className="font-medium text-gray-900">
                            {patient.firstName} {patient.lastName}
                          </h4>
                          <p className="text-sm text-gray-600">
                            {patient.medicalRecordNumber}
                          </p>
                          <p className="text-xs text-gray-500">
                            Last visit:{" "}
                            {patient.lastVisit
                              ? new Date(patient.lastVisit).toLocaleDateString()
                              : "N/A"}
                          </p>
                        </div>
                        <div className="text-right">
                          <div className="flex items-center space-x-1">
                            {patient.conditions &&
                              patient.conditions.length > 0 && (
                                <AlertCircle
                                  size={12}
                                  className="text-red-500"
                                />
                              )}
                            {patient.nextAppointment && (
                              <Calendar size={12} className="text-green-500" />
                            )}
                          </div>
                        </div>
                      </div>
                    </div>
                  ))}

                  {/* Pagination */}
                  {totalPages > 1 && (
                    <div className="p-4 border-t bg-gray-50 flex justify-between items-center">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() =>
                          setCurrentPage(Math.max(1, currentPage - 1))
                        }
                        disabled={currentPage === 1}
                      >
                        <ChevronLeft size={16} />
                      </Button>
                      <span className="text-sm text-gray-600">
                        Page {currentPage} of {totalPages}
                      </span>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() =>
                          setCurrentPage(Math.min(totalPages, currentPage + 1))
                        }
                        disabled={currentPage === totalPages}
                      >
                        <ChevronRight size={16} />
                      </Button>
                    </div>
                  )}
                </div>
              </Card>
            </div>

            {/* Patient Details */}
            <div className="lg:col-span-2">
              {selectedPatient ? (
                <div className="space-y-6">
                  {/* Patient Header */}
                  <Card>
                    <div className="p-6">
                      <div className="flex justify-between items-start mb-4">
                        <div>
                          <h2 className="text-2xl font-bold text-gray-900">
                            {selectedPatient.firstName}{" "}
                            {selectedPatient.lastName}
                          </h2>
                          <p className="text-gray-600">
                            {selectedPatient.medicalRecordNumber}
                          </p>
                        </div>
                        <div className="flex gap-2">
                          <Button variant="outline" size="sm">
                            <Edit size={16} className="mr-1" />
                            Edit
                          </Button>
                          <Button
                            variant="primary"
                            size="sm"
                            onClick={handleAddMedicalRecord}
                          >
                            <Plus size={16} className="mr-1" />
                            Add Record
                          </Button>
                        </div>
                      </div>

                      {/* Tabs */}
                      <div className="border-b border-gray-200">
                        <nav className="-mb-px flex space-x-8">
                          <button
                            onClick={() => setActiveTab("overview")}
                            className={`py-2 px-1 border-b-2 font-medium text-sm ${
                              activeTab === "overview"
                                ? "border-blue-500 text-blue-600"
                                : "border-transparent text-gray-500 hover:text-gray-700"
                            }`}
                          >
                            Overview
                          </button>
                          <button
                            onClick={() => setActiveTab("records")}
                            className={`py-2 px-1 border-b-2 font-medium text-sm ${
                              activeTab === "records"
                                ? "border-blue-500 text-blue-600"
                                : "border-transparent text-gray-500 hover:text-gray-700"
                            }`}
                          >
                            Medical Records ({medicalRecords.length})
                          </button>
                          <button
                            onClick={() => setActiveTab("appointments")}
                            className={`py-2 px-1 border-b-2 font-medium text-sm ${
                              activeTab === "appointments"
                                ? "border-blue-500 text-blue-600"
                                : "border-transparent text-gray-500 hover:text-gray-700"
                            }`}
                          >
                            Appointments
                          </button>
                        </nav>
                      </div>
                    </div>
                  </Card>

                  {/* Tab Content */}
                  {activeTab === "overview" && (
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                      {/* Basic Information */}
                      <Card>
                        <div className="p-6">
                          <h3 className="text-lg font-semibold mb-4 flex items-center">
                            <User size={20} className="mr-2" />
                            Basic Information
                          </h3>
                          <div className="space-y-3">
                            <div className="flex justify-between">
                              <span className="text-gray-600">
                                Date of Birth:
                              </span>
                              <span className="font-medium">
                                {selectedPatient.dateOfBirth}
                              </span>
                            </div>
                            <div className="flex justify-between">
                              <span className="text-gray-600">Gender:</span>
                              <span className="font-medium">
                                {selectedPatient.gender}
                              </span>
                            </div>
                            <div className="flex justify-between">
                              <span className="text-gray-600">Blood Type:</span>
                              <span className="font-medium">
                                {selectedPatient.bloodType}
                              </span>
                            </div>
                            <div className="flex justify-between">
                              <span className="text-gray-600">Height:</span>
                              <span className="font-medium">
                                {selectedPatient.height} cm
                              </span>
                            </div>
                            <div className="flex justify-between">
                              <span className="text-gray-600">Weight:</span>
                              <span className="font-medium">
                                {selectedPatient.weight} kg
                              </span>
                            </div>
                          </div>
                        </div>
                      </Card>

                      {/* Contact Information */}
                      <Card>
                        <div className="p-6">
                          <h3 className="text-lg font-semibold mb-4 flex items-center">
                            <Phone size={20} className="mr-2" />
                            Contact Information
                          </h3>
                          <div className="space-y-3">
                            <div className="flex items-center">
                              <Mail size={16} className="mr-2 text-gray-500" />
                              <span className="text-sm">
                                {selectedPatient.email}
                              </span>
                            </div>
                            <div className="flex items-center">
                              <Phone size={16} className="mr-2 text-gray-500" />
                              <span className="text-sm">
                                {selectedPatient.phone}
                              </span>
                            </div>
                            {selectedPatient.emergencyContact && (
                              <div className="mt-4 pt-4 border-t">
                                <h4 className="font-medium text-gray-900 mb-2">
                                  Emergency Contact
                                </h4>
                                <div className="space-y-1 text-sm">
                                  <p>
                                    <span className="text-gray-600">Name:</span>{" "}
                                    {selectedPatient.emergencyContact.name}
                                  </p>
                                  <p>
                                    <span className="text-gray-600">
                                      Phone:
                                    </span>{" "}
                                    {selectedPatient.emergencyContact.phone}
                                  </p>
                                  <p>
                                    <span className="text-gray-600">
                                      Relationship:
                                    </span>{" "}
                                    {
                                      selectedPatient.emergencyContact
                                        .relationship
                                    }
                                  </p>
                                </div>
                              </div>
                            )}
                          </div>
                        </div>
                      </Card>

                      {/* Medical Summary */}
                      <Card>
                        <div className="p-6">
                          <h3 className="text-lg font-semibold mb-4 flex items-center">
                            <Heart size={20} className="mr-2" />
                            Medical Summary
                          </h3>
                          <div className="space-y-4">
                            <div>
                              <h4 className="font-medium text-gray-900 mb-2">
                                Conditions
                              </h4>
                              {selectedPatient.conditions &&
                              selectedPatient.conditions.length > 0 ? (
                                <div className="flex flex-wrap gap-2">
                                  {selectedPatient.conditions.map(
                                    (condition, index) => (
                                      <span
                                        key={index}
                                        className="px-2 py-1 bg-red-100 text-red-800 text-xs rounded"
                                      >
                                        {condition}
                                      </span>
                                    )
                                  )}
                                </div>
                              ) : (
                                <p className="text-gray-500 text-sm">
                                  No known conditions
                                </p>
                              )}
                            </div>

                            <div>
                              <h4 className="font-medium text-gray-900 mb-2">
                                Allergies
                              </h4>
                              {selectedPatient.allergies &&
                              selectedPatient.allergies.length > 0 ? (
                                <div className="flex flex-wrap gap-2">
                                  {selectedPatient.allergies.map(
                                    (allergy, index) => (
                                      <span
                                        key={index}
                                        className="px-2 py-1 bg-yellow-100 text-yellow-800 text-xs rounded"
                                      >
                                        {allergy}
                                      </span>
                                    )
                                  )}
                                </div>
                              ) : (
                                <p className="text-gray-500 text-sm">
                                  No known allergies
                                </p>
                              )}
                            </div>

                            <div>
                              <h4 className="font-medium text-gray-900 mb-2">
                                Current Medications
                              </h4>
                              {selectedPatient.medications &&
                              selectedPatient.medications.length > 0 ? (
                                <div className="flex flex-wrap gap-2">
                                  {selectedPatient.medications.map(
                                    (medication, index) => (
                                      <span
                                        key={index}
                                        className="px-2 py-1 bg-green-100 text-green-800 text-xs rounded"
                                      >
                                        {medication}
                                      </span>
                                    )
                                  )}
                                </div>
                              ) : (
                                <p className="text-gray-500 text-sm">
                                  No current medications
                                </p>
                              )}
                            </div>
                          </div>
                        </div>
                      </Card>

                      {/* Visit Statistics */}
                      <Card>
                        <div className="p-6">
                          <h3 className="text-lg font-semibold mb-4 flex items-center">
                            <Calendar size={20} className="mr-2" />
                            Visit Statistics
                          </h3>
                          <div className="space-y-3">
                            <div className="flex justify-between">
                              <span className="text-gray-600">
                                Total Visits:
                              </span>
                              <span className="font-medium">
                                {selectedPatient.totalVisits}
                              </span>
                            </div>
                            <div className="flex justify-between">
                              <span className="text-gray-600">Last Visit:</span>
                              <span className="font-medium">
                                {selectedPatient.lastVisit
                                  ? new Date(
                                      selectedPatient.lastVisit
                                    ).toLocaleDateString()
                                  : "N/A"}
                              </span>
                            </div>
                            <div className="flex justify-between">
                              <span className="text-gray-600">
                                Next Appointment:
                              </span>
                              <span className="font-medium">
                                {selectedPatient.nextAppointment
                                  ? new Date(
                                      selectedPatient.nextAppointment
                                    ).toLocaleDateString()
                                  : "None scheduled"}
                              </span>
                            </div>
                          </div>
                        </div>
                      </Card>
                    </div>
                  )}

                  {activeTab === "records" && (
                    <Card>
                      <div className="p-6">
                        <div className="flex justify-between items-center mb-4">
                          <h3 className="text-lg font-semibold">
                            Medical Records
                          </h3>
                          <Button
                            variant="primary"
                            size="sm"
                            onClick={handleAddMedicalRecord}
                          >
                            <Plus size={16} className="mr-1" />
                            Add Record
                          </Button>
                        </div>

                        <div className="space-y-4">
                          {medicalRecords.map((record) => (
                            <div
                              key={record.id}
                              className="border border-gray-200 rounded-lg p-4 hover:bg-gray-50"
                            >
                              <div className="flex items-start justify-between">
                                <div className="flex items-start space-x-3">
                                  {getRecordIcon(record.type)}
                                  <div className="flex-1">
                                    <h4 className="font-medium text-gray-900">
                                      {record.title}
                                    </h4>
                                    <p className="text-sm text-gray-600 mt-1">
                                      {record.description}
                                    </p>
                                    <div className="flex items-center space-x-4 mt-2 text-xs text-gray-500">
                                      <span className="flex items-center">
                                        <Clock size={12} className="mr-1" />
                                        {new Date(
                                          record.date
                                        ).toLocaleDateString()}
                                      </span>
                                      <span className="flex items-center">
                                        <User size={12} className="mr-1" />
                                        {record.doctorName}
                                      </span>
                                    </div>
                                  </div>
                                </div>
                                <Button variant="outline" size="sm">
                                  View
                                </Button>
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    </Card>
                  )}

                  {activeTab === "appointments" && (
                    <Card>
                      <div className="p-6">
                        <h3 className="text-lg font-semibold mb-4">
                          Appointment History
                        </h3>
                        <div className="text-center py-8 text-gray-500">
                          <Calendar
                            size={48}
                            className="mx-auto mb-4 opacity-50"
                          />
                          <p>Appointment history will be displayed here</p>
                          <p className="text-sm">
                            Integration with scheduler component needed
                          </p>
                        </div>
                      </div>
                    </Card>
                  )}
                </div>
              ) : (
                <Card>
                  <div className="p-6 text-center text-gray-500">
                    <User size={48} className="mx-auto mb-4 opacity-50" />
                    <p>Select a patient from the list to view details</p>
                  </div>
                </Card>
              )}
            </div>
          </div>
        </div>
      </LoadingOverlay>
    </div>
  );
};

export default PatientManagementView;
