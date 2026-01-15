import { apiClient } from "./apiClient";
import { usersApi } from "./usersApi";

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
  credentials?: { username: string; password: string };
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
  directory: DirectoryRow;
  credentials?: { username: string; password: string };
}

export interface Service {
  id: string;
  name: string;
  description?: string;
  durationMinutes: number;
  isActive: boolean;
}

export interface AvailabilityEntry {
  dayOfWeek: number;
  start: string;
  end: string;
}

export interface DirectoryRow {
  doctorId?: string;
  id?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  specializations?: string;
  isActive?: boolean;
}

// Map PractitionerService DoctorsController directory row -> StaffMember (Doctor)
const mapDoctorDirectoryToStaff = (row: unknown): StaffMember => {
  const r = row as DirectoryRow;
  const specIds =
    r.specializations
      ?.split(",")
      .map((s) => s.trim())
      .filter(Boolean) || [];
  return {
    id: r.doctorId || r.id || "",
    role: "Doctor",
    profile: {
      firstName: r.firstName || "",
      lastName: r.lastName || "",
      email: r.email || "",
      phone: r.phone,
    },
    // We only have specialization IDs from the directory; names can be joined in the UI using the catalog if needed.
    specializations: specIds.map((id: string) => ({
      id,
      name: "",
      serviceName: "",
    })),
    isActive: r.isActive !== undefined ? !!r.isActive : true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  } as Doctor;
};

// (No-op) Previously mapped staff create requests; creation is no longer supported via staff API.

// Helper function to handle API responses
const handleApiResponse = <T>(
  response:
    | {
        data?:
          | { success?: boolean; data?: T; message?: string; errors?: string[] }
          | T;
      }
    | unknown,
  mapper?: (data: unknown) => T
): ApiResponse<T> => {
  if (response && typeof response === "object" && "data" in response) {
    const backendResponse = (response as { data: unknown }).data as {
      success?: boolean;
      data?: unknown;
      message?: string;
      errors?: string[];
    };
    if (
      backendResponse &&
      typeof backendResponse === "object" &&
      "success" in backendResponse
    ) {
      // Backend returns ApiResponse format
      return {
        success: !!backendResponse.success,
        data:
          mapper && backendResponse.data
            ? mapper(backendResponse.data)
            : (backendResponse.data as T),
        ...(backendResponse.message
          ? { message: backendResponse.message }
          : {}),
        ...(backendResponse.errors ? { errors: backendResponse.errors } : {}),
      };
    } else {
      // Direct data response
      return {
        success: true,
        data: mapper ? mapper(backendResponse) : (backendResponse as T),
        message: "Success",
      };
    }
  }
  return {
    success: true,
    data: mapper ? mapper(response) : (response as T),
    message: "Success",
  };
};

// Helper function to handle API errors
const handleApiError = <T>(error: unknown, fallbackData: T): ApiResponse<T> => {
  console.error("Staff API Error:", error);

  let errorMessage = "An unexpected error occurred";
  let errors: string[] = [];

  const err = error as {
    response?: {
      data?: { message?: string; errors?: string[] | string };
      status?: number;
    };
    message?: string;
  };

  if (err.response?.data) {
    const errorData = err.response.data;
    if (errorData.message) {
      errorMessage = errorData.message;
    }
    if (errorData.errors) {
      errors = Array.isArray(errorData.errors)
        ? errorData.errors
        : typeof errorData.errors === "string"
          ? [errorData.errors]
          : [];
    }
  } else if (err.message) {
    errorMessage = err.message;
    errors = [err.message];
  } else if (err.response?.status === 401) {
    errorMessage = "Unauthorized. Please check your permissions.";
    errors = [errorMessage];
  } else if (err.response?.status === 403) {
    errorMessage = "Forbidden. You don't have access to this resource.";
    errors = [errorMessage];
  } else if (err.response?.status === 404) {
    errorMessage = "Staff member not found.";
    errors = [errorMessage];
  } else if (err.response?.status && err.response.status >= 500) {
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
  getStaff: async (
    searchRequest?: StaffSearchRequest
  ): Promise<ApiResponse<StaffMember[]>> => {
    try {
      const role = searchRequest?.role;
      const isActive = searchRequest?.isActive;
      const searchQuery = searchRequest?.searchQuery?.toLowerCase();

      const promises: Promise<StaffMember[]>[] = [];

      // 1. Fetch Doctors (search supported by backend)
      if (!role || role === "Doctor") {
        const params = new URLSearchParams();
        if (searchRequest?.searchQuery)
          params.append("q", searchRequest.searchQuery);
        if (searchRequest?.specializationIds?.length)
          params.append("specializationId", searchRequest.specializationIds[0]);
        if (typeof isActive === "boolean")
          params.append("isActive", String(isActive));
        const query = params.toString();
        const url = `/practitioner/Doctors${query ? "?" + query : ""}`;

        promises.push(
          apiClient
            .get(url)
            .then((res) => {
              const data = res.data;
              // Verify success structure or direct array
              const items = Array.isArray(data)
                ? data
                : data.success && Array.isArray(data.data)
                  ? data.data
                  : [];
              return items.map(mapDoctorDirectoryToStaff);
            })
            .catch((err) => {
              console.error("Failed to fetch doctors:", err);
              return [];
            })
        );
      }

      // 2. Fetch Receptionists (manual aggregation)
      if (!role || role === "Receptionist") {
        promises.push(
          staffApi.getReceptionists().then((res) => res.data || [])
        );
      }

      const results = await Promise.all(promises);
      let allStaff = results.flat();

      // Post-filtering for Receptionists (since backend doesn't support search yet)
      // Doctors are already filtered by backend if query param was sent
      if (searchQuery && (!role || role === "Receptionist")) {
        // We only need to filter receptionists in JS if we fetched them
        // But since we merged them, we might be re-filtering doctors. Ideally distinct.
        // Actually, for simplicity: existing doctor properties match UserProfile fields.
        if (role === "Receptionist" || !role) {
          // If we fetched receptionists, we need to filter them.
          // Doctors are already filtered by backend search.
          // However, to keep it consistent, if searchQuery is present, we filter the *receptionist* portion.
          allStaff = allStaff.filter((s) => {
            if (s.role === "Doctor") return true; // Already filtered by backend
            const fullName =
              `${s.profile.firstName} ${s.profile.lastName}`.toLowerCase();
            const department =
              (s as Receptionist).department?.toLowerCase() || "";
            return (
              fullName.includes(searchQuery) ||
              s.profile.email.toLowerCase().includes(searchQuery) ||
              department.includes(searchQuery)
            );
          });
        }
      }

      if (typeof isActive === "boolean") {
        // Filter receptionists for active status (doctors filtered by backend)
        allStaff = allStaff.filter((s) =>
          s.role === "Doctor" ? true : s.isActive === isActive
        );
      }

      return { success: true, data: allStaff, message: "Success" };
    } catch (error) {
      return handleApiError<StaffMember[]>(error, []);
    }
  },

  // Get all receptionists (helper)
  getReceptionists: async (): Promise<ApiResponse<Receptionist[]>> => {
    try {
      const response = await apiClient.get<unknown>(
        "/practitioner/Receptionists"
      );
      const receptionistsRaw = Array.isArray(response.data)
        ? response.data
        : ((response.data as { data: unknown })?.data as unknown[]) || [];

      if (!Array.isArray(receptionistsRaw)) return { success: true, data: [] };

      const receptionists = await Promise.all(
        receptionistsRaw.map(async (r: unknown) => {
          try {
            const row = r as {
              userId: string;
              id: string;
              createdAt: string;
              updatedAt: string;
            };
            const user = await usersApi.getUser(row.userId);
            return {
              id: row.id,
              role: "Receptionist",
              profile: {
                firstName: user.firstName,
                lastName: user.lastName,
                email: user.email,
                phone: user.phoneNumber,
                dateOfBirth: user.dateOfBirth,
                addressLine1: user.address,
              },
              department: "General", // Placeholder as it's not in DB yet
              isActive: true, // Assuming active if returned
              createdAt: row.createdAt,
              updatedAt: row.updatedAt,
            } as Receptionist;
          } catch (e) {
            console.error(
              `Failed to fetch user for receptionist ${(r as { id: string }).id}`,
              e
            );
            return null;
          }
        })
      );

      return {
        success: true,
        data: receptionists.filter(Boolean) as Receptionist[],
      };
    } catch (error) {
      return handleApiError<Receptionist[]>(error, []);
    }
  },

  // Get staff members - alias for compatibility
  getStaffMembers: async (
    searchRequest?: StaffSearchRequest
  ): Promise<ApiResponse<StaffMember[]>> => {
    return staffApi.getStaff(searchRequest);
  },

  // Get a single staff member by ID
  getStaffById: async (
    id: string
  ): Promise<ApiResponse<StaffMember | null>> => {
    try {
      const response = await apiClient.get(
        `/practitioner/Doctors/${id}/directory`
      );
      return handleApiResponse<StaffMember | null>(response, (data: unknown) =>
        data ? mapDoctorDirectoryToStaff(data) : null
      );
    } catch (error) {
      return handleApiError<StaffMember | null>(error, null);
    }
  },

  // Get staff member - alias for compatibility
  getStaffMember: async (
    id: string
  ): Promise<ApiResponse<StaffMember | null>> => {
    return staffApi.getStaffById(id);
  },

  // Get staff members by role
  getStaffByRole: async (
    role: StaffRole
  ): Promise<ApiResponse<StaffMember[]>> => {
    try {
      if (role !== "Doctor") {
        return { success: true, data: [], message: "Only doctors supported" };
      }
      const response = await apiClient.get(`/practitioner/Doctors`);
      return handleApiResponse<StaffMember[]>(response, (data: unknown) =>
        (data as unknown[]).map(mapDoctorDirectoryToStaff)
      );
    } catch (error) {
      return handleApiError<StaffMember[]>(error, []);
    }
  },

  // Create a new staff member
  createStaff: async (
    _request: CreateStaffRequest
  ): Promise<ApiResponse<StaffMember>> => {
    try {
      if (_request.role !== "Doctor") {
        return {
          success: false,
          data: {} as StaffMember,
          message: "Only creating doctors is supported.",
        };
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
      const res = await apiClient.post(
        `/practitioner/Doctors/register-full`,
        payload
      );
      const out = res.data as CreateDoctorResponse;
      const staff = mapDoctorDirectoryToStaff(out.directory) as Doctor;
      if (out.credentials) {
        staff.credentials = out.credentials;
      }
      return {
        success: true,
        data: staff,
        message: out.credentials
          ? `Created. Username: ${out.credentials.username} Password: ${out.credentials.password}`
          : "Created",
      };
    } catch (error) {
      return handleApiError<StaffMember>(error, {} as StaffMember);
    }
  },

  // Create staff member - alias for compatibility
  createStaffMember: async (
    request: CreateStaffRequest
  ): Promise<ApiResponse<StaffMember>> => {
    return staffApi.createStaff(request);
  },

  // Update an existing staff member
  updateStaff: async (
    id: string,
    request: UpdateStaffRequest
  ): Promise<ApiResponse<StaffMember>> => {
    try {
      // Support updating doctor specializations only
      if (request.role === "Doctor" && request.specializations?.length) {
        const specIds = request.specializations;
        await apiClient.put(`/practitioner/Doctors/${id}/specializations`, {
          specializationIds: specIds,
        });
      }
      // Fetch latest directory row
      const refreshed = await apiClient.get(
        `/practitioner/Doctors/${id}/directory`
      );
      return handleApiResponse<StaffMember>(
        refreshed,
        mapDoctorDirectoryToStaff
      );
    } catch (error) {
      return handleApiError<StaffMember>(error, {} as StaffMember);
    }
  },

  // Update staff member - alias for compatibility
  updateStaffMember: async (
    id: string,
    request: UpdateStaffRequest
  ): Promise<ApiResponse<StaffMember>> => {
    return staffApi.updateStaff(id, request);
  },

  // Delete a staff member (soft delete)
  deleteStaff: async (id: string): Promise<ApiResponse<boolean>> => {
    try {
      const res = await apiClient.delete(`/practitioner/Doctors/${id}`);
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
      const response = await apiClient.get(
        `/practitioner/catalog/specializations`
      );
      return handleApiResponse<Specialization[]>(response, (items: unknown) =>
        (items as { id: string; name: string }[]).map((s) => ({
          id: s.id,
          name: s.name,
          serviceName: "",
        }))
      );
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
  getAvailability: async (
    doctorId: string
  ): Promise<ApiResponse<AvailabilityEntry[]>> => {
    try {
      const res = await apiClient.get(
        `/practitioner/Doctors/${doctorId}/availability`
      );
      return handleApiResponse<AvailabilityEntry[]>(res);
    } catch (error) {
      return handleApiError<AvailabilityEntry[]>(error, []);
    }
  },

  setAvailability: async (
    doctorId: string,
    entries: { dayOfWeek: number; start: string; end: string }[]
  ): Promise<ApiResponse<boolean>> => {
    try {
      const res = await apiClient.put(
        `/practitioner/Doctors/${doctorId}/availability`,
        entries
      );
      return { success: res.status >= 200 && res.status < 300, data: true };
    } catch (error) {
      return handleApiError<boolean>(error, false);
    }
  },
};
