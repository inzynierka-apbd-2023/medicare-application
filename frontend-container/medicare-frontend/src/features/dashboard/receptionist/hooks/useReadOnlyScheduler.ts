import { useCallback, useEffect, useState } from "react";
import type {
  CalendarEvent,
  Doctor,
  Patient,
} from "@features/dashboard/receptionist/types";
import { apiClient as api } from "@shared/services/apiClient";
import { staffApi } from "@shared/services/staffApi";

interface UseReadOnlySchedulerOptions {
  patientFilter?: string | undefined;
  doctorFilter?: string | undefined;
}

// Backend appointment interface
interface BackendAppointment {
  id: string;
  patientId: string;
  doctorId: string;
  scheduledAt: string;
  scheduledEndAt?: string;
  status?: string;
  notes?: string;
  appointmentType?: string;
}

// Helper to parse backend datetime
const parseBackendDate = (input: unknown): Date => {
  if (input instanceof Date) return new Date(input);
  const s = input === null || input === undefined ? "" : String(input);
  if (/Z$|[+-]\d{2}:\d{2}$/.test(s)) return new Date(s);
  const naive = s.match(
    /^(\d{4}-\d{2}-\d{2})[T ](\d{2}:\d{2})(?::(\d{2})(?:\.\d{1,7})?)?$/
  );
  if (naive) return new Date(s.replace(" ", "T"));
  return new Date(s);
};

// Helper to format as local ISO
const toLocalIso = (datetime: Date): string => {
  const pad = (n: number) => String(n).padStart(2, "0");
  return (
    `${datetime.getFullYear()}-` +
    `${pad(datetime.getMonth() + 1)}-` +
    `${pad(datetime.getDate())}T` +
    `${pad(datetime.getHours())}:` +
    `${pad(datetime.getMinutes())}:` +
    `${pad(datetime.getSeconds())}`
  );
};

// Get color based on status
const getEventColor = (status: string | undefined): string => {
  const s = (status || "scheduled").toLowerCase();
  if (s === "completed") return "#10B981"; // green
  if (s === "cancelled") return "#EF4444"; // red
  if (s === "in-progress" || s === "inprogress") return "#F59E0B"; // amber
  if (s === "overdue") return "#DC2626"; // dark red
  return "#3B82F6"; // blue (scheduled)
};

export const useReadOnlyScheduler = (
  options: UseReadOnlySchedulerOptions = {}
) => {
  const { patientFilter, doctorFilter } = options;

  const [allCalendarEvents, setAllCalendarEvents] = useState<CalendarEvent[]>(
    []
  );
  const [calendarEvents, setCalendarEvents] = useState<CalendarEvent[]>([]);
  const [patientSearchResults, setPatientSearchResults] = useState<Patient[]>(
    []
  );
  const [doctorSearchResults, setDoctorSearchResults] = useState<Doctor[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  // Doctor lookup map for calendar event titles
  const [doctorMap, setDoctorMap] = useState<
    Map<string, { firstName: string; lastName: string; specialization: string }>
  >(new Map());

  // Patients extracted from appointments for search
  const [patientsFromAppointments, setPatientsFromAppointments] = useState<
    Map<string, Patient>
  >(new Map());

  // Fetch doctors from backend
  const fetchDoctors = useCallback(async () => {
    try {
      const doctorsData = await staffApi.getStaff({ role: "Doctor" });
      if (doctorsData) {
        const doctors = doctorsData.filter((s) => s.role === "Doctor");
        const map = new Map<
          string,
          { firstName: string; lastName: string; specialization: string }
        >();

        for (const doc of doctors) {
          const specialization =
            "specializations" in doc
              ? ((doc as { specializations?: { name: string }[] })
                  .specializations?.[0]?.name ?? "General Practice")
              : "General Practice";
          map.set(doc.id, {
            firstName: doc.profile.firstName,
            lastName: doc.profile.lastName,
            specialization,
          });
        }
        setDoctorMap(map);
        return map;
      }
    } catch (error) {
      console.error("Failed to fetch doctors:", error);
    }
    return new Map();
  }, []);

  // Fetch all calendar events from backend
  const fetchCalendarEvents = useCallback(async () => {
    setIsLoading(true);

    try {
      // First ensure we have doctors
      let doctors = doctorMap;
      if (doctors.size === 0) {
        doctors = await fetchDoctors();
      }

      // Fetch appointments for all doctors
      const allAppointments: BackendAppointment[] = [];
      const tempPatientMap = new Map<string, Patient>();

      const fetchPromises = Array.from(doctors.keys()).map(async (docId) => {
        try {
          const response = await api.get<BackendAppointment[]>(
            `/appointment/appointments/doctor/${docId}`
          );
          return response.data || [];
        } catch {
          return [];
        }
      });

      const results = await Promise.all(fetchPromises);
      for (const appointments of results) {
        allAppointments.push(...appointments);
      }

      // Collect unique patient IDs to fetch details
      const uniquePatientIds = Array.from(
        new Set(allAppointments.map((a) => a.patientId))
      );
      const fetchedPatientMap = new Map<
        string,
        { firstName: string; lastName: string }
      >();

      if (uniquePatientIds.length > 0) {
        try {
          interface PatientBatchResponse {
            patientId?: string;
            id?: string;
            firstName?: string;
            lastName?: string;
          }
          const res = await api.post<PatientBatchResponse[]>(
            "/patient/Patients/batch",
            uniquePatientIds
          );
          if (res.data) {
            for (const p of res.data) {
              const pid = p.patientId || p.id;
              if (pid) {
                fetchedPatientMap.set(pid, {
                  firstName: p.firstName || "Unknown",
                  lastName: p.lastName || "Patient",
                });
              }
            }
          }
        } catch (error) {
          console.error("Failed to batch fetch patients in scheduler:", error);
        }
      }

      // Convert to calendar events and build patient map
      const events: CalendarEvent[] = allAppointments.map((apt) => {
        const start = parseBackendDate(apt.scheduledAt);
        const end = apt.scheduledEndAt
          ? parseBackendDate(apt.scheduledEndAt)
          : new Date(start.getTime() + 30 * 60000);

        const doctorInfo = doctors.get(apt.doctorId);
        const doctorName = doctorInfo
          ? `Dr. ${doctorInfo.firstName} ${doctorInfo.lastName}`
          : "Unknown Doctor";

        const pInfo = fetchedPatientMap.get(apt.patientId) || {
          firstName: "Patient",
          lastName: apt.patientId.substring(0, 8),
        };
        const patientName = `${pInfo.firstName} ${pInfo.lastName}`;

        // Add to patient map for search
        if (!tempPatientMap.has(apt.patientId)) {
          tempPatientMap.set(apt.patientId, {
            id: apt.patientId,
            firstName: pInfo.firstName,
            lastName: pInfo.lastName,
            email: "",
            phone: "",
            dateOfBirth: "",
          });
        }

        const color = getEventColor(apt.status);

        return {
          id: apt.id,
          title: `${patientName} - ${doctorName}`,
          start: toLocalIso(start),
          end: toLocalIso(end),
          backgroundColor: color,
          borderColor: color,
          extendedProps: {
            patientId: apt.patientId,
            doctorId: apt.doctorId,
            type:
              apt.appointmentType === "virtual" ||
              apt.appointmentType === "video"
                ? "video-call"
                : apt.appointmentType === "phone"
                  ? "phone"
                  : ("in-person" as "in-person" | "video-call" | "phone"),
            status:
              apt.status?.toLowerCase() === "completed"
                ? "completed"
                : apt.status?.toLowerCase() === "cancelled"
                  ? "cancelled"
                  : ("scheduled" as "scheduled" | "completed" | "cancelled"),
          },
        };
      });

      setPatientsFromAppointments(tempPatientMap);
      setAllCalendarEvents(events);
      setCalendarEvents(events);
    } catch (error) {
      console.error("Failed to fetch calendar events:", error);
    } finally {
      setIsLoading(false);
    }
  }, [doctorMap, fetchDoctors]);

  // Search patients - filter from appointments data
  const searchPatients = useCallback(
    async (query: string) => {
      setIsLoading(true);

      try {
        const lowerQuery = query.toLowerCase();
        const filtered: Patient[] = [];

        for (const patient of patientsFromAppointments.values()) {
          const fullName =
            `${patient.firstName} ${patient.lastName}`.toLowerCase();
          if (fullName.includes(lowerQuery)) {
            filtered.push(patient);
          }
        }

        setPatientSearchResults(filtered);
      } catch (error) {
        console.error("Error searching patients:", error);
        setPatientSearchResults([]);
      } finally {
        setIsLoading(false);
      }
    },
    [patientsFromAppointments]
  );

  // Search doctors from backend
  const searchDoctors = useCallback(async (query: string) => {
    setIsLoading(true);

    try {
      const doctorsData = await staffApi.getStaff({ role: "Doctor" });
      if (doctorsData) {
        const lowerQuery = query.toLowerCase();
        const filtered: Doctor[] = doctorsData
          .filter((s) => s.role === "Doctor")
          .filter((doc) => {
            const fullName =
              `${doc.profile.firstName} ${doc.profile.lastName}`.toLowerCase();
            return fullName.includes(lowerQuery);
          })
          .map((doc) => ({
            id: doc.id,
            firstName: doc.profile.firstName,
            lastName: doc.profile.lastName,
            specialization:
              "specializations" in doc
                ? ((doc as { specializations?: { name: string }[] })
                    .specializations?.[0]?.name ?? "General Practice")
                : "General Practice",
            email: doc.profile.email || "",
            phone: doc.profile.phone || "",
          }));
        setDoctorSearchResults(filtered);
      } else {
        setDoctorSearchResults([]);
      }
    } catch (error) {
      console.error("Error searching doctors:", error);
      setDoctorSearchResults([]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Initialize events on mount
  useEffect(() => {
    if (allCalendarEvents.length === 0) {
      fetchCalendarEvents();
    }
  }, [fetchCalendarEvents, allCalendarEvents.length]);

  // Apply filters when filter options change
  useEffect(() => {
    if (allCalendarEvents.length === 0) return;

    let filteredEvents = [...allCalendarEvents];

    if (patientFilter) {
      filteredEvents = filteredEvents.filter(
        (event) => event.extendedProps?.patientId === patientFilter
      );
    }

    if (doctorFilter) {
      filteredEvents = filteredEvents.filter(
        (event) => event.extendedProps?.doctorId === doctorFilter
      );
    }

    setCalendarEvents(filteredEvents);
  }, [patientFilter, doctorFilter, allCalendarEvents]);

  return {
    calendarEvents,
    patientSearchResults,
    doctorSearchResults,
    isLoading,
    searchPatients,
    searchDoctors,
  };
};
