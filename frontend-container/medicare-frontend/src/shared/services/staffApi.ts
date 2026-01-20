import { toastMessages } from "@shared/toast/toastMessages";

import { api } from "./api";
import { usersApi } from "./usersApi";

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
  licenseNumber?: string;
  yearsExperience?: number;
  biography?: string;
  officeAddress?: string;
  specializations?: string[];
  department?: string;
}

export interface UpdateStaffRequest {
  id: string;
  role: StaffRole;
  profile?: Partial<UserProfile>;
  licenseNumber?: string;
  yearsExperience?: number;
  biography?: string;
  officeAddress?: string;
  specializations?: string[];
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
    // We only have specialization IDs from the directory.
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

// Real API implementation using api
export const staffApi = {
  // Get all staff members with optional search parameters
  getStaff: async (
    searchRequest?: StaffSearchRequest
  ): Promise<StaffMember[]> => {
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
        api
          .get<unknown>(url, undefined, {
            showToastOnError: true,
            showToastOnSuccess: false,
          })
          .then((data) => {
            // Verify success structure or direct array
            const items = Array.isArray(data)
              ? data
              : (data as { success?: boolean; data: unknown }).success &&
                  Array.isArray(
                    (data as { success?: boolean; data: unknown }).data
                  )
                ? ((data as { success?: boolean; data: unknown })
                    .data as unknown[])
                : [];
            return items.map(mapDoctorDirectoryToStaff);
          })
          .catch(() => [])
      );
    }

    // 2. Fetch Receptionists (manual aggregation)
    if (!role || role === "Receptionist") {
      promises.push(staffApi.getReceptionists());
    }

    const results = await Promise.all(promises);
    let allStaff = results.flat();

    // Post-filtering for Receptionists
    if (searchQuery && (!role || role === "Receptionist")) {
      if (role === "Receptionist" || !role) {
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

    return allStaff;
  },

  // Get all receptionists (helper)
  getReceptionists: async (): Promise<Receptionist[]> => {
    const response = await api.get<unknown>(
      "/practitioner/Receptionists",
      undefined,
      {
        showToastOnError: true,
        showToastOnSuccess: false,
      }
    );
    const receptionistsRaw = Array.isArray(response)
      ? response
      : ((response as { data: unknown })?.data as unknown[]) || [];

    if (!Array.isArray(receptionistsRaw)) return [];

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
            department: "General",
            isActive: true,
            createdAt: row.createdAt,
            updatedAt: row.updatedAt,
          } as Receptionist;
        } catch {
          return null;
        }
      })
    );

    return receptionists.filter(Boolean) as Receptionist[];
  },

  getStaffMembers: async (
    searchRequest?: StaffSearchRequest
  ): Promise<StaffMember[]> => {
    return staffApi.getStaff(searchRequest);
  },

  getStaffById: async (id: string): Promise<StaffMember | null> => {
    try {
      const response = await api.get(
        `/practitioner/Doctors/${id}/directory`,
        undefined,
        {
          showToastOnError: true,
          showToastOnSuccess: false,
        }
      );
      return response ? mapDoctorDirectoryToStaff(response) : null;
    } catch {
      return null;
    }
  },

  getStaffMember: async (id: string): Promise<StaffMember | null> => {
    return staffApi.getStaffById(id);
  },
  getStaffByRole: async (role: StaffRole): Promise<StaffMember[]> => {
    if (role !== "Doctor") {
      return [];
    }
    const response = await api.get<unknown[]>(
      `/practitioner/Doctors`,
      undefined,
      {
        showToastOnError: true,
        showToastOnSuccess: false,
      }
    );
    const data = Array.isArray(response)
      ? response
      : (response as { data: unknown[] }).data;
    return (data || []).map(mapDoctorDirectoryToStaff);
  },

  createStaff: async (_request: CreateStaffRequest): Promise<StaffMember> => {
    if (_request.role !== "Doctor") {
      throw new Error("Only creating doctors is supported.");
    }
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
    const res = await api.post<CreateDoctorResponse>(
      `/practitioner/Doctors/register-full`,
      payload,
      undefined,
      {
        showToastOnError: true,
        showToastOnSuccess: true,
        successMessage: toastMessages.staff.createSuccess,
      }
    );
    const staff = mapDoctorDirectoryToStaff(res.directory) as Doctor;
    if (res.credentials) {
      staff.credentials = res.credentials;
    }
    return staff;
  },

  createStaffMember: async (
    request: CreateStaffRequest
  ): Promise<StaffMember> => {
    return staffApi.createStaff(request);
  },

  updateStaff: async (
    id: string,
    request: UpdateStaffRequest
  ): Promise<StaffMember> => {
    if (request.role === "Doctor" && request.specializations?.length) {
      const specIds = request.specializations;
      await api.put(
        `/practitioner/Doctors/${id}/specializations`,
        {
          specializationIds: specIds,
        },
        undefined,
        {
          showToastOnError: true,
          showToastOnSuccess: false,
        }
      );
    }
    const refreshed = await api.get(
      `/practitioner/Doctors/${id}/directory`,
      undefined,
      {
        showToastOnError: true,
        showToastOnSuccess: true,
        successMessage: toastMessages.staff.updateSuccess,
      }
    );
    return mapDoctorDirectoryToStaff(refreshed);
  },

  updateStaffMember: async (
    id: string,
    request: UpdateStaffRequest
  ): Promise<StaffMember> => {
    return staffApi.updateStaff(id, request);
  },

  deleteStaff: async (id: string): Promise<boolean> => {
    await api.delete(`/practitioner/Doctors/${id}`, undefined, {
      showToastOnError: true,
      showToastOnSuccess: true,
      successMessage: toastMessages.staff.deleteSuccess,
    });
    return true;
  },

  deleteStaffMember: async (id: string): Promise<boolean> => {
    return staffApi.deleteStaff(id);
  },

  getSpecializations: async (): Promise<Specialization[]> => {
    const response = await api.get<{ id: string; name: string }[]>(
      `/practitioner/catalog/specializations`,
      undefined,
      {
        showToastOnError: true,
        showToastOnSuccess: false,
      }
    );
    return response.map((s) => ({
      id: s.id,
      name: s.name,
      serviceName: "",
    }));
  },

  // Get available services
  getServices: async (): Promise<Service[]> => {
    return await api.get<Service[]>(
      `/practitioner/catalog/services`,
      undefined,
      {
        showToastOnError: true,
        showToastOnSuccess: false,
      }
    );
  },

  // Availability (schedules)
  getAvailability: async (doctorId: string): Promise<AvailabilityEntry[]> => {
    return await api.get<AvailabilityEntry[]>(
      `/practitioner/Doctors/${doctorId}/availability`,
      undefined,
      {
        showToastOnError: true,
        showToastOnSuccess: false,
      }
    );
  },

  setAvailability: async (
    doctorId: string,
    entries: { dayOfWeek: number; start: string; end: string }[]
  ): Promise<boolean> => {
    await api.put(
      `/practitioner/Doctors/${doctorId}/availability`,
      entries,
      undefined,
      {
        showToastOnError: true,
        showToastOnSuccess: true, // Assuming availability update is a direct user action
      }
    );
    return true;
  },
};
