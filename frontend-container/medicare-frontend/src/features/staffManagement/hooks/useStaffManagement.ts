import { useCallback, useEffect, useState } from "react";

import { staffApi } from "../../../shared/services/staffApi";
import type {
  CreateStaffRequest,
  Specialization,
  StaffMember,
  StaffRole,
  UpdateStaffRequest,
  StaffStatusFilter,
} from "../types";

interface UseStaffManagementReturn {
  staff: StaffMember[];
  specializations: Specialization[];
  loading: boolean;
  error: string | null;
  selectedStaff: StaffMember | null;
  searchTerm: string;
  roleFilter: StaffRole | "All";
  filteredStaff: StaffMember[];
  setSearchTerm: (term: string) => void;
  setRoleFilter: (role: StaffRole | "All") => void;
  statusFilter: StaffStatusFilter;
  setStatusFilter: (status: StaffStatusFilter) => void;
  selectStaff: (staff: StaffMember | null) => void;
  createStaff: (data: CreateStaffRequest) => Promise<boolean>;
  updateStaff: (data: UpdateStaffRequest) => Promise<boolean>;
  deleteStaff: (id: string) => Promise<boolean>;
  refreshStaff: () => Promise<void>;
}

export const useStaffManagement = (): UseStaffManagementReturn => {
  const [staff, setStaff] = useState<StaffMember[]>([]);
  const [specializations, setSpecializations] = useState<Specialization[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedStaff, setSelectedStaff] = useState<StaffMember | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [roleFilter, setRoleFilter] = useState<StaffRole | "All">("Doctor");
  const [statusFilter, setStatusFilter] = useState<StaffStatusFilter>("All");

  // Filter staff based on search term and role filter
  const filteredStaff = staff.filter((staffMember) => {

    const matchesSearch =
      `${staffMember.profile.firstName} ${staffMember.profile.lastName}`
        .toLowerCase()
        .includes(searchTerm.toLowerCase()) ||
      staffMember.profile.email
        .toLowerCase()
        .includes(searchTerm.toLowerCase()) ||
      (staffMember.role === "Doctor" &&
        staffMember.specializations.some((spec) =>
          spec.name.toLowerCase().includes(searchTerm.toLowerCase())
        )) ||
      (staffMember.role === "Receptionist" &&
        staffMember.department
          ?.toLowerCase()
          .includes(searchTerm.toLowerCase()));

    const matchesRole = roleFilter === "All" || staffMember.role === roleFilter;
    const matchesStatus =
      statusFilter === "All" ||
      (statusFilter === "Active" && staffMember.isActive) ||
      (statusFilter === "Archived" && !staffMember.isActive);
    return matchesSearch && matchesRole && matchesStatus;
  });

  const fetchStaff = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const req: { isActive?: boolean } = {};
      if (statusFilter !== "All") req.isActive = statusFilter === "Active";
      const [staffResponse, specializationsResponse] = await Promise.all([
        staffApi.getStaff(req as any),
        staffApi.getSpecializations(),
      ]);

      if (staffResponse.success) {
        // Only doctors are supported; drop any non-doctor entries if present
        setStaff(staffResponse.data.filter((s) => s.role === "Doctor"));
      } else {
        setError(staffResponse.errors?.[0] || "Failed to fetch staff");
      }

      if (specializationsResponse.success) {
        setSpecializations(specializationsResponse.data);
      } else {
        console.warn(
          "Failed to fetch specializations:",
          specializationsResponse.errors?.[0]
        );
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "An unexpected error occurred"
      );
    } finally {
      setLoading(false);
    }
  }, [statusFilter]);

  const selectStaff = useCallback((staff: StaffMember | null) => {
    setSelectedStaff(staff);
  }, []);

  const createStaff = useCallback(
    async (data: CreateStaffRequest): Promise<boolean> => {
      try {
        setError(null);
        const response = await staffApi.createStaff(data);

        if (response.success) {
          const created = response.data;
          // Optimistically add created (with credentials) to top of list
          setStaff((prev) => [created as StaffMember, ...prev.filter((s) => s.id !== created.id)]);
          // Refresh the list from server, then re-attach credentials to the created doctor (one-time)
          await fetchStaff();
          setStaff((prev) => prev.map((s) =>
            s.id === created.id && (created as any).credentials
              ? ({ ...s, credentials: (created as any).credentials } as any)
              : s
          ));
          // Best-effort toast is handled by caller
          return true;
        } else {
          setError(response.errors?.[0] || response.message || "Failed to create staff member");
          return false;
        }
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "An unexpected error occurred"
        );
        return false;
      }
    },
    [fetchStaff]
  );

  const updateStaff = useCallback(
    async (data: UpdateStaffRequest): Promise<boolean> => {
      try {
        setError(null);
        const updateRequest = {
          ...data,
          role: data.role!
        };
  const response = await staffApi.updateStaff(data.id, updateRequest);

        if (response.success) {
          await fetchStaff(); // Refresh the list
          setSelectedStaff(response.data); // Update selected staff if it was the updated one
          return true;
        } else {
          setError(response.errors?.[0] || "Failed to update staff member");
          return false;
        }
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "An unexpected error occurred"
        );
        return false;
      }
    },
    [fetchStaff]
  );

  const deleteStaff = useCallback(
    async (id: string): Promise<boolean> => {
      try {
        setError(null);
        const response = await staffApi.deleteStaff(id);

        if (response.success) {
          await fetchStaff(); // Refresh the list
          if (selectedStaff?.id === id) {
            setSelectedStaff(null); // Deselect if the deleted staff was selected
          }
          return true;
        } else {
          setError(response.errors?.[0] || "Deleting staff is not supported");
          return false;
        }
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "An unexpected error occurred"
        );
        return false;
      }
    },
    [fetchStaff, selectedStaff?.id]
  );

  const refreshStaff = useCallback(async () => {
    await fetchStaff();
  }, [fetchStaff]);

  // Initial data fetch
  useEffect(() => {
    fetchStaff();
  }, [fetchStaff]);

  return {
    staff,
    specializations,
    loading,
    error,
    selectedStaff,
    searchTerm,
    roleFilter,
  statusFilter,
    filteredStaff,
    setSearchTerm,
    setRoleFilter,
  setStatusFilter,
    selectStaff,
    createStaff,
    updateStaff,
    deleteStaff,
    refreshStaff,
  };
};
