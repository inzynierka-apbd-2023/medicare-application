import React, { useMemo } from "react";
import type { PatientListProps } from "@features/userTypes/types";

import { PatientFilters, PatientTable } from "./index";

export const PatientList: React.FC<PatientListProps> = ({
  patients,
  searchTerm,
  onSearchChange,
  sortKey,
  onSortChange,
  onPatientAction,
}) => {
  const filteredAndSortedPatients = useMemo(() => {
    let filtered = patients.filter(
      (patient) =>
        patient.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        patient.notes.toLowerCase().includes(searchTerm.toLowerCase())
    );

    filtered.sort((a, b) => {
      switch (sortKey) {
        case "name":
          return a.name.localeCompare(b.name);
        case "lastVisit":
          return (
            new Date(b.lastVisit).getTime() - new Date(a.lastVisit).getTime()
          );
        case "visits":
          return b.visits - a.visits;
        case "age":
          return b.age - a.age;
        default:
          return 0;
      }
    });

    return filtered;
  }, [patients, searchTerm, sortKey]);

  return (
    <>
      <PatientFilters
        searchTerm={searchTerm}
        onSearchChange={onSearchChange}
        sortKey={sortKey}
        onSortChange={onSortChange}
      />

      <PatientTable
        patients={filteredAndSortedPatients}
        onPatientAction={onPatientAction}
      />
    </>
  );
};
