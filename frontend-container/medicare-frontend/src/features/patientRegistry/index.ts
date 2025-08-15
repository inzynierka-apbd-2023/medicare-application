// Export the main page component
export { PatientRegistryPage } from "./PatientRegistryPage";
export { default as PatientRegistryPageDefault } from "./PatientRegistryPage";

// Export components for reuse
export { PatientDetailsModal } from "./components/PatientDetailsModal";
export { PatientRegistryView } from "./components/PatientRegistryView";
export { SimplePatientRegistrationModal } from "./components/SimplePatientRegistrationModal";

// Export hooks
export { usePatientRegistry } from "./hooks/usePatientRegistry";

// Export services
export { PatientRegistryApiService } from "./services/patientRegistryApi";

// Export types
export type {
  ApiResponse,
  CreatePatientRequest,
  Doctor,
  PatientRegistrationFormData,
  PatientRegistryData,
  PatientRegistryFilters,
  PatientRegistryInfo,
} from "./types";
