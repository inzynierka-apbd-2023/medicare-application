import React, { useEffect, useRef, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { ArrowLeft, Eye, EyeOff } from "lucide-react";

import { useAuth } from "../../shared/auth/AuthContext";
import { availabilityApi } from "../../shared/services/availabilityApi";

interface RegisterFormData {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  password: string;
  confirmPassword: string;
}

const Register: React.FC = () => {
  const [formData, setFormData] = useState<RegisterFormData>({
    firstName: "",
    lastName: "",
    email: "",
    phone: "",
    dateOfBirth: "",
    password: "",
    confirmPassword: "",
  });
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [acceptTerms, setAcceptTerms] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [emailError, setEmailError] = useState<string | null>(null);
  const [passwordStrength, setPasswordStrength] = useState<"weak" | "medium" | "strong" | null>(null);
  const emailCheckAbort = useRef<AbortController | null>(null);
  const { loading } = useAuth();
  const navigate = useNavigate();

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev: RegisterFormData) => ({
      ...prev,
      [name]: value,
    }));
  };

  // Simple password strength estimator
  const computeStrength = (pwd: string): "weak" | "medium" | "strong" => {
    let score = 0;
    if (pwd.length >= 8) { score++; }
    if (/[A-Z]/.test(pwd)) { score++; }
    if (/[a-z]/.test(pwd)) { score++; }
    if (/\d/.test(pwd)) { score++; }
    if (/[^A-Za-z0-9]/.test(pwd)) { score++; }
    if (pwd.length >= 12) { score++; }
    if (score >= 5) return "strong";
    if (score >= 3) return "medium";
    return "weak";
  };

  useEffect(() => {
    if (formData.password) setPasswordStrength(computeStrength(formData.password));
    else setPasswordStrength(null);
  }, [formData.password]);

  // Debounced email duplicate check
  useEffect(() => {
    setEmailError(null);
    const email = formData.email.trim();
    if (!email) return;
    // simple email pattern gate before pinging server
    const emailPattern = /.+@.+\..+/;
    if (!emailPattern.test(email)) return;

    if (emailCheckAbort.current) emailCheckAbort.current.abort();
    const ctrl = new AbortController();
    emailCheckAbort.current = ctrl;

  const t = setTimeout(async () => {
      try {
    const exists = await availabilityApi.checkEmail(email, ctrl.signal);
        if (!ctrl.signal.aborted && exists) setEmailError("Email is already in use");
      } catch {
        // ignore availability errors in UI; don't block typing
      }
    }, 400);
    return () => {
      ctrl.abort();
      clearTimeout(t);
    };
  }, [formData.email]);

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);

    if (formData.password !== formData.confirmPassword) {
      setError("Passwords do not match");
      return;
    }
    if (passwordStrength === "weak") {
      setError("Password too weak. Add length, numbers, upper/lowercase and a symbol.");
      return;
    }
    if (emailError) {
      setError(emailError);
      return;
    }
    if (!acceptTerms) {
      setError("Please accept the terms and conditions");
      return;
    }
    // Defer account creation until profile completion; pass data to next step
    navigate("/complete-profile", {
      state: {
        registerData: {
          username: formData.email,
          email: formData.email,
          password: formData.password,
          firstName: formData.firstName,
          lastName: formData.lastName,
          phoneNumber: formData.phone,
          dateOfBirth: formData.dateOfBirth,
          role: "Patient",
        },
      },
    });
  };

  return (
    <div className="page-container-with-scroll">
      <div className="auth-card">
        <button onClick={() => navigate("/choose-plan")} className="btn-back">
          <ArrowLeft className="icon-small" />
          Back to plans
        </button>

        <h1 className="auth-header">Create Your Account</h1>
        <p className="auth-subtitle">
          Join Medicare to access quality healthcare services
        </p>

        <form onSubmit={handleSubmit} className="auth-form-small">
          {/* Name Fields */}
          <div className="grid-2">
            <div className="form-group-small">
              <label className="form-label" htmlFor="firstName">First Name</label>
              <input
                type="text"
                name="firstName"
                id="firstName"
                className="form-input text-sm"
                placeholder="John"
                value={formData.firstName}
                onChange={handleInputChange}
                required
              />
            </div>
            <div className="form-group-small">
              <label className="form-label" htmlFor="lastName">Last Name</label>
              <input
                type="text"
                name="lastName"
                id="lastName"
                className="form-input text-sm"
                placeholder="Doe"
                value={formData.lastName}
                onChange={handleInputChange}
                required
              />
            </div>
          </div>

          {/* Email */}
          <div className="form-group-small">
            <label className="form-label" htmlFor="email">Email Address</label>
            <input
              type="email"
              name="email"
              id="email"
              className="form-input text-sm"
              placeholder="john.doe@example.com"
              value={formData.email}
              onChange={handleInputChange}
              required
            />
            {emailError && <div className="text-red-600 text-xs mt-1">{emailError}</div>}
          </div>

          {/* Phone */}
          <div className="form-group-small">
            <label className="form-label" htmlFor="phone">Phone Number</label>
            <input
              type="tel"
              name="phone"
              id="phone"
              className="form-input text-sm"
              placeholder="+48 123 456 789"
              value={formData.phone}
              onChange={handleInputChange}
              required
            />
          </div>

          {/* Date of Birth */}
          <div className="form-group-small">
            <label className="form-label" htmlFor="dateOfBirth">Date of Birth</label>
            <input
              type="date"
              name="dateOfBirth"
              id="dateOfBirth"
              className="form-input text-sm"
              value={formData.dateOfBirth}
              onChange={handleInputChange}
              required
            />
          </div>

          {/* Password */}
          <div className="form-group-small">
            <label className="form-label" htmlFor="password">Password</label>
            <div className="field-group">
              <input
                type={showPassword ? "text" : "password"}
                name="password"
                id="password"
                className="form-input form-input-with-icon text-sm"
                placeholder="Create a strong password"
                value={formData.password}
                onChange={handleInputChange}
                required
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="input-icon"
              >
                {showPassword ? (
                  <EyeOff className="icon-small" />
                ) : (
                  <Eye className="icon-small" />
                )}
              </button>
            </div>
            {passwordStrength && (() => {
              let strengthClass = "text-red-600";
              if (passwordStrength === "medium") strengthClass = "text-yellow-600";
              if (passwordStrength === "strong") strengthClass = "text-green-600";
              return (
                <div className="text-xs mt-1" aria-live="polite">
                  <span>Password strength: </span>
                  <span className={strengthClass}>{passwordStrength}</span>
                </div>
              );
            })()}
          </div>

          {/* Confirm Password */}
          <div className="form-group-small">
            <label className="form-label" htmlFor="confirmPassword">Confirm Password</label>
            <div className="field-group">
              <input
                type={showConfirmPassword ? "text" : "password"}
                name="confirmPassword"
                id="confirmPassword"
                className="form-input form-input-with-icon text-sm"
                placeholder="Confirm your password"
                value={formData.confirmPassword}
                onChange={handleInputChange}
                required
              />
              <button
                type="button"
                onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                className="input-icon"
              >
                {showConfirmPassword ? (
                  <EyeOff className="icon-small" />
                ) : (
                  <Eye className="icon-small" />
                )}
              </button>
            </div>
          </div>

          {/* Terms and Conditions */}
          <div className="terms-container">
            <input
              type="checkbox"
              id="terms"
              className="terms-checkbox"
              checked={acceptTerms}
              onChange={(e) => setAcceptTerms(e.target.checked)}
              required
            />
            <label htmlFor="terms" className="terms-text">
              I agree to the{" "}
              <button type="button" className="text-link" onClick={() => window.open("/terms", "_blank")}>Terms of Service</button>{" "}
              and{" "}
              <button type="button" className="text-link" onClick={() => window.open("/privacy", "_blank")}>Privacy Policy</button>
            </label>
          </div>

          {error && <div className="text-red-600 text-sm mb-2">{error}</div>}
          <button type="submit" className="btn-primary" disabled={loading || emailError !== null || passwordStrength === "weak"}>
            {loading ? "Continuing..." : "Continue"}
          </button>
        </form>

        <div className="auth-footer">
          <p className="auth-footer-text">
            Already have an account?{" "}
            <Link to="/login" className="text-link">
              Sign in here
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default Register;
