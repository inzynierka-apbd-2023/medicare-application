import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeft, CheckCircle, Mail } from "lucide-react";

import { apiClient } from "../../shared/services/apiClient";

export default function ForgotPassword() {
  const [email, setEmail] = useState("");
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError("");

    try {
      await apiClient.post("/auth/forgot-password", { email });
      setIsSubmitted(true);
    } catch {
      // Still show success to prevent email enumeration
      setIsSubmitted(true);
    } finally {
      setIsLoading(false);
    }
  };

  if (isSubmitted) {
    return (
      <div className="auth-container">
        <div className="auth-card">
          <div className="success-icon">
            <CheckCircle className="w-16 h-16 text-green-500" />
          </div>

          <h2 className="auth-header">Check your email</h2>
          <p className="auth-subtitle">
            If an account exists for <strong>{email}</strong>, we've sent a
            password reset link. Please check your inbox and spam folder.
          </p>

          <div className="mt-6 space-y-4">
            <button
              onClick={() => navigate("/login")}
              className="auth-submit w-full"
            >
              Back to Login
            </button>
            <button
              onClick={() => {
                setIsSubmitted(false);
                setEmail("");
              }}
              className="text-blue-600 hover:underline w-full"
            >
              Try a different email
            </button>
          </div>
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
            <Mail className="w-8 h-8 text-blue-600" />
          </div>
        </div>

        <h2 className="auth-header">Forgot your password?</h2>
        <p className="auth-subtitle">
          Enter your email address and we'll send you a link to reset your
          password.
        </p>

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label className="form-label">Email Address</label>
            <input
              type="email"
              className="auth-input"
              placeholder="you@example.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          {error && <div className="text-red-600 text-sm mb-2">{error}</div>}

          <button type="submit" className="auth-submit" disabled={isLoading}>
            {isLoading ? "Sending..." : "Send Reset Link"}
          </button>
        </form>
      </div>
    </div>
  );
}
