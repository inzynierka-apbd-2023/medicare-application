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
    <div className="h-screen flex items-center justify-center px-4 sm:px-6">
      <div className="bg-white shadow-xl rounded-2xl w-full max-w-md px-6 py-8 sm:px-8">
        <button
          onClick={() => navigate('/login')}
          className="flex items-center text-blue-600 hover:underline mb-6"
        >
          <ArrowLeft className="w-4 h-4 mr-1" />
          Back to login
        </button>

        <h2 className="text-2xl font-bold mb-4 text-center">Forgot your card number?</h2>
        <p className="text-sm text-gray-600 text-center mb-6">
          If you agreed to register your phone number with us, we can send you your card number via SMS.
          <br />
          If that is not the case, please contact us at 1300 555 123.
          <br />
        </p>

        <form onSubmit={handleSubmit} className="space-y-5">
          <div>
            <label htmlFor="emailOrCardNumber" className="block text-sm font-medium mb-1">Phone number</label>
            <input
              id="emailOrCardNumber"
              type="text"
              className="w-full px-4 py-3 border border-blue-500 rounded-xl shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
              placeholder="Enter your phone number"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          <button
            type="submit"
            className="w-full bg-blue-700 text-white py-3 rounded-xl font-semibold hover:bg-blue-800 transition"
          >
            Send card number
          </button>
        </form>
      </div>
    </div>
  );
}
