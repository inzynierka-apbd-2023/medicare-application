import React from "react";
import { CheckCircle, CreditCard, XCircle } from "lucide-react";

import { Button } from "../../../shared/components";
import type { Subscription } from "../types";

interface SubscriptionInfoProps {
  subscription: Subscription;
  onNavigateToSubscription: () => void;
}

export const SubscriptionInfo: React.FC<SubscriptionInfoProps> = ({
  subscription,
  onNavigateToSubscription,
}) => {
  return (
    <div className="flex flex-col sm:flex-row gap-6 items-center justify-between mb-5">
      <div className="flex items-center gap-3">
        <CreditCard size={28} className="text-gray-600" />
        <div>
          <div className="text-sm font-medium text-gray-800">
            Subscription: <span className="font-bold">{subscription.type}</span>
          </div>
          <div className="flex items-center gap-2">
            {subscription.active ? (
              <>
                <CheckCircle size={16} className="text-green-600" />
                <span className="text-green-800 text-xs font-medium">
                  Active
                </span>
                <span className="text-gray-600 text-xs">
                  (Renews{" "}
                  {new Date(subscription.renewalDate).toLocaleDateString()})
                </span>
              </>
            ) : (
              <>
                <XCircle size={16} className="text-red-500" />
                <span className="text-red-600 text-xs font-medium">
                  Inactive
                </span>
              </>
            )}
          </div>
        </div>
      </div>
      <Button
        variant="primary"
        onClick={onNavigateToSubscription}
        className="mt-4 sm:mt-0 shadow-md"
      >
        {subscription.active ? "View Subscription Details" : "Get Subscription"}
      </Button>
    </div>
  );
};
