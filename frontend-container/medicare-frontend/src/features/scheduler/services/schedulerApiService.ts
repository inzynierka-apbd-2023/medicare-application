import { apiClient as api } from "../../../shared/services/apiClient";
import type {
  Appointment,
  AppointmentStatus,
  AvailableSlotsRequest,
  CreateAppointmentRequest,
  Doctor,
  DoctorSchedule,
  Patient,
  Service,
  Specialization,
  TimeSlot,
  UpdateAppointmentRequest,
} from "../types";

import {
  mockAppointments,
  mockAppointmentStatuses,
  mockCurrentPatient,
  mockDoctors,
  mockDoctorSchedules,
  mockServices,
  mockSpecializations,
  mockTimeSlots,
} from "./mockData";

// Configuration flag to enable/disable mock mode
const USE_MOCK_DATA = true; // Set to false when connecting to real backend

// API_BASE_URL imported from shared client; api already configured with auth & error handling.

export class SchedulerApiService {
  // Mock data storage for simulating state changes
  private static appointments = [...mockAppointments];
  private static timeSlots = [...mockTimeSlots];

  // Helper function to simulate API delay
  private static delay(ms: number = 500): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }

  // ===== DOCTOR APPOINTMENTS =====

  /**
   * Get all appointments for a specific doctor
   */
  static async getDoctorAppointments(doctorId: string): Promise<Appointment[]> {
    await this.delay();

    if (USE_MOCK_DATA) {
      // Filter appointments for the specific doctor and add populated fields
      const doctorAppointments = this.appointments
        .filter((apt) => apt.doctorUserId === doctorId)
        .map((appointment) => {
          const result: Appointment = {
            ...appointment,
          };

          const patient = mockCurrentPatient; // In real app, fetch patient data
          const status = mockAppointmentStatuses.find(
            (s) => s.id === appointment.statusId
          );

          if (patient) result.patient = patient;
          if (status) result.status = status;

          return result;
        });

      return doctorAppointments;
    }

    try {
      const response = await api.get(`/doctors/${doctorId}/appointments`);
      return response.data;
    } catch (error) {
      console.error("Error fetching doctor appointments:", error);
      throw new Error("Failed to fetch doctor appointments");
    }
  }

  /**
   * Update appointment status
   */
  static async updateAppointmentStatus(
    appointmentId: string,
    statusId: string
  ): Promise<Appointment> {
    await this.delay(300);

    if (USE_MOCK_DATA) {
      const appointmentIndex = this.appointments.findIndex(
        (apt) => apt.id === appointmentId
      );
      if (appointmentIndex === -1) {
        throw new Error("Appointment not found");
      }

      this.appointments[appointmentIndex] = {
        ...this.appointments[appointmentIndex],
        statusId,
        updatedAt: new Date().toISOString(),
      };

      const updatedAppointment = this.appointments[appointmentIndex];
      const status = mockAppointmentStatuses.find((s) => s.id === statusId);

      if (!status) {
        throw new Error("Status not found");
      }

      return {
        ...updatedAppointment,
        status,
      };
    }

    try {
      const response = await api.patch(`/appointments/${appointmentId}`, {
        statusId,
      });
      return response.data;
    } catch (error) {
      console.error("Error updating appointment status:", error);
      throw new Error("Failed to update appointment status");
    }
  }

  /**
   * Start virtual consultation (readonly - provides information about external app)
   */
  static getVirtualConsultationInfo(appointmentId: string): string {
    return `Video call for appointment ${appointmentId} should be started in external application (e.g., Microsoft Teams, Zoom, or your organization's preferred video platform)`;
  }

  /**
   * Get appointment statistics for a specific date
   */
  static getAppointmentStats(appointments: Appointment[], date: string) {
    const dateString = date.split("T")[0];
    const dayAppointments = appointments.filter((apt) =>
      apt.day.startsWith(dateString)
    );

    return {
      total: dayAppointments.length,
      completed: dayAppointments.filter(
        (apt) => apt.status?.name === "completed"
      ).length,
      pending: dayAppointments.filter(
        (apt) =>
          apt.status?.name === "confirmed" || apt.status?.name === "pending"
      ).length,
      cancelled: dayAppointments.filter(
        (apt) => apt.status?.name === "cancelled"
      ).length,
      noShow: dayAppointments.filter((apt) => apt.status?.name === "no-show")
        .length,
    };
  }

  // ===== APPOINTMENTS =====

  /**
   * Get all appointments for the current patient
   */
  static async getPatientAppointments(
    patientId: string
  ): Promise<Appointment[]> {
    await this.delay();

    if (USE_MOCK_DATA) {
      // Filter appointments for the specific patient and add populated fields
      const patientAppointments = this.appointments
        .filter((apt) => apt.patientUserId === patientId)
        .map((appointment) => {
          const result: Appointment = {
            ...appointment,
          };

          const doctor = mockDoctors.find(
            (d) => d.id === appointment.doctorUserId
          );
          const status = mockAppointmentStatuses.find(
            (s) => s.id === appointment.statusId
          );

          if (doctor) result.doctor = doctor;
          if (status) result.status = status;

          return result;
        });

      return patientAppointments;
    }

    try {
      const response = await api.get(`/patients/${patientId}/appointments`);
      return response.data;
    } catch (error) {
      console.error("Error fetching patient appointments:", error);
      throw new Error("Failed to fetch appointments");
    }
  }

  /**
   * Get appointments within a date range
   */
  static async getAppointmentsByDateRange(
    patientId: string,
    startDate: string,
    endDate: string
  ): Promise<Appointment[]> {
    await this.delay();

    if (USE_MOCK_DATA) {
      const start = new Date(startDate);
      const end = new Date(endDate);

      const filteredAppointments = this.appointments
        .filter((apt) => {
          const aptDate = new Date(apt.day);
          return (
            apt.patientUserId === patientId &&
            aptDate >= start &&
            aptDate <= end
          );
        })
        .map((appointment) => {
          const result: Appointment = {
            ...appointment,
          };

          const doctor = mockDoctors.find(
            (d) => d.id === appointment.doctorUserId
          );
          const status = mockAppointmentStatuses.find(
            (s) => s.id === appointment.statusId
          );

          if (doctor) result.doctor = doctor;
          if (status) result.status = status;

          return result;
        });

      return filteredAppointments;
    }

    try {
      const response = await api.get(`/patients/${patientId}/appointments`, {
        params: { startDate, endDate },
      });
      return response.data;
    } catch (error) {
      console.error("Error fetching appointments by date range:", error);
      throw new Error(
        "Failed to fetch appointments for the specified date range"
      );
    }
  }

  /**
   * Create a new appointment
   */
  static async createAppointment(
    patientId: string,
    appointmentData: CreateAppointmentRequest
  ): Promise<Appointment> {
    await this.delay();

    if (USE_MOCK_DATA) {
      // Find the selected time slot
      const timeSlot = this.timeSlots.find(
        (slot) => slot.id === appointmentData.timeSlotId
      );
      if (!timeSlot) {
        throw new Error("Selected time slot not found");
      }

      // Create new appointment
      const newAppointment: Appointment = {
        id: `appointment-new-${Date.now()}`,
        scheduleId: `schedule-${Date.now()}`,
        timeSlotId: appointmentData.timeSlotId,
        day: timeSlot.startDateTime,
        durationMinutes: timeSlot.durationMinutes,
        description: appointmentData.description || "",
        appointmentType: appointmentData.appointmentType,
        doctorUserId: appointmentData.doctorUserId,
        patientUserId: patientId,
        statusId: "status-1", // Scheduled
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      };

      // Add to mock storage
      this.appointments.push(newAppointment);

      // Mark time slot as unavailable
      const slotIndex = this.timeSlots.findIndex(
        (slot) => slot.id === appointmentData.timeSlotId
      );
      if (slotIndex !== -1) {
        this.timeSlots[slotIndex] = {
          ...this.timeSlots[slotIndex],
          isAvailable: false,
        };
      }

      // Return with populated fields
      const result: Appointment = {
        ...newAppointment,
      };

      const doctor = mockDoctors.find(
        (d) => d.id === newAppointment.doctorUserId
      );
      const status = mockAppointmentStatuses.find(
        (s) => s.id === newAppointment.statusId
      );

      if (doctor) result.doctor = doctor;
      if (status) result.status = status;

      return result;
    }

    try {
      const response = await api.post(
        `/patients/${patientId}/appointments`,
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
    appointmentId: string,
    appointmentData: UpdateAppointmentRequest
  ): Promise<Appointment> {
    await this.delay();

    if (USE_MOCK_DATA) {
      const appointmentIndex = this.appointments.findIndex(
        (apt) => apt.id === appointmentId
      );
      if (appointmentIndex === -1) {
        throw new Error("Appointment not found");
      }

      // Update appointment
      const updatedAppointment: Appointment = {
        ...this.appointments[appointmentIndex],
        ...appointmentData,
        updatedAt: new Date().toISOString(),
      };

      this.appointments[appointmentIndex] = updatedAppointment;

      // Return with populated fields
      const result: Appointment = {
        ...updatedAppointment,
      };

      const doctor = mockDoctors.find(
        (d) => d.id === updatedAppointment.doctorUserId
      );
      const status = mockAppointmentStatuses.find(
        (s) => s.id === updatedAppointment.statusId
      );

      if (doctor) result.doctor = doctor;
      if (status) result.status = status;

      return result;
    }

    try {
      const response = await api.put(
        `/appointments/${appointmentId}`,
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
      if (appointmentIndex !== -1) {
        // Update status to cancelled
        this.appointments[appointmentIndex] = {
          ...this.appointments[appointmentIndex],
          statusId: "status-3", // Cancelled
          updatedAt: new Date().toISOString(),
        };

        // Mark time slot as available again
        const timeSlotId = this.appointments[appointmentIndex].timeSlotId;
        if (timeSlotId) {
          const slotIndex = this.timeSlots.findIndex(
            (slot) => slot.id === timeSlotId
          );
          if (slotIndex !== -1) {
            this.timeSlots[slotIndex] = {
              ...this.timeSlots[slotIndex],
              isAvailable: true,
            };
          }
        }
      }
      return;
    }

    try {
      await api.patch(`/appointments/${appointmentId}/cancel`);
    } catch (error) {
      console.error("Error cancelling appointment:", error);
      throw new Error("Failed to cancel appointment");
    }
  }

  /**
   * Get appointment details by ID
   */
  static async getAppointmentById(appointmentId: string): Promise<Appointment> {
    await this.delay();

    if (USE_MOCK_DATA) {
      const appointment = this.appointments.find(
        (apt) => apt.id === appointmentId
      );
      if (!appointment) {
        throw new Error("Appointment not found");
      }

      const result: Appointment = {
        ...appointment,
      };

      const doctor = mockDoctors.find((d) => d.id === appointment.doctorUserId);
      const status = mockAppointmentStatuses.find(
        (s) => s.id === appointment.statusId
      );

      if (doctor) result.doctor = doctor;
      if (status) result.status = status;

      return result;
    }

    try {
      const response = await api.get(`/appointments/${appointmentId}`);
      return response.data;
    } catch (error) {
      console.error("Error fetching appointment details:", error);
      throw new Error("Failed to fetch appointment details");
    }
  }

  // ===== DOCTORS =====

  /**
   * Get all available doctors
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
   * Get doctors by specialization
   */
  static async getDoctorsBySpecialization(
    specializationId: string
  ): Promise<Doctor[]> {
    await this.delay();

    if (USE_MOCK_DATA) {
      return mockDoctors.filter((doctor) =>
        doctor.specializations.some((spec) => spec.id === specializationId)
      );
    }

    try {
      const response = await api.get(
        `/doctors/specialization/${specializationId}`
      );
      return response.data;
    } catch (error) {
      console.error("Error fetching doctors by specialization:", error);
      throw new Error(
        "Failed to fetch doctors for the specified specialization"
      );
    }
  }

  /**
   * Get doctor details by ID
   */
  static async getDoctorById(doctorId: string): Promise<Doctor> {
    await this.delay();

    if (USE_MOCK_DATA) {
      const doctor = mockDoctors.find((d) => d.id === doctorId);
      if (!doctor) {
        throw new Error("Doctor not found");
      }
      return doctor;
    }

    try {
      const response = await api.get(`/doctors/${doctorId}`);
      return response.data;
    } catch (error) {
      console.error("Error fetching doctor details:", error);
      throw new Error("Failed to fetch doctor details");
    }
  }

  // ===== SERVICES =====

  /**
   * Get all available services
   */
  static async getServices(): Promise<Service[]> {
    await this.delay();

    if (USE_MOCK_DATA) {
      return mockServices;
    }

    try {
      const response = await api.get("/services");
      return response.data;
    } catch (error) {
      console.error("Error fetching services:", error);
      throw new Error("Failed to fetch services");
    }
  }

  /**
   * Get services by specialization
   */
  static async getServicesBySpecialization(
    specializationId: string
  ): Promise<Service[]> {
    await this.delay();

    if (USE_MOCK_DATA) {
      // Services are connected through specializations
      const specialization = mockSpecializations.find(
        (spec) => spec.id === specializationId
      );
      if (specialization) {
        return [specialization.service];
      }
      return [];
    }

    try {
      const response = await api.get(
        `/services/specialization/${specializationId}`
      );
      return response.data;
    } catch (error) {
      console.error("Error fetching services by specialization:", error);
      throw new Error(
        "Failed to fetch services for the specified specialization"
      );
    }
  }

  // ===== SPECIALIZATIONS =====

  /**
   * Get all available specializations
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

  // ===== TIME SLOTS =====

  /**
   * Get available time slots for a doctor and service
   */
  static async getAvailableTimeSlots(
    request: AvailableSlotsRequest
  ): Promise<TimeSlot[]> {
    await this.delay();

    if (USE_MOCK_DATA) {
      const startDate = new Date(request.startDate);
      const endDate = new Date(request.endDate);

      // Set time to start and end of day for proper comparison
      startDate.setHours(0, 0, 0, 0);
      endDate.setHours(23, 59, 59, 999);

      return this.timeSlots.filter((slot) => {
        const slotDate = new Date(slot.startDateTime);
        return (
          slot.doctorId === request.doctorId &&
          slotDate >= startDate &&
          slotDate <= endDate &&
          slot.isAvailable
        );
      });
    }

    try {
      const response = await api.get("/time-slots/available", {
        params: request,
      });
      return response.data;
    } catch (error) {
      console.error("Error fetching available time slots:", error);
      throw new Error("Failed to fetch available time slots");
    }
  }

  /**
   * Get doctor's schedule
   */
  static async getDoctorSchedule(doctorId: string): Promise<DoctorSchedule[]> {
    await this.delay();

    if (USE_MOCK_DATA) {
      return mockDoctorSchedules.filter(
        (schedule) => schedule.doctorId === doctorId
      );
    }

    try {
      const response = await api.get(`/doctors/${doctorId}/schedule`);
      return response.data;
    } catch (error) {
      console.error("Error fetching doctor schedule:", error);
      throw new Error("Failed to fetch doctor schedule");
    }
  }

  // ===== APPOINTMENT STATUSES =====

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

  // ===== PATIENT INFO =====

  /**
   * Get current patient info
   */
  static async getCurrentPatient(): Promise<Patient> {
    await this.delay();

    if (USE_MOCK_DATA) {
      return mockCurrentPatient;
    }

    try {
      const response = await api.get("/patients/current");
      return response.data;
    } catch (error) {
      console.error("Error fetching current patient:", error);
      throw new Error("Failed to fetch patient information");
    }
  }

  /**
   * Get patient by ID
   */
  static async getPatientById(patientId: string): Promise<Patient> {
    await this.delay();

    if (USE_MOCK_DATA) {
      if (patientId === mockCurrentPatient.id) {
        return mockCurrentPatient;
      }
      throw new Error("Patient not found");
    }

    try {
      const response = await api.get(`/patients/${patientId}`);
      return response.data;
    } catch (error) {
      console.error("Error fetching patient:", error);
      throw new Error("Failed to fetch patient information");
    }
  }

  // ===== DASHBOARD STATS =====

  /**
   * Get dashboard statistics for the logged-in patient
   */
  static async getDashboardStats(): Promise<{
    upcomingAppointments: number;
    pastAppointments: number;
    totalAppointments: number;
    thisMonthAppointments: number;
    pendingAppointments: number;
    cancelledAppointments: number;
  }> {
    await this.delay();

    if (USE_MOCK_DATA) {
      const now = new Date();
      const today = now.toISOString().split("T")[0];
      const thisMonth = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}`;

      // Calculate stats from mock appointments
      const upcomingAppointments = this.appointments.filter(
        (apt) => apt.day >= today && apt.statusId === "status-1"
      ).length;

      const pastAppointments = this.appointments.filter(
        (apt) => apt.day < today
      ).length;

      const thisMonthAppointments = this.appointments.filter((apt) =>
        apt.day.startsWith(thisMonth)
      ).length;

      const pendingAppointments = this.appointments.filter(
        (apt) => apt.statusId === "status-2"
      ).length;

      return {
        upcomingAppointments,
        pastAppointments,
        totalAppointments: this.appointments.length,
        thisMonthAppointments,
        pendingAppointments,
        cancelledAppointments: this.appointments.filter(
          (apt) => apt.statusId === "status-3"
        ).length,
      };
    }

    try {
      const response = await api.get("/dashboard/stats");
      return response.data;
    } catch (error) {
      console.error("Error fetching dashboard stats:", error);
      throw new Error("Failed to fetch dashboard stats");
    }
  }

  // ===== UTILITY METHODS =====

  /**
   * Check if a time slot is still available
   */
  static async isTimeSlotAvailable(timeSlotId: string): Promise<boolean> {
    await this.delay();

    if (USE_MOCK_DATA) {
      const slot = this.timeSlots.find((slot) => slot.id === timeSlotId);
      return slot?.isAvailable ?? false;
    }

    try {
      const response = await api.get(`/time-slots/${timeSlotId}/availability`);
      return response.data.isAvailable;
    } catch (error) {
      console.error("Error checking time slot availability:", error);
      return false;
    }
  }

  /**
   * Get appointment conflicts for rescheduling
   */
  static async getAppointmentConflicts(
    timeSlotId: string,
    excludeAppointmentId?: string
  ): Promise<Appointment[]> {
    await this.delay();

    if (USE_MOCK_DATA) {
      const conflicts = this.appointments.filter(
        (apt) =>
          apt.timeSlotId === timeSlotId &&
          apt.id !== excludeAppointmentId &&
          apt.statusId !== "status-3" // Not cancelled
      );

      // Return with populated fields
      return conflicts.map((apt) => {
        const result: Appointment = { ...apt };
        const doctor = mockDoctors.find((d) => d.id === apt.doctorUserId);
        const status = mockAppointmentStatuses.find(
          (s) => s.id === apt.statusId
        );

        if (doctor) result.doctor = doctor;
        if (status) result.status = status;

        return result;
      });
    }

    try {
      const response = await api.get(`/time-slots/${timeSlotId}/conflicts`, {
        params: { excludeAppointmentId },
      });
      return response.data;
    } catch (error) {
      console.error("Error checking appointment conflicts:", error);
      throw new Error("Failed to check appointment conflicts");
    }
  }
}

export default SchedulerApiService;
