import { useCallback, useEffect, useState } from "react";

import type { CalendarEvent, Doctor, Patient } from "../types";

interface UseReadOnlySchedulerOptions {
  patientFilter?: string | undefined;
  doctorFilter?: string | undefined;
}

export const useReadOnlyScheduler = (
  options: UseReadOnlySchedulerOptions = {}
) => {
  const { patientFilter, doctorFilter } = options;

  const [calendarEvents, setCalendarEvents] = useState<CalendarEvent[]>([]);
  const [patientSearchResults, setPatientSearchResults] = useState<Patient[]>(
    []
  );
  const [doctorSearchResults, setDoctorSearchResults] = useState<Doctor[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  // Mock data generators
  const generateMockPatients = (): Patient[] => [
    {
      id: "1",
      firstName: "Sarah",
      lastName: "Johnson",
      email: "sarah.johnson@email.com",
      phone: "+1-234-567-8901",
      dateOfBirth: "1985-03-15",
    },
    {
      id: "2",
      firstName: "Michael",
      lastName: "Davis",
      email: "michael.davis@email.com",
      phone: "+1-234-567-8902",
      dateOfBirth: "1992-07-22",
    },
    {
      id: "3",
      firstName: "Jennifer",
      lastName: "Wilson",
      email: "jennifer.wilson@email.com",
      phone: "+1-234-567-8903",
      dateOfBirth: "1979-11-08",
    },
    {
      id: "4",
      firstName: "David",
      lastName: "Brown",
      email: "david.brown@email.com",
      phone: "+1-234-567-8904",
      dateOfBirth: "1988-05-30",
    },
    {
      id: "5",
      firstName: "Amanda",
      lastName: "Garcia",
      email: "amanda.garcia@email.com",
      phone: "+1-234-567-8905",
      dateOfBirth: "1995-12-12",
    },
  ];

  const generateMockDoctors = (): Doctor[] => [
    {
      id: "1",
      firstName: "Emily",
      lastName: "Chen",
      specialization: "Cardiology",
      email: "emily.chen@clinic.com",
      phone: "+1-234-567-9001",
    },
    {
      id: "2",
      firstName: "Robert",
      lastName: "Martinez",
      specialization: "Dermatology",
      email: "robert.martinez@clinic.com",
      phone: "+1-234-567-9002",
    },
    {
      id: "3",
      firstName: "Lisa",
      lastName: "Thompson",
      specialization: "Internal Medicine",
      email: "lisa.thompson@clinic.com",
      phone: "+1-234-567-9003",
    },
    {
      id: "4",
      firstName: "James",
      lastName: "Wilson",
      specialization: "Pediatrics",
      email: "james.wilson@clinic.com",
      phone: "+1-234-567-9004",
    },
    {
      id: "5",
      firstName: "Maria",
      lastName: "Garcia",
      specialization: "Orthopedics",
      email: "maria.garcia@clinic.com",
      phone: "+1-234-567-9005",
    },
  ];

  const generateMockCalendarEvents = useCallback((): CalendarEvent[] => {
    const today = new Date();
    const currentWeek = [];

    // Generate events for the current week
    for (let i = 0; i < 7; i++) {
      const date = new Date(today);
      date.setDate(today.getDate() - today.getDay() + i); // Start from Sunday

      // Skip weekends for appointments
      if (date.getDay() === 0 || date.getDay() === 6) continue;

      // Generate 3-5 appointments per day
      const appointmentsPerDay = Math.floor(Math.random() * 3) + 3;

      for (let j = 0; j < appointmentsPerDay; j++) {
        const startHour = 9 + Math.floor(Math.random() * 8); // 9 AM to 5 PM
        const startMinute = Math.random() < 0.5 ? 0 : 30;
        const duration = 30; // 30 minutes

        const startTime = new Date(date);
        startTime.setHours(startHour, startMinute, 0, 0);

        const endTime = new Date(startTime);
        endTime.setMinutes(endTime.getMinutes() + duration);

        const patients = generateMockPatients();
        const doctors = generateMockDoctors();
        const patient = patients[Math.floor(Math.random() * patients.length)];
        const doctor = doctors[Math.floor(Math.random() * doctors.length)];

        const appointmentType =
          Math.random() < 0.7
            ? "in-person"
            : Math.random() < 0.5
              ? "video-call"
              : "phone";

        currentWeek.push({
          id: `event-${i}-${j}`,
          title: `${patient.firstName} ${patient.lastName} - Dr. ${doctor.firstName} ${doctor.lastName}`,
          start: startTime.toISOString(),
          end: endTime.toISOString(),
          backgroundColor: "#3B82F6",
          borderColor: "#3B82F6",
          extendedProps: {
            patientId: patient.id,
            doctorId: doctor.id,
            type: appointmentType as "in-person" | "video-call" | "phone",
            status: "scheduled" as const,
            ...(appointmentType === "in-person" && {
              room: `Room ${Math.floor(Math.random() * 10) + 101}`,
            }),
          },
        });
      }
    }

    return currentWeek;
  }, []);

  const searchPatients = useCallback(async (query: string) => {
    setIsLoading(true);

    // Simulate API delay
    await new Promise((resolve) => setTimeout(resolve, 300));

    const allPatients = generateMockPatients();
    const filtered = allPatients.filter(
      (patient) =>
        `${patient.firstName} ${patient.lastName}`
          .toLowerCase()
          .includes(query.toLowerCase()) ||
        patient.email.toLowerCase().includes(query.toLowerCase())
    );

    setPatientSearchResults(filtered);
    setIsLoading(false);
  }, []);

  const searchDoctors = useCallback(async (query: string) => {
    setIsLoading(true);

    // Simulate API delay
    await new Promise((resolve) => setTimeout(resolve, 300));

    const allDoctors = generateMockDoctors();
    const filtered = allDoctors.filter(
      (doctor) =>
        `${doctor.firstName} ${doctor.lastName}`
          .toLowerCase()
          .includes(query.toLowerCase()) ||
        doctor.specialization.toLowerCase().includes(query.toLowerCase())
    );

    setDoctorSearchResults(filtered);
    setIsLoading(false);
  }, []);

  // Load calendar events on mount and when filters change
  useEffect(() => {
    const loadEvents = async () => {
      setIsLoading(true);

      // Simulate API delay
      await new Promise((resolve) => setTimeout(resolve, 500));

      let events = generateMockCalendarEvents();

      // Apply filters
      if (patientFilter) {
        events = events.filter(
          (event) => event.extendedProps?.patientId === patientFilter
        );
      }

      if (doctorFilter) {
        events = events.filter(
          (event) => event.extendedProps?.doctorId === doctorFilter
        );
      }

      setCalendarEvents(events);
      setIsLoading(false);
    };

    loadEvents();
  }, [patientFilter, doctorFilter, generateMockCalendarEvents]);

  return {
    calendarEvents,
    patientSearchResults,
    doctorSearchResults,
    isLoading,
    searchPatients,
    searchDoctors,
  };
};
