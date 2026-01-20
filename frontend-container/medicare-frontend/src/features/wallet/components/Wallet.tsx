import React from "react";
import type { WalletProps } from "@features/wallet/types";
import { Loading } from "@shared/components";

import { SubscriptionInfo } from "./SubscriptionInfo";
import { AppointmentsList } from "./VisitsList";

export const Wallet: React.FC<WalletProps> = ({
  wallet,
  onPayAppointment,
  onNavigateToSubscription,
  payingAppointmentId,
}) => {
  if (!wallet) {
    return (
      <div className="flex justify-center items-center h-60">
        <Loading />
      </div>
    );
  }

  return (
    <div className="w-full max-w-4xl bg-white rounded-2xl shadow-lg p-8 flex flex-col gap-8">
      {/* Title */}
      <h1 className="text-3xl font-bold text-gray-800 mb-8 text-center">
        Wallet
      </h1>

      {/* Subscription Info */}
      {wallet.subscription ? (
        <SubscriptionInfo
          subscription={wallet.subscription}
          onNavigateToSubscription={onNavigateToSubscription}
        />
      ) : (
        <div className="flex flex-col sm:flex-row gap-6 items-center justify-between mb-5">
          <div className="text-gray-600">
            <span className="font-medium">No active subscription</span>
            <p className="text-sm text-gray-500">
              Subscribe to unlock premium features
            </p>
          </div>
          <button
            onClick={onNavigateToSubscription}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Get Subscription
          </button>
        </div>
      )}

      {/* Divider */}
      <div className="h-px bg-gray-300" />

      {/* Unpaid Appointments */}
      <AppointmentsList
        appointments={wallet.unpaidAppointments}
        onPayAppointment={onPayAppointment}
        {...(payingAppointmentId !== undefined && { payingAppointmentId })}
      />
    </div>
  );
};
