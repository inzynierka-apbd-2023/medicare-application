import { useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { CheckCircle } from "lucide-react";

export default function LoginSuccess() {
  const navigate = useNavigate();

  useEffect(() => {
    // Auto redirect to dashboard after 3 seconds
    const timer = setTimeout(() => {
      navigate("/dashboard");
    }, 3000);

    return () => clearTimeout(timer);
  }, [navigate]);

  return (
    <div className="success-container">
      <div className="success-card">
        <div className="success-icon">
          <CheckCircle className="success-icon-green" />
        </div>

        <h1 className="success-title">Welcome Back!</h1>

        <p className="success-text">
          You have successfully signed in to your Medicare account.
        </p>

        <div className="success-actions">
          <Link to="/dashboard" className="btn-primary text-center">
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
