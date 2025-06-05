import { BrowserRouter, Routes, Route } from "react-router-dom";
import "./styles/App.css";
import "./styles/styles.css";
import Login from "./pages/Login";
import MyProfile from "./pages/Profile/MyProfile";
import PatientDashboard from "./pages/Dashboard/PatientDashboard";
import Scheduler from "./pages/Scheduler/Scheduler";
import Documents from "./pages/Documents/Documents";
import WalletView from "./pages/Profile/Wallet/Wallet";
import SubscriptionPage from "./pages/Profile/Wallet/SubscriptionPage";
import AppointmentsPage from "./pages/Appointments/AppointmentsPage";

function App() {
  return (
    <div className="min-h-screen bg-gray-100">
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<PatientDashboard />} />
          <Route path="/scheduler" element={<Scheduler />} />
          <Route path="/login" element={<Login />} />
          <Route path="/documents" element={<Documents />} />
          <Route path="/appointments" element={<AppointmentsPage />} />

          {/* User view */}
          <Route path="/user/wallet" element={<WalletView />} />
          <Route path="/user/myprofile" element={<MyProfile />} />
          <Route
            path="/user/wallet/subscription"
            element={<SubscriptionPage />}
          />
        </Routes>
      </BrowserRouter>
    </div>
  );
}

export default App;
