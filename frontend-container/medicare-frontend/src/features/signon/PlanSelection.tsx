import { useEffect, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "@shared/auth/AuthContext";
import { Button } from "@shared/components";
import { plansApi } from "@shared/services/plansApi";
import { AlertCircle, ArrowLeft, Check, ShieldCheck, Star } from "lucide-react";

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
    id: "BASIC_MONTHLY",
    name: "Basic Monthly",
    price: "49 PLN / month",
    priceValue: 49,
    currency: "PLN",
    duration: "month",
    benefits: [
      "5 free visits per month",
      "Access to GP",
      "Emergency hotline",
      "Basic health records",
      "Additional visits paid separately",
    ],
    image: "/assets/pexels-pixabay-40568.jpg",
  },
  {
    id: "BASIC_YEARLY",
    name: "Basic Yearly",
    price: "490 PLN / year",
    priceValue: 490,
    currency: "PLN",
    duration: "year",
    benefits: [
      "5 free visits per month",
      "Access to GP",
      "Emergency hotline",
      "Basic health records",
      "Save 2 months!",
    ],
    image: "/assets/pexels-thirdman-5327653.jpg",
    popular: true,
  },
  {
    id: "PREMIUM_MONTHLY",
    name: "Premium Monthly",
    price: "149 PLN / month",
    priceValue: 149,
    currency: "PLN",
    duration: "month",
    benefits: [
      "All visits paid upfront",
      "Direct messaging with doctors",
      "My Prescriptions access",
      "My Documents access",
      "Specialist access",
      "Priority support",
    ],
    image: "/assets/pexels-shkrabaanthony-5215013.jpg",
  },
  {
    id: "PREMIUM_YEARLY",
    name: "Premium Yearly",
    price: "1490 PLN / year",
    priceValue: 1490,
    currency: "PLN",
    duration: "year",
    benefits: [
      "All visits paid upfront",
      "Direct messaging with doctors",
      "My Prescriptions access",
      "My Documents access",
      "Specialist access",
      "Save 2 months!",
    ],
    image: "/assets/pexels-edward-jenner-4031818.jpg",
    recommended: true,
  },
];

const freePlan: Plan = {
  id: "FREE",
  name: "Pay Per Visit",
  price: "Free",
  priceValue: 0,
  currency: "PLN",
  duration: "month",
  benefits: [
    "No subscription required",
    "Pay only when you book",
    "Access to appointment scheduler",
    "Book any doctor",
  ],
  image: "/assets/pexels-pixabay-40568.jpg",
};

export default function PlanSelection() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useAuth();
  const queryParams = new URLSearchParams(location.search);
  const selectedPlanId = queryParams.get("plan") || "FREE";

  const allPlans = [...plans, freePlan];
  const [selectedPlan, setSelectedPlan] = useState<Plan>(
    allPlans.find((p) => p.id === selectedPlanId) || freePlan
  );
  const [isLoading, setIsLoading] = useState(false);
  const [currentPlanCode, setCurrentPlanCode] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const isLoggedIn = !!user;
  // Only mark as current plan if logged in AND plan matches
  const isCurrentPlan =
    isLoggedIn &&
    currentPlanCode !== null &&
    currentPlanCode === selectedPlan.id;

  // Fetch current plan for logged-in users
  useEffect(() => {
    if (user?.id) {
      plansApi
        .getPatientPlan(user.id)
        .then((response) => {
          setCurrentPlanCode(response.plan?.code || "FREE");
        })
        .catch(() => {
          setCurrentPlanCode("FREE");
        });
    }
  }, [user?.id]);

  const handleContinue = () => {
    navigate("/register");
  };

  const handleUpdateSubscription = async () => {
    if (!user?.id) return;

    setIsLoading(true);
    setError(null);
    setSuccess(null);

    try {
      const result = await plansApi.updateSubscription(
        user.id,
        selectedPlan.id
      );
      if (result.success) {
        setSuccess(
          `Successfully updated to ${result.newPlanName || selectedPlan.name}!`
        );
        setCurrentPlanCode(selectedPlan.id);
        setTimeout(() => navigate("/user/wallet"), 2000);
      } else {
        setError(result.errorMessage || "Failed to update subscription");
      }
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      setError(
        error.response?.data?.message ||
          "Failed to update subscription. Please try again."
      );
    } finally {
      setIsLoading(false);
    }
  };

  const handleBuyNow = async () => {
    if (isLoggedIn) {
      await handleUpdateSubscription();
    } else {
      setIsLoading(true);
      try {
        navigate("/register?purchase=true");
      } catch (error) {
        console.error("Error initiating purchase:", error);
      } finally {
        setIsLoading(false);
      }
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center px-4 sm:px-6 md:px-8 py-8 bg-gray-50">
      <div className="bg-white shadow-xl rounded-2xl w-full max-w-5xl px-6 py-8 sm:px-8">
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

        {/* Subscription Plans Grid (4 columns) */}
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4 mb-6">
          {plans.map((plan) => (
            <div
              key={plan.id}
              className={`relative border-2 rounded-xl p-5 cursor-pointer transition-all hover:shadow-lg ${
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

              <div className="flex items-start justify-between mb-3">
                <div className="flex-1">
                  <h3 className="font-bold text-base mb-1">{plan.name}</h3>
                  <p className="text-xl font-bold text-blue-700 mb-1">
                    {plan.price}
                  </p>
                  <p className="text-xs text-gray-500">
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

              <ul className="text-xs text-gray-600 space-y-1.5">
                {plan.benefits.slice(0, 4).map((benefit, index) => (
                  <li key={index} className="flex items-start">
                    <Check className="w-3 h-3 text-green-500 mr-1.5 flex-shrink-0 mt-0.5" />
                    <span>{benefit}</span>
                  </li>
                ))}
                {plan.benefits.length > 4 && (
                  <li className="text-gray-400 text-xs">
                    +{plan.benefits.length - 4} more...
                  </li>
                )}
              </ul>
            </div>
          ))}
        </div>

        {/* FREE Plan - Full width below */}
        <div
          className={`border-2 rounded-xl p-5 cursor-pointer transition-all hover:shadow-lg mb-8 ${
            selectedPlan.id === "FREE"
              ? "border-blue-500 bg-blue-50 shadow-lg"
              : "border-gray-200 hover:border-blue-300"
          }`}
          onClick={() => setSelectedPlan(freePlan)}
        >
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div className="flex items-center gap-4">
              <div
                className={`w-5 h-5 rounded-full border-2 flex items-center justify-center ${
                  selectedPlan.id === "FREE"
                    ? "border-blue-500 bg-blue-500"
                    : "border-gray-300"
                }`}
              >
                {selectedPlan.id === "FREE" && (
                  <Check className="w-3 h-3 text-white" />
                )}
              </div>
              <div>
                <h3 className="font-bold text-lg">{freePlan.name}</h3>
                <p className="text-gray-600 text-sm">
                  No subscription required • Pay only when you book an
                  appointment
                </p>
              </div>
            </div>
            <div className="text-right">
              <p className="text-2xl font-bold text-blue-700">Free</p>
              <p className="text-xs text-gray-500">No monthly fees</p>
            </div>
          </div>
        </div>

        {/* Selected Plan Summary */}
        <div className="border border-blue-200 bg-blue-50 rounded-xl p-6 mb-8">
          <h3 className="font-semibold text-blue-800 mb-4 text-lg">
            Selected Plan: {selectedPlan.name}
            {isCurrentPlan && (
              <span className="ml-2 bg-green-100 text-green-800 text-xs font-semibold px-2.5 py-0.5 rounded">
                Current Plan
              </span>
            )}
          </h3>
          <div className="grid md:grid-cols-2 gap-6">
            <div>
              <p className="text-3xl font-bold text-blue-700 mb-2">
                {selectedPlan.price}
              </p>
              <p className="text-sm text-blue-600 mb-4">
                {selectedPlan.id === "FREE"
                  ? "No subscription • Pay per appointment"
                  : selectedPlan.duration === "year"
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

        {/* Error/Success Messages */}
        {error && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg flex items-center gap-2 text-red-700">
            <AlertCircle className="w-5 h-5" />
            {error}
          </div>
        )}
        {success && (
          <div className="mb-4 p-4 bg-green-50 border border-green-200 rounded-lg flex items-center gap-2 text-green-700">
            <Check className="w-5 h-5" />
            {success}
          </div>
        )}

        {/* Action Buttons */}
        <div className="flex flex-col sm:flex-row gap-4">
          {isLoggedIn ? (
            /* Logged-in user: Show update subscription button */
            <>
              <Button
                variant="secondary"
                onClick={() => navigate(-1)}
                className="flex-1"
                size="lg"
              >
                Cancel
              </Button>
              <Button
                variant="primary"
                onClick={handleBuyNow}
                disabled={isLoading || isCurrentPlan}
                className="flex-1"
                size="lg"
              >
                {isLoading
                  ? "Processing..."
                  : isCurrentPlan
                    ? "Current Plan"
                    : selectedPlan.id === "FREE"
                      ? "Downgrade to Free"
                      : `Update to ${selectedPlan.name}`}
              </Button>
            </>
          ) : (
            <>
              <Button
                variant="secondary"
                onClick={handleContinue}
                className="flex-1"
                size="lg"
              >
                Continue to Registration
              </Button>

              {selectedPlan.id !== "FREE" && (
                <Button
                  variant="primary"
                  onClick={handleBuyNow}
                  disabled={isLoading}
                  className="flex-1"
                  size="lg"
                >
                  {isLoading ? "Processing..." : `Buy ${selectedPlan.name} Now`}
                </Button>
              )}
            </>
          )}
        </div>

        {!isLoggedIn && (
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
        )}
      </div>
    </div>
  );
}
