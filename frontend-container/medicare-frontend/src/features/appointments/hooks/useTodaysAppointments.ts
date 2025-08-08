import { useEffect, useState } from "react";

import type { TodayAppointment } from "../types";

interface UseTodaysAppointmentsReturn {
  appointments: TodayAppointment[];
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
  markAsCompleted: (id: string) => Promise<boolean>;
  markAsNoShow: (id: string) => Promise<boolean>;
}

// Mock data for today's appointments
const mockTodaysAppointments: TodayAppointment[] = [
  {
    id: "apt-1",
    date: new Date().toISOString().split("T")[0],
    time: "09:00",
    duration: 30,
    patient: {
      id: "pat-1",
      name: "John Smith",
      age: 45,
      phone: "(555) 123-4567",
      email: "john.smith@email.com",
      medicalHistory: ["Hypertension", "Type 2 Diabetes"],
      allergies: ["Penicillin"],
      currentMedications: ["Metformin", "Lisinopril"],
    },
    appointmentType: "Regular Checkup",
    description: "Annual physical examination",
    status: "scheduled",
    chiefComplaint: "Routine checkup and blood pressure monitoring",
  },
  {
    id: "apt-2",
    date: new Date().toISOString().split("T")[0],
    time: "09:30",
    duration: 45,
    patient: {
      id: "pat-2",
      name: "Sarah Johnson",
      age: 32,
      phone: "(555) 987-6543",
      email: "sarah.j@email.com",
      medicalHistory: ["Asthma"],
      allergies: [],
      currentMedications: ["Albuterol inhaler"],
    },
    appointmentType: "Follow-up",
    description: "Asthma follow-up appointment",
    status: "scheduled",
    chiefComplaint: "Increased shortness of breath and wheezing",
  },
  {
    id: "apt-3",
    date: new Date().toISOString().split("T")[0],
    time: "10:15",
    duration: 60,
    patient: {
      id: "pat-3",
      name: "Michael Brown",
      age: 67,
      phone: "(555) 456-7890",
      email: "m.brown@email.com",
      medicalHistory: ["Heart Disease", "High Cholesterol"],
      allergies: ["Sulfa drugs"],
      currentMedications: ["Atorvastatin", "Metoprolol", "Aspirin"],
    },
    appointmentType: "Cardiology Consultation",
    description: "Cardiac evaluation and medication review",
    status: "completed",
    chiefComplaint: "Chest pain and irregular heartbeat",
    notes: "EKG normal, medication adjusted",
  },
  {
    id: "apt-4",
    date: new Date().toISOString().split("T")[0],
    time: "11:30",
    duration: 30,
    patient: {
      id: "pat-4",
      name: "Emily Davis",
      age: 28,
      phone: "(555) 321-9876",
      email: "emily.davis@email.com",
      medicalHistory: [],
      allergies: ["Latex"],
      currentMedications: [],
    },
    appointmentType: "Consultation",
    description: "New patient consultation",
    status: "scheduled",
    chiefComplaint: "Fatigue and headaches for the past month",
  },
  {
    id: "apt-5",
    date: new Date().toISOString().split("T")[0],
    time: "14:00",
    duration: 30,
    patient: {
      id: "pat-5",
      name: "Robert Wilson",
      age: 55,
      phone: "(555) 654-3210",
      email: "r.wilson@email.com",
      medicalHistory: ["Diabetes", "Obesity"],
      allergies: [],
      currentMedications: ["Insulin", "Metformin"],
    },
    appointmentType: "Diabetes Management",
    description: "Diabetes follow-up and glucose monitoring review",
    status: "no-show",
    chiefComplaint: "Blood sugar management issues",
  },
  {
    id: "apt-6",
    date: new Date().toISOString().split("T")[0],
    time: "15:30",
    duration: 45,
    patient: {
      id: "pat-6",
      name: "Lisa Anderson",
      age: 41,
      phone: "(555) 789-0123",
      email: "lisa.a@email.com",
      medicalHistory: ["Anxiety", "Depression"],
      allergies: ["Codeine"],
      currentMedications: ["Sertraline", "Lorazepam"],
    },
    appointmentType: "Mental Health Follow-up",
    description: "Medication review and mental health assessment",
    status: "scheduled",
    chiefComplaint: "Anxiety levels have increased recently",
  },
];

export const useTodaysAppointments = (): UseTodaysAppointmentsReturn => {
  const [appointments, setAppointments] = useState<TodayAppointment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchTodaysAppointments = async () => {
    try {
      setLoading(true);
      setError(null);

      // Simulate API call delay
      await new Promise((resolve) => setTimeout(resolve, 1000));

      // In a real app, this would be an API call:
      // const response = await todaysAppointmentsApi.getTodaysAppointments();

      setAppointments(mockTodaysAppointments);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Failed to fetch today's appointments"
      );
    } finally {
      setLoading(false);
    }
  };

  const markAsCompleted = async (id: string): Promise<boolean> => {
    try {
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 500));

      setAppointments((prev) =>
        prev.map((apt) =>
          apt.id === id ? { ...apt, status: "completed" as const } : apt
        )
      );

      return true;
    } catch (err) {
      console.error("Failed to mark appointment as completed:", err);
      return false;
    }
  };

  const markAsNoShow = async (id: string): Promise<boolean> => {
    try {
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 500));

      setAppointments((prev) =>
        prev.map((apt) =>
          apt.id === id ? { ...apt, status: "no-show" as const } : apt
        )
      );

      return true;
    } catch (err) {
      console.error("Failed to mark appointment as no-show:", err);
      return false;
    }
  };

  useEffect(() => {
    fetchTodaysAppointments();
  }, []);

  return {
    appointments,
    loading,
    error,
    refetch: fetchTodaysAppointments,
    markAsCompleted,
    markAsNoShow,
  };
};
