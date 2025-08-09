import React, { useState } from "react";

import {
  Medication,
  Patient,
  PrescriptionFormData,
  PrescriptionFormProps,
} from "../types";

export const PrescriptionForm: React.FC<PrescriptionFormProps> = ({
  prescription,
  patients,
  onSubmit,
  onCancel,
  isLoading = false,
}) => {
  const [formData, setFormData] = useState<PrescriptionFormData>(() => ({
    patientId: prescription?.patientId || "",
    diagnosis: prescription?.diagnosis || "",
    notes: prescription?.notes || "",
    medications: prescription?.medications.map((med) => ({
      name: med.name,
      ...(med.genericName && { genericName: med.genericName }),
      dosage: med.dosage,
      frequency: med.frequency,
      duration: med.duration,
      instructions: med.instructions,
      quantity: med.quantity,
      unit: med.unit,
      refills: med.refills,
      isGenericAllowed: med.isGenericAllowed,
    })) || [
      {
        name: "",
        genericName: "",
        dosage: "",
        frequency: "",
        duration: "",
        instructions: "",
        quantity: 1,
        unit: "tablets",
        refills: 0,
        isGenericAllowed: true,
      },
    ],
    validUntil:
      prescription?.validUntil ||
      new Date(Date.now() + 30 * 24 * 60 * 60 * 1000),
  }));

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  const addMedication = () => {
    setFormData((prev) => ({
      ...prev,
      medications: [
        ...prev.medications,
        {
          name: "",
          genericName: "",
          dosage: "",
          frequency: "",
          duration: "",
          instructions: "",
          quantity: 1,
          unit: "tablets",
          refills: 0,
          isGenericAllowed: true,
        },
      ],
    }));
  };

  const removeMedication = (index: number) => {
    setFormData((prev) => ({
      ...prev,
      medications: prev.medications.filter((_, i) => i !== index),
    }));
  };

  const updateMedication = (index: number, field: string, value: unknown) => {
    setFormData((prev) => ({
      ...prev,
      medications: prev.medications.map((med, i) =>
        i === index ? { ...med, [field]: value } : med
      ),
    }));
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg max-w-4xl w-full max-h-screen overflow-y-auto">
        <div className="p-6">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-xl font-semibold text-gray-900">
              {prescription ? "Edit Prescription" : "Create New Prescription"}
            </h2>
            <button
              onClick={onCancel}
              className="text-gray-400 hover:text-gray-600"
            >
              ×
            </button>
          </div>

          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Patient Selection */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Patient
              </label>
              <select
                value={formData.patientId}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    patientId: e.target.value,
                  }))
                }
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                required
              >
                <option value="">Select a patient</option>
                {patients.map((patient: Patient) => (
                  <option key={patient.id} value={patient.id}>
                    {patient.name} - {patient.email}
                  </option>
                ))}
              </select>
            </div>

            {/* Diagnosis */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Diagnosis
              </label>
              <input
                type="text"
                value={formData.diagnosis}
                onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                  setFormData((prev) => ({
                    ...prev,
                    diagnosis: e.target.value,
                  }))
                }
                required
                placeholder="Enter diagnosis"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            {/* Notes */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Notes (Optional)
              </label>
              <textarea
                value={formData.notes}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, notes: e.target.value }))
                }
                rows={3}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Additional notes or instructions"
              />
            </div>

            {/* Valid Until */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Valid Until
              </label>
              <input
                type="date"
                value={formData.validUntil.toISOString().split("T")[0]}
                onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                  setFormData((prev) => ({
                    ...prev,
                    validUntil: new Date(e.target.value),
                  }))
                }
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            {/* Medications */}
            <div>
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-medium text-gray-900">
                  Medications
                </h3>
                <button
                  type="button"
                  className="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50 transition-colors"
                  onClick={addMedication}
                >
                  Add Medication
                </button>
              </div>

              <div className="space-y-4">
                {formData.medications.map(
                  (medication: Omit<Medication, "id">, index: number) => (
                    <div
                      key={index}
                      className="border border-gray-200 rounded-lg p-4"
                    >
                      <div className="flex items-center justify-between mb-4">
                        <h4 className="font-medium text-gray-900">
                          Medication {index + 1}
                        </h4>
                        {formData.medications.length > 1 && (
                          <button
                            type="button"
                            className="px-3 py-1 text-sm border border-red-300 text-red-600 rounded hover:bg-red-50 transition-colors"
                            onClick={() => removeMedication(index)}
                          >
                            Remove
                          </button>
                        )}
                      </div>

                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <input
                          type="text"
                          placeholder="Medication Name"
                          value={medication.name}
                          onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                            updateMedication(index, "name", e.target.value)
                          }
                          required
                          className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                        />
                        <input
                          type="text"
                          placeholder="Generic Name (Optional)"
                          value={medication.genericName || ""}
                          onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                            updateMedication(
                              index,
                              "genericName",
                              e.target.value
                            )
                          }
                          className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                        />
                        <input
                          type="text"
                          placeholder="Dosage (e.g., 10mg)"
                          value={medication.dosage}
                          onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                            updateMedication(index, "dosage", e.target.value)
                          }
                          required
                          className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                        />
                        <input
                          type="text"
                          placeholder="Frequency (e.g., Once daily)"
                          value={medication.frequency}
                          onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                            updateMedication(index, "frequency", e.target.value)
                          }
                          required
                          className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                        />
                      </div>
                    </div>
                  )
                )}
              </div>
            </div>

            {/* Form Actions */}
            <div className="flex items-center justify-end gap-3 pt-6 border-t">
              <button
                type="button"
                className="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50 transition-colors"
                onClick={onCancel}
                disabled={isLoading}
              >
                Cancel
              </button>
              <button
                type="submit"
                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors disabled:opacity-50"
                disabled={isLoading}
              >
                {isLoading
                  ? "Saving..."
                  : prescription
                    ? "Update Prescription"
                    : "Create Prescription"}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};
