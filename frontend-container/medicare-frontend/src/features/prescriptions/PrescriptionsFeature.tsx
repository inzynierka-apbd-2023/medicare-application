import React, { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";

import { usePrescriptions } from "./hooks/usePrescriptions";
import { PrescriptionForm, PrescriptionList } from "./components";
import { Prescription, PrescriptionFormData } from "./types";

export const PrescriptionsFeature: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const patientIdFromUrl = searchParams.get("patientId");

  const {
    prescriptions,
    patients,
    isLoading,
    error,
    createPrescription,
    updatePrescription,
    deletePrescription,
    setSelectedPrescription,
    clearError,
  } = usePrescriptions();

  const [showForm, setShowForm] = useState(false);
  const [editingPrescription, setEditingPrescription] =
    useState<Prescription | null>(null);
  const [preSelectedPatientId, setPreSelectedPatientId] = useState<
    string | null
  >(null);
  const [searchTerm, setSearchTerm] = useState("");

  // Auto-open form when patientId is in URL
  useEffect(() => {
    if (patientIdFromUrl && patients.length > 0) {
      // Check if the patient exists in the list
      const patientExists = patients.some((p) => p.id === patientIdFromUrl);
      if (patientExists) {
        setPreSelectedPatientId(patientIdFromUrl);
        setEditingPrescription(null);
        setShowForm(true);
        // Clear the URL param after opening the modal
        setSearchParams({});
      }
    }
  }, [patientIdFromUrl, patients, setSearchParams]);

  // Filter prescriptions based on search term
  const filteredPrescriptions = prescriptions.filter(
    (prescription) =>
      prescription.diagnosis.toLowerCase().includes(searchTerm.toLowerCase()) ||
      prescription.medications.some(
        (med) =>
          med.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
          (med.genericName &&
            med.genericName.toLowerCase().includes(searchTerm.toLowerCase()))
      ) ||
      prescription.notes?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      prescription.id.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleCreatePrescription = () => {
    setEditingPrescription(null);
    setShowForm(true);
  };

  const handleEditPrescription = (prescription: Prescription) => {
    setEditingPrescription(prescription);
    setShowForm(true);
  };

  const handleFormSubmit = async (data: PrescriptionFormData) => {
    try {
      if (editingPrescription) {
        await updatePrescription(editingPrescription.id, data);
      } else {
        await createPrescription(data);
      }
      setShowForm(false);
      setEditingPrescription(null);
    } catch (_error) {
      // Error is handled by the hook
    }
  };

  const handleFormCancel = () => {
    setShowForm(false);
    setEditingPrescription(null);
    setPreSelectedPatientId(null);
  };

  const handleDeletePrescription = async (prescriptionId: string) => {
    if (window.confirm("Are you sure you want to delete this prescription?")) {
      try {
        await deletePrescription(prescriptionId);
      } catch (_error) {
        // Error is handled by the hook
      }
    }
  };

  const handlePrescriptionSelect = (prescription: Prescription) => {
    setSelectedPrescription(prescription);
    // Could open a details modal or navigate to details page
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            Prescription Management
          </h1>
          <p className="text-gray-600">
            Create and manage patient prescriptions
          </p>
        </div>
        <button
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
          onClick={handleCreatePrescription}
        >
          Create Prescription
        </button>
      </div>

      {/* Error Display */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-md p-4">
          <div className="flex items-center">
            <div className="text-red-800">
              <h3 className="text-sm font-medium">Error</h3>
              <p className="text-sm mt-1">{error}</p>
            </div>
            <button
              onClick={clearError}
              className="ml-auto text-red-400 hover:text-red-600"
            >
              ×
            </button>
          </div>
        </div>
      )}

      {/* Search and Filters */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-center gap-4">
          <div className="flex-1">
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Search prescriptions by diagnosis, medication, notes, or ID..."
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
          <div className="text-sm text-gray-500">
            {filteredPrescriptions.length} of {prescriptions.length}{" "}
            prescriptions
          </div>
        </div>
      </div>

      {/* Prescriptions List */}
      <div className="bg-white rounded-lg shadow">
        <div className="p-6">
          <PrescriptionList
            prescriptions={filteredPrescriptions}
            onPrescriptionSelect={handlePrescriptionSelect}
            onPrescriptionEdit={handleEditPrescription}
            onPrescriptionDelete={handleDeletePrescription}
            isLoading={isLoading}
          />
        </div>
      </div>

      {/* Prescription Form Modal */}
      {showForm && (
        <PrescriptionForm
          prescription={editingPrescription || undefined}
          patients={patients}
          preSelectedPatientId={preSelectedPatientId}
          onSubmit={handleFormSubmit}
          onCancel={handleFormCancel}
          isLoading={isLoading}
        />
      )}
    </div>
  );
};
