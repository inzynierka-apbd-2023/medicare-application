import { useState } from "react";
import Appointments, { Appointment } from "./Appointments";
import AppointmentDetailsModal from "./AppointmentsDetailsModal";

export default function AppointmentsPage() {
  const [appointments] = useState<Appointment[]>([
    {
      id: "appt1",
      date: "2025-06-10",
      time: "14:30",
      doctor: "Dr. Anna Nowak",
      specialization: "Cardiology",
      description:
        "Control visit to assess blood pressure regulation and discuss medication adjustment. Bring home blood pressure diary.",
      status: "upcoming",
      paymentStatus: "not_paid",
      paid: 0,
      total: 150,
    },
    {
      id: "appt2",
      date: "2025-05-10",
      time: "10:00",
      doctor: "Dr. Bob Vessel",
      specialization: "Dermatology",
      description:
        "Annual skin screening: Full-body mole and lesion check, including dermatoscopy.",
      status: "past",
      paymentStatus: "paid",
      paid: 200,
      total: 200,
    },
    {
      id: "appt3",
      date: "2025-06-15",
      time: "11:00",
      doctor: "Dr. Anna Nowak",
      specialization: "Cardiology",
      description:
        "Quarterly review of chronic cardiac condition, ECG control, bloodwork review.",
      status: "upcoming",
      paymentStatus: "partially_paid",
      paid: 50,
      total: 120,
    },
  ]);

  const [showDetails, setShowDetails] = useState(false);
  const [selectedAppointment, setSelectedAppointment] =
    useState<Appointment | null>(null);

  return (
    <>
      <Appointments
        appointments={appointments}
        onDetails={(appt) => {
          setSelectedAppointment(appt);
          setShowDetails(true);
        }}
      />
      <AppointmentDetailsModal
        open={showDetails}
        appointment={selectedAppointment ?? undefined}
        onClose={() => setShowDetails(false)}
      />
    </>
  );
}
