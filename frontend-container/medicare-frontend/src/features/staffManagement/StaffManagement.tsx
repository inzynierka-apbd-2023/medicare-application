import React, { useState } from "react";
import { Plus } from "lucide-react";

import { Button } from "../../shared/components";

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
  selectedStaff,
  onStaffSelect,
  onStaffDeselect,
  onStaffCreate,
  onStaffUpdate,
  onStaffDelete,
}) => {
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);

  const handleCreateClick = () => {
    setShowCreateModal(true);
  };

  const handleEditClick = () => {
    setShowEditModal(true);
  };

  const handleDeleteClick = async () => {
    if (
      selectedStaff &&
      window.confirm(
        `Are you sure you want to delete ${selectedStaff.profile.firstName} ${selectedStaff.profile.lastName}?`
      )
    ) {
      await onStaffDelete(selectedStaff.id);
      onStaffDeselect();
    }
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
      setShowCreateModal(false);
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
            Manage doctors and receptionists in your healthcare facility
          </p>
        </div>
        <Button
          variant="primary"
          onClick={handleCreateClick}
          className="flex items-center gap-2"
        >
          <Plus size={20} />
          Add Staff Member
        </Button>
      </div>

      {/* Filter Controls */}
      <StaffFilter
        searchTerm={searchTerm}
        onSearchChange={onSearchChange}
        roleFilter={roleFilter}
        onRoleFilterChange={onRoleFilterChange}
      />

      {/* Staff List */}
      <StaffList
        staff={staff}
        onStaffClick={onStaffSelect}
        searchTerm={searchTerm}
        roleFilter={roleFilter}
        emptyMessage={
          searchTerm || roleFilter !== "All"
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
        onDelete={handleDeleteClick}
      />

      {/* Create Staff Modal */}
      <StaffFormModal
        isOpen={showCreateModal}
        onClose={() => setShowCreateModal(false)}
        onSave={handleFormSave}
        availableSpecializations={specializations}
      />

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
