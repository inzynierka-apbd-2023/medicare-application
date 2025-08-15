import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Eye, EyeOff } from "lucide-react";

import { useAuth } from "../../shared/auth/AuthContext";
import { MOCK_CREDENTIALS } from "../../shared/services/mockAuthService";

const Login: React.FC = () => {
  const [cardNumber, setCardNumber] = useState<string>("");
  const [password, setPassword] = useState<string>("");
  const [showPassword, setShowPassword] = useState(false);
  const navigate = useNavigate();

  const { login, loading, error } = useAuth();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    await login(cardNumber, password);
    if (localStorage.getItem("authToken")) navigate("/login-success");
  };

  // Quick login function for testing
  const quickLogin = (role: keyof typeof MOCK_CREDENTIALS) => {
    const creds = MOCK_CREDENTIALS[role];
    setCardNumber(creds.username);
    setPassword(creds.password);
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h1 className="auth-header">Welcome to Medicare</h1>
        <p className="auth-subtitle">
          Are you logging in for the first time?{" "}
          <a href="#" className="text-link">
            Find out how to collect your password
          </a>
        </p>

        {/* Tabs */}
        <div className="flex justify-center space-x-4 mb-6 border-b pb-2"></div>

        {/* Mock Credentials Section for Testing */}
        <div className="mb-6 p-4 bg-blue-50 rounded-lg border-2 border-blue-200">
          <h3 className="text-sm font-semibold text-blue-800 mb-3">
            🧪 Quick Login (Testing)
          </h3>
          <div className="grid grid-cols-2 gap-2">
            <button
              type="button"
              onClick={() => quickLogin("patient")}
              className="px-3 py-2 text-xs bg-green-100 text-green-800 rounded hover:bg-green-200 transition"
            >
              Patient
            </button>
            <button
              type="button"
              onClick={() => quickLogin("doctor")}
              className="px-3 py-2 text-xs bg-blue-100 text-blue-800 rounded hover:bg-blue-200 transition"
            >
              Doctor
            </button>
            <button
              type="button"
              onClick={() => quickLogin("owner")}
              className="px-3 py-2 text-xs bg-purple-100 text-purple-800 rounded hover:bg-purple-200 transition"
            >
              Owner
            </button>
            <button
              type="button"
              onClick={() => quickLogin("receptionist")}
              className="px-3 py-2 text-xs bg-orange-100 text-orange-800 rounded hover:bg-orange-200 transition"
            >
              Receptionist
            </button>
          </div>
          <p className="text-xs text-blue-600 mt-2">
            All credentials: username/password = role name / "test"
          </p>
        </div>

        <form onSubmit={handleSubmit} className="auth-form">
          {/* Card Number */}
          <div className="form-group">
            <div className="field-row">
              <label className="field-label">Card number</label>
              <Link to="/forgot-card" className="field-link">
                I forgot the card number
              </Link>
            </div>
            <input
              type="text"
              className="auth-input"
              placeholder="Enter card number"
              value={cardNumber}
              onChange={(e) => setCardNumber(e.target.value)}
              required
            />
          </div>

          {/* Password with eye icon */}
          <div className="form-group">
            <div className="field-row">
              <label className="field-label">Password</label>
              <Link to="/forgot-password" className="field-link">
                I forgot the password
              </Link>
            </div>
            <div className="field-group">
              <input
                type={showPassword ? "text" : "password"}
                className="auth-input auth-input-with-icon"
                placeholder="Enter password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
              <button
                type="button"
                onClick={() => setShowPassword((prev) => !prev)}
                className="password-toggle"
              >
                {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
              </button>
            </div>
          </div>
          {error && <div className="text-red-600 text-sm mb-2">{error}</div>}
          <button type="submit" className="auth-submit" disabled={loading}>
            {loading ? "Logging in..." : "Log in"}
          </button>
          {/* API base shown removed to avoid leaking internal URL */}
        </form>

        {/* CTA: Choose plan */}
        <div className="auth-footer">
          <p className="auth-footer-text">Not our patient?</p>
          <Link to="/choose-plan" className="text-link">
            Choose your plan
          </Link>
          <p className="auth-cta-text">Skip queues with us!</p>
        </div>
      </div>
    </div>
  );
};

export default Login;
