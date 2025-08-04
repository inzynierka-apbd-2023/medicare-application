import React, { useState } from 'react';
import { ArrowLeft, Check, Circle } from 'lucide-react';
import { Link, useNavigate, useLocation } from 'react-router-dom';

interface Plan {
  name: string;
  price: string;
  benefits: string[];
  image: string;
  popular?: boolean;
}

const plans: Plan[] = [
  {
    name: 'Medicare Standard',
    price: '99 PLN / month',
    benefits: ['Basic consultations', 'Access to GP', 'Online prescriptions', 'Emergency hotline'],
    image: '/assets/pexels-pixabay-40568.jpg',
  },
  {
    name: 'Medicare Gold',
    price: '199 PLN / month',
    benefits: ['Everything in Standard', 'Specialist access', 'Shorter wait times', 'Basic diagnostics'],
    image: '/assets/pexels-thirdman-5327653.jpg',
    popular: true,
  },
  {
    name: 'Medicare Platinum',
    price: '249 PLN / month',
    benefits: ['All in Gold', 'Advanced diagnostics', 'Health concierge', 'Wellness programs'],
    image: '/assets/pexels-shkrabaanthony-5215013.jpg',
  },
  {
    name: 'Medicare Ultimate',
    price: '299 PLN / month',
    benefits: ['All features', 'Private rooms', '24/7 personal care assistant', 'Premium facilities'],
    image: '/assets/pexels-edward-jenner-4031818.jpg',
  },
];

export default function PlanSelection() {
  const navigate = useNavigate();
  const location = useLocation();
  const queryParams = new URLSearchParams(location.search);
  const selectedPlanName = queryParams.get('plan') || 'Medicare Gold';
  
  const [selectedPlan, setSelectedPlan] = useState<Plan>(
    plans.find(p => p.name === selectedPlanName) || plans[1]
  );

  const handleContinue = () => {
    // Store selected plan in localStorage for the registration process
    localStorage.setItem('selectedPlan', JSON.stringify(selectedPlan));
    navigate('/register');
  };

  return (
    <div className="h-screen flex items-center justify-center px-4 sm:px-6 md:px-8 py-8 overflow-y-auto">
      <div className="bg-white shadow-xl rounded-2xl w-full max-w-2xl px-6 py-8 sm:px-8">
        <button
          onClick={() => navigate('/choose-plan')}
          className="flex items-center text-blue-600 hover:underline mb-6"
        >
          <ArrowLeft className="w-4 h-4 mr-1" />
          Back to all plans
        </button>

        <h1 className="text-2xl sm:text-3xl font-bold text-center mb-2">Complete Your Plan Selection</h1>
        <p className="text-center text-gray-600 mb-8">
          Review your chosen plan and proceed to create your account
        </p>

        {/* Plan Selection */}
        <div className="mb-8">
          <h2 className="text-lg font-semibold mb-4">Choose Your Plan:</h2>
          <div className="grid gap-4 sm:grid-cols-2">
            {plans.map((plan) => (
              <div
                key={plan.name}
                className={`border-2 rounded-xl p-4 cursor-pointer transition ${
                  selectedPlan.name === plan.name
                    ? 'border-blue-500 bg-blue-50'
                    : 'border-gray-200 hover:border-blue-300'
                }`}
                onClick={() => setSelectedPlan(plan)}
              >
                <div className="flex items-start justify-between mb-2">
                  <div>
                    <h3 className="font-semibold">{plan.name}</h3>
                    {plan.popular && (
                      <span className="inline-block bg-yellow-400 text-black text-xs font-bold px-2 py-1 rounded-full mt-1">
                        Most Popular
                      </span>
                    )}
                  </div>
                  {selectedPlan.name === plan.name && (
                    <Check className="w-5 h-5 text-blue-600" />
                  )}
                </div>
                <p className="text-lg font-bold text-blue-700 mb-2">{plan.price}</p>
                <ul className="text-sm text-gray-600 space-y-1">
                  {plan.benefits.slice(0, 3).map((benefit) => (
                    <li key={benefit} className="flex items-center">
                      <Circle className="w-2 h-2 fill-current mr-2 flex-shrink-0" />
                      {benefit}
                    </li>
                  ))}
                  {plan.benefits.length > 3 && (
                    <li key="more" className="text-blue-600 flex items-center">
                      <Circle className="w-2 h-2 fill-current mr-2 flex-shrink-0" />
                      And more...
                    </li>
                  )}
                </ul>
              </div>
            ))}
          </div>
        </div>

        {/* Selected Plan Summary */}
        <div className="border border-blue-200 bg-blue-50 rounded-xl p-6 mb-8">
          <h3 className="font-semibold text-blue-800 mb-3">Your Selected Plan: {selectedPlan.name}</h3>
          <div className="grid sm:grid-cols-2 gap-4">
            <div>
              <p className="text-2xl font-bold text-blue-700 mb-2">{selectedPlan.price}</p>
              <p className="text-sm text-blue-600">Billed monthly • Cancel anytime</p>
            </div>
            <div>
              <p className="font-medium text-blue-800 mb-2">Included Benefits:</p>
              <ul className="text-sm text-blue-700 space-y-1">
                {selectedPlan.benefits.map((benefit) => (
                  <li key={benefit} className="flex items-center">
                    <Check className="w-3 h-3 mr-2 flex-shrink-0" />
                    {benefit}
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="space-y-3">
          <button
            onClick={handleContinue}
            className="w-full bg-blue-700 text-white py-3 rounded-xl font-semibold hover:bg-blue-800 transition"
          >
            Continue to Registration
          </button>
          
          <div className="text-center">
            <p className="text-sm text-gray-600">
              Already have an account?{' '}
              <Link to="/login" className="text-blue-600 hover:underline">
                Sign in here
              </Link>
            </p>
          </div>
        </div>

        <div className="mt-8 text-center">
          <p className="text-xs text-gray-500">
            Need help choosing? Call us at{' '}
            <a href="tel:+48111111111" className="text-blue-600 hover:underline">
              +48 111 111 111
            </a>
          </p>
        </div>
      </div>
    </div>
  );
}
