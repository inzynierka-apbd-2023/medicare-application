import { useCallback, useEffect, useState } from "react";
import type {
  CreateStaffRequest,
  Specialization,
  StaffMember,
  StaffRole,
  StaffStatusFilter,
  UpdateStaffRequest,
} from "@features/staffManagement/types";
import { staffApi } from "@shared/services/staffApi";

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
      const [staffData, specializationsData] = await Promise.all([
        staffApi.getStaff(req),
        staffApi.getSpecializations(),
      ]);

      setStaff(staffData);
      setSpecializations(specializationsData);
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
        const created = await staffApi.createStaff(data);

        setStaff((prev) => [
          created as StaffMember,
          ...prev.filter((s) => s.id !== created.id),
        ]);
        await fetchStaff();
        setStaff((prev) =>
          prev.map((s) =>
            s.id === created.id &&
            created.role === "Doctor" &&
            created.credentials
              ? ({ ...s, credentials: created.credentials } as StaffMember)
              : s
          )
        );
        return true;
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
          role: data.role!,
        };
        const updated = await staffApi.updateStaff(data.id, updateRequest);

        await fetchStaff(); // Refresh the list
        setSelectedStaff(updated); // Update selected staff if it was the updated one
        return true;
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
        await staffApi.deleteStaff(id);

        await fetchStaff(); // Refresh the list
        if (selectedStaff?.id === id) {
          setSelectedStaff(null); // Deselect if the deleted staff was selected
        }
        return true;
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
