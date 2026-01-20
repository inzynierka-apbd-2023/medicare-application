import React, { useState } from "react";
import type { BuySubscriptionModalProps, Plan } from "@features/wallet/types";
import { Button, Card, Modal } from "@shared/components";
import { ShieldCheck } from "lucide-react";

const DEFAULT_PLANS: Plan[] = [
  {
    id: "FREE",
    name: "Pay Per Visit",
    description: "No subscription. Pay only when you book an appointment.",
    price: 0,
    currency: "PLN",
  },
  {
    id: "BASIC_MONTHLY",
    name: "Basic Monthly",
    description: "5 free visits per month. Additional visits paid separately.",
    price: 49,
    currency: "PLN",
  },
  {
    id: "BASIC_YEARLY",
    name: "Basic Yearly",
    description: "5 free visits per month. Save 2 months!",
    price: 490,
    currency: "PLN",
    best: true,
  },
  {
    id: "PREMIUM_MONTHLY",
    name: "Premium Monthly",
    description: "Full access to all features. All visits paid upfront.",
    price: 149,
    currency: "PLN",
  },
  {
    id: "PREMIUM_YEARLY",
    name: "Premium Yearly",
    description: "Full access to all features. Save 2 months!",
    price: 1490,
    currency: "PLN",
  },
];

export const BuySubscriptionModal: React.FC<BuySubscriptionModalProps> = ({
  isOpen,
  onClose,
  onPaymentSuccess,
  plans = DEFAULT_PLANS,
}) => {
  const [selectedPlan, setSelectedPlan] = useState(plans[0]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleBuy = async () => {
    setLoading(true);
    setError("");

    try {
      // Simulate payment processing
      await new Promise((resolve) => setTimeout(resolve, 1800));
      setLoading(false);
      onPaymentSuccess(selectedPlan);
    } catch (_err) {
      setLoading(false);
      setError("Payment failed. Please try again or contact support.");
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Choose Your Subscription Plan"
      size="lg"
    >
      <div className="space-y-6">
        {/* Plans */}
        <div className="space-y-4">
          {plans.map((plan) => (
            <Card
              key={plan.id}
              variant={selectedPlan.id === plan.id ? "medical" : "default"}
              padding="lg"
              className={`cursor-pointer transition-all ${
                selectedPlan.id === plan.id
                  ? "ring-2 ring-blue-500 shadow-lg"
                  : "hover:shadow-md"
              }`}
              onClick={() => setSelectedPlan(plan)}
            >
              <div className="flex items-center justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-3">
                    <div className="flex-1">
                      <h3 className="text-lg font-semibold text-gray-800 flex items-center gap-2">
                        {plan.name}
                        {plan.best && (
                          <span className="bg-green-100 text-green-700 px-2 py-1 rounded-full text-xs font-medium">
                            Best Value
                          </span>
                        )}
                      </h3>
                      <p className="text-gray-600 text-sm">
                        {plan.description}
                      </p>
                    </div>
                    <div className="text-right">
                      <div className="text-2xl font-bold text-blue-700">
                        {plan.price} {plan.currency}
                      </div>
                      <div className="text-sm text-gray-500">
                        {plan.id === "yearly" ? "per year" : "per month"}
                      </div>
                    </div>
                  </div>
                </div>
                <div className="ml-4">
                  <div
                    className={`w-4 h-4 rounded-full border-2 ${
                      selectedPlan.id === plan.id
                        ? "bg-blue-500 border-blue-500"
                        : "border-gray-300"
                    }`}
                  >
                    {selectedPlan.id === plan.id && (
                      <div className="w-2 h-2 bg-white rounded-full mx-auto mt-1" />
                    )}
                  </div>
                </div>
              </div>
            </Card>
          ))}
        </div>

        {/* Features */}
        <Card variant="medical" padding="lg">
          <div className="flex items-start gap-3">
            <ShieldCheck className="text-green-600 mt-1" size={20} />
            <div>
              <h4 className="font-semibold text-gray-800 mb-2">
                What's included:
              </h4>
              <ul className="text-sm text-gray-600 space-y-1">
                <li>• Complete access to all medical records</li>
                <li>• Direct communication with healthcare providers</li>
                <li>• Advanced appointment scheduling</li>
                <li>• Health analytics and progress tracking</li>
                <li>• Priority customer support</li>
                <li>• Enterprise-grade security</li>
              </ul>
            </div>
          </div>
        </Card>

        {/* Error Message */}
        {error && (
          <div className="p-3 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-700 text-sm">{error}</p>
          </div>
        )}

        {/* Actions */}
        <div className="flex gap-3 justify-end">
          <Button variant="secondary" onClick={onClose} disabled={loading}>
            Cancel
          </Button>
          <Button
            variant="primary"
            onClick={handleBuy}
            disabled={loading}
            className="min-w-32"
          >
            {loading ? "Processing..." : `Buy ${selectedPlan.name} Plan`}
          </Button>
        </div>
      </div>
    </Modal>
  );
};
