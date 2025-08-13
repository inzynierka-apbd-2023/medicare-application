import type {
  Appointment,
  AppointmentStatus,
  Doctor,
  DoctorSchedule,
  Patient,
  Service,
  Specialization,
  TimeSlot,
} from "../types";

// Mock Specializations
export const mockSpecializations: Specialization[] = [
  {
    id: "spec-1",
    name: "Cardiology",
    description: "Heart and cardiovascular system care",
    serviceId: "service-1",
    service: {
      id: "service-1",
      name: "Cardiac Consultation",
      description: "Comprehensive heart health evaluation",
      durationMinutes: 45,
      isActive: true,
    },
    isActive: true,
  },
  {
    id: "spec-2",
    name: "Dermatology",
    description: "Skin, hair, and nail care",
    serviceId: "service-2",
    service: {
      id: "service-2",
      name: "Skin Examination",
      description: "Comprehensive skin health check",
      durationMinutes: 30,
      isActive: true,
    },
    isActive: true,
  },
  {
    id: "spec-3",
    name: "Internal Medicine",
    description: "General adult medical care",
    serviceId: "service-3",
    service: {
      id: "service-3",
      name: "General Consultation",
      description: "Routine health checkup and consultation",
      durationMinutes: 30,
      isActive: true,
    },
    isActive: true,
  },
  {
    id: "spec-4",
    name: "Orthopedics",
    description: "Musculoskeletal system care",
    serviceId: "service-4",
    service: {
      id: "service-4",
      name: "Orthopedic Consultation",
      description: "Bone, joint, and muscle evaluation",
      durationMinutes: 40,
      isActive: true,
    },
    isActive: true,
  },
];

// Mock Services
export const mockServices: Service[] = [
  {
    id: "service-1",
    name: "Cardiac Consultation",
    description: "Comprehensive heart health evaluation",
    durationMinutes: 45,
    isActive: true,
  },
  {
    id: "service-2",
    name: "Skin Examination",
    description: "Comprehensive skin health check",
    durationMinutes: 30,
    isActive: true,
  },
  {
    id: "service-3",
    name: "General Consultation",
    description: "Routine health checkup and consultation",
    durationMinutes: 30,
    isActive: true,
  },
  {
    id: "service-4",
    name: "Orthopedic Consultation",
    description: "Bone, joint, and muscle evaluation",
    durationMinutes: 40,
    isActive: true,
  },
  {
    id: "service-5",
    name: "Follow-up Visit",
    description: "Follow-up consultation for ongoing treatment",
    durationMinutes: 20,
    isActive: true,
  },
  {
    id: "service-6",
    name: "Emergency Consultation",
    description: "Urgent medical consultation",
    durationMinutes: 60,
    isActive: true,
  },
];

// Mock Doctors
export const mockDoctors: Doctor[] = [
  {
    id: "doctor-1",
    firstName: "Sarah",
    lastName: "Johnson",
    email: "sarah.johnson@hospital.com",
    phone: "+1-555-0101",
    licenseNumber: "MD-12345",
    yearsExperience: 12,
    biography:
      "Dr. Sarah Johnson is a board-certified cardiologist with over 12 years of experience in treating heart conditions.",
    officeAddress: "123 Medical Center Dr, Suite 200",
    specializations: [mockSpecializations[0]], // Cardiology
  },
  {
    id: "doctor-2",
    firstName: "Michael",
    lastName: "Chen",
    email: "michael.chen@hospital.com",
    phone: "+1-555-0102",
    licenseNumber: "MD-23456",
    yearsExperience: 8,
    biography:
      "Dr. Michael Chen specializes in dermatology and cosmetic procedures.",
    officeAddress: "123 Medical Center Dr, Suite 150",
    specializations: [mockSpecializations[1]], // Dermatology
  },
  {
    id: "doctor-3",
    firstName: "Emily",
    lastName: "Rodriguez",
    email: "emily.rodriguez@hospital.com",
    phone: "+1-555-0103",
    licenseNumber: "MD-34567",
    yearsExperience: 15,
    biography:
      "Dr. Emily Rodriguez is an experienced internal medicine physician.",
    officeAddress: "123 Medical Center Dr, Suite 100",
    specializations: [mockSpecializations[2]], // Internal Medicine
  },
  {
    id: "doctor-4",
    firstName: "David",
    lastName: "Thompson",
    email: "david.thompson@hospital.com",
    phone: "+1-555-0104",
    licenseNumber: "MD-45678",
    yearsExperience: 10,
    biography:
      "Dr. David Thompson specializes in orthopedic surgery and sports medicine.",
    officeAddress: "123 Medical Center Dr, Suite 300",
    specializations: [mockSpecializations[3]], // Orthopedics
  },
];

// Mock Appointment Statuses
export const mockAppointmentStatuses: AppointmentStatus[] = [
  {
    id: "status-1",
    name: "Scheduled",
    description: "Appointment is scheduled",
    colorCode: "#3b82f6", // Blue
  },
  {
    id: "status-2",
    name: "Confirmed",
    description: "Appointment is confirmed by patient",
    colorCode: "#10b981", // Green
  },
  {
    id: "status-3",
    name: "Cancelled",
    description: "Appointment was cancelled",
    colorCode: "#ef4444", // Red
  },
  {
    id: "status-4",
    name: "Completed",
    description: "Appointment was completed",
    colorCode: "#8b5cf6", // Purple
  },
  {
    id: "status-5",
    name: "No Show",
    description: "Patient did not show up",
    colorCode: "#f59e0b", // Orange
  },
];

// Mock Patient
export const mockCurrentPatient: Patient = {
  id: "patient-1",
  firstName: "John",
  lastName: "Doe",
  email: "john.doe@email.com",
  phone: "+1-555-0199",
  dateOfBirth: "1985-05-15",
  gender: "Male",
  medicalRecordNumber: "MRN-001234",
  bloodType: "A+",
  height: 175,
  weight: 80,
};

// Generate mock time slots for the next 30 days
export const generateMockTimeSlots = (): TimeSlot[] => {
  const slots: TimeSlot[] = [];
  const today = new Date();

  // Generate slots for the next 30 days
  for (let dayOffset = 1; dayOffset <= 30; dayOffset++) {
    const date = new Date(today);
    date.setDate(today.getDate() + dayOffset);

    // Skip weekends for most doctors
    if (date.getDay() === 0 || date.getDay() === 6) continue;

    // Generate time slots from 8 AM to 5 PM
    for (let hour = 8; hour < 17; hour++) {
      for (let minute = 0; minute < 60; minute += 30) {
        mockDoctors.forEach((doctor) => {
          // Create some variation in availability
          const isAvailable = Math.random() > 0.3; // 70% availability

          if (isAvailable) {
            const startTime = new Date(date);
            startTime.setHours(hour, minute, 0, 0);

            const endTime = new Date(startTime);
            endTime.setMinutes(startTime.getMinutes() + 30);

            slots.push({
              id: `slot-${doctor.id}-${date.toISOString().split("T")[0]}-${hour}-${minute}`,
              doctorId: doctor.id,
              startDateTime: startTime.toISOString(),
              endDateTime: endTime.toISOString(),
              isAvailable: true,
              durationMinutes: 30,
              slotType: "Regular",
            });
          }
        });
      }
    }
  }

  return slots;
};

// Mock appointments - mix of upcoming and past
export const generateMockAppointments = (): Appointment[] => {
  const appointments: Appointment[] = [];
  const today = new Date();

  // Past appointments
  for (let i = 1; i <= 5; i++) {
    const pastDate = new Date(today);
    pastDate.setDate(today.getDate() - i * 7); // Weekly past appointments
    pastDate.setHours(8 + i, 0, 0, 0);

    const endDate = new Date(pastDate);
    endDate.setMinutes(pastDate.getMinutes() + 30);

    appointments.push({
      id: `appointment-past-${i}`,
      scheduleId: `schedule-${i}`,
      timeSlotId: `slot-past-${i}`,
      day: pastDate.toISOString(),
      durationMinutes: 30,
      room: `Room ${100 + i}`,
      description: `Past consultation ${i}`,
      appointmentType: i % 2 === 0 ? "in-person" : "virtual",
      doctorUserId: mockDoctors[i % mockDoctors.length].id,
      patientUserId: mockCurrentPatient.id,
      statusId: "status-4", // Completed
      totalCost: 150 + i * 25,
      createdAt: new Date(pastDate.getTime() - 86400000).toISOString(), // Created 1 day before
      updatedAt: pastDate.toISOString(),
    });
  }

  // Upcoming appointments
  for (let i = 1; i <= 8; i++) {
    const futureDate = new Date(today);
    futureDate.setDate(today.getDate() + i * 3); // Every 3 days
    futureDate.setHours(8 + ((i * 2) % 9), (i % 2) * 30, 0, 0);

    const endDate = new Date(futureDate);
    endDate.setMinutes(futureDate.getMinutes() + 30);

    appointments.push({
      id: `appointment-future-${i}`,
      scheduleId: `schedule-future-${i}`,
      timeSlotId: `slot-future-${i}`,
      day: futureDate.toISOString(),
      durationMinutes: 30,
      room: `Room ${200 + i}`,
      description: `Upcoming consultation ${i}`,
      appointmentType: ["in-person", "virtual", "phone"][i % 3] as
        | "in-person"
        | "virtual"
        | "phone",
      doctorUserId: mockDoctors[i % mockDoctors.length].id,
      patientUserId: mockCurrentPatient.id,
      statusId: i % 2 === 0 ? "status-1" : "status-2", // Mix of scheduled and confirmed
      totalCost: 150 + i * 20,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    });
  }

  // Today's appointment
  const todayAppointment = new Date(today);
  todayAppointment.setHours(14, 30, 0, 0);

  appointments.push({
    id: "appointment-today",
    scheduleId: "schedule-today",
    timeSlotId: "slot-today",
    day: todayAppointment.toISOString(),
    durationMinutes: 45,
    room: "Room 150",
    description: "Today's checkup appointment",
    appointmentType: "in-person",
    doctorUserId: mockDoctors[0].id,
    patientUserId: mockCurrentPatient.id,
    statusId: "status-2", // Confirmed
    totalCost: 200,
    createdAt: new Date(today.getTime() - 172800000).toISOString(), // Created 2 days ago
    updatedAt: new Date().toISOString(),
  });

  return appointments;
};

// Mock Doctor Schedules
export const mockDoctorSchedules: DoctorSchedule[] = [
  // Dr. Sarah Johnson - Cardiology
  {
    id: "schedule-1-1",
    doctorId: "doctor-1",
    dayOfWeek: 1, // Monday
    startTime: "08:00",
    endTime: "17:00",
    isAvailable: true,
    validFrom: "2025-01-01",
    validTo: "2025-12-31",
    breakStartTime: "12:00",
    breakEndTime: "13:00",
  },
  {
    id: "schedule-1-3",
    doctorId: "doctor-1",
    dayOfWeek: 3, // Wednesday
    startTime: "08:00",
    endTime: "17:00",
    isAvailable: true,
    validFrom: "2025-01-01",
    validTo: "2025-12-31",
    breakStartTime: "12:00",
    breakEndTime: "13:00",
  },
  {
    id: "schedule-1-5",
    doctorId: "doctor-1",
    dayOfWeek: 5, // Friday
    startTime: "08:00",
    endTime: "15:00",
    isAvailable: true,
    validFrom: "2025-01-01",
    validTo: "2025-12-31",
    breakStartTime: "12:00",
    breakEndTime: "13:00",
  },
  // Dr. Michael Chen - Dermatology
  {
    id: "schedule-2-2",
    doctorId: "doctor-2",
    dayOfWeek: 2, // Tuesday
    startTime: "08:00",
    endTime: "16:00",
    isAvailable: true,
    validFrom: "2025-01-01",
    validTo: "2025-12-31",
    breakStartTime: "12:00",
    breakEndTime: "13:00",
  },
  {
    id: "schedule-2-4",
    doctorId: "doctor-2",
    dayOfWeek: 4, // Thursday
    startTime: "08:00",
    endTime: "16:00",
    isAvailable: true,
    validFrom: "2025-01-01",
    validTo: "2025-12-31",
    breakStartTime: "12:00",
    breakEndTime: "13:00",
  },
  // Add schedules for other doctors...
];

export const mockTimeSlots = generateMockTimeSlots();
export const mockAppointments = generateMockAppointments();
