import React, { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { TestTube } from "lucide-react";

import Header from "../../layout/Header";
import {
  EmptyState,
  LoadingOverlay,
  SearchInput,
} from "../../shared/components";
import { useDocuments } from "../../shared/hooks/useDocuments";
import { DocumentCard } from "../documents/components/DocumentCard";
import { Document } from "../documents/types";

export const LabResultsPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<
    "All" | "Normal" | "Abnormal" | "Critical" | "Pending"
  >("All");
  const [sortBy, setSortBy] = useState<"date" | "status" | "type">("date");

  const { documents, isLoading, error } = useDocuments();

  // Filter tylko dokumenty Lab_Results
  const labResults = useMemo(() => {
    return documents
      .filter((doc) => doc.type === "Lab_Results")
      .filter((doc) => {
        const matchesSearch =
          searchTerm === "" ||
          doc.data.testType?.toLowerCase().includes(searchTerm.toLowerCase()) ||
          doc.data.laboratory
            ?.toLowerCase()
            .includes(searchTerm.toLowerCase()) ||
          doc.notes?.toLowerCase().includes(searchTerm.toLowerCase());

        const matchesStatus =
          statusFilter === "All" || doc.data.status === statusFilter;

        return matchesSearch && matchesStatus;
      })
      .sort((a, b) => {
        switch (sortBy) {
          case "date":
            return (
              new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
            );
          case "status": {
            const statusOrder = {
              Critical: 0,
              Abnormal: 1,
              Pending: 2,
              Normal: 3,
            };
            return (
              (statusOrder[a.data.status as keyof typeof statusOrder] || 4) -
              (statusOrder[b.data.status as keyof typeof statusOrder] || 4)
            );
          }
          case "type":
            return (a.data.testType || "").localeCompare(b.data.testType || "");
          default:
            return 0;
        }
      });
  }, [documents, searchTerm, statusFilter, sortBy]);

  const handleDocumentClick = (document: Document) => {
    navigate(`/lab-results/${document.id}`);
  };

  // Statystyki
  const stats = useMemo(() => {
    const total = labResults.length;
    const normal = labResults.filter(
      (doc) => doc.data.status === "Normal"
    ).length;
    const abnormal = labResults.filter(
      (doc) => doc.data.status === "Abnormal"
    ).length;
    const critical = labResults.filter(
      (doc) => doc.data.status === "Critical"
    ).length;
    const pending = labResults.filter(
      (doc) => doc.data.status === "Pending"
    ).length;

    return { total, normal, abnormal, critical, pending };
  }, [labResults]);

  if (isLoading) {
    return (
      <LoadingOverlay isLoading={true}>
        <div className="min-h-screen bg-gray-100">
          <Header />
          <div className="pt-20 max-w-7xl mx-auto px-4 py-8">
            <div className="text-center">Loading laboratory results...</div>
          </div>
        </div>
      </LoadingOverlay>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <div className="pt-20 max-w-7xl mx-auto px-4 py-8">
          <div className="text-center">
            <p className="text-red-600">
              Error loading laboratory results: {error}
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      <Header />
      <div className="pt-20 max-w-7xl mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-8">
          <div className="flex items-center space-x-3 mb-4">
            <TestTube className="h-8 w-8 text-purple-600" />
            <h1 className="text-3xl font-bold text-gray-900">
              Laboratory Results
            </h1>
          </div>
          <p className="text-gray-600">
            View and analyze your laboratory test results
          </p>
        </div>

        {/* Statistics */}
        <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mb-8">
          <div className="bg-white rounded-lg border border-gray-200 p-4 text-center">
            <div className="text-2xl font-bold text-gray-900">
              {stats.total}
            </div>
            <div className="text-sm text-gray-600">Total</div>
          </div>
          <div className="bg-white rounded-lg border border-gray-200 p-4 text-center">
            <div className="text-2xl font-bold text-green-600">
              {stats.normal}
            </div>
            <div className="text-sm text-gray-600">Normal</div>
          </div>
          <div className="bg-white rounded-lg border border-gray-200 p-4 text-center">
            <div className="text-2xl font-bold text-yellow-600">
              {stats.abnormal}
            </div>
            <div className="text-sm text-gray-600">Abnormal</div>
          </div>
          <div className="bg-white rounded-lg border border-gray-200 p-4 text-center">
            <div className="text-2xl font-bold text-red-600">
              {stats.critical}
            </div>
            <div className="text-sm text-gray-600">Critical</div>
          </div>
          <div className="bg-white rounded-lg border border-gray-200 p-4 text-center">
            <div className="text-2xl font-bold text-blue-600">
              {stats.pending}
            </div>
            <div className="text-sm text-gray-600">Pending</div>
          </div>
        </div>

        {/* Filters */}
        <div className="bg-white rounded-lg border border-gray-200 p-6 mb-8">
          <div className="flex flex-wrap gap-4 items-center">
            <SearchInput
              placeholder="Search by test type, laboratory or notes..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-80 text-left"
            />

            <select
              className="px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-purple-200 text-sm"
              value={statusFilter}
              onChange={(e) =>
                setStatusFilter(e.target.value as typeof statusFilter)
              }
            >
              <option value="All">All statuses</option>
              <option value="Normal">Normal</option>
              <option value="Abnormal">Abnormal</option>
              <option value="Critical">Critical</option>
              <option value="Pending">Pending</option>
            </select>

            <select
              className="px-3 py-2 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-purple-200 text-sm"
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value as typeof sortBy)}
            >
              <option value="date">Sort by date</option>
              <option value="status">Sort by status</option>
              <option value="type">Sort by test type</option>
            </select>
          </div>
        </div>

        {/* Results */}
        {labResults.length === 0 ? (
          <EmptyState
            icon={<TestTube className="h-16 w-16 text-gray-400" />}
            title="No lab results found"
            description={
              searchTerm || statusFilter !== "All"
                ? "No lab results match your search criteria."
                : "You don't have any laboratory results yet."
            }
            className="bg-white rounded-lg border border-gray-200"
          />
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {labResults.map((document) => (
              <DocumentCard
                key={document.id}
                document={document}
                onClick={() => handleDocumentClick(document)}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};
