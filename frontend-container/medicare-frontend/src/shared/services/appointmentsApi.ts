import {
  createMockResponse,
  createErrorResponse,
  type ApiResponse,
} from "./api";
import type { Appointment } from "../../features/appointments/types";

// Mock appointments data - simulates API response
const mockAppointments: Appointment[] = [
  {
    id: "1",
    date: "2024-08-15",
    time: "10:30 AM",
    doctor: "Sarah Johnson",
    specialization: "Cardiology",
    description:
      "Regular heart checkup and consultation about chest pain symptoms.",
    status: "upcoming",
    paymentStatus: "paid",
    total: 200,
  },
  {
    id: "2",
    date: "2024-08-20",
    time: "2:00 PM",
    doctor: "Michael Chen",
    specialization: "Dermatology",
    description: "Skin examination and mole removal consultation.",
    status: "upcoming",
    paymentStatus: "not_paid",
    total: 300,
  },
  {
    id: "3",
    date: "2024-08-25",
    time: "11:00 AM",
    doctor: "Lisa Rodriguez",
    specialization: "Neurology",
    description: "Follow-up appointment for migraine treatment evaluation.",
    status: "upcoming",
    paymentStatus: "not_paid",
    total: 400,
  },
  {
    id: "4",
    date: "2024-07-10",
    time: "9:00 AM",
    doctor: "Emily Davis",
    specialization: "General Practice",
    description: "Annual health checkup and blood work review.",
    status: "past",
    paymentStatus: "paid",
    total: 180,
  },
  {
    id: "5",
    date: "2024-07-05",
    time: "2:30 PM",
    doctor: "David Wilson",
    specialization: "Orthopedics",
    description: "Physical therapy consultation for knee rehabilitation.",
    status: "past",
    paymentStatus: "paid",
    total: 220,
  },
  {
    id: "6",
    date: "2024-06-25",
    time: "3:30 PM",
    doctor: "Robert Wilson",
    specialization: "Orthopedics",
    description: "Knee pain evaluation and treatment plan discussion.",
    status: "cancelled",
    paymentStatus: "not_paid",
    total: 250,
  },
];

export const appointmentsApi = {
  /**
   * Fetch all appointments for the current user
   */
  getAppointments: async (): Promise<ApiResponse<Appointment[]>> => {
    try {
      // Simulate API delay
      return await createMockResponse(mockAppointments, 800);
    } catch (error) {
      return createErrorResponse("Failed to fetch appointments");
    }
  },

  /**
   * Fetch a specific appointment by ID
   */
  getAppointmentById: async (
    id: string
  ): Promise<ApiResponse<Appointment | null>> => {
    try {
      const appointment = mockAppointments.find((apt) => apt.id === id) || null;
      return await createMockResponse(appointment, 300);
    } catch (error) {
      return createErrorResponse("Failed to fetch appointment");
    }
  },

  /**
   * Update appointment payment status
   */
  updatePaymentStatus: async (
    id: string,
    paymentData: { paymentStatus: "paid" | "not_paid" }
  ): Promise<ApiResponse<Appointment>> => {
    try {
      const appointmentIndex = mockAppointments.findIndex(
        (apt) => apt.id === id
      );
      if (appointmentIndex === -1) {
        return createErrorResponse("Appointment not found");
      }

      // Update the appointment
      mockAppointments[appointmentIndex] = {
        ...mockAppointments[appointmentIndex],
        paymentStatus: paymentData.paymentStatus,
      };

      return await createMockResponse(mockAppointments[appointmentIndex], 500);
    } catch (error) {
      return createErrorResponse("Failed to update payment status");
    }
  },

  /**
   * Cancel an appointment
   */
  cancelAppointment: async (id: string): Promise<ApiResponse<Appointment>> => {
    try {
      const appointmentIndex = mockAppointments.findIndex(
        (apt) => apt.id === id
      );
      if (appointmentIndex === -1) {
        return createErrorResponse("Appointment not found");
      }

      // Update the appointment status
      mockAppointments[appointmentIndex] = {
        ...mockAppointments[appointmentIndex],
        status: "cancelled",
      };

      return await createMockResponse(mockAppointments[appointmentIndex], 400);
    } catch (error) {
      return createErrorResponse("Failed to cancel appointment");
    }
  },

  /**
   * Get upcoming appointments only
   */
  getUpcomingAppointments: async (): Promise<ApiResponse<Appointment[]>> => {
    try {
      const upcomingAppointments = mockAppointments.filter(
        (apt) => apt.status === "upcoming"
      );
      return await createMockResponse(upcomingAppointments, 600);
    } catch (error) {
      return createErrorResponse("Failed to fetch upcoming appointments");
    }
  },

  /**
   * Get past appointments only
   */
  getPastAppointments: async (): Promise<ApiResponse<Appointment[]>> => {
    try {
      const pastAppointments = mockAppointments.filter(
        (apt) => apt.status !== "upcoming"
      );
      return await createMockResponse(pastAppointments, 600);
    } catch (error) {
      return createErrorResponse("Failed to fetch past appointments");
    }
  },
};
