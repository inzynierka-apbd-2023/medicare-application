import { Link } from "react-router-dom";
import { Button } from "@shared/components";
import { ArrowLeft, Check, CreditCard, Shield } from "lucide-react";

import Header from "../../layout/Header";

interface SubscriptionPlan {
  id: string;
  name: string;
  price: string;
  period: string;
  features: string[];
  popular?: boolean;
}

const subscriptionPlans: SubscriptionPlan[] = [
  {
    id: "basic",
    name: "Basic",
    price: "$9.99",
    period: "month",
    features: [
      "Access to basic features",
      "Email support",
      "Monthly reports",
      "Up to 5 users",
    ],
  },
  {
    id: "professional",
    name: "Professional",
    price: "$19.99",
    period: "month",
    popular: true,
    features: [
      "All Basic features",
      "Priority support",
      "Advanced analytics",
      "Up to 25 users",
      "API access",
    ],
  },
  {
    id: "enterprise",
    name: "Enterprise",
    price: "$49.99",
    period: "month",
    features: [
      "All Professional features",
      "24/7 phone support",
      "Custom integrations",
      "Unlimited users",
      "Dedicated account manager",
    ],
  },
];

export default function SubscriptionView() {
  const handleSubscribe = (plan: SubscriptionPlan) => {
    // For now, just show success message
    alert(`Successfully subscribed to ${plan.name} plan!`);
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <Header />
      <div className="pt-24 py-8 px-4">
        <div className="max-w-4xl mx-auto">
          {/* Header */}
          <div className="mb-8">
            <Link
              to="/choose-plan"
              className="inline-flex items-center text-blue-600 hover:text-blue-700 mb-4"
            >
              <ArrowLeft className="w-4 h-4 mr-2" />
              Back to Plan Selection
            </Link>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">
              Choose Your Subscription
            </h1>
            <p className="text-gray-600">
              Select the perfect plan for your healthcare needs
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
            {subscriptionPlans.map((plan) => (
              <div
                key={plan.id}
                className={`relative bg-white rounded-lg shadow-md border-2 p-6 ${
                  plan.popular
                    ? "border-blue-500 ring-2 ring-blue-200"
                    : "border-gray-200"
                }`}
              >
                {plan.popular && (
                  <div className="absolute -top-3 left-1/2 transform -translate-x-1/2">
                    <span className="bg-blue-500 text-white px-3 py-1 rounded-full text-sm font-medium">
                      Most Popular
                    </span>
                  </div>
                )}

                <div className="text-center mb-6">
                  <h3 className="text-xl font-semibold text-gray-900 mb-2">
                    {plan.name}
                  </h3>
                  <div className="mb-4">
                    <span className="text-3xl font-bold text-gray-900">
                      {plan.price}
                    </span>
                    <span className="text-gray-500">/{plan.period}</span>
                  </div>
                </div>

                <ul className="space-y-3 mb-6">
                  {plan.features.map((feature, index) => (
                    <li key={index} className="flex items-start">
                      <Check className="w-5 h-5 text-green-500 mr-3 mt-0.5 flex-shrink-0" />
                      <span className="text-gray-700 text-sm">{feature}</span>
                    </li>
                  ))}
                </ul>

                <Button
                  variant={plan.popular ? "primary" : "secondary"}
                  className="w-full"
                  onClick={() => handleSubscribe(plan)}
                >
                  <CreditCard className="w-4 h-4 mr-2" />
                  Subscribe Now
                </Button>
              </div>
            ))}
          </div>

          {/* Security Badge */}
          <div className="bg-white rounded-lg shadow-md p-6 text-center">
            <Shield className="w-8 h-8 text-green-500 mx-auto mb-4" />
            <h3 className="text-lg font-semibold text-gray-900 mb-2">
              Secure & Encrypted
            </h3>
            <p className="text-gray-600 text-sm">
              Your payment information is protected with bank-level security
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
