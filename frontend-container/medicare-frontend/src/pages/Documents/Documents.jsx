import { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import {
  FileText,
  FileSignature,
  FilePlus2,
  Stethoscope,
  X,
} from "lucide-react";
import Header from "../Header";

const docTypeInfo = {
  Prescription: {
    icon: <FileText className="inline mr-2" />,
    color: "text-emerald-600",
  },
  Referral: {
    icon: <FileSignature className="inline mr-2" />,
    color: "text-indigo-600",
  },
  Sick_Leave: {
    icon: <FilePlus2 className="inline mr-2" />,
    color: "text-yellow-600",
  },
  VisitCard: {
    icon: <Stethoscope className="inline mr-2" />,
    color: "text-blue-600",
  },
  Other: { icon: <FileText className="inline mr-2" />, color: "text-gray-500" },
};

async function fetchDocuments() {
  return [
    {
      id: "d1",
      appointmentId: "appt2", // Link to appointment
      type: "Prescription",
      createdAt: "2025-05-10",
      notes: "Cholesterol meds",
      data: {
        medication: "Atorvastatin",
        dosage: "20mg",
        frequency: "1x daily",
        duration_days: 30,
        instructions: "Take after dinner",
      },
    },
    {
      id: "d42",
      appointmentId: "appt2",
      type: "Prescription",
      createdAt: "2025-05-10",
      notes: "Cholesterol meds renewed",
      data: {
        medication: "Atorvastatin",
        dosage: "20mg",
        frequency: "1x daily",
        duration_days: 30,
        instructions: "Take after dinner",
      },
    },
    {
      id: "d2",
      appointmentId: "appt2",
      type: "Referral",
      createdAt: "2025-04-22",
      notes: "Consult cardiologist",
      data: {
        specialty: "Cardiologist",
        referredTo: "Dr. Heart Strong",
        validFrom: "2025-04-22",
        validTo: "2025-06-01",
      },
    },
    {
      id: "d3",
      appointmentId: "appt1",
      type: "Sick_Leave",
      createdAt: "2025-03-15",
      notes: "Flu recovery",
      data: {
        startDate: "2025-03-15",
        endDate: "2025-03-22",
        daysOff: 8,
      },
    },
    {
      id: "d4",
      appointmentId: "appt1",
      type: "VisitCard",
      createdAt: "2025-03-15",
      notes: "First hypertension check",
      data: {
        symptoms: "Fatigue, high BP",
        findings: "Elevated BP",
        diagnosis: "Hypertension",
        recommendations: "Monitor BP daily, reduce salt",
      },
    },
  ];
}

const APPOINTMENTS = [
  {
    id: "appt1",
    date: "2025-06-10",
    doctor: "Dr. Anna Nowak",
    specialization: "Cardiology",
  },
  {
    id: "appt2",
    date: "2025-05-10",
    doctor: "Dr. Bob Vessel",
    specialization: "Dermatology",
  },
  {
    id: "appt3",
    date: "2025-06-15",
    doctor: "Dr. Anna Nowak",
    specialization: "Cardiology",
  },
];

export default function DocumentsView() {
  const location = useLocation();
  const query = new URLSearchParams(location.search);
  const [documents, setDocuments] = useState([]);
  const [selectedDoc, setSelectedDoc] = useState(null);
  const [search, setSearch] = useState("");
  const [typeFilter, setTypeFilter] = useState("All");
  const [appointmentId, setAppointmentId] = useState(
    query.get("appointmentId") || ""
  );

  useEffect(() => {
    fetchDocuments().then(setDocuments);
  }, []);

  // Listen to URL changes for appointment filter (e.g. from Documents button)
  useEffect(() => {
    setAppointmentId(query.get("appointmentId") || "");
    // eslint-disable-next-line
  }, [location.search]);

  const filteredDocs = documents.filter(
    (doc) =>
      (typeFilter === "All" || doc.type === typeFilter) &&
      (!appointmentId || doc.appointmentId === appointmentId) &&
      ((doc.notes?.toLowerCase().includes(search.toLowerCase()) ?? false) ||
        (doc.type?.toLowerCase().includes(search.toLowerCase()) ?? false))
  );

  return (
    <div className="min-h-screen bg-gray-100 px-4 py-8">
      <Header />
      <div className="max-w-5xl mx-auto">
        <h1 className="text-3xl font-bold text-blue-700 mb-6">
          Your Medical Documents
        </h1>
        <div className="flex flex-wrap gap-4 mb-4 items-center">
          <input
            type="text"
            placeholder="Search documents..."
            className="px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-200 w-64"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          <select
            className="px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-200"
            value={typeFilter}
            onChange={(e) => setTypeFilter(e.target.value)}
          >
            <option value="All">All Types</option>
            {Object.keys(docTypeInfo)
              .filter((t) => t !== "Other")
              .map((t) => (
                <option key={t} value={t}>
                  {t.replace("_", " ")}
                </option>
              ))}
          </select>
          <select
            className="px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-200"
            value={appointmentId}
            onChange={(e) => setAppointmentId(e.target.value)}
          >
            <option value="">All Appointments</option>
            {APPOINTMENTS.map((appt) => (
              <option key={appt.id} value={appt.id}>
                {new Date(appt.date).toLocaleDateString()} –{" "}
                {appt.specialization} ({appt.doctor})
              </option>
            ))}
          </select>
        </div>

        {/* Document List */}
        <div className="grid md:grid-cols-2 gap-6">
          {filteredDocs.map((doc) => (
            <div key={doc.id} className="rounded-2xl shadow-md bg-white">
              <div className="py-6 px-4 flex flex-col gap-2">
                <div className="flex items-center justify-between">
                  <span
                    className={`text-lg font-semibold ${
                      docTypeInfo[doc.type]?.color
                    }`}
                  >
                    {docTypeInfo[doc.type]?.icon}
                    {doc.type.replace("_", " ")}
                  </span>
                  <span className="text-gray-400 text-xs">{doc.createdAt}</span>
                </div>
                {doc.notes && (
                  <div className="text-gray-700 text-sm mb-1">
                    <b>Notes:</b> {doc.notes}
                  </div>
                )}
                <button
                  className="mt-2 bg-blue-100 text-blue-700 px-4 py-2 rounded-lg hover:bg-blue-200 transition duration-150 w-fit"
                  onClick={() => setSelectedDoc(doc)}
                >
                  View Details
                </button>
              </div>
            </div>
          ))}
          {filteredDocs.length === 0 && (
            <div className="text-gray-500 col-span-2 text-center py-20">
              No documents found.
            </div>
          )}
        </div>
      </div>

      {/* Document Details Modal */}
      {selectedDoc && (
        <>
          <div
            className="fixed inset-0 bg-black bg-opacity-50 z-50"
            onClick={() => setSelectedDoc(null)}
          />
          <div className="fixed inset-0 flex items-center justify-center z-50">
            <div className="bg-white rounded-2xl shadow-xl p-8 relative w-full max-w-xl animate-scale-in">
              <button
                className="absolute top-4 right-4 text-blue-300 hover:text-blue-400"
                onClick={() => setSelectedDoc(null)}
              >
                <X size={20} />
              </button>
              <h2
                className={`text-2xl font-bold mb-4 ${
                  docTypeInfo[selectedDoc.type]?.color
                }`}
              >
                {docTypeInfo[selectedDoc.type]?.icon}
                {selectedDoc.type.replace("_", " ")} Details
              </h2>
              <div className="space-y-2">
                <div className="text-gray-700">
                  <b>Issued:</b> {selectedDoc.createdAt}
                </div>
                {selectedDoc.notes && (
                  <div className="text-gray-700">
                    <b>Notes:</b> {selectedDoc.notes}
                  </div>
                )}
                {renderDocumentDetails(selectedDoc)}
              </div>
              <button
                className="mt-6 w-full bg-blue-100 text-blue-700 px-4 py-2 rounded-lg hover:bg-blue-200 transition duration-150"
                onClick={() => alert("Download not implemented")}
              >
                Download PDF
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

// Helper to render document fields based on type
function renderDocumentDetails(doc) {
  switch (doc.type) {
    case "Prescription":
      return (
        <div className="space-y-1">
          <div>
            <b>Medication:</b> {doc.data.medication}
          </div>
          <div>
            <b>Dosage:</b> {doc.data.dosage}
          </div>
          <div>
            <b>Frequency:</b> {doc.data.frequency}
          </div>
          <div>
            <b>Duration (days):</b> {doc.data.duration_days}
          </div>
          <div>
            <b>Instructions:</b> {doc.data.instructions}
          </div>
        </div>
      );
    case "Referral":
      return (
        <div className="space-y-1">
          <div>
            <b>Specialty:</b> {doc.data.specialty}
          </div>
          <div>
            <b>Referred To:</b> {doc.data.referredTo}
          </div>
          <div>
            <b>Valid From:</b> {doc.data.validFrom}
          </div>
          <div>
            <b>Valid To:</b> {doc.data.validTo}
          </div>
        </div>
      );
    case "Sick_Leave":
      return (
        <div className="space-y-1">
          <div>
            <b>Start Date:</b> {doc.data.startDate}
          </div>
          <div>
            <b>End Date:</b> {doc.data.endDate}
          </div>
          <div>
            <b>Days Off:</b> {doc.data.daysOff}
          </div>
        </div>
      );
    case "VisitCard":
      return (
        <div className="space-y-1">
          <div>
            <b>Symptoms:</b> {doc.data.symptoms}
          </div>
          <div>
            <b>Findings:</b> {doc.data.findings}
          </div>
          <div>
            <b>Diagnosis:</b> {doc.data.diagnosis}
          </div>
          <div>
            <b>Recommendations:</b> {doc.data.recommendations}
          </div>
        </div>
      );
    default:
      return <div className="text-gray-500">No extra details available.</div>;
  }
}
