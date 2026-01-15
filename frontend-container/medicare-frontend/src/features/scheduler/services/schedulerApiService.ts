import { apiClient as api } from "../../../shared/services/apiClient";
import type {
  Appointment,
  AppointmentStatus,
  AvailableSlotsRequest,
  CreateAppointmentRequest,
  Doctor,
  DoctorSchedule,
  Patient,
  SchedulerStats,
  Service,
  Specialization,
  TimeSlot,
  UpdateAppointmentRequest,
} from "../types";
import { getStatusColors } from "../utils/statusColors";

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
  serviceId?: string;
  isPaid?: boolean;
  requiresPayment?: boolean;
  patient?: {
    patientId: string;
    userId: string;
    firstName?: string;
    lastName?: string;
    email?: string;
    phone?: string;
    dateOfBirth?: string;
  };
}

// Backend doctor directory response
interface BackendDoctor {
  id?: string;
  Id?: string;
  doctorId?: string;
  DoctorId?: string;
  userId?: string;
  UserId?: string;
  firstName?: string;
  FirstName?: string;
  lastName?: string;
  LastName?: string;
  specializations?: string;
  Specializations?: string;
}

// Backend service/specialization response
interface BackendService {
  id?: string;
  Id?: string;
  name?: string;
  Name?: string;
  description?: string;
  Description?: string;
  durationMinutes?: number;
  DurationMinutes?: number;
}

interface BackendSpecialization {
  id?: string;
  Id?: string;
  name?: string;
  Name?: string;
}

// More permissive GUID validation
const isGuid = (v: string): boolean =>
  /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/.test(
    v
  );

export class SchedulerApiService {
  // Normalize a Date to a local ISO-like string without timezone designator
  private static toLocalIso(datetime: Date): string {
    const pad = (n: number) => String(n).padStart(2, "0");
    const d = new Date(datetime);
    return (
      `${d.getFullYear()}-` +
      `${pad(d.getMonth() + 1)}-` +
      `${pad(d.getDate())}T` +
      `${pad(d.getHours())}:` +
      `${pad(d.getMinutes())}:` +
      `${pad(d.getSeconds())}`
    );
  }

  // Parse backend datetime
  private static parseBackendDate(input: unknown): Date {
    if (input instanceof Date) return new Date(input);
    const s = input === null || input === undefined ? "" : String(input);
    if (/Z$|[+-]\d{2}:\d{2}$/.test(s)) return new Date(s);
    const naive = s.match(
      /^(\d{4}-\d{2}-\d{2})[T ](\d{2}:\d{2})(?::(\d{2})(?:\.\d{1,7})?)?$/
    );
    if (naive) return new Date(s.replace(" ", "T"));
    return new Date(s);
  }

  // Map backend AppointmentService entity to UI Appointment shape
  private static mapBackendAppointmentToUi(
    backend: BackendAppointment
  ): Appointment {
    const start = this.parseBackendDate(backend.scheduledAt);
    const end = this.parseBackendDate(
      backend.scheduledEndAt ?? backend.scheduledAt
    );
    const durationMinutes = Math.max(
      15,
      Math.round((end.getTime() - start.getTime()) / 60000) || 30
    );

    const backendStatusName = String(backend.status || "Scheduled");
    const colors = getStatusColors(backendStatusName);
    const status: AppointmentStatus = {
      id: `status-${backendStatusName.toLowerCase().replace(/\s+/g, "-")}`,
      name: backendStatusName,
      description: backendStatusName,
      colorCode: colors.bg,
    };

    // Use enriched patient data from backend if available, otherwise placeholder
    const patient: Patient = backend.patient
      ? {
          id: String(backend.patient.patientId || backend.patientId),
          userId: String(backend.patient.userId || backend.patientId),
          firstName: backend.patient.firstName || "",
          lastName: backend.patient.lastName || "",
          email: backend.patient.email || "",
          phone: backend.patient.phone || "",
          dateOfBirth: backend.patient.dateOfBirth || new Date(0).toISOString(),
        }
      : {
          id: String(backend.patientId),
          userId: String(backend.patientId),
          firstName: "",
          lastName: "",
          email: "",
          phone: "",
          dateOfBirth: new Date(0).toISOString(),
        };

    const ui: Appointment = {
      id: String(backend.id),
      patientId: String(backend.patientId),
      patient,
      doctorUserId: String(backend.doctorId),
      doctor: undefined, // Enriched later
      serviceId: backend.serviceId || "service-1", // Fallback if missing
      service: undefined as unknown as Service, // Enriched later
      timeSlotId: "",
      timeSlot: undefined,
      day: this.toLocalIso(start),
      durationMinutes,
      appointmentType:
        backend.appointmentType === "virtual" ||
        backend.appointmentType === "phone"
          ? backend.appointmentType
          : ("in-person" as const),
      appointmentCategory: undefined as unknown as string,
      description: backend.notes || "",
      statusId: status.id,
      status,
      createdAt: this.parseBackendDate(backend.scheduledAt).toISOString(),
      updatedAt: this.parseBackendDate(backend.scheduledAt).toISOString(),
    };

    return ui;
  }

  // ===== APPOINTMENTS =====

  /**
   * Get all appointments (Receptionist view - aggregates from all doctors)
   */
  static async getAllAppointments(): Promise<Appointment[]> {
    try {
      // 1. Fetch all doctors
      const doctors = await this.getDoctors();

      // 2. Fetch appointments for each doctor in parallel
      const appointmentPromises = doctors.map(async (doc) => {
        try {
          const response = await api.get(
            `/appointment/appointments/doctor/${doc.id}`
          );
          const data = Array.isArray(response.data) ? response.data : [];
          return data.map((a: BackendAppointment) => {
            const uiApt = this.mapBackendAppointmentToUi(a);
            uiApt.doctor = doc; // Enrich immediately with known doctor
            return uiApt;
          });
        } catch (e) {
          console.error(`Failed to fetch appointments for doctor ${doc.id}`, e);
          return [];
        }
      });

      const results = await Promise.all(appointmentPromises);
      const allAppointments = results.flat();

      return allAppointments;
    } catch (error) {
      console.error("Error fetching all appointments:", error);
      return [];
    }
  }

  /**
   * Get all appointments for specific doctor
   */
  static async getDoctorAppointments(doctorId: string): Promise<Appointment[]> {
    try {
      // Direct endpoint to AppointmentService
      const response = await api.get(
        `/appointment/appointments/doctor/${doctorId}`
      );
      const data = Array.isArray(response.data) ? response.data : [];
      return data.map((a: BackendAppointment) =>
        this.mapBackendAppointmentToUi(a)
      );
    } catch (error) {
      console.error("Error fetching doctor appointments:", error);
      throw new Error("Failed to fetch doctor appointments");
    }
  }

  /**
   * Get all appointments for the current patient
   */
  static async getPatientAppointments(
    patientId: string
  ): Promise<Appointment[]> {
    try {
      const response = await api.get(
        `/appointment/appointments/patient/${patientId}`
      );
      const items = Array.isArray(response.data) ? response.data : [];
      const appointments = items.map((a: BackendAppointment) =>
        this.mapBackendAppointmentToUi(a)
      );

      // Fetch doctors to enrich appointments
      const doctors = await this.getDoctors();
      for (const apt of appointments) {
        if (apt.doctorUserId) {
          const doctor = doctors.find(
            (d) => d.userId === apt.doctorUserId || d.id === apt.doctorUserId
          );
          if (doctor) {
            apt.doctor = doctor;
          }
        }
      }

      return appointments;
    } catch (error) {
      console.error("Error fetching patient appointments:", error);
      // Return empty list on error
      return [];
    }
  }

  /**
   * Create a new appointment
   */
  static async createAppointment(
    patientId: string,
    appointmentData: CreateAppointmentRequest
  ): Promise<Appointment> {
    try {
      // Validator
      if (
        !appointmentData.doctorUserId ||
        !isGuid(appointmentData.doctorUserId)
      ) {
        throw new Error(
          "Invalid doctor selection. Please select a doctor before booking."
        );
      }
      const doctorGuid = appointmentData.doctorUserId;

      // Resolve time slot
      let start: Date | undefined;
      let end: Date | undefined;
      const selectedSlotId = appointmentData.timeSlotId;

      // Try to parse slot ID directly if it contains date info
      if (selectedSlotId) {
        const m = selectedSlotId.match(
          /^(.+)-(\d{4}-\d{2}-\d{2})-(\d{2})(\d{2})$/
        );
        if (m) {
          const dayStr = m[2];
          const hh = m[3];
          const mm = m[4];
          const localStart = new Date(`${dayStr}T${hh}:${mm}:00`);
          const duration = appointmentData.duration || 30;
          const localEnd = new Date(localStart.getTime() + duration * 60000);
          start = localStart;
          end = localEnd;
        }
      }

      // Fallback if parsing failed or slotId format unknown (e.g. mock slots)
      if (!start || !end) {
        // This assumes we might be booking "now" or "soon" if slot logic fails
        // In real usage, the slotId coming from getAvailableTimeSlots will match the parser pattern
        const now = new Date();
        start = new Date(now.getTime() + 60 * 60000); // 1 hour from now
        end = new Date(start.getTime() + 30 * 60000);
      }

      const payload = {
        patientId,
        doctorId: doctorGuid,
        scheduledAt: this.toLocalIso(start),
        scheduledEndAt: this.toLocalIso(end),
        appointmentType: appointmentData.appointmentType,
        notes: appointmentData.description,
        serviceId: appointmentData.serviceId || null,
        Category: appointmentData.appointmentCategory,
        Room: appointmentData.room,
      };

      const response = await api.post(`/appointment/appointments`, payload);
      return this.mapBackendAppointmentToUi(response.data);
    } catch (error: unknown) {
      const err = error as {
        response?: { data?: { message?: string; title?: string } };
        message?: string;
      };

      const message =
        err?.response?.data?.message ||
        err?.response?.data?.title ||
        err?.message ||
        "Failed to create appointment";
      throw new Error(message);
    }
  }

  /**
   * Update an existing appointment status
   */
  static async updateAppointmentStatus(
    appointmentId: string,
    statusId: string
  ): Promise<Appointment> {
    try {
      // Map UI status IDs to backend status strings
      let status = "Scheduled";
      if (statusId.includes("confirmed")) status = "Confirmed";
      if (statusId.includes("completed")) status = "Completed";
      if (statusId.includes("cancelled")) status = "Cancelled";
      if (statusId.includes("no-show")) status = "NoShow";

      const response = await api.put(
        `/appointment/appointments/${appointmentId}/status`,
        {
          status,
        }
      );
      return this.mapBackendAppointmentToUi(response.data);
    } catch (error) {
      console.error("Error updating appointment status:", error);
      throw new Error("Failed to update appointment status");
    }
  }

  /**
   * Update an existing appointment details
   */
  static async updateAppointment(
    appointmentId: string,
    updateData: UpdateAppointmentRequest
  ): Promise<Appointment> {
    try {
      // Map frontend update request to backend DTO structure
      const payload: Record<string, unknown> = {};
      if (updateData.description !== undefined)
        payload.Description = updateData.description;
      if (updateData.scheduledAt) payload.ScheduledAt = updateData.scheduledAt;
      if (updateData.scheduledEndAt)
        payload.ScheduledEndAt = updateData.scheduledEndAt;
      if (updateData.appointmentType)
        payload.AppointmentType = updateData.appointmentType;
      if (updateData.serviceId) payload.ServiceId = updateData.serviceId;
      if (updateData.category) payload.Category = updateData.category;
      if (updateData.room) payload.Room = updateData.room;

      const response = await api.put(
        `/appointment/appointments/${appointmentId}`,
        payload
      );
      return this.mapBackendAppointmentToUi(response.data);
    } catch (error) {
      console.error("Error updating appointment:", error);
      throw new Error("Failed to update appointment");
    }
  }

  /**
   * Cancel an appointment
   */
  static async cancelAppointment(appointmentId: string): Promise<void> {
    try {
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
    try {
      const response = await api.get(
        `/appointment/appointments/${appointmentId}`
      );
      return this.mapBackendAppointmentToUi(response.data);
    } catch (error) {
      console.error("Error fetching appointment details:", error);
      throw new Error("Failed to fetch appointment details");
    }
  }

  /**
   * Start virtual consultation info
   */
  static getVirtualConsultationInfo(appointmentId: string): string {
    return `Video call for appointment ${appointmentId} should be started in external application.`;
  }

  /**
   * Get scheduler stats
   */
  static async getSchedulerStats(filters: {
    doctorId?: string;
    patientId?: string;
  }): Promise<SchedulerStats> {
    try {
      console.log("Fetching scheduler stats from API...");
      const response = await api.get("/appointment/appointments/stats", {
        params: filters,
      });
      return response.data;
    } catch (error) {
      console.error("Error fetching scheduler stats:", error);
      return {
        totalAppointments: 0,
        todaysAppointments: 0,
        confirmedAppointments: 0,
        cancelledAppointments: 0,
      };
    }
  }

  // ===== DOCTORS =====

  /**
   * Helper to enrich doctors with empty names from User Service
   */
  private static async enrichDoctorsWithUserProfiles(
    doctors: Doctor[]
  ): Promise<Doctor[]> {
    const doctorsWithMissingNames = doctors.filter(
      (doc) => !doc.firstName && !doc.lastName && doc.userId
    );
    if (doctorsWithMissingNames.length === 0) return doctors;

    const userProfiles = await Promise.all(
      doctorsWithMissingNames.map(async (doc) => {
        try {
          const userRes = await api.get(`/users/${doc.userId}`);
          const u = userRes.data || {};
          return {
            odId: doc.id,
            firstName: String(u.firstName ?? u.FirstName ?? ""),
            lastName: String(u.lastName ?? u.LastName ?? ""),
          };
        } catch {
          return { odId: doc.id, firstName: "", lastName: "" };
        }
      })
    );

    const profileMap = new Map(userProfiles.map((p) => [p.odId, p]));
    for (const doc of doctors) {
      const profile = profileMap.get(doc.id);
      if (profile && (!doc.firstName || !doc.lastName)) {
        doc.firstName = profile.firstName || doc.firstName;
        doc.lastName = profile.lastName || doc.lastName;
      }
    }

    return doctors;
  }

  /**
   * Get all available doctors
   */
  static async getDoctors(): Promise<Doctor[]> {
    try {
      const response = await api.get("/practitioner/doctors");
      const rows = Array.isArray(response.data) ? response.data : [];

      const mapped: Doctor[] = rows.map((d: BackendDoctor) => {
        const specIds = String(d.specializations ?? d.Specializations ?? "")
          .split(",")
          .map((s) => s.trim())
          .filter((s) => s.length > 0);

        const specializations: Specialization[] = specIds.map((id) => ({
          id,
          name: "",
          description: "",
          serviceId: "",
          service: undefined as unknown as Service,
          isActive: true,
        }));

        return {
          id: String(d.doctorId ?? d.DoctorId ?? d.id ?? d.Id),
          userId: String(d.userId ?? d.UserId ?? ""),
          firstName: String(d.firstName ?? d.FirstName ?? ""),
          lastName: String(d.lastName ?? d.LastName ?? ""),
          specializationId: "",
          specialization: undefined as unknown as Specialization,
          specializations,
          isAvailable: true,
          workingHours: { start: "08:00", end: "17:00" },
        } as Doctor;
      });

      return this.enrichDoctorsWithUserProfiles(mapped);
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
    try {
      const response = await api.get("/practitioner/doctors", {
        params: { specializationId },
      });
      const rows = Array.isArray(response.data) ? response.data : [];
      // Same mapping logic as getDoctors
      const mapped: Doctor[] = rows.map((d: BackendDoctor) => {
        const specIds = String(
          d.specializations ?? d.Specializations ?? specializationId ?? ""
        )
          .split(",")
          .map((s) => s.trim())
          .filter((s) => s.length > 0);

        const specializations = specIds.map((id) => ({
          id,
          name: "",
          description: "",
          serviceId: "",
          service: undefined as unknown as Service,
          isActive: true,
        }));

        return {
          id: String(d.doctorId ?? d.DoctorId ?? d.id ?? d.Id),
          userId: String(d.userId ?? d.UserId ?? ""),
          firstName: String(d.firstName ?? d.FirstName ?? ""),
          lastName: String(d.lastName ?? d.LastName ?? ""),
          specializationId: specializationId || "",
          specialization: undefined as unknown as Specialization,
          specializations,
          isAvailable: true,
          workingHours: { start: "08:00", end: "17:00" },
        } as Doctor;
      });

      return this.enrichDoctorsWithUserProfiles(mapped);
    } catch (error) {
      console.error("Error fetching doctors by specialization:", error);
      throw new Error("Failed to fetch doctors for specified specialization");
    }
  }

  /**
   * Get doctors by service
   */
  static async getDoctorsByService(serviceId: string): Promise<Doctor[]> {
    try {
      const response = await api.get("/practitioner/doctors", {
        params: { serviceId },
      });
      const rows = Array.isArray(response.data) ? response.data : [];
      const mapped: Doctor[] = rows.map((d: BackendDoctor) => {
        const specIds = String(d.specializations ?? d.Specializations ?? "")
          .split(",")
          .map((s) => s.trim())
          .filter((s) => s.length > 0);

        const specializations = specIds.map((id) => ({
          id,
          name: "",
          description: "",
          serviceId: "",
          service: undefined as unknown as Service,
          isActive: true,
        }));

        return {
          id: String(d.doctorId ?? d.DoctorId ?? d.id ?? d.Id),
          userId: String(d.userId ?? d.UserId ?? ""),
          firstName: String(d.firstName ?? d.FirstName ?? ""),
          lastName: String(d.lastName ?? d.LastName ?? ""),
          specializationId: "",
          specialization: undefined as unknown as Specialization,
          specializations,
          isAvailable: true,
          workingHours: { start: "08:00", end: "17:00" },
        } as Doctor;
      });

      return this.enrichDoctorsWithUserProfiles(mapped);
    } catch (error) {
      console.error("Error fetching doctors by service:", error);
      throw new Error("Failed to fetch doctors for specified service");
    }
  }

  /**
   * Get filtered doctors
   */
  static async getDoctorsFiltered(params: {
    specializationId?: string;
    serviceId?: string;
  }): Promise<Doctor[]> {
    try {
      const response = await api.get("/practitioner/doctors", { params });
      const rows = Array.isArray(response.data) ? response.data : [];
      const mapped: Doctor[] = rows.map((d: BackendDoctor) => {
        const specIds = String(d.specializations ?? d.Specializations ?? "")
          .split(",")
          .map((s) => s.trim())
          .filter((s) => s.length > 0);
        const specializations = specIds.map((id) => ({
          id,
          name: "",
          description: "",
          serviceId: "",
          service: undefined as unknown as Service,
          isActive: true,
        }));

        return {
          id: String(d.doctorId ?? d.DoctorId ?? d.id ?? d.Id),
          userId: String(d.userId ?? d.UserId ?? ""),
          firstName: String(d.firstName ?? d.FirstName ?? ""),
          lastName: String(d.lastName ?? d.LastName ?? ""),
          specializationId: "",
          specialization: undefined as unknown as Specialization,
          specializations,
          isAvailable: true,
          workingHours: { start: "08:00", end: "17:00" },
        } as Doctor;
      });
      return this.enrichDoctorsWithUserProfiles(mapped);
    } catch (error) {
      console.error("Error fetching filtered doctors:", error);
      // Fail gracefully
      return [];
    }
  }

  /**
   * Get doctor by Id
   */
  static async getDoctorById(doctorId: string): Promise<Doctor> {
    // Fetch all to find match from directory (names populated properly there usually)
    // Optimization: Call specific endpoint if possible, but directory search is safer for consistency
    const all = await this.getDoctors();
    const found = all.find((d) => d.id === doctorId || d.userId === doctorId);
    if (found) return found;

    // Fallback: minimal valid object
    return {
      id: doctorId,
      userId: doctorId,
      firstName: "Unknown",
      lastName: "Doctor",
      specializationId: "",
      specialization: undefined as unknown as Specialization,
      specializations: [],
      workingHours: { start: "08:00", end: "17:00" },
      isAvailable: false,
    } as Doctor;
  }

  // ===== SERVICES =====

  static async getServices(): Promise<Service[]> {
    try {
      const response = await api.get("/practitioner/catalog/services");
      const rows = Array.isArray(response.data) ? response.data : [];
      return rows.map((s: BackendService) => ({
        id: String(s.id ?? s.Id),
        name: String(s.name ?? s.Name ?? "Service"),
        description: String(s.description ?? s.Description ?? ""),
        durationMinutes: 30, // Default if missing
        isActive: true,
      })) as Service[];
    } catch (error) {
      console.error("Error fetching services:", error);
      return [];
    }
  }

  static async getServicesBySpecialization(
    specializationId: string
  ): Promise<Service[]> {
    try {
      const response = await api.get("/practitioner/catalog/services", {
        params: { specializationId },
      });
      const rows = Array.isArray(response.data) ? response.data : [];
      return rows.map((s: BackendService) => ({
        id: String(s.id ?? s.Id),
        name: String(s.name ?? s.Name ?? "Service"),
        description: String(s.description ?? s.Description ?? ""),
        durationMinutes: 30,
        isActive: true,
      })) as Service[];
    } catch {
      return [];
    }
  }

  static async getSpecializationsByService(
    serviceId: string
  ): Promise<Specialization[]> {
    try {
      const response = await api.get("/practitioner/catalog/specializations", {
        params: { serviceId },
      });
      const rows = Array.isArray(response.data) ? response.data : [];
      return rows.map((r: BackendSpecialization) => ({
        id: String(r.id ?? r.Id),
        name: String(r.name ?? r.Name ?? "Specialization"),
        description: "",
        serviceId: "",
        service: undefined as unknown as Service,
        isActive: true,
      })) as Specialization[];
    } catch {
      return [];
    }
  }

  // ===== SPECIALIZATIONS =====

  static async getSpecializations(): Promise<Specialization[]> {
    try {
      const response = await api.get("/practitioner/catalog/specializations");
      const rows = Array.isArray(response.data) ? response.data : [];
      return rows.map((r: BackendSpecialization) => ({
        id: String(r.id ?? r.Id),
        name: String(r.name ?? r.Name ?? "Specialization"),
        description: "",
        serviceId: "",
        service: undefined as unknown as Service,
        isActive: true,
      })) as Specialization[];
    } catch (error) {
      console.error("Error fetching specializations:", error);
      return [];
    }
  }

  // ===== TIME SLOTS =====

  static async getAvailableTimeSlots(
    request: AvailableSlotsRequest
  ): Promise<TimeSlot[]> {
    try {
      // Parse dates
      const startDate = new Date(request.startDate);
      const endDate = new Date(request.endDate);

      // 1) Availability from Practitioner Service
      const availabilityResp = await api.get(
        `/practitioner/doctors/${request.doctorId}/availability`
      );
      const availability = availabilityResp.data || [];

      // 2) Existing appointments from Appointment Service
      const aptResp = await api.get(
        `/appointment/appointments/doctor/${request.doctorId}`
      );
      const appointments = Array.isArray(aptResp.data) ? aptResp.data : [];

      // Helper: test overlap
      const overlaps = (start: Date, end: Date) =>
        appointments.some((a: BackendAppointment) => {
          const as = this.parseBackendDate(a.scheduledAt);
          const ae = this.parseBackendDate(a.scheduledEndAt || a.scheduledAt);
          return (
            Math.max(as.getTime(), start.getTime()) <
            Math.min(ae.getTime(), end.getTime())
          );
        });

      // Normalize availability
      const availRows = availability.map((r: unknown) => {
        const row = r as {
          dayOfWeek?: number;
          DayOfWeek?: number;
          startTime?: string;
          StartTime?: string;
          endTime?: string;
          EndTime?: string;
        };
        return {
          dayOfWeek: Number(row.dayOfWeek ?? row.DayOfWeek ?? 0),
          start: String(row.startTime ?? row.StartTime ?? "09:00:00"),
          end: String(row.endTime ?? row.EndTime ?? "17:00:00"),
        };
      });

      const toMinutes = (t: string) => {
        const [h, m] = t.split(":").map(Number);
        return h * 60 + m;
      };

      const slots: TimeSlot[] = [];
      const day = new Date(startDate);
      const today = new Date();
      today.setHours(0, 0, 0, 0);

      while (day <= endDate) {
        if (day.getTime() <= today.getTime()) {
          // Can't book today/past
          day.setDate(day.getDate() + 1);
          continue;
        }

        const jsDow = day.getDay();
        const dayStr = `${day.getFullYear()}-${String(day.getMonth() + 1).padStart(2, "0")}-${String(day.getDate()).padStart(2, "0")}`;
        const ranges = availRows.filter(
          (a: { dayOfWeek: number }) => a.dayOfWeek === jsDow
        );

        for (const r of ranges) {
          const startMin = toMinutes(r.start);
          const endMin = toMinutes(r.end);
          // Default 30 min slots
          for (let m = startMin; m + 30 <= endMin; m += 30) {
            const startLocal = new Date(
              `${dayStr}T${String(Math.floor(m / 60)).padStart(2, "0")}:${String(m % 60).padStart(2, "0")}:00`
            );
            const endLocal = new Date(startLocal.getTime() + 30 * 60000);

            if (!overlaps(startLocal, endLocal)) {
              slots.push({
                id: `${request.doctorId}-${dayStr}-${String(Math.floor(m / 60)).padStart(2, "0")}${String(m % 60).padStart(2, "0")}`,
                doctorId: request.doctorId,
                startDateTime: this.toLocalIso(startLocal),
                endDateTime: this.toLocalIso(endLocal),
                isAvailable: true,
                durationMinutes: 30,
                slotType: "Regular",
              });
            }
          }
        }
        day.setDate(day.getDate() + 1);
      }
      return slots;
    } catch (e) {
      console.error("Available slots error:", e);
      return [];
    }
  }

  static async getDoctorSchedule(doctorId: string): Promise<DoctorSchedule[]> {
    try {
      const response = await api.get(
        `/practitioner/doctors/${doctorId}/availability`
      );
      // Map raw response to DoctorSchedule interface roughly
      return (response.data || []).map(
        (
          r: {
            dayOfWeek?: number;
            DayOfWeek?: number;
            startTime?: string;
            StartTime?: string;
            endTime?: string;
            EndTime?: string;
          },
          i: number
        ) => ({
          id: `sched-${i}`,
          doctorId,
          dayOfWeek: r.dayOfWeek ?? r.DayOfWeek,
          startTime: r.startTime ?? r.StartTime,
          endTime: r.endTime ?? r.EndTime,
          isAvailable: true,
          validFrom: "2025-01-01",
          validTo: "2025-12-31",
        })
      );
    } catch {
      return [];
    }
  }

  static async getAppointmentStatuses(): Promise<AppointmentStatus[]> {
    // Real endpoint or static fallback that matches backend enum
    return [
      {
        id: "status-scheduled",
        name: "Scheduled",
        description: "Scheduled",
        colorCode: "#3b82f6",
      },
      {
        id: "status-confirmed",
        name: "Confirmed",
        description: "Confirmed",
        colorCode: "#10b981",
      },
      {
        id: "status-cancelled",
        name: "Cancelled",
        description: "Cancelled",
        colorCode: "#ef4444",
      },
      {
        id: "status-completed",
        name: "Completed",
        description: "Completed",
        colorCode: "#8b5cf6",
      },
    ];
  }

  // Stats - real implementation
  static async getDashboardStats(): Promise<SchedulerStats> {
    // Return empty stats.
    // or implement real endpoint later
    return {
      totalAppointments: 0,
      todaysAppointments: 0,
      confirmedAppointments: 0,
      cancelledAppointments: 0,
    };
  }

  static async getCurrentPatient(): Promise<Patient> {
    try {
      const response = await api.get("/patients/current");
      return response.data;
    } catch {
      // Fallback for non-patient users (e.g. receptionist)
      return {
        id: "",
        userId: "",
        firstName: "Guest",
        lastName: "",
        email: "",
        phone: "",
      } as Patient;
    }
  }

  static async getPatientById(patientId: string): Promise<Patient> {
    try {
      const response = await api.get(`/patients/${patientId}`);
      return response.data;
    } catch {
      return {
        id: patientId,
        userId: patientId,
        firstName: "Unknown",
        lastName: "Patient",
        email: "",
        phone: "",
      } as Patient;
    }
  }
}

export default SchedulerApiService;
