import React from "react";
import { Edit3, Trash2, User } from "lucide-react";

import {
  Badge,
  Button,
  DefinitionList,
  InfoCard,
  Modal,
} from "../../../shared/components";
import type { StaffDetailsModalProps } from "../types";

export const StaffDetailsModal: React.FC<StaffDetailsModalProps> = ({
  staff,
  isOpen,
  onClose,
  onEdit,
  onDelete,
}) => {
  if (!staff) return null;

  const handleEdit = () => {
    if (onEdit) {
      onEdit(staff);
    }
  };

  const handleDelete = () => {
    if (onDelete) {
      onDelete(staff);
    }
  };

  const getPersonalInfoItems = () => [
    {
      label: "Name",
      value: `${staff.profile.firstName} ${staff.profile.lastName}`,
    },
    {
      label: "Email",
      value: staff.profile.email,
    },
    {
      label: "Phone",
      value: staff.profile.phone,
      show: !!staff.profile.phone,
    },
    {
      label: "Date of Birth",
      value: staff.profile.dateOfBirth
        ? new Date(staff.profile.dateOfBirth).toLocaleDateString()
        : undefined,
      show: !!staff.profile.dateOfBirth,
    },
    {
      label: "Gender",
      value: staff.profile.gender,
      show: !!staff.profile.gender,
    },
    {
      label: "Address",
      value: [
        staff.profile.addressLine1,
        staff.profile.addressLine2,
        [staff.profile.city, staff.profile.state].filter(Boolean).join(", "),
        staff.profile.zipCode,
        staff.profile.country,
      ]
        .filter(Boolean)
        .join(", "),
      show: !!(staff.profile.addressLine1 || staff.profile.city),
    },
  ];

  const getRoleSpecificInfo = () => {
    if (staff.role === "Doctor") {
      return [
        {
          label: "License Number",
          value: staff.licenseNumber,
          show: !!staff.licenseNumber,
        },
        {
          label: "Years of Experience",
          value: staff.yearsExperience?.toString(),
          show: !!staff.yearsExperience,
        },
        {
          label: "Office Address",
          value: staff.officeAddress,
          show: !!staff.officeAddress,
        },
        {
          label: "Biography",
          value: staff.biography,
          show: !!staff.biography,
        },
      ];
    } else {
      return [
        {
          label: "Department",
          value: staff.department,
          show: !!staff.department,
        },
      ];
    }
  };

  const getSystemInfo = () => [
    {
      label: "Role",
      value: staff.role,
    },
    {
      label: "Status",
      value: staff.isActive ? "Active" : "Inactive",
    },
    {
      label: "Created",
      value: new Date(staff.createdAt).toLocaleDateString(),
    },
    {
      label: "Last Updated",
      value: new Date(staff.updatedAt).toLocaleDateString(),
    },
  ];

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="lg">
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-4">
            <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center">
              <User size={32} className="text-blue-600" />
            </div>
            <div>
              <h2 className="text-2xl font-bold text-gray-900">
                {staff.profile.firstName} {staff.profile.lastName}
              </h2>
              <div className="flex gap-2 mt-1">
                <Badge
                  variant={staff.role === "Doctor" ? "info" : "default"}
                  size="md"
                >
                  {staff.role}
                </Badge>
                <Badge variant={staff.isActive ? "success" : "error"} size="md">
                  {staff.isActive ? "Active" : "Inactive"}
                </Badge>
              </div>
            </div>
          </div>
        </div>

        {/* Personal Information */}
        <InfoCard title="Personal Information" variant="bordered">
          <DefinitionList variant="bordered" items={getPersonalInfoItems()} />
        </InfoCard>

        {/* Professional Information */}
        <InfoCard title="Professional Information" variant="bordered">
          <DefinitionList variant="bordered" items={getRoleSpecificInfo()} />

          {/* Specializations for doctors */}
          {staff.role === "Doctor" && staff.specializations.length > 0 && (
            <div className="mt-4">
              <h4 className="font-medium text-gray-900 mb-2">
                Specializations
              </h4>
              <div className="flex flex-wrap gap-2">
                {staff.specializations.map((spec) => (
                  <Badge
                    key={spec.id}
                    variant={spec.isPrimary ? "info" : "default"}
                    size="md"
                  >
                    {spec.name}
                    {spec.isPrimary && " (Primary)"}
                  </Badge>
                ))}
              </div>
            </div>
          )}
        </InfoCard>

        {/* System Information */}
        <InfoCard title="System Information" variant="bordered">
          <DefinitionList variant="bordered" items={getSystemInfo()} />
        </InfoCard>

        {/* Actions */}
        <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
          {onDelete && (
            <Button
              variant="outline"
              onClick={handleDelete}
              className="text-red-600 border-red-300 hover:bg-red-50"
            >
              <Trash2 size={16} className="mr-2" />
              Delete
            </Button>
          )}
          {onEdit && (
            <Button variant="primary" onClick={handleEdit}>
              <Edit3 size={16} className="mr-2" />
              Edit
            </Button>
          )}
        </div>
      </div>
    </Modal>
  );
};
