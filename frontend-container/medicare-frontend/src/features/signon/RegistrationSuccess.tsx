import { Link } from "react-router-dom";
import { CheckCircle } from "lucide-react";

export default function RegistrationSuccess() {
  return (
    <div className="success-container">
      <div className="success-card">
        <div className="success-icon">
          <CheckCircle className="success-icon-green" />
        </div>

        <h1 className="success-title">Registration Successful!</h1>

        <p className="success-text">
          Welcome to Medicare! Your account has been created successfully.
        </p>

        <div className="success-info">
          <h3 className="success-info-title">What's Next?</h3>
          <ul className="success-info-list">
            <li>
              � You'll receive your Medicare card by mail within 5-7 business
              days
            </li>
            <li>� Check your email for your temporary login credentials</li>
            <li>� Download our mobile app for easy access</li>
          </ul>
        </div>

        <div className="success-actions">
          <Link to="/login" className="btn-primary text-center">
            Sign In Now
          </Link>

          <Link to="/choose-plan" className="btn-secondary text-center">
            View Plans Again
          </Link>
        </div>

        <p className="success-help">
          Need help? Contact our support team at{" "}
          <a href="tel:1300555123" className="text-link">
            1300 555 123
          </a>
        </p>
      </div>
    </div>
  );
}
