import React, { useState } from "react";
import { Button } from "@shared/components";
import { CreditCard, Smartphone, X } from "lucide-react";

interface PaymentModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (method: "BLIK" | "Card") => void;
  amount: number;
}

export const PaymentModal: React.FC<PaymentModalProps> = ({
  isOpen,
  onClose,
  onConfirm,
  amount,
}) => {
  const [selectedMethod, setSelectedMethod] = useState<"BLIK" | "Card" | null>(
    null
  );

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-md bg-white rounded-lg shadow-xl p-6 relative">
        <button
          onClick={onClose}
          className="absolute top-4 right-4 text-gray-400 hover:text-gray-600"
        >
          <X size={24} />
        </button>

        <h2 className="text-xl font-bold mb-4">Complete Payment</h2>
        <p className="text-gray-600 mb-6">
          Total Amount: <span className="font-bold">{amount} PLN</span>
        </p>

        <div className="space-y-4 mb-8">
          <div
            className={`p-4 border rounded-lg cursor-pointer flex items-center gap-3 transition-colors ${
              selectedMethod === "BLIK"
                ? "border-blue-500 bg-blue-50"
                : "border-gray-200 hover:bg-gray-50"
            }`}
            onClick={() => setSelectedMethod("BLIK")}
          >
            <Smartphone size={24} className="text-gray-700" />
            <div className="font-semibold">BLIK</div>
          </div>

          <div
            className={`p-4 border rounded-lg cursor-pointer flex items-center gap-3 transition-colors ${
              selectedMethod === "Card"
                ? "border-blue-500 bg-blue-50"
                : "border-gray-200 hover:bg-gray-50"
            }`}
            onClick={() => setSelectedMethod("Card")}
          >
            <CreditCard size={24} className="text-gray-700" />
            <div className="font-semibold">Payment Card</div>
          </div>
        </div>

        <div className="flex justify-end gap-3">
          <Button variant="gray" onClick={onClose}>
            Cancel
          </Button>
          <Button
            variant="primary"
            disabled={!selectedMethod}
            onClick={() => selectedMethod && onConfirm(selectedMethod)}
            className="w-full sm:w-auto"
          >
            Pay Now
          </Button>
        </div>
      </div>
    </div>
  );
};
