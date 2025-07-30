import React, { useEffect, useState } from "react";

import Header from "../../layout/Header";

import { BuySubscriptionModal } from "./components/BuySubscriptionModal";
import { SubscriptionView } from "./components/SubscriptionView";
import type { Plan, Subscription, SubscriptionType } from "./types";

export const SubscriptionPage: React.FC = () => {
  const [subscription, setSubscription] = useState<Subscription | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  useEffect(() => {
    // Simulate API call to fetch subscription data
    setTimeout(() => {
      setSubscription({
        type: "Premium",
        active: false,
        renewalDate: "2025-07-01",
      });
    }, 500);
  }, []);

  const handleBuySubscription = () => {
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
  };

  const handlePaymentSuccess = (plan: Plan) => {
    setIsModalOpen(false);
    // Update subscription status
    setSubscription({
      type: plan.name as SubscriptionType,
      active: true,
      renewalDate:
        plan.id === "yearly"
          ? new Date(Date.now() + 365 * 24 * 60 * 60 * 1000).toISOString()
          : new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
    });
  };

  return (
    <div className="min-h-screen bg-gray-100">
      <Header />
      <div className="pt-20 pb-12">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="bg-white rounded-2xl shadow-lg p-8">
            <SubscriptionView
              subscription={subscription}
              onBuySubscription={handleBuySubscription}
            />
          </div>
        </div>
      </div>

      <BuySubscriptionModal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        onPaymentSuccess={handlePaymentSuccess}
      />
    </div>
  );
};
