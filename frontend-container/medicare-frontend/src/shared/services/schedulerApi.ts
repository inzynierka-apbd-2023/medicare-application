import type {
  Service,
  Specialization,
  Doctor,
  TimeSlot,
  CalendarEvent,
  AppointmentBooking,
} from "../../features/scheduler/types";
import {
  createMockResponse,
  createErrorResponse,
  type ApiResponse,
} from "./api";

// Mock data
const mockServices: Service[] = [
  {
    id: "service_general_consultation",
    name: "General Consultation",
    specializationId: "spec_general_medicine",
    doctorIds: ["doc_alice_heart", "doc_bob_vessel"],
    duration: 30,
    description: "Comprehensive health check-up and consultation",
  },
  {
    id: "service_blood_test",
    name: "Blood Test",
    specializationId: "spec_lab_services",
    doctorIds: ["doc_carol_serum"],
    duration: 15,
    description: "Laboratory blood analysis",
  },
  {
    id: "service_teleconsultation",
    name: "Teleconsultation",
    specializationId: "spec_general_medicine",
    doctorIds: ["doc_alice_heart"],
    duration: 20,
    description: "Remote consultation via video call",
  },
  {
    id: "service_prescription_renewal",
    name: "Prescription Renewal",
    specializationId: "spec_pharmacy",
    doctorIds: ["doc_deborah_dose"],
    duration: 10,
    description: "Renew existing prescriptions",
  },
];

const mockSpecializations: Specialization[] = [
  {
    id: "spec_general_medicine",
    name: "General Medicine",
    serviceIds: ["service_general_consultation", "service_teleconsultation"],
    doctorIds: ["doc_alice_heart", "doc_bob_vessel"],
    description: "Primary healthcare and general medical services",
  },
  {
    id: "spec_lab_services",
    name: "Lab Services",
    serviceIds: ["service_blood_test"],
    doctorIds: ["doc_carol_serum"],
    description: "Laboratory testing and analysis",
  },
  {
    id: "spec_pharmacy",
    name: "Pharmacy",
    serviceIds: ["service_prescription_renewal"],
    doctorIds: ["doc_deborah_dose"],
    description: "Pharmaceutical services and medication management",
  },
];

const mockDoctors: Doctor[] = [
  {
    id: "doc_alice_heart",
    name: "Dr. Alice Heart",
    specializationIds: ["spec_general_medicine"],
    email: "alice.heart@medicare.com",
    phone: "+1-555-0101",
  },
  {
    id: "doc_bob_vessel",
    name: "Dr. Bob Vessel",
    specializationIds: ["spec_general_medicine"],
    email: "bob.vessel@medicare.com",
    phone: "+1-555-0102",
  },
  {
    id: "doc_carol_serum",
    name: "Dr. Carol Serum",
    specializationIds: ["spec_lab_services"],
    email: "carol.serum@medicare.com",
    phone: "+1-555-0103",
  },
  {
    id: "doc_deborah_dose",
    name: "Dr. Deborah Dose",
    specializationIds: ["spec_pharmacy"],
    email: "deborah.dose@medicare.com",
    phone: "+1-555-0104",
  },
];

// Generate mock time slots for the next 7 days
const generateMockTimeSlots = (): TimeSlot[] => {
  const slots: TimeSlot[] = [];
  const startDate = new Date();
  startDate.setHours(9, 0, 0, 0); // Start at 9 AM

  for (let day = 0; day < 7; day++) {
    const currentDate = new Date(startDate);
    currentDate.setDate(startDate.getDate() + day);

    // Skip weekends for now
    if (currentDate.getDay() === 0 || currentDate.getDay() === 6) continue;

    // Generate slots from 9 AM to 5 PM
    for (let hour = 9; hour < 17; hour++) {
      for (let minute = 0; minute < 60; minute += 30) {
        const slotStart = new Date(currentDate);
        slotStart.setHours(hour, minute, 0, 0);

        const slotEnd = new Date(slotStart);
        slotEnd.setMinutes(slotStart.getMinutes() + 30);

        // Randomly make some slots unavailable
        const isAvailable = Math.random() > 0.3;

        // Assign to random doctors
        const doctorId =
          mockDoctors[Math.floor(Math.random() * mockDoctors.length)].id;

        slots.push({
          id: `slot_${slotStart.getTime()}_${doctorId}`,
          start: slotStart,
          end: slotEnd,
          isAvailable,
          doctorId,
        });
      }
    }
  }

  return slots;
};

const mockTimeSlots = generateMockTimeSlots();

// API functions
export const schedulerApi = {
  // Get all services
  async getServices(): Promise<ApiResponse<Service[]>> {
    await new Promise((resolve) => setTimeout(resolve, 300));
    return createMockResponse(mockServices);
  },

  // Get all specializations
  async getSpecializations(): Promise<ApiResponse<Specialization[]>> {
    await new Promise((resolve) => setTimeout(resolve, 300));
    return createMockResponse(mockSpecializations);
  },

  // Get all doctors
  async getDoctors(): Promise<ApiResponse<Doctor[]>> {
    await new Promise((resolve) => setTimeout(resolve, 300));
    return createMockResponse(mockDoctors);
  },

  // Get available time slots for a specific doctor
  async getTimeSlots(doctorId?: string): Promise<ApiResponse<TimeSlot[]>> {
    await new Promise((resolve) => setTimeout(resolve, 500));

    if (doctorId) {
      const filteredSlots = mockTimeSlots.filter(
        (slot) => slot.doctorId === doctorId
      );
      return createMockResponse(filteredSlots);
    }

    return createMockResponse(mockTimeSlots);
  },

  // Get calendar events (for future Microsoft Graph integration)
  async getEvents(doctorId?: string): Promise<ApiResponse<CalendarEvent[]>> {
    await new Promise((resolve) => setTimeout(resolve, 400));

    // For now, return empty array - will be implemented with Microsoft Graph
    return createMockResponse([]);
  },

  // Book an appointment
  async bookAppointment(
    booking: AppointmentBooking
  ): Promise<ApiResponse<{ appointmentId: string; message: string }>> {
    await new Promise((resolve) => setTimeout(resolve, 800));

    // Simulate booking logic
    if (!booking.timeSlot.isAvailable) {
      return createErrorResponse("Selected time slot is no longer available");
    }

    const appointmentId = `apt_${Date.now()}_${booking.doctorId}`;

    return createMockResponse({
      appointmentId,
      message: "Appointment booked successfully",
    });
  },

  // Cancel an appointment
  async cancelAppointment(
    appointmentId: string
  ): Promise<ApiResponse<{ message: string }>> {
    await new Promise((resolve) => setTimeout(resolve, 600));

    return createMockResponse({
      message: "Appointment cancelled successfully",
    });
  },
};
