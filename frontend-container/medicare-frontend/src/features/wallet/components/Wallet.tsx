import React from "react";
import { SubscriptionInfo } from "./SubscriptionInfo";
import { AppointmentsList } from "./VisitsList";
import { Loading } from "../../../shared/components";
import type { WalletProps } from "../types";

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
      <SubscriptionInfo
        subscription={wallet.subscription}
        onNavigateToSubscription={onNavigateToSubscription}
      />

      {/* Divider */}
      <div className="h-px bg-gray-300" />

      {/* Unpaid Appointments */}
      <AppointmentsList
        appointments={wallet.unpaidAppointments}
        onPayAppointment={onPayAppointment}
        payingAppointmentId={payingAppointmentId}
      />
    </div>
  );
};
