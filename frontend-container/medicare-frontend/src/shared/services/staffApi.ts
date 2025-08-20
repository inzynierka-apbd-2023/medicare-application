import { apiClient } from "./apiClient";

// Import types from the existing staff management feature
export type StaffRole = "Doctor" | "Receptionist";

export interface UserProfile {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  dateOfBirth?: string;
  gender?: "Male" | "Female" | "Other";
  avatarUrl?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
}

export interface Doctor {
  id: string;
  profile: UserProfile;
  role: "Doctor";
  licenseNumber?: string;
  yearsExperience?: number;
  biography?: string;
  officeAddress?: string;
  specializations: Specialization[];
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Receptionist {
  id: string;
  profile: UserProfile;
  role: "Receptionist";
  department?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export type StaffMember = Doctor | Receptionist;

export interface Specialization {
  id: string;
  name: string;
  description?: string;
  serviceName: string;
  isPrimary?: boolean;
  certifiedDate?: string;
}

export interface CreateStaffRequest {
  role: StaffRole;
  profile: UserProfile;
  // Doctor-specific fields
  licenseNumber?: string;
  yearsExperience?: number;
  biography?: string;
  officeAddress?: string;
  specializations?: string[];
  // Receptionist-specific fields
  department?: string;
}

export interface UpdateStaffRequest {
  id: string;
  role: StaffRole;
  profile?: Partial<UserProfile>;
  // Doctor-specific fields
  licenseNumber?: string;
  yearsExperience?: number;
  biography?: string;
  officeAddress?: string;
  specializations?: string[];
  // Receptionist-specific fields
  department?: string;
}

export interface StaffSearchRequest {
  role?: StaffRole;
  searchQuery?: string;
  isActive?: boolean;
  specializationIds?: string[];
  page?: number;
  pageSize?: number;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}

export interface CreateDoctorResponse {
  directory: any;
  credentials?: { username: string; password: string };
}

export interface Service {
  id: string;
  name: string;
  description?: string;
  durationMinutes: number;
  isActive: boolean;
}

// Map PractitionerService DoctorsController directory row -> StaffMember (Doctor)
const mapDoctorDirectoryToStaff = (row: any): StaffMember => {
  const specIds = (row.specializations as string | null | undefined)?.split(",").map((s) => s.trim()).filter(Boolean) || [];
  return {
    id: row.doctorId || row.id,
    role: "Doctor",
    profile: {
      firstName: row.firstName || "",
      lastName: row.lastName || "",
      email: row.email || "",
      phone: row.phone,
    },
    // We only have specialization IDs from the directory; names can be joined in the UI using the catalog if needed.
    specializations: specIds.map((id: string) => ({ id, name: "", serviceName: "" })),
  isActive: row.isActive !== undefined ? !!row.isActive : true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  } as Doctor;
};

// (No-op) Previously mapped staff create requests; creation is no longer supported via staff API.

// Helper function to handle API responses
const handleApiResponse = <T>(response: any, mapper?: (data: any) => T): ApiResponse<T> => {
  if (response?.data !== undefined) {
    const backendResponse = response.data;
  if (backendResponse && backendResponse.success !== undefined) {
      // Backend returns ApiResponse format
      return {
        success: backendResponse.success,
        data: mapper && backendResponse.data ? mapper(backendResponse.data) : backendResponse.data,
        message: backendResponse.message,
        errors: backendResponse.errors,
      };
    } else {
      // Direct data response
      return {
        success: true,
  data: mapper ? mapper(response.data) : response.data,
        message: "Success",
      };
    }
  }
  return {
    success: true,
    data: mapper ? mapper(response) : response,
    message: "Success",
  };
};

// Helper function to handle API errors
const handleApiError = <T>(error: any, fallbackData: T): ApiResponse<T> => {
  console.error("Staff API Error:", error);
  
  let errorMessage = "An unexpected error occurred";
  let errors: string[] = [];
  
  if (error.response?.data) {
    const errorData = error.response.data;
    if (errorData.message) {
      errorMessage = errorData.message;
    }
    if (errorData.errors) {
      errors = Array.isArray(errorData.errors) ? errorData.errors : [errorData.errors];
    }
  } else if (error.message) {
    errorMessage = error.message;
    errors = [error.message];
  } else if (error.response?.status === 401) {
    errorMessage = "Unauthorized. Please check your permissions.";
    errors = [errorMessage];
  } else if (error.response?.status === 403) {
    errorMessage = "Forbidden. You don't have access to this resource.";
    errors = [errorMessage];
  } else if (error.response?.status === 404) {
    errorMessage = "Staff member not found.";
    errors = [errorMessage];
  } else if (error.response?.status >= 500) {
    errorMessage = "Server error. Please try again later.";
    errors = [errorMessage];
  }

  return {
    success: false,
    data: fallbackData,
  message: errorMessage,
  errors: errors.length ? errors : [errorMessage],
  };
};

// Real API implementation using apiClient
export const staffApi = {
  // Get all staff members with optional search parameters
  getStaff: async (searchRequest?: StaffSearchRequest): Promise<ApiResponse<StaffMember[]>> => {
    try {
  const params = new URLSearchParams();
  const role = searchRequest?.role;
  // Only Doctors are supported now
  if (searchRequest?.searchQuery) params.append("q", searchRequest.searchQuery);
  if (searchRequest?.specializationIds?.length) params.append("specializationId", searchRequest.specializationIds[0]);
  if (typeof searchRequest?.isActive === "boolean") params.append("isActive", String(searchRequest.isActive));
  const query = params.toString();
  const url = `/practitioner/doctors${query ? "?" + query : ""}`;
  const response = await apiClient.get(url);
  // Response is DoctorDirectory[]
  return handleApiResponse<StaffMember[]>(response, (data: any[]) => (role && role !== "Doctor" ? [] : data.map(mapDoctorDirectoryToStaff)));
    } catch (error) {
      return handleApiError<StaffMember[]>(error, []);
    }
  },

  // Get staff members - alias for compatibility
  getStaffMembers: async (searchRequest?: StaffSearchRequest): Promise<ApiResponse<StaffMember[]>> => {
    return staffApi.getStaff(searchRequest);
  },

  // Get a single staff member by ID
  getStaffById: async (id: string): Promise<ApiResponse<StaffMember | null>> => {
    try {
  const response = await apiClient.get(`/practitioner/doctors/${id}/directory`);
  return handleApiResponse<StaffMember | null>(response, (data: any) => (data ? mapDoctorDirectoryToStaff(data) : null));
    } catch (error) {
      return handleApiError<StaffMember | null>(error, null);
    }
  },

  // Get staff member - alias for compatibility
  getStaffMember: async (id: string): Promise<ApiResponse<StaffMember | null>> => {
    return staffApi.getStaffById(id);
  },

  // Get staff members by role
  getStaffByRole: async (role: StaffRole): Promise<ApiResponse<StaffMember[]>> => {
    try {
      if (role !== "Doctor") {
        return { success: true, data: [], message: "Only doctors supported" };
      }
      const response = await apiClient.get(`/practitioner/doctors`);
      return handleApiResponse<StaffMember[]>(response, (data: any[]) => data.map(mapDoctorDirectoryToStaff));
    } catch (error) {
      return handleApiError<StaffMember[]>(error, []);
    }
  },

  // Create a new staff member
  createStaff: async (_request: CreateStaffRequest): Promise<ApiResponse<StaffMember>> => {
    try {
      if (_request.role !== "Doctor") {
        return { success: false, data: {} as StaffMember, message: "Only creating doctors is supported." };
      }
      // Call new practitioner endpoint to create user+doctor
      const payload = {
        profile: {
          firstName: _request.profile.firstName,
          lastName: _request.profile.lastName,
          email: _request.profile.email,
          phone: _request.profile.phone,
          dateOfBirth: _request.profile.dateOfBirth,
          gender: _request.profile.gender,
          addressLine1: _request.profile.addressLine1,
          addressLine2: _request.profile.addressLine2,
          city: _request.profile.city,
          state: _request.profile.state,
          zipCode: _request.profile.zipCode,
          country: _request.profile.country,
        },
        biography: _request.biography,
        specializationIds: _request.specializations,
      };
      const res = await apiClient.post(`/practitioner/doctors/register-full`, payload);
      const out = res.data as CreateDoctorResponse;
      const staff = mapDoctorDirectoryToStaff(out.directory) as Doctor;
      if (out.credentials) {
        (staff as any).credentials = out.credentials;
      }
      return { success: true, data: staff, message: out.credentials ? `Created. Username: ${out.credentials.username} Password: ${out.credentials.password}` : "Created" };
    } catch (error) {
      return handleApiError<StaffMember>(error, {} as StaffMember);
    }
  },

  // Create staff member - alias for compatibility
  createStaffMember: async (request: CreateStaffRequest): Promise<ApiResponse<StaffMember>> => {
    return staffApi.createStaff(request);
  },

  // Update an existing staff member
  updateStaff: async (id: string, request: UpdateStaffRequest): Promise<ApiResponse<StaffMember>> => {
    try {
      // Support updating doctor specializations only
      if (request.role === "Doctor" && request.specializations?.length) {
        const specIds = request.specializations;
        await apiClient.put(`/practitioner/doctors/${id}/specializations`, { specializationIds: specIds });
      }
      // Fetch latest directory row
      const refreshed = await apiClient.get(`/practitioner/doctors/${id}/directory`);
      return handleApiResponse<StaffMember>(refreshed, mapDoctorDirectoryToStaff);
    } catch (error) {
      return handleApiError<StaffMember>(error, {} as StaffMember);
    }
  },

  // Update staff member - alias for compatibility
  updateStaffMember: async (id: string, request: UpdateStaffRequest): Promise<ApiResponse<StaffMember>> => {
    return staffApi.updateStaff(id, request);
  },

  // Delete a staff member (soft delete)
  deleteStaff: async (id: string): Promise<ApiResponse<boolean>> => {
    try {
      const res = await apiClient.delete(`/practitioner/doctors/${id}`);
      // Expect 204 No Content
      return { success: res.status >= 200 && res.status < 300, data: true };
    } catch (error) {
      return handleApiError<boolean>(error, false);
    }
  },

  // Delete staff member - alias for compatibility
  deleteStaffMember: async (id: string): Promise<ApiResponse<boolean>> => {
    return staffApi.deleteStaff(id);
  },

  // Get available specializations
  getSpecializations: async (): Promise<ApiResponse<Specialization[]>> => {
    try {
  const response = await apiClient.get(`/practitioner/catalog/specializations`);
  return handleApiResponse<Specialization[]>(response, (items: any[]) => items.map((s) => ({ id: s.id, name: s.name, serviceName: "" })));
    } catch (error) {
      return handleApiError<Specialization[]>(error, []);
    }
  },

  // Get available services
  getServices: async (): Promise<ApiResponse<Service[]>> => {
    try {
  const response = await apiClient.get(`/practitioner/catalog/services`);
  return handleApiResponse<Service[]>(response);
    } catch (error) {
      return handleApiError<Service[]>(error, []);
    }
  },

  // Availability (schedules)
  getAvailability: async (doctorId: string): Promise<ApiResponse<any[]>> => {
    try {
      const res = await apiClient.get(`/practitioner/doctors/${doctorId}/availability`);
      return handleApiResponse<any[]>(res);
    } catch (error) {
      return handleApiError<any[]>(error, []);
    }
  },

  setAvailability: async (doctorId: string, entries: { dayOfWeek: number; start: string; end: string }[]): Promise<ApiResponse<boolean>> => {
    try {
      const res = await apiClient.put(`/practitioner/doctors/${doctorId}/availability`, entries);
      return { success: res.status >= 200 && res.status < 300, data: true };
    } catch (error) {
      return handleApiError<boolean>(error, false);
    }
  },
};
