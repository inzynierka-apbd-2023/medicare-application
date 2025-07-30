import React from "react";

export type Gender = "Male" | "Female" | "Other";

export type SortKey = "name" | "lastVisit" | "visits" | "age";

export interface Patient {
  id: number;
  name: string;
  age: number;
  gender: Gender;
  lastVisit: string;
  visits: number;
  notes: string;
  email?: string;
  phone?: string;
}

export interface PatientListProps {
  patients: Patient[];
  searchTerm: string;
  onSearchChange: (term: string) => void;
  sortKey: SortKey;
  onSortChange: (key: SortKey) => void;
  onPatientAction: (action: PatientAction, patient: Patient) => void;
  isLoading?: boolean;
}

export interface PatientListPageProps {
  doctorId?: string;
}

export interface PatientTableProps {
  patients: Patient[];
  onPatientAction: (action: PatientAction, patient: Patient) => void;
}

export interface PatientFiltersProps {
  searchTerm: string;
  onSearchChange: (term: string) => void;
  sortKey: SortKey;
  onSortChange: (key: SortKey) => void;
}

export interface PatientActionButtonProps {
  action: PatientAction;
  patient: Patient;
  onClick: () => void;
}

export type PatientAction =
  | "appointments"
  | "medical-records"
  | "prescription"
  | "message"
  | "notes";

export interface PatientActionConfig {
  icon: React.ReactNode;
  title: string;
  colorClass: string;
  route: string;
}
