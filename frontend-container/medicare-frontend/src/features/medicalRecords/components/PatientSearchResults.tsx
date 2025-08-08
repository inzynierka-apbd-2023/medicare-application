import React from "react";
import { Calendar, Hash, User } from "lucide-react";

import { Button, Card, EmptyState } from "../../../shared/components";
import type { PatientMedicalRecord } from "../types";

interface PatientSearchResultsProps {
  records: PatientMedicalRecord[];
  searchTerm: string;
  onSelectPatient: (patientId: string) => void;
}

export const PatientSearchResults: React.FC<PatientSearchResultsProps> = ({
  records,
  searchTerm,
  onSelectPatient,
}) => {
  const filteredRecords = records.filter(
    (record) =>
      record.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      record.medicalRecordNumber
        .toLowerCase()
        .includes(searchTerm.toLowerCase()) ||
      record.patientId.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  if (filteredRecords.length === 0) {
    return (
      <EmptyState
        icon={<User className="w-8 h-8 text-gray-400" />}
        title="No Patients Found"
        description={`No patients found matching "${searchTerm}"`}
        className="py-8"
      />
    );
  }

  return (
    <div className="space-y-3 max-h-96 overflow-y-auto">
      <p className="text-sm text-gray-600 font-medium">
        Found {filteredRecords.length} patient
        {filteredRecords.length !== 1 ? "s" : ""}
      </p>
      {filteredRecords.map((record) => (
        <Card key={record.id} variant="default" padding="sm">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-3">
              <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
                <User className="w-5 h-5 text-blue-600" />
              </div>
              <div>
                <h3 className="font-medium text-gray-900">{record.name}</h3>
                <div className="flex items-center space-x-4 text-sm text-gray-600">
                  <span className="flex items-center">
                    <Hash className="w-3 h-3 mr-1" />
                    {record.medicalRecordNumber}
                  </span>
                  <span className="flex items-center">
                    <Calendar className="w-3 h-3 mr-1" />
                    DOB: {formatDate(record.dateOfBirth)}
                  </span>
                  <span>{record.gender}</span>
                </div>
              </div>
            </div>
            <Button
              variant="primary"
              size="sm"
              onClick={() => onSelectPatient(record.patientId)}
            >
              Select
            </Button>
          </div>
        </Card>
      ))}
    </div>
  );
};
