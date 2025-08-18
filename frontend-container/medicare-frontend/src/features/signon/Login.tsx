import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Eye, EyeOff } from "lucide-react";

import { useAuth } from "../../shared/auth/AuthContext";

const Login: React.FC = () => {
  const [cardNumber, setCardNumber] = useState<string>("");
  const [password, setPassword] = useState<string>("");
  const [showPassword, setShowPassword] = useState(false);
  const navigate = useNavigate();

  const { login, loading, error } = useAuth();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    await login(cardNumber, password);
    // Prefer immediate redirect to default dashboard if token is present
    const token = localStorage.getItem("authToken");
    if (token) {
      const user = JSON.parse(sessionStorage.getItem("authUser") || "null");
      // Fallback to login success splash if we can't resolve user yet
      if (!user || !user.role) return navigate("/login-success");
    }
    navigate("/login-success");
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h1 className="auth-header">Welcome to Medicare</h1>
        <p className="auth-subtitle">
          Are you logging in for the first time?{" "}
          <button
            type="button"
            onClick={() => navigate("/forgot-password")}
            className="text-link"
          >
            Find out how to collect your password
          </button>
        </p>

        {/* Tabs */}
        <div className="flex justify-center space-x-4 mb-6 border-b pb-2"></div>

        <form onSubmit={handleSubmit} className="auth-form">
          {/* Username (was: Card Number) */}
          <div className="form-group">
            <div className="field-row">
              <label className="field-label" htmlFor="cardNumber">Username</label>
              <Link to="/forgot-card" className="field-link">
                I forgot the username
              </Link>
            </div>
            <input
              type="text"
              id="cardNumber"
              className="auth-input"
              placeholder="Enter username"
              value={cardNumber}
              onChange={(e) => setCardNumber(e.target.value)}
              required
            />
          </div>

          {/* Password with eye icon */}
          <div className="form-group">
            <div className="field-row">
              <label className="field-label" htmlFor="password">Password</label>
              <Link to="/forgot-password" className="field-link">
                I forgot the password
              </Link>
            </div>
            <div className="field-group">
              <input
                type={showPassword ? "text" : "password"}
                id="password"
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
          <p className="auth-cta-text">Skip queues with us!!!!!</p>
        </div>
      </div>
    </div>
  );
};

export default Login;
