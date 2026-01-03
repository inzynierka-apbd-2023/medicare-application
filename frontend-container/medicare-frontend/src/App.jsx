import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import "./styles/App.css";
import "./styles/styles.css";
import "./styles/dashboard.css";
import "./styles/auth.css";
import "./styles/components.css";

// Authentication Components
import Login from "./features/signon/Login";
import ForgotPassword from "./features/signon/ForgotPassword";
import ResetPassword from "./features/signon/ResetPassword";
import Register from "./features/signon/Register";
import PlanSelection from "./features/signon/PlanSelection";
import SubscriptionView from "./features/signon/SubscriptionView";
import LoginSuccess from "./features/signon/LoginSuccess";
import RegistrationSuccess from "./features/signon/RegistrationSuccess";
import PasswordResetSuccess from "./features/signon/PasswordResetSuccess";
import CompleteProfile from "./features/signon/CompleteProfile";

// Dashboard Components
import PatientDashboard from "./features/dashboard/patient/PatientDashboard";
import DoctorDashboard from "./features/dashboard/doctor/DoctorDashboard";
import { OwnerDashboard } from "./features/dashboard/owner";
import { ReceptionistDashboard } from "./features/dashboard/receptionist";

// Feature Components
import { ProfilePage } from "./features/profile";
import { SchedulerPage, DoctorSchedulerPage } from "./features/scheduler";
import { DocumentsPage } from "./features/documents";
import { WalletPage, SubscriptionPage } from "./features/wallet";
import {
  AppointmentsPage,
  TodaysAppointmentsPage,
} from "./features/appointments";
import { PatientListPage } from "./features/userTypes";
import { MedicalRecordsPage } from "./features/medicalRecords";
import { PrescriptionsPage } from "./features/prescriptions";
import SimpleMessagesPage from "./features/messages/SimpleMessagesPage";
import { LabResultsPage, LabResultDetailPage } from "./features/labResults";
import { LabResultsReviewPage } from "./features/labResultsReview";
import { AppointmentAnalyticsPage } from "./features/appointmentAnalytics";
import { StaffManagementPage } from "./features/staffManagement";
import { ReceptionistSchedulerPage } from "./features/receptionistScheduler";
import { PatientRegistryPage } from "./features/patientRegistry";
import { TermsPage, PrivacyPage } from "./features/public";

// Auth Components
import { AuthProvider } from "./shared/auth/AuthContext";
import { RoleBasedRoute } from "./shared/auth/RoleBasedRoute";
import { PlanRestrictedRoute } from "./shared/auth/PlanRestricted";

function App() {
  return (
    <div className="min-h-screen bg-gray-100">
      <BrowserRouter>
        <AuthProvider>
          {/* Mock banner removed */}
          <Routes>
            {/* ===== PUBLIC ROUTES ===== */}
            <Route path="/" element={<Login />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/complete-profile" element={<CompleteProfile />} />
            <Route path="/forgot-password" element={<ForgotPassword />} />
            <Route path="/reset-password" element={<ResetPassword />} />
            <Route path="/terms" element={<TermsPage />} />
            <Route path="/privacy" element={<PrivacyPage />} />

            <Route path="/choose-plan" element={<PlanSelection />} />
            <Route path="/subscription-view" element={<SubscriptionView />} />
            <Route path="/login-success" element={<LoginSuccess />} />
            <Route
              path="/registration-success"
              element={<RegistrationSuccess />}
            />
            <Route
              path="/password-reset-success"
              element={<PasswordResetSuccess />}
            />

            {/* ===== DASHBOARD ROUTES ===== */}
            <Route
              path="/patient-dashboard"
              element={
                <RoleBasedRoute allowedRoles={["Patient"]}>
                  <PatientDashboard />
                </RoleBasedRoute>
              }
            />
            <Route
              path="/doctor-dashboard"
              element={
                <RoleBasedRoute allowedRoles={["Doctor"]}>
                  <DoctorDashboard />
                </RoleBasedRoute>
              }
            />
            <Route
              path="/owner-dashboard"
              element={
                <RoleBasedRoute allowedRoles={["Owner", "Admin"]}>
                  <OwnerDashboard />
                </RoleBasedRoute>
              }
            />
            <Route
              path="/receptionist-dashboard"
              element={
                <RoleBasedRoute allowedRoles={["Receptionist"]}>
                  <ReceptionistDashboard />
                </RoleBasedRoute>
              }
            />

            {/* ===== PATIENT ROUTES ===== */}
            <Route
              path="/my-appointments"
              element={
                <RoleBasedRoute allowedRoles={["Patient"]}>
                  <AppointmentsPage />
                </RoleBasedRoute>
              }
            />
            <Route
              path="/appointment-scheduler"
              element={
                <RoleBasedRoute allowedRoles={["Patient"]}>
                  <SchedulerPage />
                </RoleBasedRoute>
              }
            />
            <Route
              path="/my-documents"
              element={
                <RoleBasedRoute allowedRoles={["Patient"]}>
                  <PlanRestrictedRoute feature="hasDocuments">
                    <DocumentsPage />
                  </PlanRestrictedRoute>
                </RoleBasedRoute>
              }
            />
            <Route
              path="/lab-results"
              element={
                <RoleBasedRoute allowedRoles={["Patient"]}>
                  {/* Lab results usually considered part of documents/records */}
                  <PlanRestrictedRoute feature="hasDocuments">
                    <LabResultsPage />
                  </PlanRestrictedRoute>
                </RoleBasedRoute>
              }
            />
            <Route
              path="/lab-results/:documentId"
              element={
                <RoleBasedRoute allowedRoles={["Patient"]}>
                  <PlanRestrictedRoute feature="hasDocuments">
                    <LabResultDetailPage />
                  </PlanRestrictedRoute>
                </RoleBasedRoute>
              }
            />
            <Route
              path="/my-prescriptions"
              element={
                <RoleBasedRoute allowedRoles={["Patient"]}>
                  <PlanRestrictedRoute feature="hasPrescriptions">
                    <Navigate to="/my-documents?filter=prescriptions" replace />
                  </PlanRestrictedRoute>
                </RoleBasedRoute>
              }
            />

            {/* ===== DOCTOR ROUTES ===== */}
            <Route
              path="/patient-list"
              element={
                <RoleBasedRoute allowedRoles={["Doctor"]}>
                  <PatientListPage />
                </RoleBasedRoute>
              }
            />
            {/* ... other doctor routes ... */}
            <Route
              path="/todays-appointments"
              element={
                <RoleBasedRoute allowedRoles={["Doctor"]}>
                  <TodaysAppointmentsPage />
                </RoleBasedRoute>
              }
            />
            <Route
              path="/doctor-scheduler"
              element={
                <RoleBasedRoute allowedRoles={["Doctor"]}>
                  <DoctorSchedulerPage />
                </RoleBasedRoute>
              }
            />
            <Route
              path="/medical-records"
              element={
                <RoleBasedRoute allowedRoles={["Doctor"]}>
                  <MedicalRecordsPage />
                </RoleBasedRoute>
              }
            />
            <Route
              path="/medical-records/:patientId"
              element={
                <RoleBasedRoute allowedRoles={["Doctor"]}>
                  <MedicalRecordsPage />
                </RoleBasedRoute>
              }
            />
            <Route
              path="/prescriptions-management"
              element={
                <RoleBasedRoute allowedRoles={["Doctor"]}>
                  <PrescriptionsPage />
                </RoleBasedRoute>
              }
            />
            <Route
              path="/lab-results-review"
              element={
                <RoleBasedRoute allowedRoles={["Doctor"]}>
                  <LabResultsReviewPage />
                </RoleBasedRoute>
              }
            />

            {/* ===== OWNER ROUTES ===== */}
            <Route
              path="/appointment-analytics"
              element={
                <RoleBasedRoute allowedRoles={["Owner"]}>
                  <AppointmentAnalyticsPage />
                </RoleBasedRoute>
              }
            />
            <Route
              path="/staff-management"
              element={
                <RoleBasedRoute allowedRoles={["Owner"]}>
                  <StaffManagementPage />
                </RoleBasedRoute>
              }
            />

            {/* ===== RECEPTIONIST ROUTES ===== */}
            <Route
              path="/receptionist-scheduler"
              element={
                <RoleBasedRoute allowedRoles={["Receptionist"]}>
                  <ReceptionistSchedulerPage />
                </RoleBasedRoute>
              }
            />
            <Route
              path="/patient-registry"
              element={
                <RoleBasedRoute allowedRoles={["Receptionist"]}>
                  <PatientRegistryPage />
                </RoleBasedRoute>
              }
            />

            {/* ===== SHARED ROUTES (Multiple Roles) ===== */}
            <Route
              path="/messages"
              element={
                <RoleBasedRoute
                  allowedRoles={["Patient", "Doctor", "Receptionist"]}
                >
                  <PlanRestrictedRoute feature="hasMessaging">
                    <SimpleMessagesPage />
                  </PlanRestrictedRoute>
                </RoleBasedRoute>
              }
            />

            {/* ===== USER ROUTES (Profile & Account Management) ===== */}
            <Route
              path="/user/myprofile"
              element={
                <RoleBasedRoute
                  allowedRoles={["Patient", "Doctor", "Owner", "Receptionist"]}
                >
                  <ProfilePage />
                </RoleBasedRoute>
              }
            />
            <Route
              path="/user/wallet"
              element={
                <RoleBasedRoute allowedRoles={["Patient"]}>
                  <WalletPage />
                </RoleBasedRoute>
              }
            />
            <Route
              path="/user/wallet/subscription"
              element={
                <RoleBasedRoute allowedRoles={["Patient"]}>
                  <SubscriptionPage />
                </RoleBasedRoute>
              }
            />

            {/* ===== LEGACY REDIRECTS (Optional - for backward compatibility) ===== */}
            <Route path="/dashboard" element={<PatientDashboard />} />
            <Route path="/dctdash" element={<DoctorDashboard />} />
            <Route path="/ownerdash" element={<OwnerDashboard />} />
          </Routes>
        </AuthProvider>
      </BrowserRouter>
    </div>
  );
}

export default App;
