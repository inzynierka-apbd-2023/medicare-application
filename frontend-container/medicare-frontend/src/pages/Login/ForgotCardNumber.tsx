import React, { useState } from 'react';
import { ArrowLeft } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

export default function ForgotPassword() {
  const [email, setEmail] = useState('');
  const navigate = useNavigate();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log('Send reset to:', email);
    // TODO: integrate with backend reset password API
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <button
          onClick={() => navigate('/login')}
          className="btn-back"
        >
          <ArrowLeft className="icon-small" />
          Back to login
        </button>

        <h2 className="auth-header">Forgot your card number?</h2>
        <p className="auth-subtitle">
          If you agreed to register your phone number with us, we can send you your card number via SMS.
          <br />
          If that is not the case, please contact us at 1300 555 123.
        </p>

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label htmlFor="emailOrCardNumber" className="form-label">Phone number</label>
            <input
              id="emailOrCardNumber"
              type="text"
              className="auth-input"
              placeholder="Enter your phone number"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          <button type="submit" className="auth-submit">
            Send card number
          </button>
        </form>
      </div>
    </div>
  );
}
