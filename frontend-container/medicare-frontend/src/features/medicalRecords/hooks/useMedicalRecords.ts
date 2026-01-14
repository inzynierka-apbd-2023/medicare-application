import { useCallback, useEffect, useState } from "react";

import { useAuth } from "../../../shared/auth/AuthContext";
import {
  BackendPatientHistory,
  medicalRecordsApi,
} from "../../../shared/services/medicalRecordsApi";
import type { BackendPatientProfile } from "../../../shared/services/patientsApi";
import { patientsApi } from "../../../shared/services/patientsApi";
import { staffApi } from "../../../shared/services/staffApi";
import type {
  EmergencyContact,
  InsuranceInfo,
  MedicalCondition,
  MedicalVisit,
  Medication,
  PatientMedicalRecord,
  VitalSigns,
} from "../types";

interface UseMedicalRecordsResult {
  records: PatientMedicalRecord[];
  selectedRecord: PatientMedicalRecord | null;
  isLoading: boolean;
  error: string | null;
  searchPatient: (query: string) => Promise<void>;
  selectPatient: (patientId: string) => Promise<void>;
  refetch: () => Promise<void>;
}

export const useMedicalRecords = (
  initialPatientId?: string
): UseMedicalRecordsResult => {
  const { user } = useAuth();
  const [records, setRecords] = useState<PatientMedicalRecord[]>([]);
  const [selectedRecord, setSelectedRecord] =
    useState<PatientMedicalRecord | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Helper to map backend data to frontend model
  const mapToPatientMedicalRecord = useCallback(
    (
      profile: BackendPatientProfile,
      history: BackendPatientHistory,
      doctorMap: Map<string, string> // Map of doctorId -> doctorName
    ): PatientMedicalRecord => {
      // Map Visits
      const visits: MedicalVisit[] = history.records.map((r) => {
        // Find vitals for this record
        const v = history.vitals.find((v) => v.medicalRecordId === r.id);
        let vitalSigns: VitalSigns | undefined;
        if (v) {
          vitalSigns = {};
          if (v.systolicBP !== undefined)
            vitalSigns.bloodPressureSystolic = v.systolicBP;
          if (v.diastolicBP !== undefined)
            vitalSigns.bloodPressureDiastolic = v.diastolicBP;
          if (v.heartRate !== undefined) vitalSigns.heartRate = v.heartRate;
          if (v.temperature !== undefined)
            vitalSigns.temperature = v.temperature;
          if (v.weight !== undefined) vitalSigns.weight = v.weight;
          if (v.height !== undefined) vitalSigns.height = v.height;
        }

        const visit: MedicalVisit = {
          id: r.id,
          date: r.visitDate,
          doctorName: doctorMap.get(r.doctorId) || "Dr. Unknown",
          specialty: "General Practice",
          chiefComplaint: r.chiefComplaint || "",
          diagnosis: r.assessment || "",
          treatment: r.plan || "",
          notes: r.notes || "",
          vitalSigns: vitalSigns || {},
        };

        return visit;
      });

      // Map Conditions
      const medicalConditions: MedicalCondition[] = history.conditions.map(
        (c) => ({
          id: c.id,
          code: c.icd10Code,
          name: c.description,
          diagnosedDate: c.createdAt,
          status: "Active",
          severity: "Moderate",
          notes: c.type,
        })
      );

      // Map Medications
      const currentMedications: Medication[] = history.medications.map((m) => {
        const med: Medication = {
          id: m.id,
          name: m.medicationName,
          dosage: m.dosage,
          frequency: m.frequency,
          prescribedDate: m.prescribedDate,
          prescribedBy: doctorMap.get(m.doctorId) || "Unknown",
          status: m.status as Medication["status"],
        };
        if (m.instructions) med.instructions = m.instructions;
        return med;
      });

      // Map Contacts
      const emergencyContacts: EmergencyContact[] = (
        profile.emergencyContacts || []
      ).map(
        (
          c: { name: string; relation?: string; phone?: string },
          idx: number
        ) => ({
          id: `ec-${idx}`,
          name: c.name,
          relationship: c.relation || "",
          phone: c.phone || "",
          isPrimary: idx === 0,
        })
      );

      // Map Insurance
      const insurance: InsuranceInfo[] = (profile.insurance || []).map(
        (
          i: {
            provider?: string;
            policyNumber?: string;
            validFrom?: string;
            validTo?: string;
          },
          idx: number
        ) => {
          const ins: InsuranceInfo = {
            id: `ins-${idx}`,
            provider: i.provider || "",
            policyNumber: i.policyNumber || "",
            validFrom: i.validFrom || "",
            isPrimary: idx === 0,
          };
          if (i.validTo) ins.validTo = i.validTo;
          return ins;
        }
      );

      return {
        id: "mr-" + profile.id,
        patientId: profile.id,
        name: profile.name,
        dateOfBirth: profile.dateOfBirth || "",
        gender: (profile.gender as PatientMedicalRecord["gender"]) || "Other",
        medicalRecordNumber: "MRN-" + profile.id.substring(0, 8).toUpperCase(),
        phone: profile.phone,
        email: profile.email,
        address: profile.address,
        emergencyContacts,
        insurance,
        medicalConditions,
        currentMedications,
        visits,
        lastUpdated: new Date().toISOString(),
        createdDate: new Date().toISOString(),
      };
    },
    []
  );

  const fetchFullPatientRecord = useCallback(
    async (patientId: string) => {
      setIsLoading(true);
      setError(null);
      try {
        // Fetch profile, history, and doctors in parallel
        const [profileRes, historyRes, doctorsRes] = await Promise.all([
          patientsApi.getPatientProfile(patientId),
          medicalRecordsApi.getPatientHistory(patientId),
          staffApi.getStaff({ role: "Doctor" }),
        ]);

        if (!profileRes.success || !profileRes.data) {
          // If profile fetch fails, we can't show anything.
          throw new Error(
            profileRes.message || "Failed to fetch patient profile"
          );
        }

        // Create doctor lookup map
        const doctorMap = new Map<string, string>();
        if (doctorsRes.success && doctorsRes.data) {
          for (const doc of doctorsRes.data) {
            if (doc.role === "Doctor") {
              const name =
                `Dr. ${doc.profile.firstName} ${doc.profile.lastName}`.trim();
              doctorMap.set(doc.id, name);
            }
          }
        }

        const profile = profileRes.data;
        // History might be null if 404 (new patient), that's fine.
        const history = historyRes.data || {
          patientId,
          records: [],
          conditions: [],
          medications: [],
          vitals: [],
        };

        const record = mapToPatientMedicalRecord(profile, history, doctorMap);
        setSelectedRecord(record);
        setRecords([record]);
      } catch (err: unknown) {
        setError(err instanceof Error ? err.message : "Failed to load record");
        // Console log removed as per request
      } finally {
        setIsLoading(false);
      }
    },
    [mapToPatientMedicalRecord]
  );

  const selectPatient = useCallback(
    async (patientId: string) => {
      await fetchFullPatientRecord(patientId);
    },
    [fetchFullPatientRecord]
  );

  const refetch = useCallback(async () => {
    if (selectedRecord) {
      await fetchFullPatientRecord(selectedRecord.patientId);
    }
  }, [selectedRecord, fetchFullPatientRecord]);

  const searchPatient = useCallback(
    async (query: string) => {
      if (!query) {
        setSelectedRecord(null);
        return;
      }

      setIsLoading(true);
      setError(null);

      try {
        // If query is a UUID, try to fetch directly
        if (query.match(/^[0-9a-fA-F-]{36}$/)) {
          await fetchFullPatientRecord(query);
        } else {
          // Implemented search via existing Patient List
          if (user?.id) {
            const res = await patientsApi.getPatients(user.id);
            if (res.success && res.data.length > 0) {
              // Find ANY matching patient, not just case-sensitive or exact.
              // User complains "alice" didn't work. Lowercase check is good.
              const found = res.data.find((p) =>
                p.name.toLowerCase().includes(query.toLowerCase())
              );

              if (found) {
                await fetchFullPatientRecord(found.id);
              } else {
                // Explicitly set error/state so UI knows
                setSelectedRecord(null);
                setError(`No patient found matching "${query}"`);
              }
            } else {
              // No patients in list at all
              setSelectedRecord(null);
              setError("No patients found in your list");
            }
          }
        }
      } catch (_e) {
        setError("Search failed");
        // Console log removed
      } finally {
        setIsLoading(false);
      }
    },
    [fetchFullPatientRecord, user]
  );

  useEffect(() => {
    if (initialPatientId) {
      fetchFullPatientRecord(initialPatientId);
    }
  }, [initialPatientId, fetchFullPatientRecord]);

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
