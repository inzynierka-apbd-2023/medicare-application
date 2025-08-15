import { apiClient as api } from "../../../shared/services/apiClient";
import type {
  AppointmentFilters,
  AppointmentStatus,
  CreateAppointmentRequest,
  Doctor,
  Patient,
  ReceptionistAppointment,
  Specialization,
  TimeSlot,
  UpdateAppointmentRequest,
} from "../types";

// Mock data for development
const mockPatients: Patient[] = [
  {
    id: "patient-1",
    firstName: "John",
    lastName: "Doe",
    email: "john.doe@email.com",
    phone: "+1 (555) 123-4567",
    dateOfBirth: "1985-03-15",
    medicalRecordNumber: "MRN001",
    bloodType: "O+",
  },
  {
    id: "patient-2",
    firstName: "Jane",
    lastName: "Smith",
    email: "jane.smith@email.com",
    phone: "+1 (555) 234-5678",
    dateOfBirth: "1990-07-22",
    medicalRecordNumber: "MRN002",
    bloodType: "A-",
  },
  {
    id: "patient-3",
    firstName: "Michael",
    lastName: "Johnson",
    email: "michael.johnson@email.com",
    phone: "+1 (555) 345-6789",
    dateOfBirth: "1978-11-08",
    medicalRecordNumber: "MRN003",
    bloodType: "B+",
  },
  {
    id: "patient-4",
    firstName: "Sarah",
    lastName: "Williams",
    email: "sarah.williams@email.com",
    phone: "+1 (555) 456-7890",
    dateOfBirth: "1992-05-03",
    medicalRecordNumber: "MRN004",
    bloodType: "AB+",
  },
  {
    id: "patient-5",
    firstName: "David",
    lastName: "Brown",
    email: "david.brown@email.com",
    phone: "+1 (555) 567-8901",
    dateOfBirth: "1988-09-17",
    medicalRecordNumber: "MRN005",
    bloodType: "O-",
  },
];

const mockSpecializations: Specialization[] = [
  {
    id: "spec-1",
    name: "Cardiology",
    description: "Heart and cardiovascular system",
  },
  {
    id: "spec-2",
    name: "Dermatology",
    description: "Skin conditions and diseases",
  },
  {
    id: "spec-3",
    name: "Internal Medicine",
    description: "General internal medicine",
  },
  {
    id: "spec-4",
    name: "Pediatrics",
    description: "Children's health and medicine",
  },
  {
    id: "spec-5",
    name: "Orthopedics",
    description: "Bones, joints, and muscles",
  },
];

const mockDoctors: Doctor[] = [
  {
    id: "doctor-1",
    firstName: "Emily",
    lastName: "Chen",
    email: "dr.chen@hospital.com",
    phone: "+1 (555) 111-2222",
    licenseNumber: "MD12345",
    yearsExperience: 12,
    specializations: [mockSpecializations[0]], // Cardiology
  },
  {
    id: "doctor-2",
    firstName: "Robert",
    lastName: "Martinez",
    email: "dr.martinez@hospital.com",
    phone: "+1 (555) 222-3333",
    licenseNumber: "MD23456",
    yearsExperience: 8,
    specializations: [mockSpecializations[1]], // Dermatology
  },
  {
    id: "doctor-3",
    firstName: "Lisa",
    lastName: "Thompson",
    email: "dr.thompson@hospital.com",
    phone: "+1 (555) 333-4444",
    licenseNumber: "MD34567",
    yearsExperience: 15,
    specializations: [mockSpecializations[2]], // Internal Medicine
  },
  {
    id: "doctor-4",
    firstName: "James",
    lastName: "Wilson",
    email: "dr.wilson@hospital.com",
    phone: "+1 (555) 444-5555",
    licenseNumber: "MD45678",
    yearsExperience: 10,
    specializations: [mockSpecializations[3]], // Pediatrics
  },
  {
    id: "doctor-5",
    firstName: "Maria",
    lastName: "Garcia",
    email: "dr.garcia@hospital.com",
    phone: "+1 (555) 555-6666",
    licenseNumber: "MD56789",
    yearsExperience: 7,
    specializations: [mockSpecializations[4]], // Orthopedics
  },
];

const mockAppointmentStatuses: AppointmentStatus[] = [
  {
    id: "status-1",
    name: "Scheduled",
    description: "Appointment is scheduled",
    colorCode: "#3B82F6",
  },
  {
    id: "status-2",
    name: "Confirmed",
    description: "Appointment is confirmed",
    colorCode: "#10B981",
  },
  {
    id: "status-3",
    name: "Completed",
    description: "Appointment was completed",
    colorCode: "#6B7280",
  },
  {
    id: "status-4",
    name: "Cancelled",
    description: "Appointment was cancelled",
    colorCode: "#EF4444",
  },
  {
    id: "status-5",
    name: "No-Show",
    description: "Patient did not show up",
    colorCode: "#F59E0B",
  },
  {
    id: "status-6",
    name: "Rescheduled",
    description: "Appointment was rescheduled",
    colorCode: "#8B5CF6",
  },
];

// Generate mock appointments for the next 30 days
const generateMockAppointments = (): ReceptionistAppointment[] => {
  const appointments: ReceptionistAppointment[] = [];
  const today = new Date();

  for (let i = 0; i < 30; i++) {
    const appointmentDate = new Date(today);
    appointmentDate.setDate(today.getDate() + i);

    // Skip weekends
    if (appointmentDate.getDay() === 0 || appointmentDate.getDay() === 6)
      continue;

    // Generate 3-8 appointments per day
    const numAppointments = Math.floor(Math.random() * 6) + 3;

    for (let j = 0; j < numAppointments; j++) {
      const hour = 8 + Math.floor(Math.random() * 9); // 8 AM to 5 PM
      const minute = Math.random() < 0.5 ? 0 : 30;
      const duration = Math.random() < 0.7 ? 30 : 60;

      const patient =
        mockPatients[Math.floor(Math.random() * mockPatients.length)];
      const doctor =
        mockDoctors[Math.floor(Math.random() * mockDoctors.length)];
      const status = mockAppointmentStatuses[Math.floor(Math.random() * 3)]; // Mostly scheduled/confirmed

      const appointmentTypes: Array<"in-person" | "video-call" | "phone"> = [
        "in-person",
        "video-call",
        "phone",
      ];
      const appointmentType =
        appointmentTypes[Math.floor(Math.random() * appointmentTypes.length)];

      const appointmentCategories: Array<
        | "consultation"
        | "emergency"
        | "follow-up"
        | "procedure"
        | "surgery"
        | "check-up"
        | "vaccination"
      > = ["consultation", "follow-up", "check-up", "procedure", "vaccination"];
      const appointmentCategory =
        appointmentCategories[
          Math.floor(Math.random() * appointmentCategories.length)
        ];

      const roomNumber =
        appointmentType === "in-person"
          ? `Room ${100 + Math.floor(Math.random() * 50)}`
          : undefined;

      appointments.push({
        id: `appointment-${i}-${j}`,
        patientId: patient.id,
        doctorId: doctor.id,
        day: appointmentDate.toISOString().split("T")[0],
        time: `${hour.toString().padStart(2, "0")}:${minute.toString().padStart(2, "0")}`,
        duration,
        appointmentType,
        appointmentCategory,
        statusId: status.id,
        ...(roomNumber && { room: roomNumber }),
        description: `${appointmentType === "video-call" ? "Video consultation" : appointmentType === "phone" ? "Phone consultation" : "Regular checkup"} with ${doctor.firstName} ${doctor.lastName}`,
        totalCost: Math.floor(Math.random() * 300) + 100,
        patient,
        doctor,
        status,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      });
    }
  }

  return appointments.sort((a, b) => {
    const dateTimeA = new Date(`${a.day}T${a.time}`);
    const dateTimeB = new Date(`${b.day}T${b.time}`);
    return dateTimeA.getTime() - dateTimeB.getTime();
  });
};

// Configuration flag to enable/disable mock mode
const USE_MOCK_DATA = true;

export class ReceptionistSchedulerApiService {
  private static appointments = generateMockAppointments();

  // Helper function to simulate API delay
  private static delay(ms: number = 500): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }

  // ===== APPOINTMENTS =====

  /**
   * Get all appointments with optional filters
   */
  static async getAppointments(
    filters?: AppointmentFilters
  ): Promise<ReceptionistAppointment[]> {
    await this.delay();

    if (USE_MOCK_DATA) {
      let filteredAppointments = [...this.appointments];

      if (filters) {
        if (filters.patientName && filters.patientName.trim()) {
          const searchTerm = filters.patientName.toLowerCase().trim();
          filteredAppointments = filteredAppointments.filter((apt) => {
            if (!apt.patient) return false;

            const fullName =
              `${apt.patient.firstName} ${apt.patient.lastName}`.toLowerCase();
            const firstName = apt.patient.firstName.toLowerCase();
            const lastName = apt.patient.lastName.toLowerCase();

            // Check if search term matches full name, first name, or last name
            return (
              fullName.includes(searchTerm) ||
              firstName.includes(searchTerm) ||
              lastName.includes(searchTerm)
            );
          });
        }

        if (filters.doctorId) {
          filteredAppointments = filteredAppointments.filter(
            (apt) => apt.doctorId === filters.doctorId
          );
        }

        if (filters.status) {
          filteredAppointments = filteredAppointments.filter(
            (apt) => apt.statusId === filters.status
          );
        }

        if (filters.appointmentType) {
          filteredAppointments = filteredAppointments.filter(
            (apt) => apt.appointmentType === filters.appointmentType
          );
        }

        if (filters.appointmentCategory) {
          filteredAppointments = filteredAppointments.filter(
            (apt) => apt.appointmentCategory === filters.appointmentCategory
          );
        }

        if (filters.dateRange) {
          filteredAppointments = filteredAppointments.filter(
            (apt) =>
              apt.day >= filters.dateRange!.start &&
              apt.day <= filters.dateRange!.end
          );
        }
      }

      return filteredAppointments;
    }

    try {
      const response = await api.get("/receptionist/appointments", {
        params: filters,
      });
      return response.data;
    } catch (error) {
      console.error("Error fetching appointments:", error);
      throw new Error("Failed to fetch appointments");
    }
  }

  /**
   * Create a new appointment
   */
  static async createAppointment(
    appointmentData: CreateAppointmentRequest
  ): Promise<ReceptionistAppointment> {
    await this.delay();

    if (USE_MOCK_DATA) {
      const patient = mockPatients.find(
        (p) => p.id === appointmentData.patientId
      );
      const doctor = mockDoctors.find((d) => d.id === appointmentData.doctorId);
      const status = mockAppointmentStatuses.find(
        (s) => s.name === "Scheduled"
      );

      if (!patient || !doctor || !status) {
        throw new Error("Patient, doctor, or status not found");
      }

      const newAppointment: ReceptionistAppointment = {
        id: `appointment-new-${Date.now()}`,
        ...appointmentData,
        statusId: status.id,
        totalCost: Math.floor(Math.random() * 300) + 100,
        patient,
        doctor,
        status,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      };

      this.appointments.push(newAppointment);
      return newAppointment;
    }

    try {
      const response = await api.post(
        "/receptionist/appointments",
        appointmentData
      );
      return response.data;
    } catch (error) {
      console.error("Error creating appointment:", error);
      throw new Error("Failed to create appointment");
    }
  }

  /**
   * Update an existing appointment
   */
  static async updateAppointment(
    appointmentData: UpdateAppointmentRequest
  ): Promise<ReceptionistAppointment> {
    await this.delay();

    if (USE_MOCK_DATA) {
      const appointmentIndex = this.appointments.findIndex(
        (apt) => apt.id === appointmentData.id
      );
      if (appointmentIndex === -1) {
        throw new Error("Appointment not found");
      }

      const updatedAppointment = {
        ...this.appointments[appointmentIndex],
        ...appointmentData,
        updatedAt: new Date().toISOString(),
      };

      // Update status if statusId changed
      if (appointmentData.statusId) {
        const status = mockAppointmentStatuses.find(
          (s) => s.id === appointmentData.statusId
        );
        if (status) {
          updatedAppointment.status = status;
        }
      }

      this.appointments[appointmentIndex] = updatedAppointment;
      return updatedAppointment;
    }

    try {
      const response = await api.put(
        `/receptionist/appointments/${appointmentData.id}`,
        appointmentData
      );
      return response.data;
    } catch (error) {
      console.error("Error updating appointment:", error);
      throw new Error("Failed to update appointment");
    }
  }

  /**
   * Cancel an appointment
   */
  static async cancelAppointment(appointmentId: string): Promise<void> {
    await this.delay();

    if (USE_MOCK_DATA) {
      const appointmentIndex = this.appointments.findIndex(
        (apt) => apt.id === appointmentId
      );
      if (appointmentIndex === -1) {
        throw new Error("Appointment not found");
      }

      const cancelledStatus = mockAppointmentStatuses.find(
        (s) => s.name === "Cancelled"
      );
      if (!cancelledStatus) {
        throw new Error("Cancelled status not found");
      }

      this.appointments[appointmentIndex] = {
        ...this.appointments[appointmentIndex],
        statusId: cancelledStatus.id,
        status: cancelledStatus,
        updatedAt: new Date().toISOString(),
      };

      return;
    }

    try {
      await api.delete(`/receptionist/appointments/${appointmentId}`);
    } catch (error) {
      console.error("Error cancelling appointment:", error);
      throw new Error("Failed to cancel appointment");
    }
  }

  // ===== PATIENTS =====

  /**
   * Search for patients
   */
  static async searchPatients(query: string): Promise<Patient[]> {
    await this.delay(200);

    if (USE_MOCK_DATA) {
      const searchTerm = query.toLowerCase();
      return mockPatients.filter(
        (patient) =>
          patient.firstName.toLowerCase().includes(searchTerm) ||
          patient.lastName.toLowerCase().includes(searchTerm) ||
          patient.email.toLowerCase().includes(searchTerm) ||
          patient.phone.includes(query) ||
          (patient.medicalRecordNumber &&
            patient.medicalRecordNumber.toLowerCase().includes(searchTerm))
      );
    }

    try {
      const response = await api.get("/patients/search", { params: { query } });
      return response.data;
    } catch (error) {
      console.error("Error searching patients:", error);
      throw new Error("Failed to search patients");
    }
  }

  /**
   * Get all patients
   */
  static async getPatients(): Promise<Patient[]> {
    await this.delay();

    if (USE_MOCK_DATA) {
      return mockPatients;
    }

    try {
      const response = await api.get("/patients");
      return response.data;
    } catch (error) {
      console.error("Error fetching patients:", error);
      throw new Error("Failed to fetch patients");
    }
  }

  // ===== DOCTORS =====

  /**
   * Get all doctors
   */
  static async getDoctors(): Promise<Doctor[]> {
    await this.delay();

    if (USE_MOCK_DATA) {
      return mockDoctors;
    }

    try {
      const response = await api.get("/doctors");
      return response.data;
    } catch (error) {
      console.error("Error fetching doctors:", error);
      throw new Error("Failed to fetch doctors");
    }
  }

  /**
   * Get available time slots for a doctor on a specific date
   */
  static async getDoctorAvailability(
    doctorId: string,
    date: string
  ): Promise<TimeSlot[]> {
    await this.delay();

    if (USE_MOCK_DATA) {
      // Generate mock time slots from 8 AM to 5 PM in 15-minute intervals
      const timeSlots: TimeSlot[] = [];
      const selectedDate = new Date(date);

      for (let hour = 8; hour < 17; hour++) {
        for (let minute = 0; minute < 60; minute += 15) {
          // Changed from 30 to 15 minutes
          const startDateTime = new Date(selectedDate);
          startDateTime.setHours(hour, minute, 0, 0);

          const endDateTime = new Date(startDateTime);
          endDateTime.setMinutes(endDateTime.getMinutes() + 15); // Changed from 30 to 15 minutes

          // Check if this slot conflicts with existing appointments
          const slotStart = startDateTime.getTime();
          const slotEnd = endDateTime.getTime();

          const isBooked = this.appointments.some((apt) => {
            if (
              apt.doctorId !== doctorId ||
              apt.day !== date ||
              apt.statusId === "status-4"
            ) {
              return false; // Different doctor, date, or cancelled appointment
            }

            // Parse appointment time and calculate its end time
            const [aptHour, aptMinute] = apt.time.split(":").map(Number);
            const aptStart = new Date(selectedDate);
            aptStart.setHours(aptHour, aptMinute, 0, 0);
            const aptEnd = new Date(
              aptStart.getTime() + apt.duration * 60 * 1000
            );

            const aptStartTime = aptStart.getTime();
            const aptEndTime = aptEnd.getTime();

            // Check if there's any overlap between the slot and the appointment
            return slotStart < aptEndTime && slotEnd > aptStartTime;
          });

          timeSlots.push({
            id: `slot-${doctorId}-${date}-${hour}-${minute}`,
            doctorId,
            startDateTime: startDateTime.toISOString(),
            endDateTime: endDateTime.toISOString(),
            isAvailable: !isBooked,
            durationMinutes: 15, // Changed from 30 to 15 minutes
            slotType: "Regular",
          });
        }
      }

      return timeSlots;
    }

    try {
      const response = await api.get(`/doctors/${doctorId}/availability`, {
        params: { date },
      });
      return response.data;
    } catch (error) {
      console.error("Error fetching doctor availability:", error);
      throw new Error("Failed to fetch doctor availability");
    }
  }

  // ===== STATUSES =====

  /**
   * Get all appointment statuses
   */
  static async getAppointmentStatuses(): Promise<AppointmentStatus[]> {
    await this.delay();

    if (USE_MOCK_DATA) {
      return mockAppointmentStatuses;
    }

    try {
      const response = await api.get("/appointment-statuses");
      return response.data;
    } catch (error) {
      console.error("Error fetching appointment statuses:", error);
      throw new Error("Failed to fetch appointment statuses");
    }
  }

  // ===== SPECIALIZATIONS =====

  /**
   * Get all specializations
   */
  static async getSpecializations(): Promise<Specialization[]> {
    await this.delay();

    if (USE_MOCK_DATA) {
      return mockSpecializations;
    }

    try {
      const response = await api.get("/specializations");
      return response.data;
    } catch (error) {
      console.error("Error fetching specializations:", error);
      throw new Error("Failed to fetch specializations");
    }
  }
}
