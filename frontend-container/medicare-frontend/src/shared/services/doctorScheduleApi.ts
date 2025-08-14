import type { DoctorScheduleEvent } from "../../features/scheduler/types/doctorScheduler";

import { ApiResponse, createErrorResponse, createMockResponse } from "./api";

// Mock data for doctor's schedule - synchronized with today's appointments
const mockScheduleData: DoctorScheduleEvent[] = [
  // Today's appointments
  {
    id: "apt-1",
    patientId: "pat-1",
    patientName: "John Smith",
    patientAge: 45,
    patientPhone: "(555) 123-4567",
    patientEmail: "john.smith@email.com",
    appointmentType: "Regular Checkup",
    date: new Date().toISOString().split("T")[0],
    time: "09:00",
    duration: 30,
    status: "scheduled",
    chiefComplaint: "Routine checkup and blood pressure monitoring",
    notes: "",
    medicalHistory: ["Hypertension", "Type 2 Diabetes"],
    allergies: ["Penicillin"],
    currentMedications: ["Metformin", "Lisinopril"],
  },
  {
    id: "apt-2",
    patientId: "pat-2",
    patientName: "Sarah Johnson",
    patientAge: 32,
    patientPhone: "(555) 987-6543",
    patientEmail: "sarah.johnson@email.com",
    appointmentType: "Consultation",
    date: new Date().toISOString().split("T")[0],
    time: "09:30",
    duration: 45,
    status: "scheduled",
    chiefComplaint: "Persistent headaches and dizziness",
    notes: "",
    medicalHistory: ["Migraine"],
    allergies: ["Sulfa drugs"],
    currentMedications: ["Sumatriptan"],
  },
  {
    id: "apt-3",
    patientId: "pat-3",
    patientName: "Michael Brown",
    patientAge: 28,
    patientPhone: "(555) 456-7890",
    appointmentType: "Follow-up",
    date: new Date().toISOString().split("T")[0],
    time: "10:30",
    duration: 30,
    status: "completed",
    chiefComplaint: "Post-surgery follow-up",
    notes: "Patient healing well. Cleared for normal activities.",
    medicalHistory: ["Appendectomy (recent)"],
    allergies: [],
    currentMedications: ["Ibuprofen as needed"],
  },
  {
    id: "apt-4",
    patientId: "pat-4",
    patientName: "Emily Davis",
    patientAge: 55,
    patientPhone: "(555) 234-5678",
    patientEmail: "emily.davis@email.com",
    appointmentType: "Specialist Consultation",
    date: new Date().toISOString().split("T")[0],
    time: "11:00",
    duration: 60,
    status: "scheduled",
    chiefComplaint: "Chest pain evaluation",
    notes: "",
    medicalHistory: ["High cholesterol", "Family history of heart disease"],
    allergies: ["Aspirin"],
    currentMedications: ["Atorvastatin"],
  },
  {
    id: "apt-5",
    patientId: "pat-5",
    patientName: "David Wilson",
    patientAge: 67,
    patientPhone: "(555) 345-6789",
    appointmentType: "Emergency",
    date: new Date().toISOString().split("T")[0],
    time: "14:00",
    duration: 30,
    status: "no-show",
    chiefComplaint: "Severe back pain",
    notes: "Patient did not show up for appointment. Called but no answer.",
    medicalHistory: ["Chronic lower back pain", "Arthritis"],
    allergies: [],
    currentMedications: ["Acetaminophen", "Ibuprofen"],
  },
  {
    id: "apt-6",
    patientId: "pat-6",
    patientName: "Lisa Anderson",
    patientAge: 41,
    patientPhone: "(555) 567-8901",
    patientEmail: "lisa.anderson@email.com",
    appointmentType: "Routine Checkup",
    date: new Date().toISOString().split("T")[0],
    time: "15:30",
    duration: 30,
    status: "scheduled",
    chiefComplaint: "Annual wellness visit",
    notes: "",
    medicalHistory: [],
    allergies: [],
    currentMedications: [],
  },
  // Tomorrow's appointments
  {
    id: "apt-7",
    patientId: "pat-7",
    patientName: "Robert Taylor",
    patientAge: 38,
    patientPhone: "(555) 678-9012",
    appointmentType: "Follow-up",
    date: new Date(Date.now() + 24 * 60 * 60 * 1000)
      .toISOString()
      .split("T")[0],
    time: "09:00",
    duration: 30,
    status: "scheduled",
    chiefComplaint: "Blood pressure medication adjustment",
    medicalHistory: ["Hypertension"],
    allergies: [],
    currentMedications: ["Amlodipine"],
  },
  {
    id: "apt-8",
    patientId: "pat-8",
    patientName: "Jennifer White",
    patientAge: 29,
    patientPhone: "(555) 789-0123",
    patientEmail: "jennifer.white@email.com",
    appointmentType: "Consultation",
    date: new Date(Date.now() + 24 * 60 * 60 * 1000)
      .toISOString()
      .split("T")[0],
    time: "10:00",
    duration: 45,
    status: "scheduled",
    chiefComplaint: "Skin rash and allergic reactions",
    medicalHistory: ["Eczema"],
    allergies: ["Latex", "Shellfish"],
    currentMedications: ["Hydrocortisone cream"],
  },
];

class DoctorScheduleApiService {
  private scheduleData: DoctorScheduleEvent[] = [...mockScheduleData];

  async getDoctorSchedule(
    _doctorId: string,
    startDate?: string,
    endDate?: string
  ): Promise<ApiResponse<DoctorScheduleEvent[]>> {
    try {
      let filteredData = this.scheduleData;

      if (startDate && endDate) {
        filteredData = this.scheduleData.filter((appointment) => {
          const appointmentDate = new Date(appointment.date);
          const start = new Date(startDate);
          const end = new Date(endDate);
          return appointmentDate >= start && appointmentDate <= end;
        });
      }

      return createMockResponse(filteredData);
    } catch (_error) {
      return createErrorResponse("Failed to fetch doctor schedule");
    }
  }

  async getTodaysAppointments(
    _doctorId: string
  ): Promise<ApiResponse<DoctorScheduleEvent[]>> {
    try {
      const today = new Date().toISOString().split("T")[0];
      const todaysAppointments = this.scheduleData.filter(
        (appointment) => appointment.date === today
      );

      return createMockResponse(todaysAppointments);
    } catch (_error) {
      return createErrorResponse("Failed to fetch today's appointments");
    }
  }

  async markAppointmentCompleted(
    appointmentId: string
  ): Promise<ApiResponse<boolean>> {
    try {
      const appointment = this.scheduleData.find(
        (apt) => apt.id === appointmentId
      );
      if (appointment) {
        appointment.status = "completed";
        return createMockResponse(true);
      }
      throw new Error("Appointment not found");
    } catch (_error) {
      return createErrorResponse("Failed to mark appointment as completed");
    }
  }

  async markAppointmentNoShow(
    appointmentId: string
  ): Promise<ApiResponse<boolean>> {
    try {
      const appointment = this.scheduleData.find(
        (apt) => apt.id === appointmentId
      );
      if (appointment) {
        appointment.status = "no-show";
        return createMockResponse(true);
      }
      throw new Error("Appointment not found");
    } catch (_error) {
      return createErrorResponse("Failed to mark appointment as no-show");
    }
  }

  async addAppointmentNotes(
    appointmentId: string,
    notes: string
  ): Promise<ApiResponse<boolean>> {
    try {
      const appointment = this.scheduleData.find(
        (apt) => apt.id === appointmentId
      );
      if (appointment) {
        appointment.notes = notes;
        return createMockResponse(true);
      }
      throw new Error("Appointment not found");
    } catch (_error) {
      return createErrorResponse("Failed to add appointment notes");
    }
  }

  async getAppointmentDetails(
    appointmentId: string
  ): Promise<ApiResponse<DoctorScheduleEvent>> {
    try {
      const appointment = this.scheduleData.find(
        (apt) => apt.id === appointmentId
      );
      if (!appointment) {
        throw new Error("Appointment not found");
      }
      return createMockResponse(appointment);
    } catch (_error) {
      return createErrorResponse("Failed to fetch appointment details");
    }
  }
}

export default new DoctorScheduleApiService();
