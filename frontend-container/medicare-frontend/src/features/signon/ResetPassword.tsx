import React, { useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { authService } from "@shared/services/authService";
import { toastMessages, useToast } from "@shared/toast";
import { ArrowLeft, CheckCircle, Eye, EyeOff, Lock } from "lucide-react";

interface ApiError {
  response?: {
    data?: {
      message?: string;
    };
  };
}

export default function ResetPassword() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token") || "";
  const navigate = useNavigate();
  const { showError: showErrorToast } = useToast();

  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const [isSuccess, setIsSuccess] = useState(false);

  const validatePassword = (pwd: string): string | null => {
    if (pwd.length < 8) return "Password must be at least 8 characters";
    if (!/[A-Z]/.test(pwd)) return "Password must contain an uppercase letter";
    if (!/[a-z]/.test(pwd)) return "Password must contain a lowercase letter";
    if (!/[0-9]/.test(pwd)) return "Password must contain a number";
    return null;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (password !== confirmPassword) {
      setError("Passwords do not match");
      showErrorToast(toastMessages.validation.passwordMismatch);
      return;
    }

    const validationError = validatePassword(password);
    if (validationError) {
      setError(validationError);
      showErrorToast(validationError);
      return;
    }

    setIsLoading(true);

    try {
      await authService.resetPassword(token, password);
      // showSuccess(toastMessages.auth.resetPasswordSuccess); // Handled by service
      setIsSuccess(true);
    } catch (err: unknown) {
      const apiError = err as ApiError;
      const errorMessage =
        apiError.response?.data?.message ||
        toastMessages.auth.resetPasswordError;
      setError(errorMessage);
      // showErrorToast(errorMessage); // Handled by service
    } finally {
      setIsLoading(false);
    }
  };

  if (!token) {
    return (
      <div className="auth-container">
        <div className="auth-card">
          <h2 className="auth-header">Invalid Reset Link</h2>
          <p className="auth-subtitle">
            This password reset link is invalid or has expired.
          </p>
          <Link
            to="/forgot-password"
            className="auth-submit text-center block mt-4"
          >
            Request a new link
          </Link>
        </div>
      </div>
    );
  }

  if (isSuccess) {
    return (
      <div className="auth-container">
        <div className="auth-card">
          <div className="flex justify-center mb-4">
            <CheckCircle className="w-16 h-16 text-green-500" />
          </div>

          <h2 className="auth-header">Password Reset Successfully!</h2>
          <p className="auth-subtitle">
            Your password has been changed. You can now sign in with your new
            password.
          </p>

          <button
            onClick={() => navigate("/login")}
            className="auth-submit w-full mt-6"
          >
            Sign In Now
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-container">
      <div className="auth-card">
        <button onClick={() => navigate("/login")} className="btn-back">
          <ArrowLeft className="icon-small" />
          Back to login
        </button>

        <div className="flex justify-center mb-4">
          <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center">
            <Lock className="w-8 h-8 text-blue-600" />
          </div>
        </div>

        <h2 className="auth-header">Create New Password</h2>
        <p className="auth-subtitle">Enter your new password below.</p>

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label className="form-label">New Password</label>
            <div className="field-group">
              <input
                type={showPassword ? "text" : "password"}
                className="auth-input auth-input-with-icon"
                placeholder="Enter new password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                minLength={6}
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

          <div className="form-group">
            <label className="form-label">Confirm Password</label>
            <input
              type="password"
              className="auth-input"
              placeholder="Confirm new password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
            />
          </div>

          {error && <div className="text-red-600 text-sm mb-2">{error}</div>}

          <button type="submit" className="auth-submit" disabled={isLoading}>
            {isLoading ? "Resetting..." : "Reset Password"}
          </button>
        </form>
      </div>
    </div>
  );
}
