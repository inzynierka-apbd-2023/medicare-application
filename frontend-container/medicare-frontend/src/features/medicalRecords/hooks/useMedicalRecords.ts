import { useEffect, useState } from "react";

import type { PatientMedicalRecord } from "../types";

interface UseMedicalRecordsResult {
  records: PatientMedicalRecord[];
  selectedRecord: PatientMedicalRecord | null;
  isLoading: boolean;
  error: string | null;
  searchPatient: (query: string) => Promise<void>;
  selectPatient: (patientId: string) => Promise<void>;
  refetch: () => Promise<void>;
}

// Mock data for demonstration - adjusted to match database schema
const mockMedicalRecords: PatientMedicalRecord[] = [
  {
    id: "mr-001",
    patientId: "patient-001",
    name: "John Doe",
    dateOfBirth: "1985-03-15",
    gender: "Male",
    bloodType: "A+",
    medicalRecordNumber: "MR-2024-001",
    phone: "+1234567890",
    email: "john.doe@email.com",
    address: "123 Main St, City, State 12345",

    emergencyContacts: [
      {
        id: "ec-001",
        name: "Jane Doe",
        relationship: "Spouse",
        phone: "+1234567891",
        isPrimary: true,
      },
      {
        id: "ec-002",
        name: "Bob Doe",
        relationship: "Father",
        phone: "+1234567892",
        isPrimary: false,
      },
    ],

    insurance: [
      {
        id: "ins-001",
        provider: "Blue Cross Blue Shield",
        policyNumber: "BCBS-123456789",
        groupNumber: "GRP-001",
        validFrom: "2024-01-01",
        isPrimary: true,
      },
    ],

    medicalConditions: [
      {
        id: "cond-001",
        code: "I10",
        name: "Essential Hypertension",
        diagnosedDate: "2023-05-15",
        status: "Active",
        severity: "Moderate",
        notes: "Well controlled with medication",
      },
      {
        id: "cond-002",
        code: "E11.9",
        name: "Type 2 Diabetes Mellitus",
        diagnosedDate: "2022-08-20",
        status: "Active",
        severity: "Mild",
        notes: "Managed with diet and metformin",
      },
    ],

    currentMedications: [
      {
        id: "med-001",
        name: "Lisinopril",
        dosage: "10mg",
        frequency: "Once daily",
        prescribedDate: "2023-05-15",
        prescribedBy: "Dr. Smith",
        status: "Active",
        instructions: "Take with food",
      },
      {
        id: "med-002",
        name: "Metformin",
        dosage: "500mg",
        frequency: "Twice daily",
        prescribedDate: "2022-08-20",
        prescribedBy: "Dr. Johnson",
        status: "Active",
        instructions: "Take with meals",
      },
    ],

    visits: [
      {
        id: "visit-001",
        date: "2024-01-15",
        doctorName: "Dr. Smith",
        specialty: "Internal Medicine",
        chiefComplaint: "Routine follow-up for hypertension",
        diagnosis: "Essential hypertension - stable",
        treatment: "Continue current medication",
        notes: "Blood pressure well controlled. No side effects reported.",
        followUpDate: "2024-04-15",
        vitalSigns: {
          bloodPressureSystolic: 135,
          bloodPressureDiastolic: 85,
          heartRate: 72,
          temperature: 98.6,
          weight: 180,
          height: 70,
        },
      },
      {
        id: "visit-002",
        date: "2023-12-10",
        doctorName: "Dr. Johnson",
        specialty: "Endocrinology",
        chiefComplaint: "Diabetes management check-up",
        diagnosis: "Type 2 diabetes mellitus - well controlled",
        treatment: "Continue metformin, dietary counseling",
        notes: "HbA1c improved to 6.8%. Patient compliant with medication.",
        followUpDate: "2024-03-10",
      },
    ],

    lastUpdated: "2024-01-15T10:30:00Z",
    createdDate: "2022-01-01T00:00:00Z",
  },
  {
    id: "mr-002",
    patientId: "patient-002",
    name: "Maria Garcia",
    dateOfBirth: "1978-07-22",
    gender: "Female",
    bloodType: "O-",
    medicalRecordNumber: "MR-2024-002",
    phone: "+1234567893",
    email: "maria.garcia@email.com",
    address: "456 Oak Ave, City, State 12345",

    emergencyContacts: [
      {
        id: "ec-003",
        name: "Carlos Garcia",
        relationship: "Husband",
        phone: "+1234567894",
        isPrimary: true,
      },
    ],

    insurance: [
      {
        id: "ins-002",
        provider: "Aetna",
        policyNumber: "AET-987654321",
        validFrom: "2024-01-01",
        isPrimary: true,
      },
    ],

    medicalConditions: [
      {
        id: "cond-003",
        code: "M79.1",
        name: "Fibromyalgia",
        diagnosedDate: "2021-03-10",
        status: "Active",
        severity: "Moderate",
        notes: "Responds well to exercise therapy",
      },
    ],

    currentMedications: [
      {
        id: "med-004",
        name: "Pregabalin",
        dosage: "75mg",
        frequency: "Twice daily",
        prescribedDate: "2021-03-15",
        prescribedBy: "Dr. Wilson",
        status: "Active",
        instructions: "May cause drowsiness",
      },
    ],

    visits: [
      {
        id: "visit-003",
        date: "2024-01-10",
        doctorName: "Dr. Wilson",
        specialty: "Rheumatology",
        chiefComplaint: "Fibromyalgia follow-up",
        diagnosis: "Fibromyalgia - stable",
        treatment: "Continue current regimen, physical therapy",
        notes: "Pain levels manageable with current treatment.",
        followUpDate: "2024-04-10",
        vitalSigns: {
          bloodPressureSystolic: 120,
          bloodPressureDiastolic: 80,
          heartRate: 68,
          temperature: 98.2,
          weight: 140,
          height: 65,
        },
      },
    ],

    lastUpdated: "2024-01-10T14:20:00Z",
    createdDate: "2021-01-01T00:00:00Z",
  },
];

export const useMedicalRecords = (
  initialPatientId?: string
): UseMedicalRecordsResult => {
  const [records, setRecords] = useState<PatientMedicalRecord[]>([]);
  const [selectedRecord, setSelectedRecord] =
    useState<PatientMedicalRecord | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchRecords = async () => {
    setIsLoading(true);
    setError(null);
    try {
      // Simulate API delay
      await new Promise((resolve) => setTimeout(resolve, 800));
      setRecords(mockMedicalRecords);

      // If initial patient ID is provided, select that patient
      if (initialPatientId) {
        const initialRecord = mockMedicalRecords.find(
          (r) =>
            r.patientId === initialPatientId ||
            r.id === initialPatientId ||
            r.medicalRecordNumber === initialPatientId
        );
        if (initialRecord) {
          setSelectedRecord(initialRecord);
        }
      }
    } catch (err) {
      setError("Failed to load medical records");
      console.error("Medical records fetch error:", err);
    } finally {
      setIsLoading(false);
    }
  };

  const searchPatient = async (query: string) => {
    if (!query.trim()) {
      setSelectedRecord(null);
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      // Simulate API delay
      await new Promise((resolve) => setTimeout(resolve, 500));

      const matchingRecord = mockMedicalRecords.find(
        (record) =>
          record.name.toLowerCase().includes(query.toLowerCase()) ||
          record.medicalRecordNumber
            .toLowerCase()
            .includes(query.toLowerCase()) ||
          record.patientId.toLowerCase().includes(query.toLowerCase())
      );

      if (matchingRecord) {
        setSelectedRecord(matchingRecord);
      } else {
        setSelectedRecord(null);
        setError(`No patient found matching "${query}"`);
      }
    } catch (err) {
      setError("Failed to search patient records");
      console.error("Patient search error:", err);
    } finally {
      setIsLoading(false);
    }
  };

  const selectPatient = async (patientId: string) => {
    setIsLoading(true);
    setError(null);
    try {
      // Simulate API delay
      await new Promise((resolve) => setTimeout(resolve, 300));

      const record = mockMedicalRecords.find(
        (r) =>
          r.patientId === patientId ||
          r.id === patientId ||
          r.medicalRecordNumber === patientId
      );
      if (record) {
        setSelectedRecord(record);
      } else {
        setError("Patient record not found");
      }
    } catch (err) {
      setError("Failed to load patient record");
      console.error("Patient selection error:", err);
    } finally {
      setIsLoading(false);
    }
  };

  const refetch = async () => {
    await fetchRecords();
  };

  useEffect(() => {
    const initFetch = async () => {
      setIsLoading(true);
      setError(null);
      try {
        // Simulate API delay
        await new Promise((resolve) => setTimeout(resolve, 800));
        setRecords(mockMedicalRecords);

        // If initial patient ID is provided, select that patient
        if (initialPatientId) {
          const initialRecord = mockMedicalRecords.find(
            (r) =>
              r.patientId === initialPatientId ||
              r.id === initialPatientId ||
              r.medicalRecordNumber === initialPatientId
          );
          if (initialRecord) {
            setSelectedRecord(initialRecord);
          }
        }
      } catch (err) {
        setError("Failed to load medical records");
        console.error("Medical records fetch error:", err);
      } finally {
        setIsLoading(false);
      }
    };

    initFetch();
  }, [initialPatientId]);

  return {
    records,
    selectedRecord,
    isLoading,
    error,
    searchPatient,
    selectPatient,
    refetch,
  };
};
