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
const USE_MOCK_DATA = true; // Keep mock by default for catalogs
// Prefer real backend just for appointments flows
const USE_REAL_APPOINTMENTS = true;
// When using real appointments, also source doctors from PractitionerService
const USE_REAL_DOCTORS = true;
// When using real appointments but mock doctors, map to a real test doctor GUID
const TEST_DOCTOR_ID: string = (import.meta as any).env?.VITE_TEST_DOCTOR_ID ||
  "5a576dc0-cf45-4868-9112-9ae245461020"; // fallback known test doctor id

const isGuid = (v: string): boolean =>
  /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$/.test(
    v
  );

// API_BASE_URL imported from shared client; api already configured with auth & error handling.

export class SchedulerApiService {
  // Mock data storage for simulating state changes
  private static appointments = [...mockAppointments];
  private static timeSlots = [...mockTimeSlots];

  // Map backend AppointmentService entity to UI Appointment shape
  private static mapBackendAppointmentToUi(backend: any): Appointment {
    const start = new Date(backend.scheduledAt);
    const end = new Date(backend.scheduledEndAt ?? backend.scheduledAt);
    const durationMinutes = Math.max(15, Math.round((end.getTime() - start.getTime()) / 60000) || 30);

    const status = mockAppointmentStatuses.find(
      (s) => s.name.toLowerCase() === String(backend.status || "Scheduled").toLowerCase()
    ) || mockAppointmentStatuses[0];

    // Default to a general service
    const service = mockServices[2] || mockServices[0];

    const ui: Appointment = {
      id: String(backend.id),
      patientId: String(backend.patientId),
      patient: {
        id: String(backend.patientId),
        userId: String(backend.patientId),
        firstName: "",
        lastName: "",
        email: "",
        phone: "",
        dateOfBirth: new Date(0).toISOString(),
      } as any,
      doctorUserId: String(backend.doctorId) as any,
      // Minimal associations; detailed doctor/timeSlot may be populated elsewhere
      doctor: undefined as any,
      serviceId: service.id,
      service: service as any,
      timeSlotId: "",
      timeSlot: undefined as any,
      day: new Date(start).toISOString(),
      durationMinutes,
      appointmentType: (backend.appointmentType) === "virtual" || (backend.appointmentType) === "phone"
        ? backend.appointmentType
        : ("in-person" as const),
      appointmentCategory: undefined as unknown as string,
      description: backend.notes || "",
      statusId: status.id,
      status: status as any,
      createdAt: new Date(backend.createdAt || backend.scheduledAt).toISOString(),
      updatedAt: new Date(backend.updatedAt || backend.scheduledAt).toISOString(),
    };

    return ui;
  }

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

  if (USE_MOCK_DATA && !USE_REAL_APPOINTMENTS) {
      // Filter appointments for the specific patient and add populated fields
      const patientAppointments = this.appointments
    .filter((apt) => (apt as any).patientUserId === patientId)
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
      // Map to AppointmentService route via Nginx: /api/appointment/appointments/patient/{patientId}
  const response = await api.get(`/appointment/appointments/patient/${patientId}`);
  const items = Array.isArray(response.data) ? response.data : [];
  return items.map((a: any) => this.mapBackendAppointmentToUi(a));
    } catch (error) {
      // Be resilient: log and return empty list to avoid blocking the UX
      console.error("Error fetching patient appointments:", error);
      return [];
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

  if (USE_MOCK_DATA && !USE_REAL_APPOINTMENTS) {
      const start = new Date(startDate);
      const end = new Date(endDate);

      const filteredAppointments = this.appointments
        .filter((apt) => {
          const aptDate = new Date(apt.day);
          return (
      (apt as any).patientUserId === patientId &&
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

  if (USE_MOCK_DATA && !USE_REAL_APPOINTMENTS) {
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
        timeSlotId: appointmentData.timeSlotId,
        day: timeSlot.startDateTime,
        durationMinutes: timeSlot.durationMinutes,
        description: appointmentData.description || "",
        appointmentType: appointmentData.appointmentType,
        doctorUserId: appointmentData.doctorUserId,
    // @ts-expect-error mock field not in type
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
      // Translate to AppointmentService DTO
      // Determine doctor guid
      const doctorGuid = isGuid(appointmentData.doctorUserId)
        ? appointmentData.doctorUserId
        : TEST_DOCTOR_ID;
      // If a timeSlotId was selected and exists in local cache, honor its window; else next half-hour
      let start: Date;
      let end: Date;
      const selectedSlot = this.timeSlots.find((s) => s.id === appointmentData.timeSlotId);
      if (selectedSlot) {
        start = new Date(selectedSlot.startDateTime);
        end = new Date(selectedSlot.endDateTime);
      } else {
        const now = new Date();
        start = new Date(now);
        const minutes = now.getMinutes();
        const add = minutes === 0 || minutes <= 30 ? 30 - (minutes % 30 || 30) : 60 - (minutes % 30);
        start.setMinutes(minutes + add, 0, 0);
        end = new Date(start.getTime() + 30 * 60000);
      }

      const payload = {
        patientId,
        doctorId: doctorGuid,
        scheduledAt: start.toISOString(),
        scheduledEndAt: end.toISOString(),
        appointmentType: appointmentData.appointmentType,
        notes: appointmentData.description,
      };
  const response = await api.post(`/appointment/appointments`, payload);
  return this.mapBackendAppointmentToUi(response.data);
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

    if (USE_MOCK_DATA && !USE_REAL_APPOINTMENTS) {
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
      // Real backend: Update status via AppointmentService endpoint
      await api.put(`/appointment/appointments/${appointmentId}/status`, {
        status: "Cancelled",
      });
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

    if (USE_MOCK_DATA && !USE_REAL_DOCTORS) {
      return mockDoctors;
    }
    try {
      // PractitionerService: Doctor directory search
      const response = await api.get("/practitioner/doctors");
      const rows = Array.isArray(response.data) ? response.data : [];
      // Map DoctorDirectory -> UI Doctor
      const mapped: Doctor[] = rows.map((d: any) => {
        const specIdsCsv = String(d.specializations ?? d.Specializations ?? "");
        const specIds = specIdsCsv
          .split(",")
          .map((s: string) => s.trim())
          .filter((s: string) => s.length > 0);
        const specializations = specIds.map((id: string) => ({
          id,
          name: "",
          description: "",
          serviceId: "",
          service: undefined as any,
          isActive: true,
        }));
        return {
        id: String(d.doctorId ?? d.DoctorId ?? d.id ?? d.Id),
        userId: String(d.userId ?? d.UserId ?? ""),
        firstName: String(d.firstName ?? d.FirstName ?? ""),
        lastName: String(d.lastName ?? d.LastName ?? ""),
        specializationId: "",
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        specialization: undefined as any,
        specializations,
        isAvailable: true,
        workingHours: { start: "08:00", end: "17:00" },
        // extra field for filters compatibility in UI code
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        } as any;
      }) as any;
      return mapped;
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

    if (USE_MOCK_DATA && !USE_REAL_DOCTORS) {
      return mockDoctors.filter((doctor) =>
        (doctor as any).specializations?.some((spec: any) => spec.id === specializationId)
      );
    }

    try {
      // Filter via practitioner search for now (view doesn't expose specialization names)
      const response = await api.get("/practitioner/doctors", {
        params: { specializationId },
      });
      const rows = Array.isArray(response.data) ? response.data : [];
      const mapped: Doctor[] = rows.map((d: any) => ({
        id: String(d.doctorId ?? d.DoctorId ?? d.id ?? d.Id),
        userId: String(d.userId ?? d.UserId ?? ""),
        firstName: String(d.firstName ?? d.FirstName ?? ""),
        lastName: String(d.lastName ?? d.LastName ?? ""),
        specializationId: specializationId || "",
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        specialization: undefined as any,
        specializations: [],
        isAvailable: true,
        workingHours: { start: "08:00", end: "17:00" },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      })) as any;
      return mapped;
    } catch (error) {
      console.error("Error fetching doctors by specialization:", error);
      throw new Error(
        "Failed to fetch doctors for the specified specialization"
      );
    }
  }

  /**
   * Get doctors by service
   */
  static async getDoctorsByService(serviceId: string): Promise<Doctor[]> {
    await this.delay();

    if (USE_MOCK_DATA && !USE_REAL_DOCTORS) {
      // Mock does not model services->doctors; return all
      return mockDoctors;
    }

    try {
      const response = await api.get("/practitioner/doctors", {
        params: { serviceId },
      });
      const rows = Array.isArray(response.data) ? response.data : [];
      const mapped: Doctor[] = rows.map((d: any) => ({
        id: String(d.doctorId ?? d.DoctorId ?? d.id ?? d.Id),
        userId: String(d.userId ?? d.UserId ?? ""),
        firstName: String(d.firstName ?? d.FirstName ?? ""),
        lastName: String(d.lastName ?? d.LastName ?? ""),
        specializationId: "",
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        specialization: undefined as any,
        specializations: [],
        isAvailable: true,
        workingHours: { start: "08:00", end: "17:00" },
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      })) as any;
      return mapped;
    } catch (error) {
      console.error("Error fetching doctors by service:", error);
      throw new Error("Failed to fetch doctors for the specified service");
    }
  }

  /**
   * Get doctor details by ID
   */
  static async getDoctorById(doctorId: string): Promise<Doctor> {
    try {
      // Try directory listing and match by either DoctorId or UserId
      const listResp = await api.get("/practitioner/doctors");
      const rows = Array.isArray(listResp.data) ? listResp.data : [];
      const row = rows.find(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        (d: any) =>
          String(d.DoctorId ?? d.doctorId) === doctorId ||
          String(d.UserId ?? d.userId) === doctorId
      );
      if (row) {
        return {
          id: String(row.DoctorId ?? row.doctorId ?? doctorId),
          userId: String(row.UserId ?? row.userId ?? ""),
          firstName: String(row.FirstName ?? row.firstName ?? ""),
          lastName: String(row.LastName ?? row.lastName ?? ""),
          specializationId: "",
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          specialization: undefined as any,
          specializations: [],
          isAvailable: true,
          workingHours: { start: "08:00", end: "17:00" },
        } as any;
      }

      // Try practitioner entity by Id (doctorId)
      try {
        const response = await api.get(`/practitioner/doctors/${doctorId}`);
        const d = response.data || {};
        let firstName = "";
        let lastName = "";
        const userId = String(d.userId ?? d.UserId ?? "");
        if (userId) {
          try {
            const userRes = await api.get(`/users/${userId}`);
            const u = userRes.data || {};
            firstName = String(u.firstName ?? u.FirstName ?? firstName);
            lastName = String(u.lastName ?? u.LastName ?? lastName);
          } catch {
            // ignore, names remain empty
          }
        }
        return {
          id: String(d.id ?? d.Id ?? doctorId),
          userId,
          firstName,
          lastName,
          specializationId: "",
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          specialization: undefined as any,
          specializations: [],
          isAvailable: true,
          workingHours: { start: "08:00", end: "17:00" },
        } as any;
      } catch {
        // Not found by doctor entity; treat input as a userId and fetch names
        try {
          const userRes = await api.get(`/users/${doctorId}`);
          const u = userRes.data || {};
          return {
            id: String(doctorId),
            userId: String(u.id ?? u.Id ?? doctorId),
            firstName: String(u.firstName ?? u.FirstName ?? ""),
            lastName: String(u.lastName ?? u.LastName ?? ""),
            specializationId: "",
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            specialization: undefined as any,
            specializations: [],
            isAvailable: true,
            workingHours: { start: "08:00", end: "17:00" },
          } as any;
        } catch (e2) {
          console.error("Doctor lookup failed by both doctorId and userId:", e2);
          throw new Error("Failed to fetch doctor details");
        }
      }
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

    if (USE_MOCK_DATA && !USE_REAL_DOCTORS) {
      return mockServices;
    }

    try {
      // Practitioner catalog services
      const response = await api.get("/practitioner/catalog/services");
      const rows = Array.isArray(response.data) ? response.data : [];
      return rows.map((s: any) => ({
        id: String(s.id ?? s.Id),
        name: String(s.name ?? s.Name ?? "Service"),
        description: String(s.description ?? s.Description ?? ""),
        durationMinutes: 30,
        isActive: true,
      })) as Service[];
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

    if (USE_MOCK_DATA && !USE_REAL_DOCTORS) {
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
      // No direct mapping yet; return full list
      return await this.getServices();
    } catch (error) {
      console.error("Error fetching services by specialization:", error);
      throw new Error(
        "Failed to fetch services for the specified specialization"
      );
    }
  }

  /**
   * Get specializations by service
   */
  static async getSpecializationsByService(serviceId: string): Promise<Specialization[]> {
    await this.delay();

    if (USE_MOCK_DATA && !USE_REAL_DOCTORS) {
      // Return all in mock
      return mockSpecializations;
    }

    try {
      const response = await api.get("/practitioner/catalog/specializations", { params: { serviceId } });
      const rows = Array.isArray(response.data) ? response.data : [];
      return rows.map((r: any) => ({
        id: String(r.id ?? r.Id),
        name: String(r.name ?? r.Name ?? "Specialization"),
        description: "",
        serviceId: "",
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        service: undefined as any,
        isActive: true,
      })) as Specialization[];
    } catch (error) {
      console.error("Error fetching specializations by service:", error);
      throw new Error("Failed to fetch specializations for the specified service");
    }
  }

  // ===== SPECIALIZATIONS =====

  /**
   * Get all available specializations
   */
  static async getSpecializations(): Promise<Specialization[]> {
    await this.delay();

    if (USE_MOCK_DATA && !USE_REAL_DOCTORS) {
      return mockSpecializations;
    }

    try {
      const response = await api.get("/practitioner/catalog/specializations");
      const rows = Array.isArray(response.data) ? response.data : [];
      return rows.map((r: any) => ({
        id: String(r.id ?? r.Id),
        name: String(r.name ?? r.Name ?? "Specialization"),
        description: "",
        serviceId: "",
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        service: undefined as any,
        isActive: true,
      })) as Specialization[];
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
