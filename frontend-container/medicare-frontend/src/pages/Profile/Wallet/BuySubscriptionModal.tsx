import { X, ShieldCheck } from "lucide-react";
import { useState } from "react";

// Example plans, adapt to your backend!
const PLANS = [
  {
    id: "monthly",
    name: "Monthly",
    description: "Full access for one month",
    price: 49,
    currency: "PLN",
  },
  {
    id: "yearly",
    name: "Yearly",
    description: "Full access for one year",
    price: 499,
    currency: "PLN",
    best: true,
  },
];

export default function BuySubscriptionModal({
  open,
  onClose,
  onPaymentSuccess,
  paymentService, // pass a payment handler/service if needed
}) {
  const [selectedPlan, setSelectedPlan] = useState(PLANS[0]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  // Payment logic placeholder
  async function handleBuy() {
    setLoading(true);
    setError("");

    // Here, call your backend/payment provider!
    try {
      // Example: await paymentService.pay(selectedPlan);
      await new Promise((res) => setTimeout(res, 1800)); // Simulate payment delay
      setLoading(false);
      onPaymentSuccess(selectedPlan);
    } catch (err) {
      setLoading(false);
      setError("Payment failed. Please try again or contact support.");
    }
  }

  if (!open) return null;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="relative w-full max-w-xl bg-white rounded-2xl shadow-2xl p-8 pt-10 animate-fadeIn">
        {/* Close Button */}
        <button
          className="absolute right-6 top-6 text-gray-400 hover:text-blue-600 transition"
          onClick={onClose}
          aria-label="Close"
        >
          <X size={30} />
        </button>
        {/* Modal Icon */}
        <div className="flex flex-col items-center mb-8">
          <ShieldCheck
            size={48}
            className="text-blue-600 mb-2 drop-shadow-lg"
          />
          <h2 className="text-3xl font-extrabold text-blue-800 text-center mb-1">
            Choose your subscription
          </h2>
          <div className="text-lg text-gray-600 text-center">
            Access all your medical data, appointments, and digital services
            securely and conveniently.
          </div>
        </div>
        {/* Plans */}
        <div className="flex flex-col gap-4 mb-6">
          {PLANS.map((plan) => (
            <label
              key={plan.id}
              className={`flex items-center justify-between rounded-xl border-2 px-6 py-4 cursor-pointer transition
                ${
                  selectedPlan.id === plan.id
                    ? "border-blue-700 bg-blue-50 shadow-lg"
                    : "border-gray-200 bg-white hover:bg-blue-50/70"
                }`}
            >
              <div>
                <div className="flex items-center gap-2">
                  <input
                    type="radio"
                    name="subscription"
                    checked={selectedPlan.id === plan.id}
                    onChange={() => setSelectedPlan(plan)}
                    className="accent-blue-700 w-5 h-5"
                  />
                  <span className="text-2xl font-bold text-blue-900">
                    {plan.name}
                  </span>
                  {plan.best && (
                    <span className="ml-2 px-2 py-0.5 text-xs rounded-full bg-yellow-100 text-yellow-800 font-semibold">
                      Best value
                    </span>
                  )}
                </div>
                <div className="text-gray-700 mt-1">{plan.description}</div>
              </div>
              <div className="text-2xl font-extrabold text-blue-700">
                {plan.price} <span className="text-lg">{plan.currency}</span>
              </div>
            </label>
          ))}
        </div>
        {/* Payment Button & Error */}
        <div className="flex flex-col gap-3 mt-4">
          <button
            className={`w-full py-4 rounded-xl text-xl font-bold shadow-lg transition
              ${
                loading
                  ? "bg-blue-300 text-white cursor-not-allowed"
                  : "bg-blue-700 hover:bg-blue-800 text-white"
              }`}
            disabled={loading}
            onClick={handleBuy}
          >
            {loading
              ? "Processing payment..."
              : `Buy ${selectedPlan.name} subscription`}
          </button>
          {error && (
            <div className="text-center text-red-500 font-semibold mt-1">
              {error}
            </div>
          )}
        </div>
        {/* Note */}
        <div className="mt-6 text-gray-500 text-center text-sm">
          Payments are handled securely. Your access will be activated
          immediately after payment.
        </div>
      </div>
    </div>
  );
}
