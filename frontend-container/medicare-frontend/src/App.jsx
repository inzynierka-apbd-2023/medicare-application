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
import LoginSuccess from "./features/signon/LoginSuccess";
import RegistrationSuccess from "./features/signon/RegistrationSuccess";
import PasswordResetSuccess from "./features/signon/PasswordResetSuccess";
import { ProfilePage } from "./features/profile";
import PatientDashboard from "./features/dashboard/patient/PatientDashboard";
import { SchedulerPage } from "./features/scheduler";
import { DocumentsPage } from "./features/documents";
import { WalletPage, SubscriptionPage } from "./features/wallet";
import { AppointmentsPage } from "./features/appointments";
import DoctorDashboard from "./features/dashboard/doctor/DoctorDashboard";
import { PatientListPage } from "./features/userTypes";

function App() {
  return (
    <div className="min-h-screen bg-gray-100">
      <BrowserRouter>
        <Routes>
          {/* Patient view */}
          <Route path="/" element={<PatientDashboard />} />
          <Route path="/scheduler" element={<SchedulerPage />} />
          <Route path="/documents" element={<DocumentsPage />} />
          <Route path="/appointments" element={<AppointmentsPage />} />

          {/* User view */}
          <Route path="/user/wallet" element={<WalletPage />} />
          <Route path="/user/myprofile" element={<ProfilePage />} />
          <Route
            path="/user/wallet/subscription"
            element={<SubscriptionPage />}
          />

          {/* Doctor view */}
          <Route path="/dctdash" element={<DoctorDashboard />} />
          <Route path="/patientlist" element={<PatientListPage />} />
          <Route path="/" element={<Login />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/forgot-password" element={<ForgotPassword />} />
          <Route path="/forgot-card" element={<ForgotCardNumber />} />
          <Route path="/choose-plan" element={<PlanSelection />} />
          <Route path="/login-success" element={<LoginSuccess />} />
          <Route
            path="/registration-success"
            element={<RegistrationSuccess />}
          />
          <Route
            path="/password-reset-success"
            element={<PasswordResetSuccess />}
          />
        </Routes>
      </BrowserRouter>
    </div>
  );
}

export default App;
