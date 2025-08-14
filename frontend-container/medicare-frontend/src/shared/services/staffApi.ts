import type {
  CreateStaffRequest,
  Doctor,
  Receptionist,
  Service,
  Specialization,
  StaffMember,
  UpdateStaffRequest,
} from "../../features/staffManagement/types";

import {
  type ApiResponse,
  createErrorResponse,
  createMockResponse,
} from "./api";

// Mock specializations data
const mockSpecializations: Specialization[] = [
  {
    id: "spec1",
    name: "Cardiology",
    description: "Heart and cardiovascular system",
    serviceName: "Cardiac Care",
    isPrimary: true,
  },
  {
    id: "spec2",
    name: "Neurology",
    description: "Brain and nervous system",
    serviceName: "Neurological Care",
  },
  {
    id: "spec3",
    name: "Dermatology",
    description: "Skin conditions and diseases",
    serviceName: "Skin Care",
  },
  {
    id: "spec4",
    name: "Orthopedics",
    description: "Bones, joints, and muscles",
    serviceName: "Orthopedic Care",
  },
  {
    id: "spec5",
    name: "Internal Medicine",
    description: "General internal medicine",
    serviceName: "General Medicine",
    isPrimary: true,
  },
];

// Mock services data
const mockServices: Service[] = [
  {
    id: "svc1",
    name: "Cardiac Care",
    description: "Comprehensive heart care services",
    durationMinutes: 60,
    isActive: true,
  },
  {
    id: "svc2",
    name: "Neurological Care",
    description: "Brain and nervous system treatment",
    durationMinutes: 45,
    isActive: true,
  },
  {
    id: "svc3",
    name: "Skin Care",
    description: "Dermatological treatments",
    durationMinutes: 30,
    isActive: true,
  },
  {
    id: "svc4",
    name: "Orthopedic Care",
    description: "Bone and joint treatments",
    durationMinutes: 45,
    isActive: true,
  },
  {
    id: "svc5",
    name: "General Medicine",
    description: "Primary care services",
    durationMinutes: 30,
    isActive: true,
  },
];

// Mock staff data
const mockDoctors: Doctor[] = [
  {
    id: "dr1",
    role: "Doctor",
    profile: {
      firstName: "Sarah",
      lastName: "Johnson",
      email: "sarah.johnson@imup.com",
      phone: "+1-555-0101",
      dateOfBirth: "1978-03-15",
      gender: "Female",
      addressLine1: "123 Medical Center Dr",
      city: "Boston",
      state: "MA",
      zipCode: "02115",
      country: "USA",
    },
    licenseNumber: "MD-123456",
    yearsExperience: 15,
    biography:
      "Dr. Sarah Johnson is a board-certified cardiologist with over 15 years of experience in treating cardiovascular diseases. She specializes in interventional cardiology and has published numerous research papers.",
    officeAddress: "Room 301, Cardiology Wing",
    specializations: [
      {
        id: "spec1",
        name: "Cardiology",
        description: "Heart and cardiovascular system",
        serviceName: "Cardiac Care",
        isPrimary: true,
        certifiedDate: "2010-05-20",
      },
    ],
    isActive: true,
    createdAt: "2023-01-15T08:00:00Z",
    updatedAt: "2025-08-10T14:30:00Z",
  },
  {
    id: "dr2",
    role: "Doctor",
    profile: {
      firstName: "Michael",
      lastName: "Chen",
      email: "michael.chen@imup.com",
      phone: "+1-555-0102",
      dateOfBirth: "1985-07-22",
      gender: "Male",
      addressLine1: "456 Healthcare Blvd",
      city: "Boston",
      state: "MA",
      zipCode: "02115",
      country: "USA",
    },
    licenseNumber: "MD-789012",
    yearsExperience: 8,
    biography:
      "Dr. Michael Chen is a neurologist specializing in movement disorders and epilepsy. He completed his fellowship at Massachusetts General Hospital and is actively involved in clinical research.",
    officeAddress: "Room 205, Neurology Department",
    specializations: [
      {
        id: "spec2",
        name: "Neurology",
        description: "Brain and nervous system",
        serviceName: "Neurological Care",
        isPrimary: true,
        certifiedDate: "2017-09-15",
      },
    ],
    isActive: true,
    createdAt: "2023-03-10T09:15:00Z",
    updatedAt: "2025-08-10T16:45:00Z",
  },
  {
    id: "dr3",
    role: "Doctor",
    profile: {
      firstName: "Emily",
      lastName: "Rodriguez",
      email: "emily.rodriguez@imup.com",
      phone: "+1-555-0103",
      dateOfBirth: "1982-11-08",
      gender: "Female",
      addressLine1: "789 Medical Plaza",
      city: "Boston",
      state: "MA",
      zipCode: "02115",
      country: "USA",
    },
    licenseNumber: "MD-345678",
    yearsExperience: 12,
    biography:
      "Dr. Emily Rodriguez is a family medicine physician with expertise in preventive care and chronic disease management. She is passionate about patient education and community health.",
    officeAddress: "Room 150, Family Medicine",
    specializations: [
      {
        id: "spec5",
        name: "Internal Medicine",
        description: "General internal medicine",
        serviceName: "General Medicine",
        isPrimary: true,
        certifiedDate: "2013-06-30",
      },
    ],
    isActive: true,
    createdAt: "2023-02-20T10:30:00Z",
    updatedAt: "2025-08-09T11:20:00Z",
  },
];

const mockReceptionists: Receptionist[] = [
  {
    id: "rec1",
    role: "Receptionist",
    profile: {
      firstName: "Jessica",
      lastName: "Williams",
      email: "jessica.williams@imup.com",
      phone: "+1-555-0201",
      dateOfBirth: "1990-05-12",
      gender: "Female",
      addressLine1: "101 Admin St",
      city: "Boston",
      state: "MA",
      zipCode: "02115",
      country: "USA",
    },
    department: "Front Desk - Main Lobby",
    isActive: true,
    createdAt: "2023-01-20T08:00:00Z",
    updatedAt: "2025-08-08T15:00:00Z",
  },
  {
    id: "rec2",
    role: "Receptionist",
    profile: {
      firstName: "David",
      lastName: "Thompson",
      email: "david.thompson@imup.com",
      phone: "+1-555-0202",
      dateOfBirth: "1988-09-30",
      gender: "Male",
      addressLine1: "202 Staff Ave",
      city: "Boston",
      state: "MA",
      zipCode: "02115",
      country: "USA",
    },
    department: "Appointment Scheduling",
    isActive: true,
    createdAt: "2023-04-05T09:30:00Z",
    updatedAt: "2025-08-07T12:15:00Z",
  },
  {
    id: "rec3",
    role: "Receptionist",
    profile: {
      firstName: "Maria",
      lastName: "Garcia",
      email: "maria.garcia@imup.com",
      phone: "+1-555-0203",
      dateOfBirth: "1992-12-03",
      gender: "Female",
      addressLine1: "303 Office Dr",
      city: "Boston",
      state: "MA",
      zipCode: "02115",
      country: "USA",
    },
    department: "Patient Registration",
    isActive: false, // Inactive staff member for testing
    createdAt: "2023-06-15T11:00:00Z",
    updatedAt: "2025-07-20T16:30:00Z",
  },
];

const mockStaff: StaffMember[] = [...mockDoctors, ...mockReceptionists];

// API Functions
export const staffApi = {
  // Get all staff members
  async getStaff(): Promise<ApiResponse<StaffMember[]>> {
    return createMockResponse(mockStaff);
  },

  // Get staff member by ID
  async getStaffById(id: string): Promise<ApiResponse<StaffMember>> {
    const staff = mockStaff.find((s) => s.id === id);
    if (!staff) {
      return createErrorResponse("Staff member not found");
    }
    return createMockResponse(staff);
  },

  // Get staff by role
  async getStaffByRole(
    role: "Doctor" | "Receptionist"
  ): Promise<ApiResponse<StaffMember[]>> {
    const filteredStaff = mockStaff.filter((s) => s.role === role);
    return createMockResponse(filteredStaff);
  },

  // Create new staff member
  async createStaff(
    data: CreateStaffRequest
  ): Promise<ApiResponse<StaffMember>> {
    const newId = `${data.role.toLowerCase()}${Date.now()}`;
    const now = new Date().toISOString();

    let newStaff: StaffMember;

    if (data.role === "Doctor") {
      // Find specializations by IDs
      const selectedSpecializations = (data.specializations || [])
        .map((specId: string) =>
          mockSpecializations.find((s) => s.id === specId)
        )
        .filter((spec): spec is Specialization => spec !== undefined);

      newStaff = {
        id: newId,
        role: "Doctor",
        profile: data.profile,
        licenseNumber: data.licenseNumber,
        yearsExperience: data.yearsExperience,
        biography: data.biography,
        officeAddress: data.officeAddress,
        specializations: selectedSpecializations,
        isActive: true,
        createdAt: now,
        updatedAt: now,
      } as Doctor;
    } else {
      newStaff = {
        id: newId,
        role: "Receptionist",
        profile: data.profile,
        department: data.department,
        isActive: true,
        createdAt: now,
        updatedAt: now,
      } as Receptionist;
    }

    // Add to mock data
    mockStaff.push(newStaff);

    return createMockResponse(newStaff);
  },

  // Update staff member
  async updateStaff(
    data: UpdateStaffRequest
  ): Promise<ApiResponse<StaffMember>> {
    const index = mockStaff.findIndex((s) => s.id === data.id);
    if (index === -1) {
      return createErrorResponse("Staff member not found");
    }

    const existingStaff = mockStaff[index];
    const now = new Date().toISOString();

    // Update common fields
    const updatedStaff: StaffMember = {
      ...existingStaff,
      profile: {
        ...existingStaff.profile,
        ...data.profile,
      },
      updatedAt: now,
    };

    // Update role-specific fields
    if (updatedStaff.role === "Doctor" && data.role === "Doctor") {
      const doctorStaff = updatedStaff as Doctor;

      if (data.licenseNumber !== undefined)
        doctorStaff.licenseNumber = data.licenseNumber;
      if (data.yearsExperience !== undefined)
        doctorStaff.yearsExperience = data.yearsExperience;
      if (data.biography !== undefined) doctorStaff.biography = data.biography;
      if (data.officeAddress !== undefined)
        doctorStaff.officeAddress = data.officeAddress;

      if (data.specializations) {
        const selectedSpecializations = data.specializations
          .map((specId: string) =>
            mockSpecializations.find((s) => s.id === specId)
          )
          .filter((spec): spec is Specialization => spec !== undefined);
        doctorStaff.specializations = selectedSpecializations;
      }
    } else if (
      updatedStaff.role === "Receptionist" &&
      data.role === "Receptionist"
    ) {
      const receptionistStaff = updatedStaff as Receptionist;
      if (data.department !== undefined)
        receptionistStaff.department = data.department;
    }

    // Update in mock data
    mockStaff[index] = updatedStaff;

    return createMockResponse(updatedStaff);
  },

  // Delete staff member (soft delete - set isActive to false)
  async deleteStaff(id: string): Promise<ApiResponse<boolean>> {
    const index = mockStaff.findIndex((s) => s.id === id);
    if (index === -1) {
      return createErrorResponse("Staff member not found");
    }

    mockStaff[index] = {
      ...mockStaff[index],
      isActive: false,
      updatedAt: new Date().toISOString(),
    };

    return createMockResponse(true);
  },

  // Get available specializations
  async getSpecializations(): Promise<ApiResponse<Specialization[]>> {
    return createMockResponse(mockSpecializations);
  },

  // Get available services
  async getServices(): Promise<ApiResponse<Service[]>> {
    return createMockResponse(mockServices);
  },
};
