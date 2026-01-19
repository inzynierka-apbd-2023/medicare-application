import { lazy, Suspense } from "react";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";

import { AuthProvider } from "./shared/auth/AuthContext";
import { PlanRestrictedRoute } from "./shared/auth/PlanRestricted";
import { RoleBasedRoute } from "./shared/auth/RoleBasedRoute";
import { Loading } from "./shared/components";
import { ToastProvider } from "./shared/toast";

import "./styles/App.css";
import "./styles/styles.css";
import "./styles/dashboard.css";
import "./styles/auth.css";
import "./styles/components.css";
import "./shared/toast/toast.css";

// Lazy-loaded components
const Login = lazy(() => import("./features/signon/Login"));
const ForgotPassword = lazy(() => import("./features/signon/ForgotPassword"));
const ResetPassword = lazy(() => import("./features/signon/ResetPassword"));
const Register = lazy(() => import("./features/signon/Register"));
const PlanSelection = lazy(() => import("./features/signon/PlanSelection"));
const SubscriptionView = lazy(
  () => import("./features/signon/SubscriptionView")
);
const LoginSuccess = lazy(() => import("./features/signon/LoginSuccess"));
const RegistrationSuccess = lazy(
  () => import("./features/signon/RegistrationSuccess")
);
const PasswordResetSuccess = lazy(
  () => import("./features/signon/PasswordResetSuccess")
);
const CompleteProfile = lazy(() => import("./features/signon/CompleteProfile"));

const PatientDashboard = lazy(
  () => import("./features/dashboard/patient/PatientDashboard")
);
const DoctorDashboard = lazy(
  () => import("./features/dashboard/doctor/DoctorDashboard")
);
const OwnerDashboard = lazy(() =>
  import("./features/dashboard/owner").then((module) => ({
    default: module.OwnerDashboard,
  }))
);
const ReceptionistDashboard = lazy(() =>
  import("./features/dashboard/receptionist").then((module) => ({
    default: module.ReceptionistDashboard,
  }))
);

const ProfilePage = lazy(() =>
  import("./features/profile").then((module) => ({
    default: module.ProfilePage,
  }))
);
const SchedulerPage = lazy(() =>
  import("./features/scheduler").then((module) => ({
    default: module.SchedulerPage,
  }))
);
const DoctorSchedulerPage = lazy(() =>
  import("./features/scheduler").then((module) => ({
    default: module.DoctorSchedulerPage,
  }))
);
const DocumentsPage = lazy(() =>
  import("./features/documents").then((module) => ({
    default: module.DocumentsPage,
  }))
);
const WalletPage = lazy(() =>
  import("./features/wallet").then((module) => ({ default: module.WalletPage }))
);
const SubscriptionPage = lazy(() =>
  import("./features/wallet").then((module) => ({
    default: module.SubscriptionPage,
  }))
);
const AppointmentsPage = lazy(() =>
  import("./features/appointments").then((module) => ({
    default: module.AppointmentsPage,
  }))
);
const TodaysAppointmentsPage = lazy(() =>
  import("./features/appointments").then((module) => ({
    default: module.TodaysAppointmentsPage,
  }))
);
const PatientListPage = lazy(() =>
  import("./features/userTypes").then((module) => ({
    default: module.PatientListPage,
  }))
);
const MedicalRecordsPage = lazy(() =>
  import("./features/medicalRecords").then((module) => ({
    default: module.MedicalRecordsPage,
  }))
);
const PrescriptionsPage = lazy(() =>
  import("./features/prescriptions").then((module) => ({
    default: module.PrescriptionsPage,
  }))
);
const SimpleMessagesPage = lazy(
  () => import("./features/messages/SimpleMessagesPage")
);
const LabResultsPage = lazy(() =>
  import("./features/labResults").then((module) => ({
    default: module.LabResultsPage,
  }))
);
const LabResultDetailPage = lazy(() =>
  import("./features/labResults").then((module) => ({
    default: module.LabResultDetailPage,
  }))
);
const LabResultsReviewPage = lazy(() =>
  import("./features/labResultsReview").then((module) => ({
    default: module.LabResultsReviewPage,
  }))
);
const AppointmentAnalyticsPage = lazy(() =>
  import("./features/appointmentAnalytics").then((module) => ({
    default: module.AppointmentAnalyticsPage,
  }))
);
const StaffManagementPage = lazy(() =>
  import("./features/staffManagement").then((module) => ({
    default: module.StaffManagementPage,
  }))
);
const ReceptionistSchedulerPage = lazy(() =>
  import("./features/receptionistScheduler").then((module) => ({
    default: module.ReceptionistSchedulerPage,
  }))
);
const PatientRegistryPage = lazy(() =>
  import("./features/patientRegistry").then((module) => ({
    default: module.PatientRegistryPage,
  }))
);
const TermsPage = lazy(() =>
  import("./features/public").then((module) => ({ default: module.TermsPage }))
);
const PrivacyPage = lazy(() =>
  import("./features/public").then((module) => ({
    default: module.PrivacyPage,
  }))
);

function App() {
  return (
    <div className="min-h-screen bg-gray-100">
      <BrowserRouter>
        <ToastProvider>
          <AuthProvider>
            <Suspense fallback={<Loading text="Loading application..." />}>
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
                <Route
                  path="/subscription-view"
                  element={<SubscriptionView />}
                />
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
                        <Navigate
                          to="/my-documents?filter=prescriptions"
                          replace
                        />
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
                      allowedRoles={[
                        "Patient",
                        "Doctor",
                        "Owner",
                        "Receptionist",
                      ]}
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
            </Suspense>
          </AuthProvider>
        </ToastProvider>
      </BrowserRouter>
    </div>
  );
}

export default App;
