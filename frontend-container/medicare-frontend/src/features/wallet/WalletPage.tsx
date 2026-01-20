import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import Header from "@layout/Header";
import { ErrorDisplay, Loading } from "@shared/components";
import { useWallet } from "@shared/hooks/useWallet";

import { PaymentModal } from "./components/PaymentModal";
import { Wallet } from "./components/Wallet";

export const WalletPage: React.FC = () => {
  const navigate = useNavigate();
  const [payingAppointmentId, setPayingAppointmentId] = useState<string | null>(
    null
  );

  const [isPaymentModalOpen, setIsPaymentModalOpen] = useState(false);

  const { wallet, loading, error, refetch, payAppointment } = useWallet();

  const handlePayAppointment = (appointmentId: string) => {
    setPayingAppointmentId(appointmentId);
    setIsPaymentModalOpen(true);
  };

  const handleConfirmPayment = async (method: "BLIK" | "Card") => {
    if (!payingAppointmentId) return;

    setIsPaymentModalOpen(false);

    try {
      const success = await payAppointment(payingAppointmentId, method);
      if (!success) {
        console.error("Payment failed");
      }
    } catch (error) {
      console.error("Payment error:", error);
    } finally {
      setPayingAppointmentId(null);
    }
  };

  const handleCloseModal = () => {
    setIsPaymentModalOpen(false);
    setPayingAppointmentId(null);
  };

  const handleNavigateToSubscription = () => {
    navigate("/user/wallet/subscription");
  };

  // Find amount for modal
  const payingAmount =
    wallet?.unpaidAppointments.find((a) => a.id === payingAppointmentId)
      ?.total || 300.0;

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <main className="pt-24 px-4 md:px-8 pb-10 flex justify-center">
          <div className="w-full max-w-2xl flex justify-center items-center h-60">
            <Loading />
          </div>
        </main>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <main className="pt-24 px-4 md:px-8 pb-10 flex justify-center">
          <div className="w-full max-w-2xl">
            <ErrorDisplay message={error} onRetry={refetch} />
          </div>
        </main>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      <Header />
      <main className="pt-24 px-4 md:px-8 pb-10 flex justify-center">
        <Wallet
          wallet={wallet}
          onPayAppointment={handlePayAppointment}
          onNavigateToSubscription={handleNavigateToSubscription}
          payingAppointmentId={payingAppointmentId}
        />
      </main>

      <PaymentModal
        isOpen={isPaymentModalOpen}
        onClose={handleCloseModal}
        onConfirm={handleConfirmPayment}
        amount={payingAmount}
      />
    </div>
  );
};
