import { useState, useEffect } from "react";
import { useNavigate } from 'react-router-dom';
import Header from "../Header";
import { X } from "lucide-react";

export default function PatientDashboard() {
  const navigate = useNavigate();

  const data = {
    services: [
      {
        id: "service_general_consultation",
        name: "General Consultation",
        specializationId: "spec_general_medicine",
        doctorIds: ["doc_alice_heart", "doc_bob_vessel"]
      },
      {
        id: "service_blood_test",
        name: "Blood Test",
        specializationId: "spec_lab_services",
        doctorIds: ["doc_carol_serum"]
      },
      {
        id: "service_teleconsultation",
        name: "Teleconsultation",
        specializationId: "spec_general_medicine",
        doctorIds: ["doc_alice_heart"]
      },
      {
        id: "service_prescription_renewal",
        name: "Prescription Renewal",
        specializationId: "spec_pharmacy",
        doctorIds: ["doc_deborah_dose"]
      }
    ],
    specializations: [
      {
        id: "spec_general_medicine",
        name: "General Medicine",
        serviceIds: ["service_general_consultation", "service_teleconsultation"],
        doctorIds: ["doc_alice_heart", "doc_bob_vessel"]
      },
      {
        id: "spec_lab_services",
        name: "Lab Services",
        serviceIds: ["service_blood_test"],
        doctorIds: ["doc_carol_serum"]
      },
      {
        id: "spec_pharmacy",
        name: "Pharmacy",
        serviceIds: ["service_prescription_renewal"],
        doctorIds: ["doc_deborah_dose"]
      }
    ],
    doctors: [
      { id: "doc_alice_heart", name: "Dr. Alice Heart" },
      { id: "doc_bob_vessel", name: "Dr. Bob Vessel" },
      { id: "doc_carol_serum", name: "Dr. Carol Serum" },
      { id: "doc_deborah_dose", name: "Dr. Deborah Dose" }
    ]
  };

  const [showNotifications, setShowNotifications] = useState(false);
  const [closing, setClosing] = useState(false);
  const [selectedService, setSelectedService] = useState("");
  const [selectedSpec, setSelectedSpec] = useState("");
  const [selectedDoctor, setSelectedDoctor] = useState("");
  const [serviceOptions, setServiceOptions] = useState(data.services.map(s => s.id));
  const [specOptions, setSpecOptions] = useState(data.specializations.map(s => s.id));
  const [doctorOptions, setDoctorOptions] = useState(data.doctors.map(d => d.id));

  const notifications = [
    "Your appointment with Dr. Alice Heart is tomorrow at 10:00 AM.",
    "Lab results from your blood test are available.",
    "Reminder: Teleconsultation on May 20, 2025 at 3:00 PM.",
    "Prescription #456 has been renewed.",
    "New message from Dr. Bob Vessel regarding your test."
  ];

  const openModal = () => {
    setShowNotifications(true);
    setClosing(false);
  };

  const closeModal = () => {
    setClosing(true);
    setTimeout(() => {
      setShowNotifications(false);
      setClosing(false);
    }, 150);
  };

  useEffect(() => {
    if (selectedDoctor) {
      setServiceOptions([]);
      setSpecOptions([]);
    } else if (selectedService) {
      const svc = data.services.find(s => s.id === selectedService);
      if (svc) {
        setSelectedSpec(svc.specializationId);
        setServiceOptions([svc.id]);
        setSpecOptions([svc.specializationId]);
        setDoctorOptions(svc.doctorIds);
      }
    } else if (selectedSpec) {
      const spec = data.specializations.find(sp => sp.id === selectedSpec);
      if (spec) {
        setServiceOptions(spec.serviceIds);
        setSpecOptions([spec.id]);
        setDoctorOptions(spec.doctorIds);
      }
    } else {
      setServiceOptions(data.services.map(s => s.id));
      setSpecOptions(data.specializations.map(sp => sp.id));
      setDoctorOptions(data.doctors.map(d => d.id));
    }
  }, [selectedService, selectedSpec, selectedDoctor]);

  // Helpers to resolve names from ids
  const getServiceName = id => data.services.find(s => s.id === id)?.name || id;
  const getSpecName = id => data.specializations.find(sp => sp.id === id)?.name || id;
  const getDoctorName = id => data.doctors.find(d => d.id === id)?.name || id;

  return (
    <div className="min-h-screen bg-gray-100 overflow-x-hidden">
      <Header />
      <div className="container mx-auto max-w-[160rem] px-4 sm:px-6 lg:px-8">
        <main className="pt-24 pb-10">
          <h1 className="text-3xl font-bold text-blue-700 mb-8">Welcome, Patient</h1>
          <div className="flex space-x-6">

            {/* Left Column (75%) */}
            <div className="w-3/4 space-y-6">
              {/* Linked selects */}
              <div className="bg-white rounded-2xl shadow-md p-3">
                <h2 className="text-xl font-semibold text-blue-600 text-center">Your Schedule</h2>
              </div>

              {/* Scheduler */}
              <div className="bg-white rounded-2xl shadow-md p-6 h-[600px] flex flex-col">
                <div className="flex-1 bg-blue-50 rounded-lg flex items-center justify-center text-blue-300">
                  Scheduler Placeholder
                </div>
              </div>


              {/* Available Services (Buttons) */}
              <div className="bg-white rounded-2xl shadow-md p-3">
                <div className="flex justify-center space-x-4">
                  <button className="px-4 py-1 bg-blue-500 hover:bg-blue-600 text-white rounded-1g">Make Appointment</button>
                </div>
              </div>

              {/* Upcoming appointments */}
              <div className="bg-white rounded-2xl shadow-md p-6">
                <h3 className="text-lg font-semibold text-blue-600 mb-2">Upcoming appointments </h3>
                <ul className="list-disc list-inside text-left space-y-1 text-sm text-gray-700">
                  {data.specializations.map(sp => (
                    <li key={sp.id}>{sp.name} - {sp.doctorIds.map(getDoctorName).join(", ")}</li>
                  ))}
                </ul>
              </div>
            </div>

            {/* Right Column (25%) */}
            <div className="w-1/4 space-y-6">
              {/* Notifications Section */}
              <div className="bg-white rounded-2xl shadow-md p-6">
                <h2 className="text-xl font-semibold text-blue-600 mb-4">Notifications</h2>
                <ul className="space-y-2 list-disc list-inside text-left">
                  <li className="text-sm text-gray-600">Appointment Reminder: May 14, 2025 at 10:00 AM with Dr. Alice Heart</li>
                  <li className="text-sm text-gray-600">Lab Results Available: Cholesterol Panel</li>
                  <li className="text-sm text-gray-600">New Message: Follow-up from Dr. Bob Vessel</li>
                </ul>
                <button
                  onClick={openModal}
                  className="mt-4 w-full px-4 py-2 bg-blue-100 text-blue-700 rounded-lg hover:bg-blue-200 transition duration-150"
                >
                  View All Notifications
                </button>
              </div>

              {/* Recent Documents Section */}
              <div className="bg-white rounded-2xl shadow-md p-6">
                <h2 className="text-xl font-semibold text-blue-600 mb-2">Recent Documents</h2>
                <ul className="list-disc list-inside text-left space-y-2 text-sm text-gray-700">
                  <li>Prescription #456 issued on May 10, 2025</li>
                  <li>Referral to Cardiologist on April 22, 2025</li>
                  <li>Blood Test Results on March 15, 2025</li>
                </ul>
                <button
                  onClick={() => navigate('/schedule')}
                  className="mt-4 w-full px-4 py-2 bg-blue-100 text-blue-700 rounded-lg hover:bg-blue-200 transition duration-150"
                >
                  View All Documents
                </button>
              </div>
            </div>

          </div>
        </main>
      </div>

      {/* Notifications Modal Overlay */}
      {showNotifications && (
        <>
          <div
            className={`fixed inset-0 bg-black bg-opacity-50 z-50 transition-opacity duration-150 ease-out ${
              closing ? "opacity-0" : "opacity-100"
            }`}
            onClick={closeModal}
          />
          <div className="fixed inset-0 flex items-center justify-center z-50">
            <div
              className={`${
                closing ? "animate-scale-out" : "animate-scale-in"
              } bg-white rounded-2xl shadow-lg p-6 relative w-full md:w-3/4 lg:w-2/3 xl:w-1/2`}
            >
              <button
                className="absolute top-4 right-4 text-blue-300 hover:text-blue-400 transition duration-150"
                onClick={closeModal}
              >
                <X size={16} />
              </button>
              <h2 className="text-3xl font-semibold text-blue-600 mb-4">All Notifications</h2>
              <ul className="space-y-3 max-h-80 overflow-y-auto list-disc list-inside text-left">
                {notifications.map((note, idx) => (
                  <li key={idx} className="text-base text-gray-700">
                    {note}
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
