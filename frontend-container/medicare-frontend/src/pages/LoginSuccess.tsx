import React, { useEffect } from 'react';
import { CheckCircle } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';

export default function LoginSuccess() {
  const navigate = useNavigate();

  useEffect(() => {
    // Auto redirect to home after 3 seconds
    const timer = setTimeout(() => {
      navigate('/home');
    }, 3000);

    return () => clearTimeout(timer);
  }, [navigate]);

  return (
    <div className="h-screen flex items-center justify-center px-4 sm:px-6 md:px-8">
      <div className="bg-white shadow-xl rounded-2xl w-full max-w-md px-6 py-8 sm:px-8 text-center">
        <div className="flex justify-center mb-6">
          <CheckCircle className="w-16 h-16 text-green-500" />
        </div>
        
        <h1 className="text-2xl font-bold text-center mb-4">Welcome Back!</h1>
        
        <p className="text-gray-600 mb-8">
          You have successfully signed in to your Medicare account.
        </p>
        
        <div className="space-y-3 mb-6">
          <Link
            to="/home"
            className="block w-full bg-blue-700 text-white py-3 rounded-xl font-semibold hover:bg-blue-800 transition text-center"
          >
            Go to Dashboard
          </Link>
          
          <Link
            to="/choose-plan"
            className="block w-full bg-gray-100 text-gray-700 py-3 rounded-xl font-semibold hover:bg-gray-200 transition text-center"
          >
            Browse Plans
          </Link>
        </div>
        
        <p className="text-sm text-gray-500">
          Redirecting to dashboard in 3 seconds...
        </p>
      </div>
    </div>
  );
}
