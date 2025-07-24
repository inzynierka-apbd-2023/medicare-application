import React, { useState, useEffect } from "react";
import Header from "../../layout/Header";

export default function SchedulePage() {
  const data = {
    services: [
      {
        id: "service_general_consultation",
        name: "General Consultation",
        specializationId: "spec_general_medicine",
        doctorIds: ["doc_alice_heart", "doc_bob_vessel"],
      },
      {
        id: "service_blood_test",
        name: "Blood Test",
        specializationId: "spec_lab_services",
        doctorIds: ["doc_carol_serum"],
      },
      {
        id: "service_teleconsultation",
        name: "Teleconsultation",
        specializationId: "spec_general_medicine",
        doctorIds: ["doc_alice_heart"],
      },
      {
        id: "service_prescription_renewal",
        name: "Prescription Renewal",
        specializationId: "spec_pharmacy",
        doctorIds: ["doc_deborah_dose"],
      },
    ],
    specializations: [
      {
        id: "spec_general_medicine",
        name: "General Medicine",
        serviceIds: [
          "service_general_consultation",
          "service_teleconsultation",
        ],
        doctorIds: ["doc_alice_heart", "doc_bob_vessel"],
      },
      {
        id: "spec_lab_services",
        name: "Lab Services",
        serviceIds: ["service_blood_test"],
        doctorIds: ["doc_carol_serum"],
      },
      {
        id: "spec_pharmacy",
        name: "Pharmacy",
        serviceIds: ["service_prescription_renewal"],
        doctorIds: ["doc_deborah_dose"],
      },
    ],
    doctors: [
      { id: "doc_alice_heart", name: "Dr. Alice Heart" },
      { id: "doc_bob_vessel", name: "Dr. Bob Vessel" },
      { id: "doc_carol_serum", name: "Dr. Carol Serum" },
      { id: "doc_deborah_dose", name: "Dr. Deborah Dose" },
    ],
  };

  // State for selectors
  const [selectedService, setSelectedService] = useState("");
  const [selectedSpec, setSelectedSpec] = useState("");
  const [selectedDoctor, setSelectedDoctor] = useState("");
  const [serviceOptions, setServiceOptions] = useState(
    data.services.map((s) => s.id)
  );
  const [specOptions, setSpecOptions] = useState(
    data.specializations.map((s) => s.id)
  );
  const [doctorOptions, setDoctorOptions] = useState(
    data.doctors.map((d) => d.id)
  );

  useEffect(() => {
    if (selectedDoctor) {
      setServiceOptions([]);
      setSpecOptions([]);
    } else if (selectedService) {
      const svc = data.services.find((s) => s.id === selectedService);
      if (svc) {
        setSelectedSpec(svc.specializationId);
        setServiceOptions([svc.id]);
        setSpecOptions([svc.specializationId]);
        setDoctorOptions(svc.doctorIds);
      }
    } else if (selectedSpec) {
      const spec = data.specializations.find((sp) => sp.id === selectedSpec);
      if (spec) {
        setServiceOptions(spec.serviceIds);
        setSpecOptions([spec.id]);
        setDoctorOptions(spec.doctorIds);
      }
    } else {
      setServiceOptions(data.services.map((s) => s.id));
      setSpecOptions(data.specializations.map((sp) => sp.id));
      setDoctorOptions(data.doctors.map((d) => d.id));
    }
  }, [selectedService, selectedSpec, selectedDoctor]);

  // Name resolvers
  const getServiceName = (id) =>
    data.services.find((s) => s.id === id)?.name || id;
  const getSpecName = (id) =>
    data.specializations.find((sp) => sp.id === id)?.name || id;
  const getDoctorName = (id) =>
    data.doctors.find((d) => d.id === id)?.name || id;

  return (
    <div className="min-h-screen bg-gray-100 overflow-x-hidden">
      <Header />

      <div className="container mx-auto max-w-[160rem] px-4 sm:px-6 lg:px-8">
        <main className="pt-24 pb-10">
          <h1 className="text-3xl font-bold text-blue-700 mb-8">
            Schedule Appointment
          </h1>

          <div className="flex justify-center">
            <div className="w-3/4 bg-white rounded-2xl shadow-md p-6">
              {/* Selectors Row */}
              <div className="flex flex-col md:flex-row md:space-x-4 space-y-4 md:space-y-0 mb-6">
                <select
                  value={selectedService}
                  onChange={(e) => {
                    setSelectedService(e.target.value);
                    setSelectedDoctor("");
                  }}
                  disabled={!!selectedDoctor}
                  className="flex-1 p-2 border rounded-lg"
                >
                  <option value="">Select Service</option>
                  {serviceOptions.map((id) => (
                    <option key={id} value={id}>
                      {getServiceName(id)}
                    </option>
                  ))}
                </select>

                <select
                  value={selectedSpec}
                  onChange={(e) => {
                    setSelectedSpec(e.target.value);
                    setSelectedService("");
                    setSelectedDoctor("");
                  }}
                  disabled={!!selectedDoctor}
                  className="flex-1 p-2 border rounded-lg"
                >
                  <option value="">Select Specialization</option>
                  {specOptions.map((id) => (
                    <option key={id} value={id}>
                      {getSpecName(id)}
                    </option>
                  ))}
                </select>

                <select
                  value={selectedDoctor}
                  onChange={(e) => setSelectedDoctor(e.target.value)}
                  className="flex-1 p-2 border rounded-lg"
                >
                  <option value="">Select Doctor</option>
                  {doctorOptions.map((id) => (
                    <option key={id} value={id}>
                      {getDoctorName(id)}
                    </option>
                  ))}
                </select>
              </div>

              <div className="h-[600px] bg-blue-50 rounded-lg flex items-center justify-center text-blue-300">
                <span>Scheduler Placeholder</span>
              </div>
            </div>
          </div>
        </main>
      </div>
    </div>
  );
}
