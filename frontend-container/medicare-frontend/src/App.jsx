import { BrowserRouter, Routes, Route } from "react-router-dom";
import "./styles/App.css";
import "./styles/styles.css";
import "./styles/dashboard.css";
import Login from "./features/auth/Login";
import MyProfile from "./pages/Profile/MyProfile";
import PatientDashboard from "./features/dashboard/patient/PatientDashboard";
import Scheduler from "./pages/Scheduler/Scheduler";
import { DocumentsPage } from "./features/documents";
import { WalletPage, SubscriptionPage } from "./features/wallet";
import { AppointmentsPage } from "./features/appointments";
import DoctorDashboard from "./features/dashboard/doctor/DoctorDashboard";
import PatientList from "./pages/UserTypes/PatientListView";

function App() {
  return (
    <div className="min-h-screen bg-gray-100">
      <BrowserRouter>
        <Routes>
          {/* Patient view */}
          <Route path="/" element={<PatientDashboard />} />
          <Route path="/scheduler" element={<Scheduler />} />
          <Route path="/login" element={<Login />} />
          <Route path="/documents" element={<DocumentsPage />} />
          <Route path="/appointments" element={<AppointmentsPage />} />

          {/* User view */}
          <Route path="/user/wallet" element={<WalletPage />} />
          <Route path="/user/myprofile" element={<MyProfile />} />
          <Route
            path="/user/wallet/subscription"
            element={<SubscriptionPage />}
          />

          {/* Doctor view */}
          <Route path="/dctdash" element={<DoctorDashboard />} />
          <Route path="/patientlist" element={<PatientList />} />
        </Routes>
      </BrowserRouter>
    </div>
  );
}

export default App;
