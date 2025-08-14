import { BrowserRouter, Routes, Route } from "react-router-dom";
import "./styles/App.css";
import "./styles/styles.css";
import "./styles/dashboard.css";
import "./styles/auth.css";
import "./styles/components.css";
import Login from "./features/signon/Login";
import ForgotPassword from "./features/signon/ForgotPassword";
import ForgotCardNumber from "./features/signon/ForgotCardNumber";
import Register from "./features/signon/Register";
import PlanSelection from "./features/signon/PlanSelection";
import SubscriptionView from "./features/signon/SubscriptionView";
import LoginSuccess from "./features/signon/LoginSuccess";
import RegistrationSuccess from "./features/signon/RegistrationSuccess";
import PasswordResetSuccess from "./features/signon/PasswordResetSuccess";
import { ProfilePage } from "./features/profile";
import PatientDashboard from "./features/dashboard/patient/PatientDashboard";
import DoctorDashboard from "./features/dashboard/doctor/DoctorDashboard";
import { OwnerDashboard } from "./features/dashboard/owner";
import { SchedulerPage } from "./features/scheduler";
import { DocumentsPage } from "./features/documents";
import { WalletPage, SubscriptionPage } from "./features/wallet";
import { AppointmentsPage } from "./features/appointments";
import { PatientListPage } from "./features/userTypes";
import { TodaysAppointmentsPage } from "./features/appointments";
import { MedicalRecordsPage } from "./features/medicalRecords";
import { PrescriptionsPage } from "./features/prescriptions";
import TestMessagesPage from "./TestMessagesPage";
import { AuthProvider } from "./shared/auth/AuthContext";
import { ProtectedRoute } from "./shared/auth/ProtectedRoute";
import { LabResultsPage, LabResultDetailPage } from "./features/labResults";
import { LabResultsReviewPage } from "./features/labResultsReview";
import { AppointmentAnalyticsPage } from "./features/appointmentAnalytics";
import { StaffManagementPage } from "./features/staffManagement";

function App() {
  return (
    <div className="min-h-screen bg-gray-100">
      <BrowserRouter>
        <AuthProvider>
          <Routes>
            {/* Authentication routes */}
            <Route path="/" element={<PatientDashboard />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/forgot-password" element={<ForgotPassword />} />
            <Route path="/forgot-card" element={<ForgotCardNumber />} />
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

            {/* Patient view */}
            {/* <Route path="/dashboard" element={<ProtectedRoute><PatientDashboard /></ProtectedRoute>} /> */}
            <Route path="/dashboard" element={<PatientDashboard />} />
            {/* <Route path="/messages" element={<ProtectedRoute><TestMessagesPage /></ProtectedRoute>} /> */}
            <Route path="/messages" element={<TestMessagesPage />} />
            {/* <Route path="/scheduler" element={<ProtectedRoute><SchedulerPage /></ProtectedRoute>} /> */}
            <Route path="/scheduler" element={<SchedulerPage />} />
            {/* <Route path="/documents" element={<ProtectedRoute><DocumentsPage /></ProtectedRoute>} /> */}
            <Route path="/documents" element={<DocumentsPage />} />
            {/* <Route path="/lab-results" element={<ProtectedRoute><LabResultsPage /></ProtectedRoute>} /> */}
            <Route path="/lab-results" element={<LabResultsPage />} />
            {/* <Route path="/lab-results/:documentId" element={<ProtectedRoute><LabResultDetailPage /></ProtectedRoute>} /> */}
            <Route
              path="/lab-results/:documentId"
              element={<LabResultDetailPage />}
            />
            {/* <Route path="/appointments" element={<ProtectedRoute><AppointmentsPage /></ProtectedRoute>} /> */}
            <Route path="/appointments" element={<AppointmentsPage />} />
            {/* <Route path="/prescriptions" element={<ProtectedRoute><PrescriptionsPage /></ProtectedRoute>} /> */}
            <Route path="/prescriptions" element={<PrescriptionsPage />} />

            {/* User view */}
            {/* <Route path="/user/wallet" element={<ProtectedRoute><WalletPage /></ProtectedRoute>} /> */}
            <Route path="/user/wallet" element={<WalletPage />} />
            {/* <Route path="/user/myprofile" element={<ProtectedRoute><ProfilePage /></ProtectedRoute>} /> */}
            <Route path="/user/myprofile" element={<ProfilePage />} />
            {/* <Route path="/user/wallet/subscription" element={<ProtectedRoute><SubscriptionPage /></ProtectedRoute>} /> */}
            <Route
              path="/user/wallet/subscription"
              element={<SubscriptionPage />}
            />

            {/* Doctor view */}
            {/* <Route path="/dctdash" element={<ProtectedRoute><DoctorDashboard /></ProtectedRoute>} /> */}
            <Route path="/dctdash" element={<DoctorDashboard />} />
            {/* <Route path="/patientlist" element={<ProtectedRoute><PatientListPage /></ProtectedRoute>} /> */}
            <Route path="/patientlist" element={<PatientListPage />} />
            {/* <Route path="/todays-appointments" element={<ProtectedRoute><TodaysAppointmentsPage /></ProtectedRoute>} /> */}
            <Route
              path="/todays-appointments"
              element={<TodaysAppointmentsPage />}
            />
            {/* <Route path="/medical-records" element={<ProtectedRoute><MedicalRecordsPage /></ProtectedRoute>} /> */}
            <Route path="/medical-records" element={<MedicalRecordsPage />} />
            {/* <Route path="/medical-records/:patientId" element={<ProtectedRoute><MedicalRecordsPage /></ProtectedRoute>} /> */}
            <Route
              path="/medical-records/:patientId"
              element={<MedicalRecordsPage />}
            />
            {/* <Route path="/prescriptions" element={<ProtectedRoute><PrescriptionsPage /></ProtectedRoute>} /> */}
            <Route path="/prescriptions" element={<PrescriptionsPage />} />
            {/* <Route path="/lab-results-review" element={<ProtectedRoute><LabResultsReviewPage /></ProtectedRoute>} /> */}
            <Route
              path="/lab-results-review"
              element={<LabResultsReviewPage />}
            />

            {/* Owner view */}
            {/* <Route path="/ownerdash" element={<ProtectedRoute><OwnerDashboard /></ProtectedRoute>} /> */}
            <Route path="/ownerdash" element={<OwnerDashboard />} />
            {/* <Route path="/analytics" element={<ProtectedRoute><AppointmentAnalyticsPage /></ProtectedRoute>} /> */}
            <Route path="/analytics" element={<AppointmentAnalyticsPage />} />
            {/* <Route path="/staff-management" element={<ProtectedRoute><StaffManagementPage /></ProtectedRoute>} /> */}
            <Route path="/staff-management" element={<StaffManagementPage />} />
          </Routes>
        </AuthProvider>
      </BrowserRouter>
    </div>
  );
}

export default App;
