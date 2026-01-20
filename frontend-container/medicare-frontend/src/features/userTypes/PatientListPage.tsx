import React, { useState } from "react";
import Header from "@layout/Header";
import { useAuth } from "@shared/auth/AuthContext";
import { ErrorDisplay, LoadingOverlay } from "@shared/components";
import { usePatients } from "@shared/hooks/usePatients";

import { PatientList } from "./components";
import type { PatientListPageProps, SortKey } from "./types";

export const PatientListPage: React.FC<PatientListPageProps> = ({
  doctorId: propDoctorId,
}) => {
  const { user } = useAuth();
  const doctorId =
    propDoctorId || (user?.role === "Doctor" ? user.id : undefined);

  const { patients, isLoading, error, refetch } = usePatients(doctorId);

  const [searchTerm, setSearchTerm] = useState("");
  const [sortKey, setSortKey] = useState<SortKey>("name");

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <LoadingOverlay isLoading={true} message="Loading your patients...">
          <div className="min-h-screen" />
        </LoadingOverlay>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100 pt-24 px-8 pb-10">
        <Header />
        <div className="max-w-5xl mx-auto">
          <h1 className="text-3xl font-bold text-blue-700 mb-8">
            Your Patients
          </h1>
          <ErrorDisplay message={error} onRetry={refetch} />
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 pt-24 px-8 pb-10">
      <Header />
      <div className="max-w-5xl mx-auto">
        <h1 className="text-3xl font-bold text-blue-700 mb-8">Your Patients</h1>

        <PatientList
          patients={patients}
          searchTerm={searchTerm}
          onSearchChange={setSearchTerm}
          sortKey={sortKey}
          onSortChange={setSortKey}
          isLoading={isLoading}
        />
      </div>
    </div>
  );
};
