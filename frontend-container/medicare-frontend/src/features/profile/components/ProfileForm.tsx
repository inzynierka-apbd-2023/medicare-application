import React, { useState } from "react";

import { Button, Card, Input } from "../../../shared/components";
import type { ProfileData, ProfileFormProps } from "../types";

export const ProfileForm: React.FC<ProfileFormProps> = ({
  profileData,
  onSave,
  onCancel,
  isLoading = false,
}) => {
  // Derive split names if available
  const initialFirst =
    profileData.firstName || profileData.name.split(" ")[0] || "";
  const initialLast =
    profileData.lastName ||
    profileData.name.split(" ").slice(1).join(" ") ||
    "";
  const [formData, setFormData] = useState<Partial<ProfileData>>({
    firstName: initialFirst,
    lastName: initialLast,
    name: profileData.name,
    email: profileData.email,
    phone: profileData.phone,
    address: profileData.address,
    dateOfBirth: profileData.dateOfBirth,
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const handleInputChange = (field: keyof ProfileData, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    // Clear error when user starts typing
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: "" }));
    }
  };

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.firstName?.trim())
      newErrors.firstName = "First name is required";
    if (!formData.lastName?.trim())
      newErrors.lastName = "Last name is required";
    if (!formData.email?.trim()) newErrors.email = "Email is required";

    if (!formData.phone?.trim()) {
      newErrors.phone = "Phone number is required";
    }

    // Address optional (not persisted yet)

    if (!formData.dateOfBirth) {
      newErrors.dateOfBirth = "Date of birth is required";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    try {
      await onSave(formData);
    } catch (error) {
      console.error("Save error:", error);
    }
  };

  return (
    <Card
      variant="medical"
      header={
        <h3 className="text-xl font-semibold text-blue-600">Edit Profile</h3>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input
            label="First Name"
            value={formData.firstName || ""}
            onChange={(e) => handleInputChange("firstName", e.target.value)}
            error={errors.firstName}
            required
          />
          <Input
            label="Last Name"
            value={formData.lastName || ""}
            onChange={(e) => handleInputChange("lastName", e.target.value)}
            error={errors.lastName}
            required
          />
        </div>

        <Input
          label="Email Address"
          type="email"
          value={formData.email || ""}
          onChange={(e) => handleInputChange("email", e.target.value)}
          error={errors.email}
          required
        />

        <Input
          label="Phone Number"
          type="tel"
          value={formData.phone || ""}
          onChange={(e) => handleInputChange("phone", e.target.value)}
          error={errors.phone}
          placeholder="Enter your phone number"
          required
        />

        {/* Address not persisted yet */}
        <Input
          label="Address (not saved yet)"
          value={formData.address || ""}
          onChange={(e) => handleInputChange("address", e.target.value)}
          placeholder="Enter your address"
        />

        <Input
          label="Date of Birth"
          type="date"
          value={formData.dateOfBirth || ""}
          onChange={(e) => handleInputChange("dateOfBirth", e.target.value)}
          error={errors.dateOfBirth}
          required
        />

        <div className="flex space-x-3 pt-4">
          <Button
            type="submit"
            loading={isLoading}
            disabled={isLoading}
            className="flex-1"
          >
            Save Changes
          </Button>
          <Button
            type="button"
            variant="ghost"
            onClick={onCancel}
            disabled={isLoading}
            className="flex-1"
          >
            Cancel
          </Button>
        </div>
      </form>
    </Card>
  );
};
