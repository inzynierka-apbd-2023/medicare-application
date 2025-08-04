import React from 'react';
import { CheckCircle } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function RegistrationSuccess() {
  return (
    <div className="h-screen flex items-center justify-center px-4 sm:px-6 md:px-8">
      <div className="bg-white shadow-xl rounded-2xl w-full max-w-md px-6 py-8 sm:px-8 text-center">
        <div className="flex justify-center mb-6">
          <CheckCircle className="w-16 h-16 text-green-500" />
        </div>
        
        <h1 className="text-2xl font-bold text-center mb-4">Registration Successful!</h1>
        
        <p className="text-gray-600 mb-6">
          Welcome to Medicare! Your account has been created successfully.
        </p>
        
        <div className="bg-blue-50 border border-blue-200 rounded-xl p-4 mb-6">
          <h3 className="font-semibold text-blue-800 mb-2">What's Next?</h3>
          <ul className="text-sm text-blue-700 space-y-1">
            <li>• You'll receive your Medicare card by mail within 5-7 business days</li>
            <li>• Check your email for your temporary login credentials</li>
            <li>• Download our mobile app for easy access</li>
          </ul>
        </div>
        
        <div className="space-y-3">
          <Link
            to="/login"
            className="block w-full bg-blue-700 text-white py-3 rounded-xl font-semibold hover:bg-blue-800 transition text-center"
          >
            Sign In Now
          </Link>
          
          <Link
            to="/choose-plan"
            className="block w-full bg-gray-100 text-gray-700 py-3 rounded-xl font-semibold hover:bg-gray-200 transition text-center"
          >
            View Plans Again
          </Link>
        </div>
        
        <p className="text-xs text-gray-500 mt-6">
          Need help? Contact our support team at{' '}
          <a href="tel:1300555123" className="text-blue-600 hover:underline">
            1300 555 123
          </a>
        </p>
      </div>
    </div>
  );
}
