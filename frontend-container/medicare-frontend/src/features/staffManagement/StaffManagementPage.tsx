import React from "react";

import Header from "../../layout/Header";
import { ErrorDisplay, Loading } from "../../shared/components";

import { useStaffManagement } from "./hooks/useStaffManagement";
import { StaffManagement } from "./StaffManagement";
import type { StaffManagementPageProps, StaffRole } from "./types";

export const StaffManagementPage: React.FC<StaffManagementPageProps> = ({
  initialRoleFilter = "All",
}) => {
  const {
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
  } = useStaffManagement();

  React.useEffect(() => {
    if (initialRoleFilter !== "All") {
      setRoleFilter(initialRoleFilter as StaffRole | "All");
    }
  }, [initialRoleFilter, setRoleFilter]);

  if (loading) {
    return (
      <>
        <Header />
        <div className="pt-16 flex justify-center items-center min-h-[400px]">
          <Loading text="Loading staff members..." size="lg" />
        </div>
      </>
    );
  }

  if (error) {
    return (
      <>
        <Header />
        <div className="pt-16 container mx-auto px-4 py-8">
          <ErrorDisplay
            message="Failed to load staff members"
            onRetry={() => window.location.reload()}
          />
        </div>
      </>
    );
  }

  return (
    <>
      <Header />
      <div className="pt-16 min-h-screen bg-gray-50">
        <div className="container mx-auto px-4 py-8">
          <StaffManagement
            staff={filteredStaff}
            specializations={specializations}
            searchTerm={searchTerm}
            onSearchChange={setSearchTerm}
            roleFilter={roleFilter}
            onRoleFilterChange={setRoleFilter}
            statusFilter={statusFilter}
            onStatusFilterChange={setStatusFilter}
            selectedStaff={selectedStaff}
            onStaffSelect={selectStaff}
            onStaffDeselect={() => selectStaff(null)}
            onStaffCreate={createStaff}
            onStaffUpdate={updateStaff}
            onStaffDelete={deleteStaff}
          />
        </div>
      </div>
    </>
  );
};
