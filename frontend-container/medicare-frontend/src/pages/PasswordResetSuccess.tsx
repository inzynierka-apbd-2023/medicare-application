import React from 'react';
import { Mail, ArrowLeft } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function PasswordResetSuccess() {
  return (
    <div className="success-container">
      <div className="success-card">
        <div className="success-icon">
          <Mail className="success-icon-blue" />
        </div>
        
        <h1 className="success-title">Check Your Email</h1>
        
        <p className="success-text">
          We've sent a password reset link to your email address. Please check your inbox and follow the instructions to reset your password.
        </p>
        
        <div className="success-info">
          <p className="text-sm text-blue-700">
            <strong>Didn't receive the email?</strong><br />
            Check your spam folder or try again in a few minutes.
          </p>
        </div>
        
        <div className="success-actions">
          <Link to="/login" className="btn-primary flex items-center justify-center">
            <ArrowLeft className="icon-small" />
            Back to Login
          </Link>
          
          <Link to="/forgot-password" className="btn-secondary text-center">
            Try Again
          </Link>
        </div>
        
        <p className="success-help">
          Need help? Contact support at{' '}
          <a href="tel:1300555123" className="text-link">
            1300 555 123
          </a>
        </p>
      </div>
    </div>
  );
}
