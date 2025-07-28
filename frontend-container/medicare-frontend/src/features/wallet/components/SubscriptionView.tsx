import React from "react";
import { CheckCircle, XCircle, HeartPulse } from "lucide-react";
import { Button } from "../../../shared/components";
import type { SubscriptionViewProps } from "../types";

export const SubscriptionView: React.FC<SubscriptionViewProps> = ({
  subscription,
  onBuySubscription,
}) => {
  const isActive = subscription?.active;
  const renewal = subscription?.renewalDate
    ? new Date(subscription.renewalDate).toLocaleDateString()
    : null;

  return (
    <div className="space-y-10">
      {/* Header Section */}
      <div className="text-center">
        <div className="mb-6">
          <HeartPulse
            size={64}
            className="mx-auto text-blue-600 drop-shadow-lg"
          />
        </div>
        <h2 className="text-4xl font-bold text-blue-700 mb-4">
          Subscription Management
        </h2>
        <p className="text-lg text-gray-600">
          Manage your healthcare subscription and access premium features
        </p>
      </div>

      {/* Status Card */}
      <div
        className={`rounded-xl shadow-lg p-8 border ${
          isActive
            ? "bg-gradient-to-br from-green-50 to-emerald-100 border-green-200"
            : "bg-gradient-to-br from-red-50 to-rose-100 border-red-200"
        }`}
      >
        <div className="relative flex flex-col items-center text-center">
          {/* Content centered */}
          <div className="mb-6 relative">
            {/* Icon positioned close to header text */}
            <div className="absolute -left-14 top-0">
              {isActive ? (
                <div className="p-2 bg-green-500 rounded-full flex items-center justify-center w-12 h-12">
                  <CheckCircle size={28} className="text-white" />
                </div>
              ) : (
                <div className="p-2 bg-red-500 rounded-full flex items-center justify-center w-12 h-12">
                  <XCircle size={28} className="text-white" strokeWidth={3} />
                </div>
              )}
            </div>

            {isActive ? (
              <>
                <h3 className="text-2xl font-bold text-green-700 mb-3">
                  Your subscription is active
                </h3>
                <div className="text-gray-700 text-lg">
                  Plan:{" "}
                  <span className="font-semibold text-green-600">
                    {subscription?.type}
                  </span>
                  {renewal && (
                    <>
                      <br />
                      Renewal:{" "}
                      <span className="font-semibold text-green-600">
                        {renewal}
                      </span>
                    </>
                  )}
                </div>
              </>
            ) : (
              <>
                <h3 className="text-2xl font-bold text-red-600 mb-3">
                  No active subscription
                </h3>
                <p className="text-gray-700 text-lg">
                  Subscribe to unlock premium healthcare features
                </p>
              </>
            )}
          </div>

          {/* Button */}
          {!isActive && (
            <Button
              variant="primary"
              size="lg"
              onClick={onBuySubscription}
              className="px-10 py-4 text-lg font-semibold shadow-lg hover:shadow-xl transition-shadow mx-auto block"
            >
              Buy Subscription
            </Button>
          )}
        </div>
      </div>

      {/* Information Cards */}
      <div className="grid gap-8">
        {/* Benefits Section */}
        <div className="bg-gradient-to-br from-blue-50 to-indigo-100 rounded-xl shadow-lg p-8 border border-blue-200">
          <div className="text-center mb-8">
            <h3 className="text-3xl font-bold text-blue-700 mb-2">
              Subscription Benefits
            </h3>
            <p className="text-blue-600">
              Unlock premium healthcare features with your subscription
            </p>
          </div>
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            <div className="bg-white rounded-lg p-4 shadow-md hover:shadow-lg transition-shadow">
              <div className="flex items-start gap-3">
                <span className="text-3xl">📱</span>
                <div>
                  <h4 className="font-bold text-gray-900 mb-1">
                    Complete Digital Access
                  </h4>
                  <p className="text-gray-600 text-sm leading-relaxed">
                    View all your appointments, health records, and documents in
                    one secure place
                  </p>
                </div>
              </div>
            </div>
            <div className="bg-white rounded-lg p-4 shadow-md hover:shadow-lg transition-shadow">
              <div className="flex items-start gap-3">
                <span className="text-3xl">💬</span>
                <div>
                  <h4 className="font-bold text-gray-900 mb-1">
                    Direct Doctor Communication
                  </h4>
                  <p className="text-gray-600 text-sm leading-relaxed">
                    Send messages and questions directly to your healthcare
                    providers
                  </p>
                </div>
              </div>
            </div>
            <div className="bg-white rounded-lg p-4 shadow-md hover:shadow-lg transition-shadow">
              <div className="flex items-start gap-3">
                <span className="text-3xl">📊</span>
                <div>
                  <h4 className="font-bold text-gray-900 mb-1">
                    Health Analytics
                  </h4>
                  <p className="text-gray-600 text-sm leading-relaxed">
                    Track your health progress with detailed reports and
                    insights
                  </p>
                </div>
              </div>
            </div>
            <div className="bg-white rounded-lg p-4 shadow-md hover:shadow-lg transition-shadow">
              <div className="flex items-start gap-3">
                <span className="text-3xl">🔒</span>
                <div>
                  <h4 className="font-bold text-gray-900 mb-1">
                    Enhanced Security
                  </h4>
                  <p className="text-gray-600 text-sm leading-relaxed">
                    Your medical data is protected with enterprise-grade
                    encryption
                  </p>
                </div>
              </div>
            </div>
            <div className="bg-white rounded-lg p-4 shadow-md hover:shadow-lg transition-shadow">
              <div className="flex items-start gap-3">
                <span className="text-3xl">⚡</span>
                <div>
                  <h4 className="font-bold text-gray-900 mb-1">
                    Priority Support
                  </h4>
                  <p className="text-gray-600 text-sm leading-relaxed">
                    Get faster response times and dedicated customer support
                  </p>
                </div>
              </div>
            </div>
            <div className="bg-white rounded-lg p-4 shadow-md hover:shadow-lg transition-shadow">
              <div className="flex items-start gap-3">
                <span className="text-3xl">📅</span>
                <div>
                  <h4 className="font-bold text-gray-900 mb-1">
                    Advanced Scheduling
                  </h4>
                  <p className="text-gray-600 text-sm leading-relaxed">
                    Book appointments with advanced scheduling features and
                    reminders
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* About Section */}
        <div className="bg-gradient-to-br from-green-50 to-emerald-100 rounded-xl shadow-lg p-8 border border-green-200">
          <div className="text-center mb-6">
            <h3 className="text-3xl font-bold text-green-700 mb-2">
              Why choose a medical subscription?
            </h3>
            <p className="text-green-600">
              Discover the advantages of digital healthcare management
            </p>
          </div>
          <div className="max-w-4xl mx-auto">
            <div className="grid md:grid-cols-2 gap-6">
              <div className="bg-white rounded-lg p-6 shadow-md">
                <h4 className="font-bold text-gray-900 mb-3 text-lg">
                  🏥 Comprehensive Care
                </h4>
                <p className="text-gray-700 leading-relaxed">
                  With a subscription, you unlock secure access to all your
                  health information, the convenience of digital care, and the
                  support of your clinic anytime you need it.
                </p>
              </div>
              <div className="bg-white rounded-lg p-6 shadow-md">
                <h4 className="font-bold text-gray-900 mb-3 text-lg">
                  🌟 Peace of Mind
                </h4>
                <p className="text-gray-700 leading-relaxed">
                  Enjoy the peace of mind that comes with managing your
                  healthcare in one place. Your subscription is your key to
                  modern, accessible healthcare.
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
