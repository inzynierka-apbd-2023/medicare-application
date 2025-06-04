import { X, Calendar, User, CreditCard, Stethoscope } from "lucide-react";
import { Appointment } from "./Appointments";

type Props = {
  open: boolean;
  appointment?: Appointment;
  onClose: () => void;
};

export default function AppointmentDetailsModal({
  open,
  appointment,
  onClose,
}: Props) {
  if (!open || !appointment) return null;

  return (
    <div className="fixed inset-0 flex items-center justify-center bg-black/40 z-50">
      <div className="bg-white text-gray-900 rounded-2xl shadow-2xl p-8 w-full max-w-lg relative animate-fadeIn">
        <button
          className="absolute top-4 right-4 text-blue-200 hover:text-blue-600 transition"
          onClick={onClose}
          aria-label="Close"
        >
          <X size={28} />
        </button>
        <div className="mb-6 flex items-center gap-2">
          <Calendar className="text-blue-600" size={28} />
          <h2 className="text-2xl font-bold text-blue-800">
            Appointment Details
          </h2>
        </div>
        <div className="flex flex-col gap-3 text-lg">
          <div className="flex items-center gap-2">
            <Calendar size={18} className="text-blue-400" />
            <span>
              <span className="font-semibold">Date:</span>{" "}
              {new Date(appointment.date).toLocaleDateString()} at{" "}
              {appointment.time}
            </span>
          </div>
          <div className="flex items-center gap-2">
            <User size={18} className="text-blue-400" />
            <span>
              <span className="font-semibold">Doctor:</span>{" "}
              {appointment.doctor}
            </span>
          </div>
          {appointment.specialization && (
            <div className="flex items-center gap-2">
              <Stethoscope size={18} className="text-blue-400" />
              <span>
                <span className="font-semibold">Specialization:</span>{" "}
                {appointment.specialization}
              </span>
            </div>
          )}
          <div className="flex items-center gap-2">
            <CreditCard size={18} className="text-blue-400" />
            <span>
              <span className="font-semibold">Payment:</span> Paid:{" "}
              {appointment.paid} / {appointment.total} PLN
            </span>
          </div>
          <div>
            <span className="font-semibold">Status:</span>{" "}
            {appointment.status === "upcoming" && (
              <span className="text-green-700 font-semibold">Upcoming</span>
            )}
            {appointment.status === "past" && (
              <span className="text-gray-700 font-semibold">Completed</span>
            )}
            {appointment.status === "cancelled" && (
              <span className="text-red-600 font-semibold">Cancelled</span>
            )}
          </div>
          <div>
            <span className="font-semibold">Visit Purpose:</span>
            <div className="bg-blue-50 rounded-lg mt-2 px-3 py-2 text-base text-gray-700">
              {appointment.description}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
