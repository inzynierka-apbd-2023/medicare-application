import React from "react";
import { useNavigate } from "react-router-dom";

import type { PatientAction, PatientTableProps } from "../types";

import { PatientActionButton } from "./PatientActionButton";

// Action routes with query parameter patterns
const actionConfigs: Record<
  PatientAction,
  { route: string; paramName: string }
> = {
  appointments: { route: "/doctor-scheduler", paramName: "patientId" },
  "medical-records": { route: "/medical-records", paramName: "patientId" },
  prescription: { route: "/prescriptions-management", paramName: "patientId" },
  message: { route: "/messages", paramName: "recipientId" }, // Opens chat with this patient
  notes: { route: "/notes", paramName: "patientId" },
};

export const PatientTable: React.FC<PatientTableProps> = ({ patients }) => {
  const navigate = useNavigate();

  const handleAction = (action: PatientAction, patientId: string) => {
    const config = actionConfigs[action];
    navigate(`${config.route}?${config.paramName}=${patientId}`);
  };

  if (patients.length === 0) {
    return (
      <div className="bg-white rounded-2xl shadow-lg p-8">
        <div className="text-center text-gray-800">No patients found.</div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-2xl shadow-lg overflow-x-auto">
      <table className="min-w-full divide-y divide-gray-100">
        <thead>
          <tr>
            <th className="py-3 px-4 text-left text-blue-600 font-semibold">
              Name
            </th>
            <th className="py-3 px-4 text-left text-blue-600 font-semibold">
              Age
            </th>
            <th className="py-3 px-4 text-left text-blue-600 font-semibold">
              Gender
            </th>
            <th className="py-3 px-4 text-left text-blue-600 font-semibold">
              Last Visit
            </th>
            <th className="py-3 px-4 text-left text-blue-600 font-semibold">
              Visits
            </th>
            <th className="py-3 px-4 text-left text-blue-600 font-semibold">
              Notes
            </th>
            <th className="py-3 px-4 text-left text-blue-600 font-semibold">
              Actions
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100">
          {patients.map((patient) => (
            <tr key={patient.id} className="hover:bg-blue-50 transition">
              <td className="py-3 px-4 font-medium text-gray-800">
                {patient.name}
              </td>
              <td className="py-3 px-4 text-gray-800">{patient.age}</td>
              <td className="py-3 px-4 text-gray-800">{patient.gender}</td>
              <td className="py-3 px-4 text-gray-800">
                {new Date(patient.lastVisit).toLocaleDateString()}
              </td>
              <td className="py-3 px-4 text-gray-800">{patient.visits}</td>
              <td className="py-3 px-4 max-w-xs text-sm text-gray-800">
                {patient.notes || (
                  <span className="text-gray-500">No notes</span>
                )}
              </td>
              <td className="py-3 px-4">
                <div className="flex flex-wrap gap-2">
                  {(
                    [
                      "appointments",
                      "medical-records",
                      "prescription",
                      "message",
                      "notes",
                    ] as PatientAction[]
                  ).map((action) => (
                    <PatientActionButton
                      key={action}
                      action={action}
                      patient={patient}
                      onClick={() => handleAction(action, patient.id)}
                    />
                  ))}
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
