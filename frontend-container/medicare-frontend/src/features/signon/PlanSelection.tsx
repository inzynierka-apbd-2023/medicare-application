import { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { Button } from "@shared/components";
import { ArrowLeft, Check, ShieldCheck, Star } from "lucide-react";

interface Plan {
  id: string;
  name: string;
  price: string;
  priceValue: number;
  currency: string;
  duration: "month" | "year";
  benefits: string[];
  image: string;
  popular?: boolean;
  recommended?: boolean;
}

const plans: Plan[] = [
  {
    id: "standard-monthly",
    name: "Medicare Standard",
    price: "99 PLN / month",
    priceValue: 99,
    currency: "PLN",
    duration: "month",
    benefits: [
      "Basic consultations",
      "Access to GP",
      "Online prescriptions",
      "Emergency hotline",
      "Basic health records",
    ],
    image: "/assets/pexels-pixabay-40568.jpg",
  },
  {
    id: "gold-monthly",
    name: "Medicare Gold",
    price: "199 PLN / month",
    priceValue: 199,
    currency: "PLN",
    duration: "month",
    benefits: [
      "Everything in Standard",
      "Specialist access",
      "Shorter wait times",
      "Basic diagnostics",
      "Prescription delivery",
      "Health analytics",
    ],
    image: "/assets/pexels-thirdman-5327653.jpg",
    popular: true,
  },
  {
    id: "platinum-monthly",
    name: "Medicare Platinum",
    price: "249 PLN / month",
    priceValue: 249,
    currency: "PLN",
    duration: "month",
    benefits: [
      "All in Gold",
      "Advanced diagnostics",
      "Health concierge",
      "Wellness programs",
      "Priority support",
      "Mental health services",
    ],
    image: "/assets/pexels-shkrabaanthony-5215013.jpg",
  },
  {
    id: "ultimate-yearly",
    name: "Medicare Ultimate",
    price: "2999 PLN / year",
    priceValue: 2999,
    currency: "PLN",
    duration: "year",
    benefits: [
      "All features included",
      "Private rooms",
      "24/7 personal care assistant",
      "Premium facilities",
      "Annual health checkup",
      "VIP treatment",
    ],
    image: "/assets/pexels-edward-jenner-4031818.jpg",
    recommended: true,
  },
];

export default function PlanSelection() {
  const navigate = useNavigate();
  const location = useLocation();
  const queryParams = new URLSearchParams(location.search);
  const selectedPlanId = queryParams.get("plan") || "gold-monthly";

  const [selectedPlan, setSelectedPlan] = useState<Plan>(
    plans.find((p) => p.id === selectedPlanId) || plans[1]
  );
  const [isLoading, setIsLoading] = useState(false);

  const handleContinue = () => {
    // Store selected plan in localStorage for the registration process
    localStorage.setItem("selectedPlan", JSON.stringify(selectedPlan));
    navigate("/register");
  };

  const handleBuyNow = async () => {
    setIsLoading(true);
    try {
      // Store selected plan for immediate purchase
      localStorage.setItem("selectedPlan", JSON.stringify(selectedPlan));
      localStorage.setItem("purchaseIntent", "immediate");

      // Navigate to registration with purchase intent
      navigate("/register?purchase=true");
    } catch (error) {
      console.error("Error initiating purchase:", error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center px-4 sm:px-6 md:px-8 py-8 bg-gray-50">
      <div className="bg-white shadow-xl rounded-2xl w-full max-w-4xl px-6 py-8 sm:px-8">
        <button
          onClick={() => navigate(-1)}
          className="flex items-center text-blue-600 hover:underline mb-6"
        >
          <ArrowLeft className="w-4 h-4 mr-1" />
          Back
        </button>

        <div className="text-center mb-8">
          <h1 className="text-3xl sm:text-4xl font-bold mb-2">
            Choose Your Healthcare Plan
          </h1>
          <p className="text-gray-600 text-lg">
            Select the plan that best fits your healthcare needs
          </p>
        </div>

        {/* Plan Selection Grid */}
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4 mb-8">
          {plans.map((plan) => (
            <div
              key={plan.id}
              className={`relative border-2 rounded-xl p-6 cursor-pointer transition-all hover:shadow-lg ${
                selectedPlan.id === plan.id
                  ? "border-blue-500 bg-blue-50 shadow-lg"
                  : "border-gray-200 hover:border-blue-300"
              }`}
              onClick={() => setSelectedPlan(plan)}
            >
              {plan.popular && (
                <div className="absolute -top-3 left-1/2 transform -translate-x-1/2">
                  <span className="bg-yellow-400 text-black text-xs font-bold px-3 py-1 rounded-full flex items-center gap-1">
                    <Star className="w-3 h-3" />
                    Most Popular
                  </span>
                </div>
              )}

              {plan.recommended && (
                <div className="absolute -top-3 left-1/2 transform -translate-x-1/2">
                  <span className="bg-green-500 text-white text-xs font-bold px-3 py-1 rounded-full flex items-center gap-1">
                    <ShieldCheck className="w-3 h-3" />
                    Best Value
                  </span>
                </div>
              )}

              <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                  <h3 className="font-bold text-lg mb-1">{plan.name}</h3>
                  <p className="text-2xl font-bold text-blue-700 mb-1">
                    {plan.price}
                  </p>
                  <p className="text-sm text-gray-500">
                    {plan.duration === "year" ? "Save 20%" : "Billed monthly"}
                  </p>
                </div>
                <div
                  className={`w-5 h-5 rounded-full border-2 flex items-center justify-center ${
                    selectedPlan.id === plan.id
                      ? "border-blue-500 bg-blue-500"
                      : "border-gray-300"
                  }`}
                >
                  {selectedPlan.id === plan.id && (
                    <Check className="w-3 h-3 text-white" />
                  )}
                </div>
              </div>

              <ul className="text-sm text-gray-600 space-y-2">
                {plan.benefits.map((benefit, index) => (
                  <li key={index} className="flex items-start">
                    <Check className="w-4 h-4 text-green-500 mr-2 flex-shrink-0 mt-0.5" />
                    <span>{benefit}</span>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        {/* Selected Plan Summary */}
        <div className="border border-blue-200 bg-blue-50 rounded-xl p-6 mb-8">
          <h3 className="font-semibold text-blue-800 mb-4 text-lg">
            Selected Plan: {selectedPlan.name}
          </h3>
          <div className="grid md:grid-cols-2 gap-6">
            <div>
              <p className="text-3xl font-bold text-blue-700 mb-2">
                {selectedPlan.price}
              </p>
              <p className="text-sm text-blue-600 mb-4">
                {selectedPlan.duration === "year"
                  ? "Billed annually • Cancel anytime • Save 20%"
                  : "Billed monthly • Cancel anytime"}
              </p>
            </div>
            <div>
              <p className="font-medium text-blue-800 mb-3">What's included:</p>
              <ul className="text-sm text-blue-700 space-y-1">
                {selectedPlan.benefits.slice(0, 4).map((benefit, index) => (
                  <li key={index} className="flex items-center">
                    <Check className="w-3 h-3 mr-2 flex-shrink-0" />
                    {benefit}
                  </li>
                ))}
                {selectedPlan.benefits.length > 4 && (
                  <li className="text-blue-600">
                    And {selectedPlan.benefits.length - 4} more features...
                  </li>
                )}
              </ul>
            </div>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex flex-col sm:flex-row gap-4">
          <Button
            variant="secondary"
            onClick={handleContinue}
            className="flex-1"
            size="lg"
          >
            Continue to Registration
          </Button>

          <Button
            variant="primary"
            onClick={handleBuyNow}
            disabled={isLoading}
            className="flex-1"
            size="lg"
          >
            {isLoading ? "Processing..." : `Buy ${selectedPlan.name} Now`}
          </Button>
        </div>

        <div className="mt-6 text-center">
          <p className="text-sm text-gray-600 mb-2">
            Already have an account?{" "}
            <Link to="/login" className="text-blue-600 hover:underline">
              Sign in here
            </Link>
          </p>
          <p className="text-xs text-gray-500">
            Need help choosing? Call us at{" "}
            <a
              href="tel:+48111111111"
              className="text-blue-600 hover:underline"
            >
              +48 111 111 111
            </a>
          </p>
        </div>
      </div>
    </div>
  );
}
