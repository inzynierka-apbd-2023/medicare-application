import React, { useEffect, useState } from "react";
import { Plus, Save } from "lucide-react";

import { Button, Input, Modal } from "../../../shared/components";
import type {
  CreateStaffRequest,
  StaffFormModalProps,
  StaffRole,
  UpdateStaffRequest,
  UserProfile,
} from "../types";

export const StaffFormModal: React.FC<StaffFormModalProps> = ({
  isOpen,
  onClose,
  onSave,
  staff = null,
  availableSpecializations,
}) => {
  const isEdit = !!staff;

  // Define a more specific form data type
  interface FormData {
    id?: string;
    role: StaffRole;
    profile: UserProfile;
    licenseNumber?: string;
    yearsExperience?: number;
    biography?: string;
    officeAddress?: string;
    specializations?: string[];
    department?: string;
  }

  const [formData, setFormData] = useState<FormData>({
    role: "Doctor" as StaffRole,
    profile: {
      firstName: "",
      lastName: "",
      email: "",
      phone: "",
      dateOfBirth: "",
      gender: "Male",
      addressLine1: "",
      addressLine2: "",
      city: "",
      state: "",
      zipCode: "",
      country: "",
    },
    licenseNumber: "",
    yearsExperience: 0,
    biography: "",
    officeAddress: "",
    specializations: [],
    department: "",
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  // Initialize form data when staff changes
  useEffect(() => {
    if (staff) {
      const updateData: FormData = {
        id: staff.id,
        role: staff.role,
        profile: staff.profile,
        licenseNumber: "",
        yearsExperience: 0,
        biography: "",
        officeAddress: "",
        specializations: [],
        department: "",
      };

      if (staff.role === "Doctor") {
        updateData.licenseNumber = staff.licenseNumber || "";
        updateData.yearsExperience = staff.yearsExperience || 0;
        updateData.biography = staff.biography || "";
        updateData.officeAddress = staff.officeAddress || "";
        updateData.specializations =
          staff.specializations?.map((s) => s.id) || [];
      } else if (staff.role === "Receptionist") {
        updateData.department = staff.department || "";
      }

      setFormData(updateData);
    } else {
      setFormData({
        role: "Doctor" as StaffRole,
        profile: {
          firstName: "",
          lastName: "",
          email: "",
          phone: "",
          dateOfBirth: "",
          gender: "Male",
          addressLine1: "",
          addressLine2: "",
          city: "",
          state: "",
          zipCode: "",
          country: "",
        },
        licenseNumber: "",
        yearsExperience: 0,
        biography: "",
        officeAddress: "",
        specializations: [],
        department: "",
      });
    }
  }, [staff]);

  const handleProfileChange = (field: keyof UserProfile, value: string) => {
    setFormData((prev) => ({
      ...prev,
      profile: {
        ...prev.profile,
        [field]: value,
      },
    }));
    // Clear error when user starts typing
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: "" }));
    }
  };

  const handleFieldChange = (
    field: string,
    value: string | number | string[]
  ) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }));
    // Clear error when user starts typing
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: "" }));
    }
  };

  const handleSpecializationToggle = (specializationId: string) => {
    const currentSpecs = formData.specializations || [];
    const newSpecs = currentSpecs.includes(specializationId)
      ? currentSpecs.filter((id) => id !== specializationId)
      : [...currentSpecs, specializationId];

    handleFieldChange("specializations", newSpecs);
  };

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    // Validate required fields
    if (!formData.profile.firstName.trim()) {
      newErrors.firstName = "First name is required";
    }
    if (!formData.profile.lastName.trim()) {
      newErrors.lastName = "Last name is required";
    }
    if (!formData.profile.email.trim()) {
      newErrors.email = "Email is required";
    } else if (!/\S+@\S+\.\S+/.test(formData.profile.email)) {
      newErrors.email = "Please enter a valid email";
    }

    // Role-specific validation
    if (formData.role === "Doctor") {
      if (!formData.licenseNumber?.trim()) {
        newErrors.licenseNumber = "License number is required for doctors";
      }
      if (!formData.specializations?.length) {
        newErrors.specializations =
          "At least one specialization is required for doctors";
      }
    } else if (formData.role === "Receptionist") {
      if (!formData.department?.trim()) {
        newErrors.department = "Department is required for receptionists";
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    // Convert FormData to appropriate request type
    if (isEdit) {
      const updateRequest: UpdateStaffRequest = {
        id: formData.id!,
        role: formData.role,
        profile: formData.profile,
      };

      if (formData.role === "Doctor") {
        if (formData.licenseNumber)
          updateRequest.licenseNumber = formData.licenseNumber;
        if (formData.yearsExperience !== undefined)
          updateRequest.yearsExperience = formData.yearsExperience;
        if (formData.biography) updateRequest.biography = formData.biography;
        if (formData.officeAddress)
          updateRequest.officeAddress = formData.officeAddress;
        if (formData.specializations)
          updateRequest.specializations = formData.specializations;
      } else if (formData.role === "Receptionist") {
        if (formData.department) updateRequest.department = formData.department;
      }

      onSave(updateRequest);
    } else {
      const createRequest: CreateStaffRequest = {
        role: formData.role,
        profile: formData.profile,
      };

      if (formData.role === "Doctor") {
        createRequest.licenseNumber = formData.licenseNumber!;
        createRequest.yearsExperience = formData.yearsExperience!;
        createRequest.biography = formData.biography!;
        createRequest.officeAddress = formData.officeAddress!;
        createRequest.specializations = formData.specializations!;
      } else if (formData.role === "Receptionist") {
        createRequest.department = formData.department!;
      }

      onSave(createRequest);
    }

    onClose();
  };

  const handleClose = () => {
    setErrors({});
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} size="lg">
      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <h2 className="text-2xl font-bold text-gray-900">
            {isEdit ? "Edit Staff Member" : "Add New Staff Member"}
          </h2>
        </div>

        {/* Role Selection */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Role *
          </label>
          <select
            className="w-full px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-200"
            value={formData.role}
            onChange={(e) =>
              handleFieldChange("role", e.target.value as StaffRole)
            }
            disabled={isEdit} // Can't change role when editing
          >
            <option value="Doctor">Doctor</option>
            <option value="Receptionist">Receptionist</option>
          </select>
        </div>

        {/* Personal Information */}
        <div className="border-t pt-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            Personal Information
          </h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Input
                label="First Name *"
                value={formData.profile.firstName}
                onChange={(e) =>
                  handleProfileChange("firstName", e.target.value)
                }
                error={errors.firstName}
                placeholder="Enter first name"
              />
            </div>
            <div>
              <Input
                label="Last Name *"
                value={formData.profile.lastName}
                onChange={(e) =>
                  handleProfileChange("lastName", e.target.value)
                }
                error={errors.lastName}
                placeholder="Enter last name"
              />
            </div>
            <div>
              <Input
                label="Email *"
                type="email"
                value={formData.profile.email}
                onChange={(e) => handleProfileChange("email", e.target.value)}
                error={errors.email}
                placeholder="Enter email address"
              />
            </div>
            <div>
              <Input
                label="Phone"
                type="tel"
                value={formData.profile.phone || ""}
                onChange={(e) => handleProfileChange("phone", e.target.value)}
                placeholder="Enter phone number"
              />
            </div>
            <div>
              <Input
                label="Date of Birth"
                type="date"
                value={formData.profile.dateOfBirth || ""}
                onChange={(e) =>
                  handleProfileChange("dateOfBirth", e.target.value)
                }
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Gender
              </label>
              <select
                className="w-full px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-200"
                value={formData.profile.gender || "Male"}
                onChange={(e) =>
                  handleProfileChange(
                    "gender",
                    e.target.value as "Male" | "Female" | "Other"
                  )
                }
              >
                <option value="Male">Male</option>
                <option value="Female">Female</option>
                <option value="Other">Other</option>
              </select>
            </div>
          </div>
        </div>

        {/* Address Information */}
        <div className="border-t pt-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            Address Information
          </h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="md:col-span-2">
              <Input
                label="Address Line 1"
                value={formData.profile.addressLine1 || ""}
                onChange={(e) =>
                  handleProfileChange("addressLine1", e.target.value)
                }
                placeholder="Enter street address"
              />
            </div>
            <div className="md:col-span-2">
              <Input
                label="Address Line 2"
                value={formData.profile.addressLine2 || ""}
                onChange={(e) =>
                  handleProfileChange("addressLine2", e.target.value)
                }
                placeholder="Apartment, suite, etc."
              />
            </div>
            <div>
              <Input
                label="City"
                value={formData.profile.city || ""}
                onChange={(e) => handleProfileChange("city", e.target.value)}
                placeholder="Enter city"
              />
            </div>
            <div>
              <Input
                label="State"
                value={formData.profile.state || ""}
                onChange={(e) => handleProfileChange("state", e.target.value)}
                placeholder="Enter state"
              />
            </div>
            <div>
              <Input
                label="ZIP Code"
                value={formData.profile.zipCode || ""}
                onChange={(e) => handleProfileChange("zipCode", e.target.value)}
                placeholder="Enter ZIP code"
              />
            </div>
            <div>
              <Input
                label="Country"
                value={formData.profile.country || ""}
                onChange={(e) => handleProfileChange("country", e.target.value)}
                placeholder="Enter country"
              />
            </div>
          </div>
        </div>

        {/* Role-specific fields */}
        {formData.role === "Doctor" && (
          <div className="border-t pt-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">
              Professional Information
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <Input
                  label="License Number *"
                  value={formData.licenseNumber || ""}
                  onChange={(e) =>
                    handleFieldChange("licenseNumber", e.target.value)
                  }
                  error={errors.licenseNumber}
                  placeholder="Enter medical license number"
                />
              </div>
              <div>
                <Input
                  label="Years of Experience"
                  type="number"
                  value={formData.yearsExperience?.toString() || "0"}
                  onChange={(e) =>
                    handleFieldChange(
                      "yearsExperience",
                      parseInt(e.target.value) || 0
                    )
                  }
                  placeholder="Enter years of experience"
                  min="0"
                />
              </div>
              <div className="md:col-span-2">
                <Input
                  label="Office Address"
                  value={formData.officeAddress || ""}
                  onChange={(e) =>
                    handleFieldChange("officeAddress", e.target.value)
                  }
                  placeholder="Enter office location"
                />
              </div>
              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Biography
                </label>
                <textarea
                  className="w-full px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-200"
                  rows={4}
                  value={formData.biography || ""}
                  onChange={(e) =>
                    handleFieldChange("biography", e.target.value)
                  }
                  placeholder="Enter biography or professional summary"
                />
              </div>
            </div>

            {/* Specializations */}
            <div className="mt-6">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Specializations *
              </label>
              {errors.specializations && (
                <p className="text-red-600 text-sm mb-2">
                  {errors.specializations}
                </p>
              )}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
                {availableSpecializations.map((specialization) => (
                  <label
                    key={specialization.id}
                    className="flex items-center p-3 rounded-lg border border-gray-200 hover:bg-gray-50 cursor-pointer"
                  >
                    <input
                      type="checkbox"
                      className="mr-3"
                      checked={(formData.specializations || []).includes(
                        specialization.id
                      )}
                      onChange={() =>
                        handleSpecializationToggle(specialization.id)
                      }
                    />
                    <div>
                      <div className="font-medium text-gray-900">
                        {specialization.name}
                      </div>
                      {specialization.description && (
                        <div className="text-sm text-gray-600">
                          {specialization.description}
                        </div>
                      )}
                    </div>
                  </label>
                ))}
              </div>
            </div>
          </div>
        )}

        {formData.role === "Receptionist" && (
          <div className="border-t pt-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">
              Professional Information
            </h3>
            <div>
              <Input
                label="Department *"
                value={formData.department || ""}
                onChange={(e) =>
                  handleFieldChange("department", e.target.value)
                }
                error={errors.department}
                placeholder="Enter department name"
              />
            </div>
          </div>
        )}

        {/* Actions */}
        <div className="flex justify-end gap-3 pt-6 border-t border-gray-200">
          <Button type="button" variant="outline" onClick={handleClose}>
            Cancel
          </Button>
          <Button type="submit" variant="primary">
            {isEdit ? (
              <>
                <Save size={16} className="mr-2" />
                Update
              </>
            ) : (
              <>
                <Plus size={16} className="mr-2" />
                Create
              </>
            )}
          </Button>
        </div>
      </form>
    </Modal>
  );
};
