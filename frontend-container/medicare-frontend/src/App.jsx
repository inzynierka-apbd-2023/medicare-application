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

function App() {
  return (
    <div className="min-h-screen bg-gray-100">
      <BrowserRouter>
        <AuthProvider>
        <Routes>
          {/* Authentication routes */}
          <Route path="/" element={<Login />} />
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
          <Route path="/dashboard" element={<ProtectedRoute><PatientDashboard /></ProtectedRoute>} />
          <Route path="/messages" element={<ProtectedRoute><TestMessagesPage /></ProtectedRoute>} />
          <Route path="/scheduler" element={<ProtectedRoute><SchedulerPage /></ProtectedRoute>} />
          <Route path="/documents" element={<ProtectedRoute><DocumentsPage /></ProtectedRoute>} />
          <Route path="/lab-results" element={<ProtectedRoute><LabResultsPage /></ProtectedRoute>} />
          <Route path="/lab-results/:documentId" element={<ProtectedRoute><LabResultDetailPage /></ProtectedRoute>} />
          <Route path="/appointments" element={<ProtectedRoute><AppointmentsPage /></ProtectedRoute>} />
          <Route path="/prescriptions" element={<ProtectedRoute><PrescriptionsPage /></ProtectedRoute>} />

          {/* User view */}
          <Route path="/user/wallet" element={<ProtectedRoute><WalletPage /></ProtectedRoute>} />
          <Route path="/user/myprofile" element={<ProtectedRoute><ProfilePage /></ProtectedRoute>} />
          <Route path="/user/wallet/subscription" element={<ProtectedRoute><SubscriptionPage /></ProtectedRoute>} />

          {/* Doctor view */}
          <Route path="/dctdash" element={<ProtectedRoute><DoctorDashboard /></ProtectedRoute>} />
          <Route path="/patientlist" element={<ProtectedRoute><PatientListPage /></ProtectedRoute>} />
          <Route path="/todays-appointments" element={<ProtectedRoute><TodaysAppointmentsPage /></ProtectedRoute>} />
          <Route path="/medical-records" element={<ProtectedRoute><MedicalRecordsPage /></ProtectedRoute>} />
          <Route path="/medical-records/:patientId" element={<ProtectedRoute><MedicalRecordsPage /></ProtectedRoute>} />
          <Route path="/prescriptions" element={<ProtectedRoute><PrescriptionsPage /></ProtectedRoute>} />
          <Route path="/lab-results-review" element={<ProtectedRoute><LabResultsReviewPage /></ProtectedRoute>} />

          {/* Owner view */}
          <Route path="/ownerdash" element={<ProtectedRoute><OwnerDashboard /></ProtectedRoute>} />
          <Route path="/analytics" element={<ProtectedRoute><AppointmentAnalyticsPage /></ProtectedRoute>} />
        </Routes>
        </AuthProvider>
      </BrowserRouter>
    </div>
  );
}

export default App;
