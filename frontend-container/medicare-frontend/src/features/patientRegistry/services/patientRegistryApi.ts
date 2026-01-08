import { apiClient } from "../../../shared/services/apiClient";
import type {
  ApiResponse,
  CreatePatientRequest,
  Doctor,
  PatientRegistryData,
  PatientRegistryFilters,
  PatientRegistryInfo,
} from "../types";

// --- Types tailored for Backend Responses ---

// From PatientService ListPatients (PatientOverview)
interface PatientOverviewDto {
  patientId: string;
  userId: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  dateOfBirth?: string;
  // ... other overview fields
  currentStatus?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
}

// From PatientService GetById (Patient Entity) OR GetPatientHandler (Profile DTO)
// Based on my research, GetPatientHandler returns a rich DTO!
interface PatientProfileDto {
  patientId: string;
  userId: string;
  primaryDoctorId?: string;
  fullName: string;
  email: string;
  phone: string;
  address: string;
  dateOfBirth?: string;
  gender: string;
  emergencyContacts: Array<{ name: string; relation?: string; phone?: string }>;
  insurance: Array<{
    provider?: string;
    policyNumber?: string;
    validFrom?: string;
    validTo?: string;
  }>;
}

// From UserService GetById
interface UserResponseDto {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  address?: string; // Generic address string if aggregated
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
  isActive: boolean;
}

// Service URLs (assuming Vite env or defaults)
const PATIENT_SERVICE_URL =
  import.meta.env.VITE_PATIENT_SERVICE_URL || "http://localhost:9084";
const USER_SERVICE_URL =
  import.meta.env.VITE_USER_SERVICE_URL || "http://localhost:5001";
const PRACTITIONER_SERVICE_URL =
  import.meta.env.VITE_PRACTITIONER_SERVICE_URL || "http://localhost:9082";

export class PatientRegistryApiService {
  /**
   * Get paginated list of patients with optional filters
   * Fetches from PatientService (List) and enriches with UserService (Profiles) if needed.
   */
  static async getPatients(
    page = 1,
    limit = 10,
    filters?: PatientRegistryFilters
  ): Promise<ApiResponse<PatientRegistryData>> {
    try {
      const params: Record<string, string | number> = { page, pageSize: limit };
      if (filters?.searchTerm) params.q = filters.searchTerm;

      // 1. Fetch List from PatientService
      const response = await apiClient.get<{
        items: PatientOverviewDto[];
        totalCount: number;
        currentPage: number;
        totalPages: number;
      }>(`${PATIENT_SERVICE_URL}/api/patient/patients`, { params });

      const data = response.data;
      const items = data.items || [];

      // 2. Map directly from PatientService response (no N+1 calls)
      // If names are "Unknown", the backend PatientOverview needs to be synced.
      const patients: PatientRegistryInfo[] = items.map((p) => ({
        id: p.patientId,
        firstName: p.firstName || "Unknown",
        lastName: p.lastName || "Unknown",
        email: p.email || "",
        phone: p.phone || "",
        dateOfBirth: p.dateOfBirth || "2000-01-01",
        gender: "prefer-not-to-say",
        addressLine1: "Unknown",
        city: "Unknown",
        state: "Unknown",
        zipCode: "",
        country: "Poland",
        medicalRecordNumber: "MRN-" + p.patientId.substring(0, 4),
        isActive: p.currentStatus === "Active",
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        emergencyContacts: p.emergencyContactName
          ? [
              {
                name: p.emergencyContactName,
                phone: p.emergencyContactPhone || "",
                relationship: "Unknown",
                isPrimary: true,
              },
            ]
          : [],
      }));

      return {
        success: true,
        data: {
          patients,
          totalCount: data.totalCount,
          currentPage: data.currentPage,
          totalPages: data.totalPages,
        },
      };
    } catch (error) {
      console.error("Failed to fetch patients", error);
      return {
        success: false,
        data: { patients: [], totalCount: 0, currentPage: 1, totalPages: 0 },
        message: "Failed to fetch patients",
      };
    }
  }

  /**
   * Get single patient by ID
   */
  static async getPatient(
    patientId: string
  ): Promise<ApiResponse<PatientRegistryInfo>> {
    try {
      // 1. Fetch from PatientService (GetById -> returns PatientProfileDto based on my check of handler)
      // Actually, verify if endpoint /api/patient/patients/{id} returns PatientProfileDto
      // The Handler returns PatientProfileDto, so yes!
      const response = await apiClient.get<PatientProfileDto>(
        `${PATIENT_SERVICE_URL}/api/patient/patients/${patientId}`
      );

      if (response.status === 200 && response.data) {
        const p = response.data;

        // 2. Fetch User Profile to get separated address fields if not in DTO
        // PatientProfileDto has `Address` (string) but frontend expects city, state etc.
        // Also to get latest metadata.
        let userData: Partial<UserResponseDto> = {};
        if (p.userId) {
          try {
            const uRes = await apiClient.get<UserResponseDto>(
              `${USER_SERVICE_URL}/api/users/${p.userId}`
            );
            userData = uRes.data;
          } catch {
            // User service might not return 200 if user not found, ignore
          }
        }

        return {
          success: true,
          data: {
            id: p.patientId || patientId,
            firstName:
              userData.firstName ||
              (p.fullName ? p.fullName.split(" ")[0] : "Unknown"),
            lastName:
              userData.lastName ||
              (p.fullName
                ? p.fullName.split(" ").slice(1).join(" ")
                : "Unknown"),
            email: userData.email || p.email || "",
            phone: userData.phoneNumber || p.phone || "",
            dateOfBirth:
              (userData.dateOfBirth as string) || p.dateOfBirth || "2000-01-01",
            gender: (p.gender as string) || "prefer-not-to-say",

            // Address mapping
            addressLine1: userData.addressLine1 || "",
            addressLine2: userData.addressLine2 || "",
            city: userData.city || "",
            state: userData.state || "",
            zipCode: userData.zipCode || "",
            country: userData.country || "Poland",

            medicalRecordNumber:
              "MRN-" + (p.patientId || patientId).substring(0, 4),
            isActive: userData.isActive ?? true, // Default true
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),

            emergencyContacts:
              p.emergencyContacts?.map((c) => ({
                name: c.name,
                phone: c.phone || "",
                relationship: c.relation || "Unknown",
                isPrimary: true,
              })) || [],

            insurance:
              p.insurance?.map((i) => ({
                providerName: i.provider || "",
                policyNumber: i.policyNumber || "",
                validFrom: i.validFrom ? i.validFrom.toString() : "",
                validTo: i.validTo ? i.validTo.toString() : "",
                isPrimary: true,
                isActive: true,
              })) || [],
          } as PatientRegistryInfo,
        };
      }
    } catch (e) {
      console.error(e);
    }

    return {
      success: false,
      data: {} as PatientRegistryInfo,
      message: "Patient not found",
    };
  }

  /**
   * Create new patient
   */
  static async createPatient(
    _patientData: CreatePatientRequest
  ): Promise<ApiResponse<PatientRegistryInfo>> {
    // Not implemented in this iteration as per user request (focus on list/details/edit)
    return {
      success: false,
      data: {} as PatientRegistryInfo,
      message: "Not implemented in this refactor",
    };
  }

  /**
   * Update existing patient
   * Updates BOTH User Service (Profile) and Patient Service (Status/Contacts/Insurance)
   */
  static async updatePatient(
    patientId: string,
    data: Partial<PatientRegistryInfo>
  ): Promise<ApiResponse<PatientRegistryInfo>> {
    try {
      // 1. Get current patient to find userId
      await this.getPatient(patientId);
      // We need userId. getPatient response doesn't strictly have userId in PatientRegistryInfo interface...
      // Wait, PatientRegistryInfo doesn't have userId. I need to fetch it or store it.
      // It's not in the interface! I should probably add it or double fetch.
      // Let's assume we can fetch it again via GetById from PatientService to get the Link.

      // Fetch raw link
      // Actually, let's just fetch the patient profile DTO again to get userId
      const linkRes = await apiClient.get<PatientProfileDto>(
        `${PATIENT_SERVICE_URL}/api/patient/patients/${patientId}`
      );
      const userId = linkRes.data?.userId;

      if (!userId)
        throw new Error("Could not find linked User for this patient");

      // 2. Update UserService - only update address fields (firstName, lastName, phone, email are read-only)
      const userUpdatePayload: Record<string, unknown> = {};

      // Only add address fields if they have values
      if (data.addressLine1 !== undefined)
        userUpdatePayload.addressLine1 = data.addressLine1;
      if (data.addressLine2 !== undefined)
        userUpdatePayload.addressLine2 = data.addressLine2;
      if (data.city !== undefined) userUpdatePayload.city = data.city;
      if (data.state !== undefined) userUpdatePayload.state = data.state;
      if (data.zipCode !== undefined) userUpdatePayload.zipCode = data.zipCode;
      if (data.country !== undefined) userUpdatePayload.country = data.country;
      if (data.isActive !== undefined)
        userUpdatePayload.isActive = data.isActive;

      // Only call UserService if there are fields to update
      if (Object.keys(userUpdatePayload).length > 0) {
        await apiClient.put(
          `${USER_SERVICE_URL}/api/users/${userId}`,
          userUpdatePayload
        );
      }

      // 3. Update PatientService
      // Status
      if (data.isActive !== undefined) {
        const status = data.isActive ? "Active" : "Inactive";
        await apiClient.put(
          `${PATIENT_SERVICE_URL}/api/patient/patients/${patientId}/status`,
          { status }
        );
      }

      // Emergency Contacts
      if (data.emergencyContacts) {
        const contactsPayload = data.emergencyContacts.map((c) => ({
          name: c.name,
          relation: c.relationship,
          phone: c.phone,
        }));
        await apiClient.put(
          `${PATIENT_SERVICE_URL}/api/patient/patients/${patientId}/emergency-contacts`,
          contactsPayload
        );
      }

      // Insurance
      if (data.insurance && data.insurance.length > 0) {
        const i = data.insurance[0]; // Backend only accepts one main insurance in command for now
        const insurancePayload = {
          provider: i.providerName,
          policyNumber: i.policyNumber,
          validFrom: i.validFrom || new Date().toISOString(),
          validTo: i.validTo,
        };
        await apiClient.put(
          `${PATIENT_SERVICE_URL}/api/patient/patients/${patientId}/insurance`,
          insurancePayload
        );
      }

      // Primary Doctor
      if (data.generalDoctorId !== undefined) {
        await apiClient.put(
          `${PATIENT_SERVICE_URL}/api/patient/patients/${patientId}/primary-doctor`,
          {
            doctorId: data.generalDoctorId || null,
          }
        );
      }

      return { success: true, data: data as PatientRegistryInfo };
    } catch (error) {
      console.error("Update failed", error);
      return {
        success: false,
        data: {} as PatientRegistryInfo,
        message: "Failed to update",
      };
    }
  }

  /**
   * Get all doctors for dropdown
   */
  static async getDoctors(): Promise<ApiResponse<Doctor[]>> {
    try {
      // Fetch from PractitionerService DoctorDirectory
      interface DoctorDirectoryDto {
        doctorId: string;
        userId: string;
        firstName: string;
        lastName: string;
        email?: string;
        phone?: string;
        isActive: boolean;
        specializations?: string;
      }

      const response = await apiClient.get<DoctorDirectoryDto[]>(
        `${PRACTITIONER_SERVICE_URL}/api/practitioner/doctors`,
        { params: { isActive: true } }
      );

      const doctors: Doctor[] = (response.data || []).map((d) => ({
        id: d.doctorId,
        userId: d.userId,
        firstName: d.firstName || "Unknown",
        lastName: d.lastName || "Doctor",
        email: d.email || "",
        specialty: d.specializations || "General",
        isActive: d.isActive,
      }));

      return { success: true, data: doctors };
    } catch (error) {
      console.error("Failed to fetch doctors", error);
      return { success: true, data: [] };
    }
  }

  /**
   * Check if email is already in use
   */
  static async checkEmailAvailability(
    email: string
  ): Promise<ApiResponse<{ available: boolean }>> {
    // Use UserService check
    try {
      const res = await apiClient.get<{ emailExists: boolean }>(
        `${USER_SERVICE_URL}/api/users/availability`,
        { params: { email } }
      );
      return { success: true, data: { available: !res.data.emailExists } };
    } catch {
      return { success: true, data: { available: true } };
    }
  }

  /**
   * Delete patient
   */
  static async deletePatient(patientId: string): Promise<ApiResponse<boolean>> {
    try {
      await apiClient.delete(
        `${PATIENT_SERVICE_URL}/api/patient/patients/${patientId}`
      );
      return { success: true, data: true };
    } catch (error) {
      console.error("Delete failed", error);
      return { success: false, data: false, message: "Failed to delete" };
    }
  }
}
