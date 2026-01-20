import { Link } from "react-router-dom";
import { useAuth } from "@shared/auth/AuthContext";
import { CheckCircle } from "lucide-react";

export default function RegistrationSuccess() {
  const { user } = useAuth();

  const greeting = (() => {
    if (!user) return "Registration Successful!";
    if (user.role === "Doctor") return `Welcome, Dr. ${user.lastName || ""}`;
    if (user.role === "Patient" || user.role === "Receptionist")
      return `Welcome, ${user.firstName || ""}`;
    return "Registration Successful!";
  })();

  return (
    <div className="success-container">
      <div className="success-card">
        <div className="success-icon">
          <CheckCircle className="success-icon-green" />
        </div>

        <h1 className="success-title">{greeting}</h1>

        <p className="success-text">
          Your account has been created successfully.
        </p>

        <div className="success-info">
          <h3 className="success-info-title">What's Next?</h3>
          <ul className="success-info-list">
            <li>You can now sign in with the credentials you just created</li>
            <li>Book your first appointment from the dashboard</li>
            <li>Complete your profile to get personalized care</li>
          </ul>
        </div>

        <div className="success-actions">
          <Link to="/login" className="btn-primary text-center">
            Sign In Now
          </Link>
        </div>

        <p className="success-help">
          Need help? Contact our support team at{" "}
          <a href="tel:+48111111111" className="text-link">
            +48 111 111 111
          </a>
        </p>
      </div>
    </div>
  );
}
