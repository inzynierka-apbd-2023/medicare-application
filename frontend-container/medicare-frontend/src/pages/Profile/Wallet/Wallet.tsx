import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { CreditCard, CheckCircle, XCircle } from "lucide-react";
import Header from "../../Header";

type Visit = {
  id: string;
  date: string;
  doctor: string;
  total: number;
  paid: number;
  status: "partial" | "paid";
};

type WalletData = {
  subscription: {
    type: string;
    active: boolean;
    renewalDate: string;
  };
  visits: Visit[];
};

export default function WalletView() {
  const navigate = useNavigate();

  const [wallet, setWallet] = useState<WalletData | null>(null);
  const [payingVisitId, setPayingVisitId] = useState<string | null>(null);

  useEffect(() => {
    setTimeout(() => {
      setWallet({
        subscription: {
          type: "Premium",
          active: false,
          renewalDate: "2025-07-01",
        },
        visits: [
          {
            id: "vst_001",
            date: "2025-05-20",
            doctor: "Dr. Taylor",
            total: 150,
            paid: 100,
            status: "partial",
          },
          {
            id: "vst_002",
            date: "2025-05-10",
            doctor: "Dr. Lee",
            total: 200,
            paid: 200,
            status: "paid",
          },
          {
            id: "vst_003",
            date: "2025-06-01",
            doctor: "Dr. Smith",
            total: 120,
            paid: 50,
            status: "partial",
          },
        ],
      });
    }, 600);
  }, []);

  if (!wallet)
    return (
      <div className="flex justify-center items-center h-60">
        <Header />
        <span className="text-blue-400 animate-pulse">Loading wallet...</span>
      </div>
    );

  // Only visits not fully paid
  const notFullyPaidVisits = wallet.visits.filter(
    (v) => v.status === "partial"
  );

  // Payment logic placeholder
  async function handlePay(visitId: string, amountOwed: number) {
    setPayingVisitId(visitId);
    setTimeout(() => {
      setPayingVisitId(null);
      setWallet((prev) =>
        prev
          ? {
              ...prev,
              visits: prev.visits.map((v) =>
                v.id === visitId ? { ...v, paid: v.total, status: "paid" } : v
              ),
            }
          : prev
      );
    }, 1500);
  }

  return (
    <div className="min-h-screen bg-gray-100">
      <Header />
      <main className="pt-24 px-4 md:px-8 pb-10 flex justify-center">
        <div className="w-full max-w-2xl bg-white rounded-2xl shadow-xl p-8 flex flex-col gap-8">
          {/* Title */}
          <h1 className="text-3xl font-bold text-blue-700 mb-8 text-center">
            Wallet
          </h1>

          {/* Subscription Info + Button */}
          <div className="flex flex-col sm:flex-row gap-6 items-center justify-between mb-4">
            <div className="flex items-center gap-3">
              <CreditCard size={28} className="text-blue-400" />
              <div>
                <div className="text-sm font-medium text-gray-700">
                  Subscription:{" "}
                  <span className="font-bold">{wallet.subscription.type}</span>
                </div>
                <div className="flex items-center gap-2">
                  {wallet.subscription.active ? (
                    <>
                      <CheckCircle size={16} className="text-green-500" />
                      <span className="text-green-700 text-xs">Active</span>
                      <span className="text-gray-500 text-xs">
                        (Renews{" "}
                        {new Date(
                          wallet.subscription.renewalDate
                        ).toLocaleDateString()}
                        )
                      </span>
                    </>
                  ) : (
                    <>
                      <XCircle size={16} className="text-red-400" />
                      <span className="text-red-500 text-xs">Inactive</span>
                    </>
                  )}
                </div>
              </div>
            </div>
            <button
              className="mt-4 sm:mt-0 px-5 py-2 bg-blue-700 text-white rounded-lg font-semibold hover:bg-blue-800 transition"
              onClick={() => navigate("/user/wallet/subscription")}
            >
              {wallet.subscription.active
                ? "View Subscription Details"
                : "Get Subscription"}
            </button>
          </div>

          {/* Divider */}
          <div className="h-px bg-blue-100" />

          {/* Visits Not Fully Paid */}
          <div>
            <div className="mb-2 flex items-center gap-2">
              <span className="font-semibold text-gray-700">
                Visits Not Fully Paid
              </span>
              <span className="bg-blue-100 text-blue-600 rounded-full px-2 py-0.5 text-xs">
                {notFullyPaidVisits.length}
              </span>
            </div>
            {notFullyPaidVisits.length === 0 ? (
              <div className="text-gray-400 text-sm">
                All visits are fully paid!
              </div>
            ) : (
              <div className="flex flex-col gap-3">
                {notFullyPaidVisits.map((visit) => {
                  const owed = visit.total - visit.paid;
                  return (
                    <div
                      key={visit.id}
                      className="flex items-center justify-between bg-blue-50 rounded-xl px-4 py-3 shadow-sm"
                    >
                      <div>
                        <div className="text-sm font-medium text-blue-700">
                          {visit.doctor}
                        </div>
                        <div className="text-xs text-gray-500">
                          {new Date(visit.date).toLocaleDateString()}
                        </div>
                        <div className="text-xs text-gray-500">
                          Owed:{" "}
                          <span className="font-bold text-blue-500">
                            ${owed.toFixed(2)}
                          </span>
                        </div>
                        <div>
                          <span className="bg-yellow-100 text-yellow-600 px-2 py-1 rounded text-xs">
                            Paid: ${visit.paid} / ${visit.total}
                          </span>
                        </div>
                      </div>
                      <div>
                        <button
                          className={`ml-2 px-4 py-2 rounded-lg font-semibold transition text-white ${
                            payingVisitId === visit.id
                              ? "bg-blue-300 cursor-not-allowed"
                              : "bg-blue-700 hover:bg-blue-800"
                          }`}
                          disabled={payingVisitId === visit.id}
                          onClick={() => handlePay(visit.id, owed)}
                        >
                          {payingVisitId === visit.id
                            ? "Paying..."
                            : `Pay $${owed}`}
                        </button>
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        </div>
      </main>
    </div>
  );
}
