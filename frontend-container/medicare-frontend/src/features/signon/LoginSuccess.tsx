import { useCallback, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { CheckCircle } from "lucide-react";

import { useAuth } from "../../shared/auth/AuthContext";
import { getDefaultDashboard } from "../../shared/constants/routes";

export default function LoginSuccess() {
  const navigate = useNavigate();
  const { user } = useAuth();

  const redirectToDashboard = useCallback(() => {
    const dashboardRoute = user?.role
      ? getDefaultDashboard(user.role)
      : "/patient-dashboard";
    navigate(dashboardRoute);
  }, [navigate, user?.role]);

  useEffect(() => {
    const timer = setTimeout(redirectToDashboard, 3000);
    return () => clearTimeout(timer);
  }, [redirectToDashboard]);

  const getDashboardRoute = () => {
    return user?.role ? getDefaultDashboard(user.role) : "/patient-dashboard";
  };

  return (
    <div className="success-container">
      <div className="success-card">
        <div className="success-icon">
          <CheckCircle className="success-icon-green" />
        </div>

        <h1 className="success-title">
          {user?.role === "Doctor"
            ? `Welcome, Dr. ${user?.lastName || ""}`
            : user?.role === "Patient" || user?.role === "Receptionist"
              ? `Welcome, ${user?.firstName || ""}`
              : "Welcome Back!"}
        </h1>

        <p className="success-text">
          You have successfully signed in to your Medicare account.
        </p>

        <div className="success-actions">
          <Link to={getDashboardRoute()} className="btn-primary text-center">
            Go to Dashboard
          </Link>

          <Link to="/choose-plan" className="btn-secondary text-center">
            Browse Plans
          </Link>
        </div>

        <p className="success-help">Redirecting to dashboard in 3 seconds...</p>
      </div>
    </div>
  );
}
