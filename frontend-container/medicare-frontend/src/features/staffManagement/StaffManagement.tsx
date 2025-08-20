import React, { useState } from "react";
import { useToastContext } from "../../shared/ui/toast";

import {
  StaffDetailsModal,
  StaffFilter,
  StaffFormModal,
  StaffList,
} from "./components";
import type {
  CreateStaffRequest,
  StaffManagementProps,
  UpdateStaffRequest,
} from "./types";

export const StaffManagement: React.FC<StaffManagementProps> = ({
  staff,
  specializations,
  searchTerm,
  onSearchChange,
  roleFilter,
  onRoleFilterChange,
  statusFilter,
  onStatusFilterChange,
  selectedStaff,
  onStaffSelect,
  onStaffDeselect,
  onStaffCreate,
  onStaffUpdate,
  onStaffDelete,
}) => {
  const [showEditModal, setShowEditModal] = useState(false);
  const { showToast } = useToastContext();

  const handleEditClick = () => {
    setShowEditModal(true);
  };

  // Show create modal
  const handleCreateClick = () => {
    setShowEditModal(true);
  };

  const handleFormSave = async (
    data: CreateStaffRequest | UpdateStaffRequest
  ) => {
    let success = false;
    if ("id" in data) {
      success = await onStaffUpdate(data);
    } else {
      success = await onStaffCreate(data);
    }

    if (success) {
      if (!("id" in data)) {
        // We can't directly access credentials here; user will see them on the new card/details if present
        showToast("Doctor created. Username/password will be shown on the card.", { type: "success" });
      }
      setShowEditModal(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Staff Management</h1>
          <p className="text-gray-600 mt-1">Manage doctors and their specializations</p>
        </div>
        <button
          onClick={handleCreateClick}
          className="inline-flex items-center bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition duration-150 text-sm font-medium"
        >
          + Add Doctor
        </button>
      </div>

      {/* Filter Controls */}
      <StaffFilter
        searchTerm={searchTerm}
        onSearchChange={onSearchChange}
        roleFilter={roleFilter}
        onRoleFilterChange={onRoleFilterChange}
  statusFilter={statusFilter}
  onStatusFilterChange={onStatusFilterChange}
      />

      {/* Staff List */}
      <StaffList
        staff={staff}
        onStaffClick={onStaffSelect}
        searchTerm={searchTerm}
        roleFilter={roleFilter}
        emptyMessage={
          searchTerm || roleFilter !== "All" || statusFilter !== "All"
            ? "No staff members found matching your criteria"
            : "No staff members added yet"
        }
      />

      {/* Staff Details Modal */}
      <StaffDetailsModal
        staff={selectedStaff}
        isOpen={!!selectedStaff}
        onClose={onStaffDeselect}
        onEdit={handleEditClick}
        onDelete={async () => {
          if (selectedStaff && window.confirm(`Delete ${selectedStaff.profile.firstName} ${selectedStaff.profile.lastName}? This will archive the doctor and remove their appointments.`)) {
            const ok = await onStaffDelete(selectedStaff.id);
            if (ok) {
              showToast("Doctor removed and archived; their appointments were purged.", { type: "success" });
            } else {
              showToast("Failed to remove doctor.", { type: "error" });
            }
            onStaffDeselect();
          }
        }}
      />

      {/* Creation modal disabled */}

      {/* Edit Staff Modal */}
  <StaffFormModal
        isOpen={showEditModal}
        onClose={() => setShowEditModal(false)}
        onSave={handleFormSave}
        staff={selectedStaff}
        availableSpecializations={specializations}
      />
    </div>
  );
};
