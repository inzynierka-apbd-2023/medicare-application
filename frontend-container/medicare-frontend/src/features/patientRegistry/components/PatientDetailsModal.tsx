import React, { useEffect, useState } from "react";
import type {
  Doctor,
  PatientRegistryInfo,
} from "@features/patientRegistry/types";
import { Button, Input, Modal } from "@shared/components";

interface PatientDetailsModalProps {
  isOpen: boolean;
  onClose: () => void;
  patient: PatientRegistryInfo;
  doctors: Doctor[];
  isEditMode?: boolean;
  onUpdate: (data: Partial<PatientRegistryInfo>) => Promise<void>;
}

export const PatientDetailsModal: React.FC<PatientDetailsModalProps> = ({
  isOpen,
  onClose,
  patient,
  doctors,
  isEditMode = false,
  onUpdate,
}) => {
  const [isEditing, setIsEditing] = useState(isEditMode);
  const [formData, setFormData] = useState<Partial<PatientRegistryInfo>>({});
  const [isLoading, setIsLoading] = useState(false);

  const doctor = doctors.find((d) => d.id === patient.generalDoctorId);

  // Update form data when patient changes
  useEffect(() => {
    setFormData({
      firstName: patient.firstName,
      lastName: patient.lastName,
      email: patient.email,
      phone: patient.phone,
      addressLine1: patient.addressLine1 || "",
      addressLine2: patient.addressLine2 || "",
      city: patient.city || "",
      state: patient.state || "",
      zipCode: patient.zipCode || "",
      country: patient.country || "",
      bloodType: patient.bloodType || "",
      isActive: patient.isActive ?? true,
      ...(patient.height && { height: patient.height }),
      ...(patient.weight && { weight: patient.weight }),
      generalDoctorId: patient.generalDoctorId || "",
    });
  }, [patient]);

  // Update editing state when isEditMode changes
  useEffect(() => {
    setIsEditing(isEditMode);
  }, [isEditMode]);

  const handleInputChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]:
        name === "height" || name === "weight"
          ? value
            ? Number(value)
            : undefined
          : value,
    }));
  };

  const handleSave = async () => {
    setIsLoading(true);
    try {
      await onUpdate(formData);
      setIsEditing(false);
    } catch (error) {
      console.error("Failed to update patient:", error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCancel = () => {
    setFormData({
      firstName: patient.firstName,
      lastName: patient.lastName,
      email: patient.email,
      phone: patient.phone,
      addressLine1: patient.addressLine1 || "",
      addressLine2: patient.addressLine2 || "",
      city: patient.city || "",
      state: patient.state || "",
      zipCode: patient.zipCode || "",
      country: patient.country || "",
      bloodType: patient.bloodType || "",
      isActive: patient.isActive ?? true,
      ...(patient.height && { height: patient.height }),
      ...(patient.weight && { weight: patient.weight }),
      generalDoctorId: patient.generalDoctorId || "",
    });
    setIsEditing(false);
  };

  const handleStatusToggle = () => {
    setFormData((prev) => ({
      ...prev,
      isActive: !prev.isActive,
    }));
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="lg">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold text-gray-900">
          {isEditing ? "Edit Patient" : "Patient Details"}
        </h2>
        {!isEditing && (
          <Button
            variant="secondary"
            onClick={() => setIsEditing(true)}
            className="flex items-center space-x-2"
          >
            <span>Edit</span>
          </Button>
        )}
      </div>

      <div className="space-y-6">
        {/* Basic Information */}
        <div>
          <h3 className="text-lg font-medium text-gray-900 mb-4">
            Basic Information
          </h3>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">
                First Name
              </label>
              <p className="mt-1 text-sm text-gray-900">{patient.firstName}</p>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Last Name
              </label>
              <p className="mt-1 text-sm text-gray-900">{patient.lastName}</p>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Medical Record Number
              </label>
              <p className="mt-1 text-sm text-gray-900 font-mono">
                {patient.medicalRecordNumber}
              </p>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Email
              </label>
              <p className="mt-1 text-sm text-gray-900">{patient.email}</p>
              {isEditing && (
                <p className="text-xs text-gray-500 italic">
                  Email cannot be changed
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Phone
              </label>
              <p className="mt-1 text-sm text-gray-900">{patient.phone}</p>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Date of Birth
              </label>
              <p className="mt-1 text-sm text-gray-900">
                {new Date(patient.dateOfBirth).toLocaleDateString()}
              </p>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Gender
              </label>
              <p className="mt-1 text-sm text-gray-900 capitalize">
                {patient.gender || "Not specified"}
              </p>
            </div>
            {/* Status Toggle */}
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Status
              </label>
              {isEditing ? (
                <button
                  type="button"
                  onClick={handleStatusToggle}
                  className={`mt-1 px-3 py-1 rounded-full text-sm font-medium transition-colors ${
                    formData.isActive
                      ? "bg-green-100 text-green-800 hover:bg-green-200"
                      : "bg-red-100 text-red-800 hover:bg-red-200"
                  }`}
                >
                  {formData.isActive ? "Active" : "Inactive"}
                </button>
              ) : (
                <p
                  className={`mt-1 text-sm font-medium ${patient.isActive ? "text-green-600" : "text-red-600"}`}
                >
                  {patient.isActive ? "Active" : "Inactive"}
                </p>
              )}
            </div>
          </div>
        </div>

        {/* Address Information */}
        <div>
          <h3 className="text-lg font-medium text-gray-900 mb-4">Address</h3>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Address Line 1
              </label>
              {isEditing ? (
                <Input
                  name="addressLine1"
                  value={formData.addressLine1 || ""}
                  onChange={handleInputChange}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-sm text-gray-900">
                  {patient.addressLine1}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Address Line 2
              </label>
              {isEditing ? (
                <Input
                  name="addressLine2"
                  value={formData.addressLine2 || ""}
                  onChange={handleInputChange}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-sm text-gray-900">
                  {patient.addressLine2 || "N/A"}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                City
              </label>
              {isEditing ? (
                <Input
                  name="city"
                  value={formData.city || ""}
                  onChange={handleInputChange}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-sm text-gray-900">{patient.city}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                State
              </label>
              {isEditing ? (
                <Input
                  name="state"
                  value={formData.state || ""}
                  onChange={handleInputChange}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-sm text-gray-900">{patient.state}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Zip Code
              </label>
              {isEditing ? (
                <Input
                  name="zipCode"
                  value={formData.zipCode || ""}
                  onChange={handleInputChange}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-sm text-gray-900">{patient.zipCode}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Country
              </label>
              {isEditing ? (
                <Input
                  name="country"
                  value={formData.country || ""}
                  onChange={handleInputChange}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-sm text-gray-900">{patient.country}</p>
              )}
            </div>
          </div>
        </div>

        {/* Medical Information */}
        <div>
          <h3 className="text-lg font-medium text-gray-900 mb-4">
            Medical Information
          </h3>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">
                General Doctor
              </label>
              {isEditing ? (
                <select
                  name="generalDoctorId"
                  value={formData.generalDoctorId || ""}
                  onChange={handleInputChange}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                >
                  <option value="">Select a doctor</option>
                  {doctors.map((doc) => (
                    <option key={doc.id} value={doc.id}>
                      {doc.firstName} {doc.lastName}
                      {doc.specialty || doc.specialization
                        ? ` - ${doc.specialty || doc.specialization}`
                        : ""}
                    </option>
                  ))}
                </select>
              ) : (
                <p className="mt-1 text-sm text-gray-900">
                  {doctor
                    ? `${doctor.firstName} ${doctor.lastName}${doctor.specialty || doctor.specialization ? ` - ${doctor.specialty || doctor.specialization}` : ""}`
                    : "Not assigned"}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Blood Type
              </label>
              {isEditing ? (
                <select
                  name="bloodType"
                  value={formData.bloodType || ""}
                  onChange={handleInputChange}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                >
                  <option value="">Select blood type</option>
                  <option value="A+">A+</option>
                  <option value="A-">A-</option>
                  <option value="B+">B+</option>
                  <option value="B-">B-</option>
                  <option value="AB+">AB+</option>
                  <option value="AB-">AB-</option>
                  <option value="O+">O+</option>
                  <option value="O-">O-</option>
                </select>
              ) : (
                <p className="mt-1 text-sm text-gray-900">
                  {patient.bloodType || "Not specified"}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Height (cm)
              </label>
              {isEditing ? (
                <Input
                  name="height"
                  type="number"
                  value={formData.height?.toString() || ""}
                  onChange={handleInputChange}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-sm text-gray-900">
                  {patient.height ? `${patient.height} cm` : "Not specified"}
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Weight (kg)
              </label>
              {isEditing ? (
                <Input
                  name="weight"
                  type="number"
                  value={formData.weight?.toString() || ""}
                  onChange={handleInputChange}
                  className="mt-1"
                />
              ) : (
                <p className="mt-1 text-sm text-gray-900">
                  {patient.weight ? `${patient.weight} kg` : "Not specified"}
                </p>
              )}
            </div>
          </div>
        </div>

        {/* Insurance Information - Read only for now */}
        {patient.insurance && patient.insurance.length > 0 && (
          <div>
            <h3 className="text-lg font-medium text-gray-900 mb-4">
              Insurance Information
            </h3>
            {patient.insurance.map((ins, index) => (
              <div
                key={index}
                className="border border-gray-200 rounded-md p-4 mb-4"
              >
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">
                      Provider
                    </label>
                    <p className="mt-1 text-sm text-gray-900">
                      {ins.providerName}
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">
                      Policy Number
                    </label>
                    <p className="mt-1 text-sm text-gray-900">
                      {ins.policyNumber}
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">
                      Group Number
                    </label>
                    <p className="mt-1 text-sm text-gray-900">
                      {ins.groupNumber || "N/A"}
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">
                      Valid From
                    </label>
                    <p className="mt-1 text-sm text-gray-900">
                      {new Date(ins.validFrom).toLocaleDateString()}
                    </p>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Emergency Contacts - Read only for now */}
        {patient.emergencyContacts && patient.emergencyContacts.length > 0 && (
          <div>
            <h3 className="text-lg font-medium text-gray-900 mb-4">
              Emergency Contacts
            </h3>
            {patient.emergencyContacts.map((contact, index) => (
              <div
                key={index}
                className="border border-gray-200 rounded-md p-4 mb-4"
              >
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">
                      Name
                    </label>
                    <p className="mt-1 text-sm text-gray-900">{contact.name}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">
                      Relationship
                    </label>
                    <p className="mt-1 text-sm text-gray-900">
                      {contact.relationship}
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">
                      Phone
                    </label>
                    <p className="mt-1 text-sm text-gray-900">
                      {contact.phone}
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700">
                      Primary Contact
                    </label>
                    <p className="mt-1 text-sm text-gray-900">
                      {contact.isPrimary ? "Yes" : "No"}
                    </p>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Modal Actions */}
      <div className="flex justify-end space-x-4 mt-8 pt-6 border-t border-gray-200">
        {isEditing ? (
          <>
            <Button variant="secondary" onClick={handleCancel}>
              Cancel
            </Button>
            <Button onClick={handleSave} disabled={isLoading}>
              {isLoading ? "Saving..." : "Save Changes"}
            </Button>
          </>
        ) : (
          <Button variant="secondary" onClick={onClose}>
            Close
          </Button>
        )}
      </div>
    </Modal>
  );
};
