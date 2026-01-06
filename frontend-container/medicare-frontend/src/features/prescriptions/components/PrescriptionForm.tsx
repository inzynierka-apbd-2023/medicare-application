import React, { useState } from "react";

import { Patient, PrescriptionFormData, PrescriptionFormProps } from "../types";

export const PrescriptionForm: React.FC<PrescriptionFormProps> = ({
  prescription,
  patients,
  preSelectedPatientId,
  onSubmit,
  onCancel,
  isLoading = false,
}) => {
  const isEditMode = !!prescription;

  const [formData, setFormData] = useState<PrescriptionFormData>(() => ({
    patientId: prescription?.patientId || preSelectedPatientId || "",
    diagnosis: prescription?.diagnosis || "",
    notes: prescription?.notes || "",
    medications: [
      {
        name: prescription?.medications[0]?.name || "",
        genericName: prescription?.medications[0]?.genericName || "",
        dosage: prescription?.medications[0]?.dosage || "",
        frequency: prescription?.medications[0]?.frequency || "",
        duration: prescription?.medications[0]?.duration || "30",
        instructions: prescription?.medications[0]?.instructions || "",
        quantity: prescription?.medications[0]?.quantity || 1,
        unit: prescription?.medications[0]?.unit || "tablets",
        refills: prescription?.medications[0]?.refills || 0,
        isGenericAllowed:
          prescription?.medications[0]?.isGenericAllowed ?? true,
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

  const updateMedication = (field: string, value: unknown) => {
    setFormData((prev) => ({
      ...prev,
      medications: [{ ...prev.medications[0], [field]: value }],
    }));
  };

  // Get selected patient name for display in edit mode
  const selectedPatient = patients.find((p) => p.id === formData.patientId);

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg max-w-2xl w-full max-h-screen overflow-y-auto">
        <div className="p-6">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-xl font-semibold text-gray-900">
              {isEditMode ? "Edit Prescription" : "Create New Prescription"}
            </h2>
            <button
              onClick={onCancel}
              className="text-gray-400 hover:text-gray-600"
            >
              ×
            </button>
          </div>

          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Patient Selection - disabled in edit mode */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Patient
              </label>
              {isEditMode ? (
                <div className="w-full px-3 py-2 border border-gray-200 rounded-md bg-gray-50 text-gray-700">
                  {selectedPatient?.name || "Unknown Patient"} -{" "}
                  {selectedPatient?.email || ""}
                </div>
              ) : (
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
              )}
            </div>

            {/* Medication Details - single medication per prescription */}
            <div className="border border-gray-200 rounded-lg p-4">
              <h3 className="text-lg font-medium text-gray-900 mb-4">
                Medication Details
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <input
                  type="text"
                  placeholder="Medication Name *"
                  value={formData.medications[0].name}
                  onChange={(e) => updateMedication("name", e.target.value)}
                  required
                  className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <input
                  type="text"
                  placeholder="Generic Name (Optional)"
                  value={formData.medications[0].genericName || ""}
                  onChange={(e) =>
                    updateMedication("genericName", e.target.value)
                  }
                  className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <input
                  type="text"
                  placeholder="Dosage (e.g., 10mg) *"
                  value={formData.medications[0].dosage}
                  onChange={(e) => updateMedication("dosage", e.target.value)}
                  required
                  className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <input
                  type="text"
                  placeholder="Frequency (e.g., Once daily) *"
                  value={formData.medications[0].frequency}
                  onChange={(e) =>
                    updateMedication("frequency", e.target.value)
                  }
                  required
                  className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <input
                  type="number"
                  placeholder="Duration (days)"
                  value={formData.medications[0].duration}
                  onChange={(e) => updateMedication("duration", e.target.value)}
                  className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <input
                  type="text"
                  placeholder="Instructions"
                  value={formData.medications[0].instructions}
                  onChange={(e) =>
                    updateMedication("instructions", e.target.value)
                  }
                  className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>

            {/* Notes */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Diagnosis / Notes
              </label>
              <textarea
                value={formData.notes}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    notes: e.target.value,
                    diagnosis: e.target.value,
                  }))
                }
                rows={3}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Diagnosis and additional notes"
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
                  : isEditMode
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
