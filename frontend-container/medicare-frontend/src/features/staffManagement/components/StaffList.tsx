import React from "react";
import type { StaffListProps } from "@features/staffManagement/types";
import { EmptyState } from "@shared/components";

import { StaffCard } from "./StaffCard";

export const StaffList: React.FC<StaffListProps> = ({
  staff,
  onStaffClick,
  searchTerm = "",
  roleFilter = "All",
  emptyMessage = "No staff members found",
}) => {
  if (staff.length === 0) {
    return (
      <div className="flex justify-center items-center min-h-[400px]">
        <EmptyState
          title="No Staff Members"
          description={emptyMessage}
          action={
            searchTerm || roleFilter !== "All"
              ? "Try adjusting your filters"
              : undefined
          }
        />
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {staff.map((staffMember) => (
        <StaffCard
          key={staffMember.id}
          staff={staffMember}
          onClick={onStaffClick}
        />
      ))}
    </div>
  );
};
