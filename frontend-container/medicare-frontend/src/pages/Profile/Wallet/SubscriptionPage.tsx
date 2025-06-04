import { useState } from "react";
import SubscriptionInfoView from "./SubscriptionView";
import BuySubscriptionModal from "./BuySubscriptionModal";

export default function SubscriptionPage({
  subscription,
  refreshSubscription,
}) {
  const [modalOpen, setModalOpen] = useState(false);

  function handlePaymentSuccess(plan) {
    setModalOpen(false);
    if (refreshSubscription) refreshSubscription();
  }

  return (
    <>
      <SubscriptionInfoView
        subscription={subscription}
        onBuy={() => setModalOpen(true)}
      />
      <BuySubscriptionModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        onPaymentSuccess={handlePaymentSuccess}
        paymentService={undefined}
      />
    </>
  );
}
