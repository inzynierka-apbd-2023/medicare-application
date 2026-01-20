import React from "react";
import type { MedicalRecordDetailModalProps } from "@features/medicalRecords/types";
import { Badge, Button, Modal } from "@shared/components";
import { Calendar, Heart, Phone, Pill, Shield, User } from "lucide-react";

export const MedicalRecordDetailModal: React.FC<
  MedicalRecordDetailModalProps
> = ({ isOpen, onClose, record, section }) => {
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const getStatusBadgeVariant = (status: string) => {
    switch (status.toLowerCase()) {
      case "active":
        return "success";
      case "resolved":
        return "default";
      case "chronic":
        return "warning";
      case "critical":
        return "error";
      case "discontinued":
        return "error";
      case "completed":
        return "success";
      default:
        return "default";
    }
  };

  const getSeverityBadgeVariant = (severity: string) => {
    switch (severity.toLowerCase()) {
      case "mild":
        return "success";
      case "moderate":
        return "warning";
      case "severe":
      case "life-threatening":
        return "error";
      case "critical":
        return "error";
      default:
        return "default";
    }
  };

  const renderContent = () => {
    switch (section) {
      case "conditions":
        return (
          <div className="space-y-4">
            <h3 className="text-lg font-semibold text-blue-600 flex items-center">
              <Heart className="w-5 h-5 mr-2" />
              Medical Conditions
            </h3>
            {record.medicalConditions.length > 0 ? (
              <div className="space-y-4">
                {record.medicalConditions.map((condition) => (
                  <div
                    key={condition.id}
                    className="p-4 border border-gray-200 rounded-lg"
                  >
                    <div className="flex items-start justify-between mb-2">
                      <div>
                        <h4 className="font-medium text-gray-900">
                          {condition.name}
                        </h4>
                        <p className="text-sm text-gray-600">
                          Code: {condition.code}
                        </p>
                      </div>
                      <div className="flex space-x-2">
                        <Badge
                          variant={getStatusBadgeVariant(condition.status)}
                        >
                          {condition.status}
                        </Badge>
                        <Badge
                          variant={getSeverityBadgeVariant(condition.severity)}
                        >
                          {condition.severity}
                        </Badge>
                      </div>
                    </div>
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <span className="font-medium text-gray-700">
                          Diagnosed Date:
                        </span>
                        <p className="text-gray-600">
                          {formatDate(condition.diagnosedDate)}
                        </p>
                      </div>
                      <div>
                        <span className="font-medium text-gray-700">
                          Treating Physician:
                        </span>
                        <p className="text-gray-600">
                          {condition.notes || "Not specified"}
                        </p>
                      </div>
                    </div>
                    {condition.notes && (
                      <div className="mt-3">
                        <span className="font-medium text-gray-700">
                          Notes:
                        </span>
                        <p className="text-gray-600 mt-1">{condition.notes}</p>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-gray-500 italic">
                No medical conditions recorded
              </p>
            )}
          </div>
        );

      // Allergies section removed - not supported by database schema

      case "medications":
        return (
          <div className="space-y-6">
            <h3 className="text-lg font-semibold text-blue-600 flex items-center">
              <Pill className="w-5 h-5 mr-2" />
              Medications
            </h3>

            {/* Current Medications */}
            <div>
              <h4 className="font-medium text-gray-900 mb-3">
                Current Medications
              </h4>
              {record.currentMedications.length > 0 ? (
                <div className="space-y-4">
                  {record.currentMedications.map((medication) => (
                    <div
                      key={medication.id}
                      className="p-4 border border-green-200 rounded-lg bg-green-50"
                    >
                      <div className="flex items-start justify-between mb-2">
                        <div>
                          <h5 className="font-medium text-green-900">
                            {medication.name}
                          </h5>
                          <p className="text-sm text-green-700">
                            {medication.dosage} • {medication.frequency}
                          </p>
                        </div>
                        <Badge
                          variant={getStatusBadgeVariant(medication.status)}
                        >
                          {medication.status}
                        </Badge>
                      </div>
                      <div className="grid grid-cols-2 gap-4 text-sm">
                        <div>
                          <span className="font-medium text-green-700">
                            Instructions:
                          </span>
                          <p className="text-green-600">
                            {medication.instructions || "See prescription"}
                          </p>
                        </div>
                        <div>
                          <span className="font-medium text-green-700">
                            Prescribed By:
                          </span>
                          <p className="text-green-600">
                            {medication.prescribedBy}
                          </p>
                        </div>
                        <div>
                          <span className="font-medium text-green-700">
                            Prescribed Date:
                          </span>
                          <p className="text-green-600">
                            {formatDate(medication.prescribedDate)}
                          </p>
                        </div>
                        {medication.duration && (
                          <div>
                            <span className="font-medium text-green-700">
                              Duration:
                            </span>
                            <p className="text-green-600">
                              {medication.duration}
                            </p>
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-gray-500 italic">No current medications</p>
              )}

              {/* Past Medications section removed - only current medications from prescriptions are supported */}
            </div>
          </div>
        );

      // Vitals section removed - vital signs are now embedded in visit documents

      case "visits":
        return (
          <div className="space-y-4">
            <h3 className="text-lg font-semibold text-blue-600 flex items-center">
              <Calendar className="w-5 h-5 mr-2" />
              Visit History
            </h3>
            {record.visits.length > 0 ? (
              <div className="space-y-4">
                {record.visits.map((visit) => (
                  <div
                    key={visit.id}
                    className="p-4 border border-gray-200 rounded-lg"
                  >
                    <div className="flex items-start justify-between mb-3">
                      <div>
                        <h4 className="font-medium text-gray-900">
                          {visit.doctorName}
                        </h4>
                        <p className="text-sm text-gray-600">
                          {visit.specialty}
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="font-medium text-gray-900">
                          {formatDate(visit.date)}
                        </p>
                        {visit.followUpDate && (
                          <p className="text-sm text-blue-600">
                            Follow-up: {formatDate(visit.followUpDate)}
                          </p>
                        )}
                      </div>
                    </div>
                    <div className="space-y-3">
                      <div>
                        <span className="font-medium text-gray-700">
                          Chief Complaint:
                        </span>
                        <p className="text-gray-600 mt-1">
                          {visit.chiefComplaint}
                        </p>
                      </div>
                      <div>
                        <span className="font-medium text-gray-700">
                          Diagnosis:
                        </span>
                        <p className="text-gray-600 mt-1">{visit.diagnosis}</p>
                      </div>
                      <div>
                        <span className="font-medium text-gray-700">
                          Treatment:
                        </span>
                        <p className="text-gray-600 mt-1">{visit.treatment}</p>
                      </div>
                      {visit.notes && (
                        <div>
                          <span className="font-medium text-gray-700">
                            Notes:
                          </span>
                          <p className="text-gray-600 mt-1">{visit.notes}</p>
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-gray-500 italic">No visit history recorded</p>
            )}
          </div>
        );

      case "contacts":
        return (
          <div className="space-y-4">
            <h3 className="text-lg font-semibold text-blue-600 flex items-center">
              <Phone className="w-5 h-5 mr-2" />
              Emergency Contacts
            </h3>
            {record.emergencyContacts.length > 0 ? (
              <div className="space-y-4">
                {record.emergencyContacts.map((contact) => (
                  <div
                    key={contact.id}
                    className="p-4 border border-gray-200 rounded-lg"
                  >
                    <div className="flex items-start justify-between mb-2">
                      <div>
                        <h4 className="font-medium text-gray-900">
                          {contact.name}
                        </h4>
                        <p className="text-sm text-gray-600">
                          {contact.relationship}
                        </p>
                      </div>
                      {contact.isPrimary && (
                        <Badge variant="info">Primary Contact</Badge>
                      )}
                    </div>
                    <div className="space-y-2 text-sm">
                      <div>
                        <span className="font-medium text-gray-700">
                          Phone:
                        </span>
                        <p className="text-gray-600">{contact.phone}</p>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-gray-500 italic">
                No emergency contacts recorded
              </p>
            )}
          </div>
        );

      case "insurance":
        return (
          <div className="space-y-4">
            <h3 className="text-lg font-semibold text-blue-600 flex items-center">
              <Shield className="w-5 h-5 mr-2" />
              Insurance Information
            </h3>
            {record.insurance.length > 0 ? (
              <div className="space-y-4">
                {record.insurance.map((insurance) => (
                  <div
                    key={insurance.id}
                    className="p-4 border border-gray-200 rounded-lg"
                  >
                    <div className="flex items-start justify-between mb-3">
                      <div>
                        <h4 className="font-medium text-gray-900">
                          {insurance.provider}
                        </h4>
                        <p className="text-sm text-gray-600">
                          Policy: {insurance.policyNumber}
                        </p>
                      </div>
                      {insurance.isPrimary && (
                        <Badge variant="info">Primary Insurance</Badge>
                      )}
                    </div>
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <span className="font-medium text-gray-700">
                          Policy Number:
                        </span>
                        <p className="text-gray-600">
                          {insurance.policyNumber}
                        </p>
                      </div>
                      {insurance.groupNumber && (
                        <div>
                          <span className="font-medium text-gray-700">
                            Group Number:
                          </span>
                          <p className="text-gray-600">
                            {insurance.groupNumber}
                          </p>
                        </div>
                      )}
                      <div>
                        <span className="font-medium text-gray-700">
                          Valid From:
                        </span>
                        <p className="text-gray-600">
                          {formatDate(insurance.validFrom)}
                        </p>
                      </div>
                      {insurance.validTo && (
                        <div>
                          <span className="font-medium text-gray-700">
                            Valid To:
                          </span>
                          <p className="text-gray-600">
                            {formatDate(insurance.validTo)}
                          </p>
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-gray-500 italic">
                No insurance information recorded
              </p>
            )}
          </div>
        );

      case "overview":
      default:
        return (
          <div className="space-y-6">
            <h3 className="text-lg font-semibold text-blue-600 flex items-center">
              <User className="w-5 h-5 mr-2" />
              Patient Overview
            </h3>

            {/* Basic Information */}
            <div className="grid grid-cols-2 gap-6">
              <div className="space-y-3">
                <h4 className="font-medium text-gray-900">Basic Information</h4>
                <div className="space-y-2 text-sm">
                  <div>
                    <span className="font-medium text-gray-700">
                      Full Name:
                    </span>
                    <p className="text-gray-600">{record.name}</p>
                  </div>
                  <div>
                    <span className="font-medium text-gray-700">
                      Date of Birth:
                    </span>
                    <p className="text-gray-600">
                      {formatDate(record.dateOfBirth)}
                    </p>
                  </div>
                  <div>
                    <span className="font-medium text-gray-700">Gender:</span>
                    <p className="text-gray-600">{record.gender}</p>
                  </div>
                  <div>
                    <span className="font-medium text-gray-700">
                      Blood Type:
                    </span>
                    <p className="text-gray-600">
                      {record.bloodType || "Not specified"}
                    </p>
                  </div>
                </div>
              </div>

              <div className="space-y-3">
                <h4 className="font-medium text-gray-900">
                  Contact Information
                </h4>
                <div className="space-y-2 text-sm">
                  <div>
                    <span className="font-medium text-gray-700">Phone:</span>
                    <p className="text-gray-600">{record.phone}</p>
                  </div>
                  <div>
                    <span className="font-medium text-gray-700">Email:</span>
                    <p className="text-gray-600">{record.email}</p>
                  </div>
                  <div>
                    <span className="font-medium text-gray-700">Address:</span>
                    <p className="text-gray-600">{record.address}</p>
                  </div>
                </div>
              </div>
            </div>

            {/* Medical Summary */}
            <div className="grid grid-cols-2 gap-6">
              <div className="space-y-3">
                <h4 className="font-medium text-gray-900">Medical Summary</h4>
                <div className="space-y-2 text-sm">
                  <div>
                    <span className="font-medium text-gray-700">
                      Active Conditions:
                    </span>
                    <p className="text-gray-600">
                      {
                        record.medicalConditions.filter(
                          (c) => c.status === "Active"
                        ).length
                      }
                    </p>
                  </div>
                  <div>
                    <span className="font-medium text-gray-700">
                      Current Medications:
                    </span>
                    <p className="text-gray-600">
                      {record.currentMedications.length}
                    </p>
                  </div>
                  <div>
                    <span className="font-medium text-gray-700">
                      Total Visits:
                    </span>
                    <p className="text-gray-600">{record.visits.length}</p>
                  </div>
                </div>
              </div>

              <div className="space-y-3">
                <h4 className="font-medium text-gray-900">
                  Record Information
                </h4>
                <div className="space-y-2 text-sm">
                  <div>
                    <span className="font-medium text-gray-700">
                      Record Created:
                    </span>
                    <p className="text-gray-600">
                      {formatDate(record.createdDate)}
                    </p>
                  </div>
                  <div>
                    <span className="font-medium text-gray-700">
                      Last Updated:
                    </span>
                    <p className="text-gray-600">
                      {formatDateTime(record.lastUpdated)}
                    </p>
                  </div>
                  <div>
                    <span className="font-medium text-gray-700">
                      Medical Record Number:
                    </span>
                    <p className="text-gray-600">
                      {record.medicalRecordNumber}
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        );
    }
  };

  const getSectionTitle = () => {
    switch (section) {
      case "conditions":
        return "Medical Conditions";
      case "medications":
        return "Medications";
      case "visits":
        return "Visit History";
      case "contacts":
        return "Emergency Contacts";
      case "insurance":
        return "Insurance Information";
      case "overview":
      default:
        return "Patient Overview";
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={`${getSectionTitle()} - ${record.name}`}
      size="xl"
    >
      <div className="max-h-96 overflow-y-auto">{renderContent()}</div>
      <div className="flex justify-end mt-6 pt-4 border-t border-gray-200">
        <Button variant="secondary" onClick={onClose}>
          Close
        </Button>
      </div>
    </Modal>
  );
};
