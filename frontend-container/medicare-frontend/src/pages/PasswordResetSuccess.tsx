import React from 'react';
import { Mail, ArrowLeft } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function PasswordResetSuccess() {
  return (
    <div className="h-screen flex items-center justify-center px-4 sm:px-6 md:px-8">
      <div className="bg-white shadow-xl rounded-2xl w-full max-w-md px-6 py-8 sm:px-8 text-center">
        <div className="flex justify-center mb-6">
          <Mail className="w-16 h-16 text-blue-500" />
        </div>
        
        <h1 className="text-2xl font-bold text-center mb-4">Check Your Email</h1>
        
        <p className="text-gray-600 mb-6">
          We've sent a password reset link to your email address. Please check your inbox and follow the instructions to reset your password.
        </p>
        
        <div className="bg-blue-50 border border-blue-200 rounded-xl p-4 mb-6">
          <p className="text-sm text-blue-700">
            <strong>Didn't receive the email?</strong><br />
            Check your spam folder or try again in a few minutes.
          </p>
        </div>
        
        <div className="space-y-3">
          <Link
            to="/login"
            className="flex items-center justify-center w-full bg-blue-700 text-white py-3 rounded-xl font-semibold hover:bg-blue-800 transition"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Back to Login
          </Link>
          
          <Link
            to="/forgot-password"
            className="block w-full bg-gray-100 text-gray-700 py-3 rounded-xl font-semibold hover:bg-gray-200 transition text-center"
          >
            Try Again
          </Link>
        </div>
        
        <p className="text-xs text-gray-500 mt-6">
          Need help? Contact support at{' '}
          <a href="tel:1300555123" className="text-blue-600 hover:underline">
            1300 555 123
          </a>
        </p>
      </div>
    </div>
  );
}
