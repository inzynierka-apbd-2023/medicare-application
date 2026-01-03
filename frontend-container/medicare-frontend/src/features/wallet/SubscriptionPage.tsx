import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import Header from "../../layout/Header";
import { useAuth } from "../../shared/auth/AuthContext";
import { ErrorDisplay, Loading } from "../../shared/components";
import {
  type PatientPlanResponse,
  plansApi,
} from "../../shared/services/plansApi";

import { BuySubscriptionModal } from "./components/BuySubscriptionModal";
import { SubscriptionView } from "./components/SubscriptionView";
import type { Plan, Subscription } from "./types";

export const SubscriptionPage: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [subscription, setSubscription] = useState<Subscription | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchSubscription = async () => {
      if (!user?.id) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);
        const response: PatientPlanResponse = await plansApi.getPatientPlan(
          user.id
        );

        if (response.subscription) {
          setSubscription({
            id: response.subscription.id,
            type: response.plan?.name || "Unknown",
            active: response.subscription.status === "Active",
            renewalDate: response.subscription.periodEnd,
            periodStart: response.subscription.periodStart,
            periodEnd: response.subscription.periodEnd,
          });
        } else {
          setSubscription(null);
        }
      } catch (err) {
        console.error("Failed to fetch subscription:", err);
        // Don't set error for missing subscription - it's a valid state
        setSubscription(null);
      } finally {
        setLoading(false);
      }
    };

    fetchSubscription();
  }, [user?.id]);

  const handleBuySubscription = () => {
    navigate("/choose-plan");
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
  };

  const handlePaymentSuccess = (plan: Plan) => {
    setIsModalOpen(false);
    // Update subscription status
    setSubscription({
      id: plan.id,
      type: plan.name,
      active: true,
      renewalDate: new Date(
        Date.now() + 30 * 24 * 60 * 60 * 1000
      ).toISOString(),
      periodStart: new Date().toISOString(),
      periodEnd: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
    });
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <div className="pt-20 pb-12">
          <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
            <Loading />
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <div className="pt-20 pb-12">
          <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
            <ErrorDisplay message={error} />
          </div>
        </div>
      </div>
    );
  }

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
