import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Wallet } from "./components/Wallet";
import { useWallet } from "../../shared/hooks/useWallet";
import { Loading, ErrorDisplay } from "../../shared/components";
import Header from "../../layout/Header";

export const WalletPage: React.FC = () => {
  const navigate = useNavigate();
  const [payingAppointmentId, setPayingAppointmentId] = useState<string | null>(
    null
  );

  const { wallet, loading, error, refetch, payAppointment } = useWallet();

  const handlePayAppointment = async (appointmentId: string) => {
    setPayingAppointmentId(appointmentId);

    try {
      const success = await payAppointment(appointmentId);
      if (!success) {
        // Error handling is done in the hook
        console.error("Payment failed");
      }
    } catch (error) {
      console.error("Payment error:", error);
    } finally {
      setPayingAppointmentId(null);
    }
  };

  const handleNavigateToSubscription = () => {
    navigate("/user/wallet/subscription");
  };

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
    </div>
  );
};
