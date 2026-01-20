import React from "react";
import { Link } from "react-router-dom";
import { Button, Card } from "@shared/components";
import { useSubscription } from "@shared/hooks/useSubscription";
import { ArrowUpRight, Check, CreditCard, X } from "lucide-react";

export const SubscriptionCard: React.FC = () => {
  const { plan, isLoading, error, features } = useSubscription();

  if (isLoading) {
    return (
      <Card variant="medical" className="animate-pulse">
        <div className="h-6 w-1/3 bg-gray-200 rounded mb-4"></div>
        <div className="h-4 w-full bg-gray-200 rounded mb-2"></div>
        <div className="h-4 w-2/3 bg-gray-200 rounded"></div>
      </Card>
    );
  }

  // If error, show minimal error or just fallback to free content
  // Missing plan means free/default access.
  const planName = plan?.name || "Free Plan";
  const planPrice = plan?.priceCents
    ? `${plan.priceCents / 100} ${plan.currency} / ${plan.billingPeriod}`
    : "Free";
  const isFree = !plan || plan.code === "FREE";

  return (
    <Card
      variant="medical"
      header={
        <div className="flex justify-between items-center">
          <h3 className="text-xl font-semibold text-blue-600 flex items-center gap-2">
            <CreditCard className="w-5 h-5" />
            Subscription Plan
          </h3>
          {plan?.code && plan.code !== "FREE" && (
            <span className="bg-green-100 text-green-800 text-xs font-semibold px-2.5 py-0.5 rounded border border-green-200">
              Active
            </span>
          )}
        </div>
      }
    >
      <div className="space-y-4">
        <div className="flex justify-between items-end border-b pb-4">
          <div>
            <p className="text-sm text-gray-500 uppercase tracking-wide font-semibold">
              Current Plan
            </p>
            <h4 className="text-2xl font-bold text-gray-900 mt-1">
              {planName}
            </h4>
            <p className="text-gray-600 font-medium">{planPrice}</p>
          </div>
          {isFree && (
            <Link to="/user/wallet">
              <Button
                size="sm"
                variant="primary"
                className="flex items-center gap-1"
              >
                Upgrade <ArrowUpRight className="w-4 h-4" />
              </Button>
            </Link>
          )}
          {!isFree && (
            <Link to="/user/wallet">
              <Button size="sm" variant="outline">
                Manage Subscription
              </Button>
            </Link>
          )}
        </div>

        <div>
          <p className="text-sm text-gray-500 font-semibold mb-3">
            Plan Features
          </p>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
            <div className="flex items-center gap-2">
              {features.hasMessaging ? (
                <Check className="w-4 h-4 text-green-600" />
              ) : (
                <X className="w-4 h-4 text-gray-400" />
              )}
              <span
                className={
                  features.hasMessaging ? "text-gray-800" : "text-gray-400"
                }
              >
                Direct Messaging
              </span>
            </div>
            <div className="flex items-center gap-2">
              {features.hasDocuments ? (
                <Check className="w-4 h-4 text-green-600" />
              ) : (
                <X className="w-4 h-4 text-gray-400" />
              )}
              <span
                className={
                  features.hasDocuments ? "text-gray-800" : "text-gray-400"
                }
              >
                Medical Documents
              </span>
            </div>
            <div className="flex items-center gap-2">
              {features.hasPrescriptions ? (
                <Check className="w-4 h-4 text-green-600" />
              ) : (
                <X className="w-4 h-4 text-gray-400" />
              )}
              <span
                className={
                  features.hasPrescriptions ? "text-gray-800" : "text-gray-400"
                }
              >
                Prescription Access
              </span>
            </div>
          </div>
        </div>

        {error && <p className="text-xs text-red-500 mt-2">{error}</p>}
      </div>
    </Card>
  );
};
