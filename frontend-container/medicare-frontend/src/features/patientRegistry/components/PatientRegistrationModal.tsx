import React, { useState } from "react";
import type {
  CreatePatientRequest,
  Doctor,
  PatientRegistrationFormData,
} from "@features/patientRegistry/types";
import { Button, Input, Modal } from "@shared/components";
import { Eye, EyeOff, X } from "lucide-react";

interface PatientRegistrationModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CreatePatientRequest) => Promise<void>;
  doctors: Doctor[];
  isLoading?: boolean;
}

const initialFormData: PatientRegistrationFormData = {
  // Personal Information
  firstName: "",
  lastName: "",
  email: "",
  phone: "",
  dateOfBirth: "",
  gender: "prefer-not-to-say",

  // Address
  addressLine1: "",
  addressLine2: "",
  city: "",
  state: "",
  zipCode: "",
  country: "Poland",

  // Medical Information
  bloodType: "",
  height: "",
  weight: "",
  generalDoctorId: "",

  // Insurance
  insuranceProvider: "",
  policyNumber: "",
  groupNumber: "",
  validFrom: "",
  validTo: "",

  // Emergency Contact
  emergencyContactName: "",
  emergencyContactPhone: "",
  emergencyContactRelationship: "",

  // Account Setup
  password: "",
  confirmPassword: "",
};

export const PatientRegistrationModal: React.FC<
  PatientRegistrationModalProps
> = ({ isOpen, onClose, onSubmit, doctors, isLoading = false }) => {
  const [formData, setFormData] =
    useState<PatientRegistrationFormData>(initialFormData);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [currentStep, setCurrentStep] = useState(1);
  const [errors, setErrors] = useState<Partial<PatientRegistrationFormData>>(
    {}
  );

  const steps = [
    { number: 1, title: "Personal Information" },
    { number: 2, title: "Address & Contact" },
    { number: 3, title: "Medical Information" },
    { number: 4, title: "Insurance & Emergency Contact" },
    { number: 5, title: "Account Setup" },
  ];

  const handleInputChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));

    // Clear error for this field
    if (errors[name as keyof PatientRegistrationFormData]) {
      setErrors((prev) => ({
        ...prev,
        [name]: undefined,
      }));
    }
  };

  const validateStep = (step: number): boolean => {
    const newErrors: Partial<PatientRegistrationFormData> = {};

    switch (step) {
      case 1:
        if (!formData.firstName.trim())
          newErrors.firstName = "First name is required";
        if (!formData.lastName.trim())
          newErrors.lastName = "Last name is required";
        if (!formData.email.trim()) newErrors.email = "Email is required";
        if (!formData.phone.trim()) newErrors.phone = "Phone is required";
        if (!formData.dateOfBirth)
          newErrors.dateOfBirth = "Date of birth is required";
        break;
      case 2:
        if (!formData.addressLine1.trim())
          newErrors.addressLine1 = "Address is required";
        if (!formData.city.trim()) newErrors.city = "City is required";
        if (!formData.state.trim()) newErrors.state = "State is required";
        if (!formData.zipCode.trim())
          newErrors.zipCode = "Zip code is required";
        break;
      case 3:
        if (!formData.generalDoctorId)
          newErrors.generalDoctorId = "General doctor is required";
        break;
      case 4:
        if (!formData.emergencyContactName.trim())
          newErrors.emergencyContactName = "Emergency contact name is required";
        if (!formData.emergencyContactPhone.trim())
          newErrors.emergencyContactPhone =
            "Emergency contact phone is required";
        if (!formData.emergencyContactRelationship.trim())
          newErrors.emergencyContactRelationship = "Relationship is required";
        break;
      case 5:
        if (!formData.password) newErrors.password = "Password is required";
        if (formData.password.length < 8)
          newErrors.password = "Password must be at least 8 characters";
        if (formData.password !== formData.confirmPassword)
          newErrors.confirmPassword = "Passwords do not match";
        break;
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleNext = () => {
    if (validateStep(currentStep)) {
      setCurrentStep((prev) => Math.min(prev + 1, steps.length));
    }
  };

  const handlePrevious = () => {
    setCurrentStep((prev) => Math.max(prev - 1, 1));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateStep(currentStep)) return;

    const request: CreatePatientRequest = {
      personalInfo: {
        firstName: formData.firstName,
        lastName: formData.lastName,
        email: formData.email,
        phone: formData.phone,
        dateOfBirth: formData.dateOfBirth,
        gender: formData.gender,
      },
      address: {
        addressLine1: formData.addressLine1,
        ...(formData.addressLine2 && { addressLine2: formData.addressLine2 }),
        city: formData.city,
        state: formData.state,
        zipCode: formData.zipCode,
        country: formData.country,
      },
      medicalInfo: {
        ...(formData.bloodType && { bloodType: formData.bloodType }),
        ...(formData.height && { height: parseFloat(formData.height) }),
        ...(formData.weight && { weight: parseFloat(formData.weight) }),
        ...(formData.generalDoctorId && {
          generalDoctorId: formData.generalDoctorId,
        }),
      },
      ...(formData.insuranceProvider && {
        insurance: {
          providerName: formData.insuranceProvider,
          policyNumber: formData.policyNumber,
          ...(formData.groupNumber && { groupNumber: formData.groupNumber }),
          validFrom: formData.validFrom,
          ...(formData.validTo && { validTo: formData.validTo }),
          isPrimary: true,
        },
      }),
      emergencyContact: {
        name: formData.emergencyContactName,
        phone: formData.emergencyContactPhone,
        relationship: formData.emergencyContactRelationship,
      },
      accountSetup: {
        password: formData.password,
      },
    };

    try {
      await onSubmit(request);
      setFormData(initialFormData);
      setCurrentStep(1);
      setErrors({});
      onClose();
    } catch (error) {
      // Error handled by parent
      console.error("Registration failed:", error);
    }
  };

  const handleClose = () => {
    setFormData(initialFormData);
    setCurrentStep(1);
    setErrors({});
    onClose();
  };

  const renderStep = () => {
    switch (currentStep) {
      case 1:
        return (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  First Name *
                </label>
                <Input
                  name="firstName"
                  value={formData.firstName}
                  onChange={handleInputChange}
                  placeholder="John"
                  required
                  {...(errors.firstName && { error: errors.firstName })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Last Name *
                </label>
                <Input
                  name="lastName"
                  value={formData.lastName}
                  onChange={handleInputChange}
                  placeholder="Doe"
                  required
                  {...(errors.lastName && { error: errors.lastName })}
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Email Address *
              </label>
              <Input
                name="email"
                type="email"
                value={formData.email}
                onChange={handleInputChange}
                placeholder="john.doe@example.com"
                required
                {...(errors.email && { error: errors.email })}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Phone Number *
              </label>
              <Input
                name="phone"
                type="tel"
                value={formData.phone}
                onChange={handleInputChange}
                placeholder="+48 123 456 789"
                required
                {...(errors.phone && { error: errors.phone })}
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Date of Birth *
                </label>
                <Input
                  name="dateOfBirth"
                  type="date"
                  value={formData.dateOfBirth}
                  onChange={handleInputChange}
                  required
                  {...(errors.dateOfBirth && { error: errors.dateOfBirth })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Gender
                </label>
                <select
                  name="gender"
                  value={formData.gender}
                  onChange={handleInputChange}
                  className="w-full p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="prefer-not-to-say">Prefer not to say</option>
                  <option value="male">Male</option>
                  <option value="female">Female</option>
                  <option value="other">Other</option>
                </select>
              </div>
            </div>
          </div>
        );

      case 2:
        return (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Address Line 1 *
              </label>
              <Input
                name="addressLine1"
                value={formData.addressLine1}
                onChange={handleInputChange}
                placeholder="123 Main Street"
                required
                {...(errors.addressLine1 && { error: errors.addressLine1 })}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Address Line 2
              </label>
              <Input
                name="addressLine2"
                value={formData.addressLine2}
                onChange={handleInputChange}
                placeholder="Apartment, suite, etc."
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  City *
                </label>
                <Input
                  name="city"
                  value={formData.city}
                  onChange={handleInputChange}
                  placeholder="Warsaw"
                  required
                  {...(errors.city && { error: errors.city })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  State/Province *
                </label>
                <Input
                  name="state"
                  value={formData.state}
                  onChange={handleInputChange}
                  placeholder="Mazowieckie"
                  required
                  {...(errors.state && { error: errors.state })}
                />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Zip Code *
                </label>
                <Input
                  name="zipCode"
                  value={formData.zipCode}
                  onChange={handleInputChange}
                  placeholder="00-001"
                  required
                  {...(errors.zipCode && { error: errors.zipCode })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Country
                </label>
                <Input
                  name="country"
                  value={formData.country}
                  onChange={handleInputChange}
                  placeholder="Poland"
                />
              </div>
            </div>
          </div>
        );

      case 3:
        return (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                General Doctor *
              </label>
              <select
                name="generalDoctorId"
                value={formData.generalDoctorId}
                onChange={handleInputChange}
                required
                className="w-full p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Select a doctor</option>
                {doctors.map((doctor) => (
                  <option key={doctor.id} value={doctor.id}>
                    {doctor.firstName} {doctor.lastName} -{" "}
                    {doctor.specialization}
                  </option>
                ))}
              </select>
              {errors.generalDoctorId && (
                <p className="text-red-600 text-sm mt-1">
                  {errors.generalDoctorId}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Blood Type
              </label>
              <select
                name="bloodType"
                value={formData.bloodType}
                onChange={handleInputChange}
                className="w-full p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
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
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Height (cm)
                </label>
                <Input
                  name="height"
                  type="number"
                  value={formData.height}
                  onChange={handleInputChange}
                  placeholder="170"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Weight (kg)
                </label>
                <Input
                  name="weight"
                  type="number"
                  value={formData.weight}
                  onChange={handleInputChange}
                  placeholder="70"
                />
              </div>
            </div>
          </div>
        );

      case 4:
        return (
          <div className="space-y-4">
            <div className="border-b pb-4 mb-4">
              <h4 className="text-lg font-medium text-gray-900 mb-3">
                Insurance Information
              </h4>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Insurance Provider
                  </label>
                  <Input
                    name="insuranceProvider"
                    value={formData.insuranceProvider}
                    onChange={handleInputChange}
                    placeholder="NFZ, PZU Zdrowie, etc."
                  />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Policy Number
                    </label>
                    <Input
                      name="policyNumber"
                      value={formData.policyNumber}
                      onChange={handleInputChange}
                      placeholder="POL123456"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Group Number
                    </label>
                    <Input
                      name="groupNumber"
                      value={formData.groupNumber}
                      onChange={handleInputChange}
                      placeholder="GRP123"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Valid From
                    </label>
                    <Input
                      name="validFrom"
                      type="date"
                      value={formData.validFrom}
                      onChange={handleInputChange}
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Valid To
                    </label>
                    <Input
                      name="validTo"
                      type="date"
                      value={formData.validTo}
                      onChange={handleInputChange}
                    />
                  </div>
                </div>
              </div>
            </div>

            <div>
              <h4 className="text-lg font-medium text-gray-900 mb-3">
                Emergency Contact
              </h4>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Contact Name *
                  </label>
                  <Input
                    name="emergencyContactName"
                    value={formData.emergencyContactName}
                    onChange={handleInputChange}
                    placeholder="Jane Doe"
                    required
                    {...(errors.emergencyContactName && {
                      error: errors.emergencyContactName,
                    })}
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Contact Phone *
                  </label>
                  <Input
                    name="emergencyContactPhone"
                    type="tel"
                    value={formData.emergencyContactPhone}
                    onChange={handleInputChange}
                    placeholder="+48 123 456 789"
                    required
                    {...(errors.emergencyContactPhone && {
                      error: errors.emergencyContactPhone,
                    })}
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Relationship *
                  </label>
                  <select
                    name="emergencyContactRelationship"
                    value={formData.emergencyContactRelationship}
                    onChange={handleInputChange}
                    required
                    className="w-full p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="">Select relationship</option>
                    <option value="Spouse">Spouse</option>
                    <option value="Parent">Parent</option>
                    <option value="Child">Child</option>
                    <option value="Sibling">Sibling</option>
                    <option value="Friend">Friend</option>
                    <option value="Other">Other</option>
                  </select>
                  {errors.emergencyContactRelationship && (
                    <p className="text-red-600 text-sm mt-1">
                      {errors.emergencyContactRelationship}
                    </p>
                  )}
                </div>
              </div>
            </div>
          </div>
        );

      case 5:
        return (
          <div className="space-y-4">
            <div className="text-center mb-6">
              <h4 className="text-lg font-medium text-gray-900 mb-2">
                Account Setup
              </h4>
              <p className="text-sm text-gray-600">
                Create login credentials for the patient's account
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Password *
              </label>
              <Input
                name="password"
                type={showPassword ? "text" : "password"}
                value={formData.password}
                onChange={handleInputChange}
                placeholder="Create a strong password"
                required
                {...(errors.password && { error: errors.password })}
                rightIcon={
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="text-gray-400 hover:text-gray-600 focus:outline-none"
                  >
                    {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                  </button>
                }
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Confirm Password *
              </label>
              <Input
                name="confirmPassword"
                type={showConfirmPassword ? "text" : "password"}
                value={formData.confirmPassword}
                onChange={handleInputChange}
                placeholder="Confirm the password"
                required
                {...(errors.confirmPassword && {
                  error: errors.confirmPassword,
                })}
                rightIcon={
                  <button
                    type="button"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                    className="text-gray-400 hover:text-gray-600 focus:outline-none"
                  >
                    {showConfirmPassword ? (
                      <EyeOff size={20} />
                    ) : (
                      <Eye size={20} />
                    )}
                  </button>
                }
              />
            </div>

            <div className="bg-blue-50 p-4 rounded-md">
              <p className="text-sm text-blue-700">
                <strong>Note:</strong> The patient will receive their login
                credentials via email and will be able to change their password
                on first login.
              </p>
            </div>
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} size="lg">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-xl font-semibold text-gray-900">
          Register New Patient
        </h2>
        <button
          onClick={handleClose}
          className="text-gray-400 hover:text-gray-600"
        >
          <X size={24} />
        </button>
      </div>

      {/* Step Indicator */}
      <div className="mb-8">
        <div className="flex items-center justify-between">
          {steps.map((step, index) => (
            <div key={step.number} className="flex items-center">
              <div
                className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium ${
                  step.number === currentStep
                    ? "bg-blue-600 text-white"
                    : step.number < currentStep
                      ? "bg-green-500 text-white"
                      : "bg-gray-200 text-gray-600"
                }`}
              >
                {step.number}
              </div>
              {index < steps.length - 1 && (
                <div
                  className={`h-0.5 w-16 mx-2 ${
                    step.number < currentStep ? "bg-green-500" : "bg-gray-200"
                  }`}
                />
              )}
            </div>
          ))}
        </div>
        <div className="mt-2">
          <p className="text-sm font-medium text-gray-900">
            Step {currentStep}: {steps[currentStep - 1].title}
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit}>
        {renderStep()}

        {/* Navigation Buttons */}
        <div className="flex justify-between mt-8">
          <Button
            type="button"
            variant="secondary"
            onClick={handlePrevious}
            disabled={currentStep === 1}
          >
            Previous
          </Button>

          <div className="flex space-x-3">
            <Button type="button" variant="secondary" onClick={handleClose}>
              Cancel
            </Button>

            {currentStep < steps.length ? (
              <Button type="button" onClick={handleNext}>
                Next
              </Button>
            ) : (
              <Button type="submit" disabled={isLoading}>
                {isLoading ? "Creating..." : "Create Patient"}
              </Button>
            )}
          </div>
        </div>
      </form>
    </Modal>
  );
};
