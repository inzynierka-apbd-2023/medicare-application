import {
  CalendarCheck,
  CreditCard,
  CheckCircle,
  XCircle,
  Info,
} from "lucide-react";
import Header from "../Header";
import { useNavigate } from "react-router-dom";

export type Appointment = {
  id: string;
  date: string;
  time: string;
  doctor: string;
  specialization?: string;
  description?: string;
  status: "upcoming" | "past" | "cancelled" | string;
  paymentStatus: "paid" | "not_paid" | "partially_paid" | string;
  paid: number;
  total: number;
};

type AppointmentsProps = {
  appointments: Appointment[];
  onDetails: (appt: Appointment) => void;
};

export default function Appointments({
  appointments,
  onDetails,
}: AppointmentsProps) {
  const navigate = useNavigate();
  const upcoming = appointments.filter((appt) => appt.status === "upcoming");
  const previous = appointments.filter((appt) => appt.status !== "upcoming");

  return (
    <div className="min-h-screen bg-gray-100 flex flex-col pt-20 pb-12 items-center">
      <Header />
      <div className="w-full max-w-4xl bg-white/90 rounded-3xl shadow-2xl p-10 mt-10">
        <h2 className="text-4xl font-extrabold text-blue-800 mb-10 text-center flex items-center justify-center gap-3">
          <CalendarCheck className="text-blue-600" size={42} />
          Your Appointments
        </h2>

        {/* UPCOMING */}
        <h3 className="text-2xl font-bold text-blue-700 mb-4">
          Upcoming Appointments
        </h3>
        {upcoming.length === 0 ? (
          <div className="text-center text-gray-400 text-lg mb-8">
            You don’t have any upcoming appointments.
          </div>
        ) : (
          <div className="flex flex-col gap-7 mb-12">
            {upcoming.map((appt) => (
              <div
                key={appt.id}
                className="flex flex-col md:flex-row md:items-center md:justify-between gap-6 bg-blue-50 rounded-xl shadow p-7"
              >
                {/* Info block */}
                <div className="flex-1 flex flex-col sm:flex-row gap-8 items-start sm:items-center">
                  <div>
                    <div className="text-xl font-bold text-blue-700 flex gap-3 items-center">
                      {new Date(appt.date).toLocaleDateString()}{" "}
                      <span className="text-gray-500 font-normal text-base">
                        {appt.time}
                      </span>
                    </div>
                    <div className="text-lg text-gray-800">
                      Doctor:{" "}
                      <span className="font-semibold">{appt.doctor}</span>
                    </div>
                    {appt.specialization && (
                      <div className="text-sm text-gray-500">
                        Specialization: {appt.specialization}
                      </div>
                    )}
                    {/* Payment info */}
                    <div className="mt-3">
                      <span
                        className={`px-2 py-1 rounded text-xs font-semibold 
                        ${
                          appt.paymentStatus === "paid"
                            ? "bg-green-100 text-green-700"
                            : appt.paymentStatus === "partially_paid"
                            ? "bg-yellow-100 text-yellow-600"
                            : "bg-red-100 text-red-600"
                        }`}
                      >
                        Paid: {appt.paid} / {appt.total} PLN
                      </span>
                    </div>
                  </div>
                  {/* Statuses */}
                  <div className="flex flex-col gap-2">
                    <div className="flex items-center gap-2">
                      {appt.status === "upcoming" ? (
                        <span className="bg-green-100 text-green-800 px-2 py-1 rounded text-xs flex items-center gap-1">
                          <CheckCircle size={14} /> Upcoming
                        </span>
                      ) : appt.status === "cancelled" ? (
                        <span className="bg-red-100 text-red-700 px-2 py-1 rounded text-xs flex items-center gap-1">
                          <XCircle size={14} /> Cancelled
                        </span>
                      ) : (
                        <span className="bg-gray-100 text-gray-700 px-2 py-1 rounded text-xs flex items-center gap-1">
                          <Info size={14} /> {appt.status}
                        </span>
                      )}
                    </div>
                    <div className="flex items-center gap-2">
                      {appt.paymentStatus === "paid" ? (
                        <span className="bg-green-200 text-green-800 px-2 py-1 rounded text-xs flex items-center gap-1">
                          <CreditCard size={14} /> Paid
                        </span>
                      ) : appt.paymentStatus === "partially_paid" ? (
                        <span className="bg-yellow-100 text-yellow-800 px-2 py-1 rounded text-xs flex items-center gap-1">
                          <CreditCard size={14} /> Partially Paid
                        </span>
                      ) : (
                        <span className="bg-yellow-100 text-yellow-800 px-2 py-1 rounded text-xs flex items-center gap-1">
                          <CreditCard size={14} /> Not Paid
                        </span>
                      )}
                    </div>
                  </div>
                </div>
                {/* Action buttons */}
                <div className="flex gap-4 self-end md:self-center">
                  <button
                    className="px-6 py-2 rounded-lg bg-blue-700 text-white font-bold hover:bg-blue-800 shadow transition"
                    onClick={() => onDetails(appt)}
                  >
                    Details
                  </button>
                  <button
                    className={`px-6 py-2 rounded-lg font-bold shadow transition text-white ${
                      appt.paymentStatus === "paid"
                        ? "bg-gray-300 cursor-not-allowed"
                        : "bg-green-600 hover:bg-green-700"
                    }`}
                    onClick={() => navigate("/user/wallet")}
                    disabled={appt.paymentStatus === "paid"}
                  >
                    Pay
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* PREVIOUS */}
        <h1 className="text-2xl font-bold text-blue-700 mb-4">
          Previous Appointments
        </h1>
        {previous.length === 0 ? (
          <div className="text-center text-gray-400 text-lg">
            No previous appointments.
          </div>
        ) : (
          <div className="flex flex-col gap-7">
            {previous.map((appt) => (
              <div
                key={appt.id}
                className="flex flex-col md:flex-row md:items-center md:justify-between gap-6 bg-gray-100 rounded-xl shadow-sm p-7 opacity-70"
              >
                <div className="flex-1 flex flex-col sm:flex-row gap-8 items-start sm:items-center">
                  <div>
                    <div className="text-xl font-bold text-blue-700 flex gap-3 items-center">
                      {new Date(appt.date).toLocaleDateString()}{" "}
                      <span className="text-gray-500 font-normal text-base">
                        {appt.time}
                      </span>
                    </div>
                    <div className="text-lg text-gray-800">
                      Doctor:{" "}
                      <span className="font-semibold">{appt.doctor}</span>
                    </div>
                    {appt.specialization && (
                      <div className="text-sm text-gray-500">
                        Specialization: {appt.specialization}
                      </div>
                    )}
                    <div className="mt-3">
                      <span
                        className={`px-2 py-1 rounded text-xs font-semibold 
                        ${
                          appt.paymentStatus === "paid"
                            ? "bg-green-100 text-green-700"
                            : appt.paymentStatus === "partially_paid"
                            ? "bg-yellow-100 text-yellow-600"
                            : "bg-red-100 text-red-600"
                        }`}
                      >
                        Paid: {appt.paid} / {appt.total} PLN
                      </span>
                    </div>
                  </div>
                  <div className="flex flex-col gap-2">
                    <div className="flex items-center gap-2">
                      {appt.status === "cancelled" ? (
                        <span className="bg-red-100 text-red-700 px-2 py-1 rounded text-xs flex items-center gap-1">
                          <XCircle size={14} /> Cancelled
                        </span>
                      ) : (
                        <span className="bg-gray-100 text-gray-700 px-2 py-1 rounded text-xs flex items-center gap-1">
                          <Info size={14} /> {appt.status}
                        </span>
                      )}
                    </div>
                    <div className="flex items-center gap-2">
                      {appt.paymentStatus === "paid" ? (
                        <span className="bg-green-200 text-green-800 px-2 py-1 rounded text-xs flex items-center gap-1">
                          <CreditCard size={14} /> Paid
                        </span>
                      ) : appt.paymentStatus === "partially_paid" ? (
                        <span className="bg-yellow-100 text-yellow-800 px-2 py-1 rounded text-xs flex items-center gap-1">
                          <CreditCard size={14} /> Partially Paid
                        </span>
                      ) : (
                        <span className="bg-yellow-100 text-yellow-800 px-2 py-1 rounded text-xs flex items-center gap-1">
                          <CreditCard size={14} /> Not Paid
                        </span>
                      )}
                    </div>
                  </div>
                </div>
                <div className="flex gap-4 self-end md:self-center">
                  <button
                    className="px-6 py-2 rounded-lg bg-emerald-100 text-emerald-700 font-bold hover:bg-emerald-200 shadow transition"
                    onClick={() =>
                      navigate(`/documents?appointmentId=${appt.id}`)
                    }
                  >
                    Documents
                  </button>
                  <button
                    className="px-6 py-2 rounded-lg bg-blue-200 text-blue-800 font-bold hover:bg-blue-300 shadow transition"
                    onClick={() => onDetails(appt)}
                  >
                    Details
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
