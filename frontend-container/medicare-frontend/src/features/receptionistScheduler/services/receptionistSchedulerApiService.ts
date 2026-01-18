import { PatientRegistryApiService } from "../../patientRegistry/services/patientRegistryApi";
import { SchedulerApiService } from "../../scheduler/services/schedulerApiService";
import type {
  Appointment as SchedulerAppointment,
  CreateAppointmentRequest as SchedulerCreateAppointmentRequest,
} from "../../scheduler/types";
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

export class ReceptionistSchedulerApiService {
  // Mapper: SchedulerAppointment -> ReceptionistAppointment
  private static mapToReceptionist(
    apt: SchedulerAppointment
  ): ReceptionistAppointment {
    const dateObj = new Date(apt.day); // apt.day is ISO string from SchedulerApiService
    const day = dateObj.toISOString().split("T")[0];
    const time = dateObj.toTimeString().slice(0, 5); // HH:mm

    let aptType: ReceptionistAppointment["appointmentType"] = "in-person";
    if (apt.appointmentType === "virtual") aptType = "video-call";
    else if (apt.appointmentType === "phone") aptType = "phone";
    else aptType = "in-person";

    const category = (apt.appointmentCategory ||
      "consultation") as ReceptionistAppointment["appointmentCategory"];

    return {
      id: apt.id,
      patientId: apt.patientId,
      doctorId: apt.doctorUserId,
      day,
      time,
      duration: apt.durationMinutes,
      appointmentType: aptType,
      appointmentCategory: category,
      statusId: apt.statusId,
      description: apt.description || "",
      ...(apt.patient
        ? {
            patient: {
              id: apt.patient.id,
              firstName: apt.patient.firstName,
              lastName: apt.patient.lastName,
              email: apt.patient.email,
              phone: apt.patient.phone,
              dateOfBirth: apt.patient.dateOfBirth,
              medicalRecordNumber: "MRN-" + apt.patient.id.substring(0, 4),
            },
          }
        : {}),
      ...(apt.doctor
        ? {
            doctor: {
              id: apt.doctor.id,
              userId: apt.doctor.userId,
              firstName: apt.doctor.firstName,
              lastName: apt.doctor.lastName,
              email: "",
              phone: "",
              specializations: apt.doctor.specializations || [],
            },
          }
        : {}),
      status: apt.status,
      createdAt: apt.createdAt,
      updatedAt: apt.updatedAt,
      room: "Room 101",
      totalCost: 0,
    };
  }

  // ===== APPOINTMENTS =====

  static async getAppointments(
    filters?: AppointmentFilters
  ): Promise<ReceptionistAppointment[]> {
    try {
      const [allAppointments, patientsResult] = await Promise.all([
        SchedulerApiService.getAllAppointments(),
        PatientRegistryApiService.getPatients(1, 1000), // Fetch large batch to enrich names
      ]);

      const patients = patientsResult.success
        ? patientsResult.data?.patients || []
        : [];
      const patientMap = new Map(patients.map((p) => [p.id, p]));

      let mapped = allAppointments.map((apt) => {
        const enriched = this.mapToReceptionist(apt);
        // Enrich patient if placeholder
        if (
          (!enriched.patient?.firstName ||
            enriched.patient.firstName === "Unknown") &&
          enriched.patient?.id
        ) {
          const p = patientMap.get(enriched.patient.id);
          if (p) {
            enriched.patient = {
              id: p.id || "",
              firstName: p.firstName,
              lastName: p.lastName,
              email: p.email,
              phone: p.phone,
              dateOfBirth: p.dateOfBirth,
              ...(p.medicalRecordNumber && {
                medicalRecordNumber: p.medicalRecordNumber,
              }),
            };
          }
        }
        return enriched;
      });

      if (filters) {
        if (filters.patientName && filters.patientName.trim()) {
          const searchTerm = filters.patientName.toLowerCase().trim();
          mapped = mapped.filter((apt) => {
            if (!apt.patient) return false;
            const fullName =
              `${apt.patient.firstName} ${apt.patient.lastName}`.toLowerCase();
            return fullName.includes(searchTerm);
          });
        }

        if (filters.doctorId) {
          mapped = mapped.filter((apt) => apt.doctorId === filters.doctorId);
        }

        if (filters.status) {
          mapped = mapped.filter((apt) => apt.statusId === filters.status);
        }

        if (filters.appointmentType) {
          mapped = mapped.filter(
            (apt) => apt.appointmentType === filters.appointmentType
          );
        }

        if (filters.dateRange) {
          mapped = mapped.filter(
            (apt) =>
              apt.day >= filters.dateRange!.start &&
              apt.day <= filters.dateRange!.end
          );
        }
      }

      return mapped;
    } catch (error) {
      console.error("Error fetching appointments:", error);
      return [];
    }
  }

  static async createAppointment(
    appointmentData: CreateAppointmentRequest
  ): Promise<ReceptionistAppointment> {
    try {
      const [hh, mm] = appointmentData.time.split(":");
      const syntheticTimeSlotId = `manual-${appointmentData.day}-${hh}${mm}`;

      const aptType =
        appointmentData.appointmentType === "video-call"
          ? "virtual"
          : appointmentData.appointmentType;

      // Use explicit type or cast if necessary to match Scheduler's expected CreateRequest
      // Scheduler's CreateAppointmentRequest expects 'appointmentType' to be AppointmentType (Scheduler union)
      const schedulerReq = {
        patientId: appointmentData.patientId,
        doctorUserId: appointmentData.doctorId,
        serviceId: appointmentData.serviceId,
        timeSlotId: syntheticTimeSlotId,
        appointmentType: aptType,
        appointmentCategory: appointmentData.appointmentCategory,
        description: appointmentData.description,
        duration: appointmentData.duration,
        room: appointmentData.room,
      } as unknown as SchedulerCreateAppointmentRequest;

      const result = await SchedulerApiService.createAppointment(
        appointmentData.patientId,
        schedulerReq
      );
      return this.mapToReceptionist(result);
    } catch (error) {
      console.error("Error creating appointment:", error);
      throw error;
    }
  }

  static async updateAppointment(
    appointmentData: UpdateAppointmentRequest
  ): Promise<ReceptionistAppointment> {
    try {
      if (appointmentData.statusId) {
        const res = await SchedulerApiService.updateAppointmentStatus(
          appointmentData.id,
          appointmentData.statusId
        );
        return this.mapToReceptionist(res);
      }

      // Calculate ISO times
      const startDateTime = new Date(
        `${appointmentData.day}T${appointmentData.time}:00`
      );
      const endDateTime = new Date(
        startDateTime.getTime() + (appointmentData.duration || 30) * 60000
      );

      const res = await SchedulerApiService.updateAppointment(
        appointmentData.id,
        {
          description: appointmentData.description || "",
          scheduledAt: startDateTime.toISOString(),
          scheduledEndAt: endDateTime.toISOString(),
          appointmentType: (appointmentData.appointmentType === "video-call"
            ? "virtual"
            : appointmentData.appointmentType) as
            | "in-person"
            | "virtual"
            | "phone",
          ...(appointmentData.appointmentCategory && {
            category: appointmentData.appointmentCategory,
          }),
          ...(appointmentData.room && { room: appointmentData.room }),
          // Update service ID if available (not currently in modal data, but valid in backend)
        }
      );
      return this.mapToReceptionist(res);
    } catch (error) {
      console.error("Error updating appointment:", error);
      throw error;
    }
  }

  static async cancelAppointment(appointmentId: string): Promise<void> {
    return SchedulerApiService.cancelAppointment(appointmentId);
  }

  // ===== PATIENTS =====

  static async searchPatients(query: string): Promise<Patient[]> {
    try {
      const response = await PatientRegistryApiService.getPatients(1, 50, {
        searchTerm: query,
      });
      if (!response.success || !response.data?.patients) return [];

      const patients = response.data.patients;

      return patients.map((p) => ({
        id: p.id || "",
        firstName: p.firstName,
        lastName: p.lastName,
        email: p.email,
        phone: p.phone || "",
        dateOfBirth: p.dateOfBirth,
        medicalRecordNumber:
          p.medicalRecordNumber || "MRN-" + (p.id || "").substring(0, 4),
      }));
    } catch (error) {
      console.error("Error searching patients:", error);
      return [];
    }
  }

  static async getPatients(): Promise<Patient[]> {
    return this.searchPatients("");
  }

  // ===== DOCTORS =====

  static async getDoctors(): Promise<Doctor[]> {
    try {
      const docs = await SchedulerApiService.getDoctors();
      return docs.map((d) => ({
        id: d.id,
        userId: d.userId,
        firstName: d.firstName,
        lastName: d.lastName,
        email: "",
        phone: "",
        specializations: d.specializations || [],
      }));
    } catch {
      return [];
    }
  }

  static async getDoctorAvailability(
    doctorId: string,
    date: string
  ): Promise<TimeSlot[]> {
    try {
      const slots = await SchedulerApiService.getAvailableTimeSlots({
        doctorId,
        startDate: date,
        endDate: date,
      });

      return slots.map((s) => ({
        id: s.id,
        doctorId: s.doctorId,
        startDateTime: s.startDateTime,
        endDateTime: s.endDateTime,
        isAvailable: s.isAvailable,
        durationMinutes: s.durationMinutes,
        slotType: s.slotType,
      }));
    } catch {
      return [];
    }
  }

  // ===== STATUSES =====

  static async getAppointmentStatuses(): Promise<AppointmentStatus[]> {
    return [
      { id: "status-suggested", name: "Scheduled", colorCode: "#blue" },
      { id: "status-confirmed", name: "Confirmed", colorCode: "#green" },
      { id: "status-cancelled", name: "Cancelled", colorCode: "#red" },
    ];
  }

  static async getSpecializations(): Promise<Specialization[]> {
    const specs = await SchedulerApiService.getSpecializations();
    return specs.map((s) => ({
      id: s.id,
      name: s.name,
      description: s.description || "",
    }));
  }
}
