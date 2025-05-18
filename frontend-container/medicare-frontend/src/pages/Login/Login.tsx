import React, { useState } from 'react';
import { Eye, EyeOff } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function Login() {
  const [cardNumber, setCardNumber] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log('Card:', cardNumber);
    console.log('Password:', password);
  };

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4 sm:px-6 md:px-8">
      <div className="bg-white shadow-xl rounded-2xl w-full max-w-md px-6 py-8 sm:px-8">
        <h1 className="text-xl sm:text-2xl font-bold text-center mb-2">Welcome to Medicare</h1>
        <p className="text-sm text-center text-gray-600 mb-4">
          Are you logging in for the first time?{' '}
          <a href="#" className="text-blue-600 hover:underline">
            Find out how to collect your password
          </a>
        </p>

        {/* Tabs */}
        <div className="flex justify-center space-x-4 mb-6 border-b pb-2">
        </div>

        <form onSubmit={handleSubmit} className="space-y-5">
          {/* Card Number */}
          <div>
            <div className="flex justify-between text-sm mb-1">
              <label className="font-medium">Card number</label>
              <Link to="/forgot-password" className="text-blue-600 hover:underline text-xs bg-transparent border-none">
                I forgot the card number
              </Link>
            </div>
            <input
              type="text"
              className="w-full px-4 py-3 border border-blue-500 rounded-xl shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
              placeholder="Enter card number"
              value={cardNumber}
              onChange={(e) => setCardNumber(e.target.value)}
              required
            />
          </div>

          {/* Password with eye icon */}
          <div>
            <div className="flex justify-between text-sm mb-1">
              <label className="font-medium">Password</label>
              <Link
                to="/forgot-password"
                className="text-blue-600 hover:underline text-xs bg-transparent border-none"
              >
                I forgot the password
              </Link>
            </div>
            <div className="relative">
              <input
                type={showPassword ? 'text' : 'password'}
                className="w-full px-4 py-3 pr-10 border border-blue-500 rounded-xl shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                placeholder="Enter password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
              <button
                type="button"
                onClick={() => setShowPassword((prev) => !prev)}
                className="absolute inset-y-0 right-3 flex items-center justify-center text-blue-600 hover:text-blue-800 focus:outline-none p-0 m-0 bg-transparent border-none"
              >
                {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
              </button>
            </div>
          </div>
          <button
            type="submit"
            className="w-full bg-blue-700 text-white py-3 rounded-xl font-semibold hover:bg-blue-800 transition"
          >
            Log in
          </button>
        </form>

        {/* CTA: Choose plan */}
        <div className="mt-8 text-center">
          <p className="text-sm text-gray-600 mb-2">Not our patient?</p>
          <Link to="/choose-plan" className="text-blue-600 hover:underline mb-2">
            Choose your plan
          </Link>
          <p className="text-xs text-gray-500 mt-1">Skip queues with us!</p>
        </div>
      </div>
    </div>
  );
}
