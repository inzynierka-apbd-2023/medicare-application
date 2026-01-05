import React, { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Eye, EyeOff } from "lucide-react";

import { useAuth } from "../../shared/auth/AuthContext";
import { getDefaultDashboard } from "../../shared/constants/routes";

const Login: React.FC = () => {
  const [username, setUsername] = useState<string>("");
  const [password, setPassword] = useState<string>("");
  const [showPassword, setShowPassword] = useState(false);
  const navigate = useNavigate();

  const { login, loading, error, user } = useAuth();

  useEffect(() => {
    if (user && !loading) {
      // Redirect to appropriate dashboard based on role
      const dashboard = getDefaultDashboard(user.role);
      navigate(dashboard);
    }
  }, [user, loading, navigate]);

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const success = await login(username, password);

    if (success) {
      navigate("/login-success");
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h1 className="auth-header">Welcome to Medicare</h1>
        <p className="auth-subtitle">
          Sign in to access your healthcare dashboard
        </p>

        <form onSubmit={handleSubmit} className="auth-form">
          {/* Username */}
          <div className="form-group">
            <label className="field-label" htmlFor="username">
              Username
            </label>
            <input
              type="text"
              id="username"
              className="auth-input"
              placeholder="Enter your username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </div>

          {/* Password */}
          <div className="form-group">
            <label className="field-label" htmlFor="password">
              Password
            </label>
            <div className="field-group">
              <input
                type={showPassword ? "text" : "password"}
                id="password"
                className="auth-input auth-input-with-icon"
                placeholder="Enter your password"
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
            <Link
              to="/forgot-password"
              className="text-link text-sm mt-1"
              style={{ display: "block", textAlign: "right" }}
            >
              Forgot Password?
            </Link>
          </div>

          {error && <div className="text-red-600 text-sm mb-2">{error}</div>}

          <button type="submit" className="auth-submit" disabled={loading}>
            {loading ? "Logging in..." : "Log in"}
          </button>
        </form>

        {/* Register CTA */}
        <div className="auth-footer">
          <p className="auth-footer-text">Don't have an account?</p>
          <Link to="/choose-plan" className="text-link">
            Register now
          </Link>
        </div>
      </div>
    </div>
  );
};

export default Login;
