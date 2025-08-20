import { useCallback, useEffect, useState } from "react";

import { staffApi } from "../../../shared/services/staffApi";
import type {
  CreateStaffRequest,
  Specialization,
  StaffMember,
  StaffRole,
  UpdateStaffRequest,
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
  const [roleFilter, setRoleFilter] = useState<StaffRole | "All">("All");

  // Filter staff based on search term and role filter
  const filteredStaff = staff.filter((staffMember) => {
    // Only show active staff members
    if (!staffMember.isActive) {
      return false;
    }

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

    return matchesSearch && matchesRole;
  });

  const fetchStaff = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const [staffResponse, specializationsResponse] = await Promise.all([
        staffApi.getStaff(),
        staffApi.getSpecializations(),
      ]);

      if (staffResponse.success) {
        setStaff(staffResponse.data);
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
  }, []);

  const selectStaff = useCallback((staff: StaffMember | null) => {
    setSelectedStaff(staff);
  }, []);

  const createStaff = useCallback(
    async (data: CreateStaffRequest): Promise<boolean> => {
      try {
        setError(null);
        const response = await staffApi.createStaff(data);

        if (response.success) {
          await fetchStaff(); // Refresh the list
          return true;
        } else {
          setError(response.errors?.[0] || "Failed to create staff member");
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
        const response = await staffApi.updateStaff(data.id!, updateRequest);

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
          setError(response.errors?.[0] || "Failed to delete staff member");
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
    filteredStaff,
    setSearchTerm,
    setRoleFilter,
    selectStaff,
    createStaff,
    updateStaff,
    deleteStaff,
    refreshStaff,
  };
};
