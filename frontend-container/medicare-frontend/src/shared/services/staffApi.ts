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

export interface Service {
  id: string;
  name: string;
  description?: string;
  durationMinutes: number;
  isActive: boolean;
}

// Helper function to map backend DTO to frontend types
const mapBackendStaffToFrontend = (backendStaff: any): StaffMember => {
  const baseStaff = {
    id: backendStaff.id,
    profile: {
      firstName: backendStaff.profile?.firstName || "",
      lastName: backendStaff.profile?.lastName || "",
      email: backendStaff.profile?.email || "",
      phone: backendStaff.profile?.phone,
      dateOfBirth: backendStaff.profile?.dateOfBirth,
      gender: backendStaff.profile?.gender as "Male" | "Female" | "Other" | undefined,
      avatarUrl: backendStaff.profile?.avatarUrl,
      addressLine1: backendStaff.profile?.addressLine1,
      addressLine2: backendStaff.profile?.addressLine2,
      city: backendStaff.profile?.city,
      state: backendStaff.profile?.state,
      zipCode: backendStaff.profile?.zipCode,
      country: backendStaff.profile?.country,
    },
    isActive: backendStaff.isActive ?? true,
    createdAt: backendStaff.createdAt || new Date().toISOString(),
    updatedAt: backendStaff.updatedAt || new Date().toISOString(),
  };

  if (backendStaff.role === "Doctor") {
    return {
      ...baseStaff,
      role: "Doctor" as const,
      licenseNumber: backendStaff.licenseNumber,
      yearsExperience: backendStaff.yearsExperience,
      biography: backendStaff.biography,
      officeAddress: backendStaff.officeAddress,
      specializations: backendStaff.specializations || [],
    } as Doctor;
  } else {
    return {
      ...baseStaff,
      role: "Receptionist" as const,
      department: backendStaff.department,
    } as Receptionist;
  }
};

// Helper function to map frontend create request to backend format
const mapCreateRequestToBackend = (request: CreateStaffRequest): any => {
  return {
    role: request.role,
    profile: {
      firstName: request.profile.firstName,
      lastName: request.profile.lastName,
      email: request.profile.email,
      phone: request.profile.phone,
      dateOfBirth: request.profile.dateOfBirth || new Date().toISOString(),
      gender: request.profile.gender || "Other",
      addressLine1: request.profile.addressLine1 || "",
      addressLine2: request.profile.addressLine2,
      city: request.profile.city || "",
      state: request.profile.state || "",
      zipCode: request.profile.zipCode || "",
      country: request.profile.country || "USA",
    },
    licenseNumber: request.licenseNumber,
    yearsExperience: request.yearsExperience,
    biography: request.biography,
    officeAddress: request.officeAddress,
    specializations: request.specializations,
    department: request.department,
  };
};

// Helper function to handle API responses
const handleApiResponse = <T>(response: any, mapper?: (data: any) => T): ApiResponse<T> => {
  if (response.data) {
    const backendResponse = response.data;
    if (backendResponse.success !== undefined) {
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
    errors,
  };
};

// Real API implementation using apiClient
export const staffApi = {
  // Get all staff members with optional search parameters
  getStaff: async (searchRequest?: StaffSearchRequest): Promise<ApiResponse<StaffMember[]>> => {
    try {
      const params = new URLSearchParams();
      
      if (searchRequest?.role) params.append("role", searchRequest.role);
      if (searchRequest?.searchQuery) params.append("searchQuery", searchRequest.searchQuery);
      if (searchRequest?.isActive !== undefined) params.append("isActive", searchRequest.isActive.toString());
      if (searchRequest?.page) params.append("page", searchRequest.page.toString());
      if (searchRequest?.pageSize) params.append("pageSize", searchRequest.pageSize.toString());
      if (searchRequest?.specializationIds?.length) {
        searchRequest.specializationIds.forEach(id => params.append("specializationIds", id));
      }

      const queryString = params.toString();
      const url = `/practitioner/staff${queryString ? `?${queryString}` : ""}`;
      
      const response = await apiClient.get(url);
      return handleApiResponse<StaffMember[]>(response, (data: any[]) => 
        data.map(mapBackendStaffToFrontend)
      );
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
      const response = await apiClient.get(`/practitioner/staff/${id}`);
      return handleApiResponse<StaffMember | null>(response, (data: any) => 
        data ? mapBackendStaffToFrontend(data) : null
      );
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
      const response = await apiClient.get(`/practitioner/staff/role/${role}`);
      return handleApiResponse<StaffMember[]>(response, (data: any[]) => 
        data.map(mapBackendStaffToFrontend)
      );
    } catch (error) {
      return handleApiError<StaffMember[]>(error, []);
    }
  },

  // Create a new staff member
  createStaff: async (request: CreateStaffRequest): Promise<ApiResponse<StaffMember>> => {
    try {
      const backendRequest = mapCreateRequestToBackend(request);
      const response = await apiClient.post(`/practitioner/staff`, backendRequest);
      return handleApiResponse<StaffMember>(response, mapBackendStaffToFrontend);
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
      // Ensure the ID is set in the request
      const updateRequest = { ...request, id };
      const response = await apiClient.put(`/practitioner/staff/${id}`, updateRequest);
      return handleApiResponse<StaffMember>(response, mapBackendStaffToFrontend);
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
      const response = await apiClient.delete(`/practitioner/staff/${id}`);
      return handleApiResponse<boolean>(response);
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
      const response = await apiClient.get(`/practitioner/staff/specializations`);
      return handleApiResponse<Specialization[]>(response);
    } catch (error) {
      return handleApiError<Specialization[]>(error, []);
    }
  },

  // Get available services
  getServices: async (): Promise<ApiResponse<Service[]>> => {
    try {
      const response = await apiClient.get(`/practitioner/staff/services`);
      return handleApiResponse<Service[]>(response);
    } catch (error) {
      return handleApiError<Service[]>(error, []);
    }
  },
};
