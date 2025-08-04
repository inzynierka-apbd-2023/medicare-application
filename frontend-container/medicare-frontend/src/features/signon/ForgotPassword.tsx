import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeft } from "lucide-react";

export default function ForgotPassword() {
  const [email, setEmail] = useState("");
  const navigate = useNavigate();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log("Send reset to:", email);
    // TODO: integrate with backend reset password API
    navigate("/password-reset-success");
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <button onClick={() => navigate("/login")} className="btn-back">
          <ArrowLeft className="icon-small" />
          Back to login
        </button>

        <h2 className="auth-header">Forgot your password?</h2>
        <p className="auth-subtitle">
          Enter your card number or email and we will send you a link to reset
          your password.
        </p>

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label className="form-label">Card Number / Email</label>
            <input
              type="text"
              className="auth-input"
              placeholder="Enter your email or card number"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          <button type="submit" className="auth-submit">
            Send reset link
          </button>
        </form>
      </div>
    </div>
  );
}
