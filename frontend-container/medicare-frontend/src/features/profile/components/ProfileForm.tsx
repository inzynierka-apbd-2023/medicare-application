import React, { useState } from "react";
import { Input, Button, Card } from "../../../shared/components";
import type { ProfileFormProps, ProfileData } from "../types";

export const ProfileForm: React.FC<ProfileFormProps> = ({
  profileData,
  onSave,
  onCancel,
  isLoading = false,
}) => {
  const [formData, setFormData] = useState<Partial<ProfileData>>({
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

    if (!formData.name?.trim()) {
      newErrors.name = "Name is required";
    }

    if (!formData.email?.trim()) {
      newErrors.email = "Email is required";
    } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
      newErrors.email = "Please enter a valid email address";
    }

    if (!formData.phone?.trim()) {
      newErrors.phone = "Phone number is required";
    }

    if (!formData.address?.trim()) {
      newErrors.address = "Address is required";
    }

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
        <Input
          label="Full Name"
          value={formData.name || ""}
          onChange={(e) => handleInputChange("name", e.target.value)}
          error={errors.name}
          placeholder="Enter your full name"
          required
        />

        <Input
          label="Email Address"
          type="email"
          value={formData.email || ""}
          onChange={(e) => handleInputChange("email", e.target.value)}
          error={errors.email}
          placeholder="Enter your email address"
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

        <Input
          label="Address"
          value={formData.address || ""}
          onChange={(e) => handleInputChange("address", e.target.value)}
          error={errors.address}
          placeholder="Enter your address"
          required
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
