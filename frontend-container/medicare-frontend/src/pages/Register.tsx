import React, { useState } from 'react';
import { Eye, EyeOff, ArrowLeft } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';

export default function Register() {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    dateOfBirth: '',
    password: '',
    confirmPassword: '',
  });
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [acceptTerms, setAcceptTerms] = useState(false);
  const navigate = useNavigate();

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    // Basic validation
    if (formData.password !== formData.confirmPassword) {
      alert('Passwords do not match');
      return;
    }
    
    if (!acceptTerms) {
      alert('Please accept the terms and conditions');
      return;
    }
    
    console.log('Registration data:', formData);
    // TODO: integrate with backend registration API
    
    // For now, redirect to success page
    navigate('/registration-success');
  };

  return (
    <div className="page-container-with-scroll">
      <div className="auth-card">
        <button
          onClick={() => navigate('/choose-plan')}
          className="btn-back"
        >
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
              <label className="form-label">First Name</label>
              <input
                type="text"
                name="firstName"
                className="form-input text-sm"
                placeholder="John"
                value={formData.firstName}
                onChange={handleInputChange}
                required
              />
            </div>
            <div className="form-group-small">
              <label className="form-label">Last Name</label>
              <input
                type="text"
                name="lastName"
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
            <label className="form-label">Email Address</label>
            <input
              type="email"
              name="email"
              className="form-input text-sm"
              placeholder="john.doe@example.com"
              value={formData.email}
              onChange={handleInputChange}
              required
            />
          </div>

          {/* Phone */}
          <div className="form-group-small">
            <label className="form-label">Phone Number</label>
            <input
              type="tel"
              name="phone"
              className="form-input text-sm"
              placeholder="+48 123 456 789"
              value={formData.phone}
              onChange={handleInputChange}
              required
            />
          </div>

          {/* Date of Birth */}
          <div className="form-group-small">
            <label className="form-label">Date of Birth</label>
            <input
              type="date"
              name="dateOfBirth"
              className="form-input text-sm"
              value={formData.dateOfBirth}
              onChange={handleInputChange}
              required
            />
          </div>

          {/* Password */}
          <div className="form-group-small">
            <label className="form-label">Password</label>
            <div className="field-group">
              <input
                type={showPassword ? 'text' : 'password'}
                name="password"
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
                {showPassword ? <EyeOff className="icon-small" /> : <Eye className="icon-small" />}
              </button>
            </div>
          </div>

          {/* Confirm Password */}
          <div className="form-group-small">
            <label className="form-label">Confirm Password</label>
            <div className="field-group">
              <input
                type={showConfirmPassword ? 'text' : 'password'}
                name="confirmPassword"
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
                {showConfirmPassword ? <EyeOff className="icon-small" /> : <Eye className="icon-small" />}
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
              I agree to the{' '}
              <a href="#" className="text-link">Terms of Service</a>
              {' '}and{' '}
              <a href="#" className="text-link">Privacy Policy</a>
            </label>
          </div>

          <button type="submit" className="btn-primary">
            Create Account
          </button>
        </form>

        <div className="auth-footer">
          <p className="auth-footer-text">
            Already have an account?{' '}
            <Link to="/login" className="text-link">
              Sign in here
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
