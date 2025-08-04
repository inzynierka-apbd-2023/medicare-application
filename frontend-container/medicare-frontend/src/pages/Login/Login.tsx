import React, { useState } from 'react';
import { Eye, EyeOff } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';

export default function Login() {
  const [cardNumber, setCardNumber] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log('Card:', cardNumber);
    console.log('Password:', password);
    
    // TODO: integrate with backend authentication API
    // For now, simulate successful login
    navigate('/login-success');
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h1 className="auth-header">Welcome to Medicare</h1>
        <p className="auth-subtitle">
          Are you logging in for the first time?{' '}
          <a href="#" className="text-link">
            Find out how to collect your password
          </a>
        </p>

        {/* Tabs */}
        <div className="flex justify-center space-x-4 mb-6 border-b pb-2">
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
                type={showPassword ? 'text' : 'password'}
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
          <button type="submit" className="auth-submit">
            Log in
          </button>
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
}
