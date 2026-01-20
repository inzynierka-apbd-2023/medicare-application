import React, { useState } from "react";
import { toastMessages, useToast } from "@shared/toast";

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
}) => {
  const [showEditModal, setShowEditModal] = useState(false);
  const { showSuccess } = useToast();

  const handleEditClick = () => {
    setShowEditModal(true);
  };

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
        showSuccess(toastMessages.staff.createSuccess);
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
          <p className="text-gray-600 mt-1">
            Manage doctors and their specializations
          </p>
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

      <StaffDetailsModal
        staff={selectedStaff}
        isOpen={!!selectedStaff}
        onClose={onStaffDeselect}
        onEdit={handleEditClick}
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
