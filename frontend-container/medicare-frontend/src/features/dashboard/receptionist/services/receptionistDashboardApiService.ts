import { apiClient as api } from "../../../../shared/services/apiClient";
import { staffApi } from "../../../../shared/services/staffApi";
import type {
  DoctorAvailability,
  QuickAppointment,
  ReceptionistDashboardData,
  ReceptionistDashboardStats,
} from "../types";

// Backend appointment interface (from AppointmentService)
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
}

// Helper to format time from Date
const formatTime = (date: Date): string => {
  return date.toLocaleTimeString("en-US", {
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
  });
};

// Helper to check if date is today
const isToday = (date: Date): boolean => {
  const today = new Date();
  return (
    date.getDate() === today.getDate() &&
    date.getMonth() === today.getMonth() &&
    date.getFullYear() === today.getFullYear()
  );
};

// Map backend status to UI status
const mapAppointmentStatus = (
  backendStatus: string | undefined,
  scheduledAt: Date,
  scheduledEndAt: Date
): QuickAppointment["status"] => {
  const now = new Date();
  const status = (backendStatus || "Scheduled").toLowerCase();

  if (status === "completed") return "completed";
  if (status === "cancelled") return "cancelled";
  if (status === "in-progress" || status === "inprogress") return "in-progress";

  // Check if currently in progress based on time
  if (now >= scheduledAt && now <= scheduledEndAt) {
    return "in-progress";
  }

  return "waiting";
};

// Map backend appointment type to UI type
const mapAppointmentType = (
  backendType: string | undefined
): QuickAppointment["type"] => {
  const type = (backendType || "in-person").toLowerCase();
  if (type === "virtual" || type === "video" || type === "video-call")
    return "video-call";
  if (type === "phone" || type === "telephone") return "phone";
  return "in-person";
};

export class ReceptionistDashboardApiService {
  /**
   * Get dashboard data for receptionist - fetches real data from backend
   */
  static async getDashboardData(): Promise<ReceptionistDashboardData> {
    try {
      // Fetch doctors from PractitionerService
      const doctorsData = await staffApi.getStaff({ role: "Doctor" });
      const doctors = doctorsData
        ? doctorsData.filter((s) => s.role === "Doctor")
        : [];

      // Build doctor lookup map: userId -> { name, specialization }
      const doctorMap = new Map<
        string,
        { id: string; name: string; specialization: string }
      >();
      for (const doc of doctors) {
        const name = `Dr. ${doc.profile.firstName} ${doc.profile.lastName}`;
        const specialization =
          doc.role === "Doctor" && "specializations" in doc
            ? ((doc as { specializations?: { name: string }[] })
                .specializations?.[0]?.name ?? "General Practice")
            : "General Practice";
        doctorMap.set(doc.id, { id: doc.id, name, specialization });
      }

      // Fetch all appointments for today from each doctor
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      const tomorrow = new Date(today);
      tomorrow.setDate(tomorrow.getDate() + 1);

      const allAppointments: BackendAppointment[] = [];

      // Fetch appointments for all doctors in parallel
      const appointmentPromises = Array.from(doctorMap.keys()).map(
        async (doctorId) => {
          try {
            const response = await api.get<BackendAppointment[]>(
              `/appointment/Appointments/doctor/${doctorId}`
            );
            return response.data || [];
          } catch (err) {
            console.error(
              `Failed to fetch appointments for doctor ${doctorId}:`,
              err
            );
            return [];
          }
        }
      );

      const appointmentResults = await Promise.all(appointmentPromises);
      for (const appointments of appointmentResults) {
        allAppointments.push(...appointments);
      }

      // Filter to today's appointments
      const todaysAppointments = allAppointments.filter((apt) => {
        const aptDate = new Date(apt.scheduledAt);
        return isToday(aptDate);
      });

      // Calculate stats
      const stats: ReceptionistDashboardStats = {
        totalAppointments: allAppointments.length,
        todayAppointments: todaysAppointments.length,
        totalDoctors: doctors.length,
        availableDoctors: 0, // Will be calculated below
      };

      // Fetch patient details for today's appointments in batch
      const uniquePatientIds = Array.from(
        new Set(todaysAppointments.map((a) => a.patientId))
      );

      const patientMap = new Map<string, string>();

      if (uniquePatientIds.length > 0) {
        try {
          // Batch fetch using the new endpoint
          interface PatientBatchResponse {
            patientId?: string;
            id?: string;
            name?: string;
            firstName?: string;
            lastName?: string;
          }
          const res = await api.post<PatientBatchResponse[]>(
            "/patient/Patients/batch",
            uniquePatientIds
          );
          if (res.data) {
            for (const p of res.data) {
              let name = "Unknown Patient";
              // Check if p has name/firstName/lastName properties
              if (p.name) {
                name = p.name;
              } else if (p.firstName || p.lastName) {
                name = `${p.firstName || ""} ${p.lastName || ""}`.trim();
              }
              const pid = p.patientId || p.id; // handle case sensitivity or different model mapping if needed
              if (pid) {
                patientMap.set(pid, name || "Unknown Patient");
              }
            }
          }
        } catch (err) {
          console.error("Failed to batch fetch patients:", err);
          // Fallback or just leave unknown
        }
      }

      // Map today's appointments to QuickAppointment format
      const quickAppointments: QuickAppointment[] = todaysAppointments
        .map((apt) => {
          const scheduledAt = new Date(apt.scheduledAt);
          const scheduledEndAt = apt.scheduledEndAt
            ? new Date(apt.scheduledEndAt)
            : new Date(scheduledAt.getTime() + 30 * 60000);

          const doctorInfo = doctorMap.get(apt.doctorId);
          const patientName =
            patientMap.get(apt.patientId) ||
            `Patient ${apt.patientId.substring(0, 8)}`;

          const aptObj: QuickAppointment = {
            id: apt.id,
            patientName: patientName,
            doctorName: doctorInfo?.name || "Unknown Doctor",
            time: formatTime(scheduledAt),
            type: mapAppointmentType(apt.appointmentType),
            status: mapAppointmentStatus(
              apt.status,
              scheduledAt,
              scheduledEndAt
            ),
          };

          // Only add room if in-person and we want to generate/fetch it (currently skipped to avoid mock data)
          // if (apt.appointmentType === "in-person") {
          //   aptObj.room = ...;
          // }

          return aptObj;
        })
        .sort((a, b) => a.time.localeCompare(b.time));

      // Calculate doctor availability
      const now = new Date();
      const doctorAvailability: DoctorAvailability[] = [];

      for (const [doctorId, doctorInfo] of doctorMap) {
        const doctorAppointments = todaysAppointments.filter(
          (apt) => apt.doctorId === doctorId
        );

        // Find current appointment (in progress)
        let currentPatient: string | undefined;
        let status: DoctorAvailability["status"] = "available";
        let nextAvailable: string | undefined;

        const sortedAppointments = [...doctorAppointments].sort(
          (a, b) =>
            new Date(a.scheduledAt).getTime() -
            new Date(b.scheduledAt).getTime()
        );

        for (const apt of sortedAppointments) {
          const aptStart = new Date(apt.scheduledAt);
          const aptEnd = apt.scheduledEndAt
            ? new Date(apt.scheduledEndAt)
            : new Date(aptStart.getTime() + 30 * 60000);

          if (now >= aptStart && now <= aptEnd) {
            status = "busy";
            currentPatient =
              patientMap.get(apt.patientId) ||
              `Patient ${apt.patientId.substring(0, 8)}`;
            nextAvailable = formatTime(aptEnd);
            break;
          }

          if (aptStart > now && !nextAvailable) {
            nextAvailable = formatTime(aptStart);
          }
        }

        const completedToday = doctorAppointments.filter(
          (apt) => apt.status?.toLowerCase() === "completed"
        ).length;

        const docAvail: DoctorAvailability = {
          id: doctorId,
          name: doctorInfo.name,
          specialization: doctorInfo.specialization,
          status,
          totalAppointments: doctorAppointments.length,
          completedToday,
        };

        if (currentPatient) docAvail.currentPatient = currentPatient;
        if (nextAvailable) docAvail.nextAvailable = nextAvailable;

        doctorAvailability.push(docAvail);
      }

      // Count available doctors
      stats.availableDoctors = doctorAvailability.filter(
        (d) => d.status === "available"
      ).length;

      return {
        stats,
        todayAppointments: quickAppointments,
        doctorAvailability,
      };
    } catch (error) {
      console.error("Error fetching dashboard data:", error);
      throw error; // Propagate error to hook
    }
  }

  /**
   * Get real-time updates for dashboard
   */
  static async getRealtimeUpdates(): Promise<
    Partial<ReceptionistDashboardData>
  > {
    // Re-fetch full data for now; could be optimized with WebSocket later
    return this.getDashboardData();
  }
}
