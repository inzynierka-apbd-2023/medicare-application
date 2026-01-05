import React from "react";
import { Link } from "react-router-dom";
import { ArrowUpRight, CheckCircle, CreditCard, XCircle } from "lucide-react";

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
  // FREE/Pay Per Visit plan should show as inactive (no renewal, show upgrade button)
  const isFreeOrPayPerVisit =
    subscription.type === "Pay Per Visit" ||
    subscription.type === "FREE" ||
    subscription.type?.toLowerCase().includes("free");

  const isActive = subscription.active && !isFreeOrPayPerVisit;

  return (
    <div className="flex flex-col sm:flex-row gap-6 items-center justify-between mb-5">
      <div className="flex items-center gap-3">
        <CreditCard size={28} className="text-gray-600" />
        <div>
          <div className="text-sm font-medium text-gray-800">
            Subscription: <span className="font-bold">{subscription.type}</span>
          </div>
          <div className="flex items-center gap-2">
            {isActive ? (
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
            ) : isFreeOrPayPerVisit ? (
              <>
                <span className="text-gray-600 text-xs font-medium">
                  Pay per appointment
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
      <div className="flex flex-col sm:flex-row gap-3 mt-4 sm:mt-0">
        <Link to="/choose-plan">
          <Button variant="outline" className="shadow-sm w-full sm:w-auto">
            Choose Plan
          </Button>
        </Link>
        <Button
          variant="primary"
          onClick={onNavigateToSubscription}
          className="shadow-md w-full sm:w-auto"
        >
          {isActive ? (
            "View Subscription Details"
          ) : (
            <>
              Upgrade Plan <ArrowUpRight className="w-4 h-4 inline ml-1" />
            </>
          )}
        </Button>
      </div>
    </div>
  );
};
