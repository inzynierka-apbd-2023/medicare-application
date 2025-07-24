import { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Search,
  Calendar,
  FileText,
  MessageCircle,
  ClipboardList,
  PlusCircle,
} from "lucide-react";
import Header from "../../layout/Header";

export default function PatientList() {
  const navigate = useNavigate();

  // Demo data for assigned patients
  const [patients] = useState([
    {
      id: 1,
      name: "John Doe",
      age: 45,
      gender: "Male",
      lastVisit: "2025-05-18",
      visits: 4,
      notes: "High cholesterol, regular check-ups.",
    },
    {
      id: 2,
      name: "Maria Smith",
      age: 33,
      gender: "Female",
      lastVisit: "2025-05-12",
      visits: 2,
      notes: "Post-surgery recovery.",
    },
    {
      id: 3,
      name: "Adam Nowak",
      age: 52,
      gender: "Male",
      lastVisit: "2025-04-29",
      visits: 8,
      notes: "Diabetic, hypertension.",
    },
    {
      id: 4,
      name: "Paulina Zielińska",
      age: 29,
      gender: "Female",
      lastVisit: "2025-03-10",
      visits: 1,
      notes: "",
    },
  ]);
  const [search, setSearch] = useState("");
  const [sortKey, setSortKey] = useState("name");

  // Filter and sort patients
  const filtered = patients
    .filter(
      (p) =>
        p.name.toLowerCase().includes(search.toLowerCase()) ||
        p.notes.toLowerCase().includes(search.toLowerCase())
    )
    .sort((a, b) => {
      if (sortKey === "name") return a.name.localeCompare(b.name);
      if (sortKey === "lastVisit")
        return b.lastVisit.localeCompare(a.lastVisit);
      if (sortKey === "visits") return b.visits - a.visits;
      return 0;
    });

  return (
    <div className="min-h-screen bg-gray-100 pt-24 px-8 pb-10">
      <Header />
      <div className="max-w-5xl mx-auto">
        <h1 className="text-3xl font-bold text-blue-700 mb-8">Your Patients</h1>
        {/* Controls */}
        <div className="flex flex-col md:flex-row md:items-center md:space-x-4 mb-6 space-y-4 md:space-y-0">
          <div className="flex-1 flex items-center bg-white rounded-xl px-3 py-2 shadow-sm">
            <Search className="text-blue-400 mr-2" size={18} />
            <input
              type="text"
              placeholder="Search patients..."
              className="outline-none w-full bg-transparent text-sm text-gray-800"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          <div>
            <select
              className="bg-white border px-3 py-2 rounded-xl text-sm text-gray-800 shadow-sm"
              value={sortKey}
              onChange={(e) => setSortKey(e.target.value)}
            >
              <option value="name">Sort: Name</option>
              <option value="lastVisit">Sort: Last Visit</option>
              <option value="visits">Sort: Total Visits</option>
            </select>
          </div>
        </div>
        {/* Patient List Table */}
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
              {filtered.length === 0 ? (
                <tr>
                  <td colSpan={7} className="py-8 text-center text-gray-800">
                    No patients found.
                  </td>
                </tr>
              ) : (
                filtered.map((patient) => (
                  <tr key={patient.id} className="hover:bg-blue-50 transition">
                    <td className="py-3 px-4 font-medium text-gray-800">
                      {patient.name}
                    </td>
                    <td className="py-3 px-4 text-gray-800">{patient.age}</td>
                    <td className="py-3 px-4 text-gray-800">
                      {patient.gender}
                    </td>
                    <td className="py-3 px-4 text-gray-800">
                      {patient.lastVisit}
                    </td>
                    <td className="py-3 px-4 text-gray-800">
                      {patient.visits}
                    </td>
                    <td className="py-3 px-4 max-w-xs text-sm text-gray-800">
                      {patient.notes || (
                        <span className="text-gray-500">No notes</span>
                      )}
                    </td>
                    <td className="py-3 px-4 flex flex-wrap gap-2">
                      {/* View appointments history with this patient */}
                      <button
                        title="View appointments"
                        className="p-2 rounded-lg bg-blue-100 hover:bg-blue-200 text-blue-700"
                        onClick={() =>
                          navigate(`/appointments?patientId=${patient.id}`)
                        }
                      >
                        <Calendar size={16} />
                      </button>
                      {/* View medical records */}
                      <button
                        title="View medical records"
                        className="p-2 rounded-lg bg-green-100 hover:bg-green-200 text-green-700"
                        onClick={() =>
                          navigate(`/medical-records?patientId=${patient.id}`)
                        }
                      >
                        <FileText size={16} />
                      </button>
                      {/* Write prescription */}
                      <button
                        title="Write new prescription"
                        className="p-2 rounded-lg bg-purple-100 hover:bg-purple-200 text-purple-700"
                        onClick={() =>
                          navigate(`/prescriptions/new?patientId=${patient.id}`)
                        }
                      >
                        <PlusCircle size={16} />
                      </button>
                      {/* Send message */}
                      <button
                        title="Send message"
                        className="p-2 rounded-lg bg-yellow-100 hover:bg-yellow-200 text-yellow-700"
                        onClick={() =>
                          navigate(`/messages?patientId=${patient.id}`)
                        }
                      >
                        <MessageCircle size={16} />
                      </button>
                      {/* Notes */}
                      <button
                        title="See/add notes"
                        className="p-2 rounded-lg bg-gray-200 hover:bg-gray-300 text-gray-800"
                        onClick={() =>
                          navigate(`/notes?patientId=${patient.id}`)
                        }
                      >
                        <ClipboardList size={16} />
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
