import React from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ArrowLeft, Calendar, Download, MapPin } from "lucide-react";

import Header from "../../layout/Header";
import { Badge, Button, LoadingOverlay } from "../../shared/components";
import { useDocuments } from "../../shared/hooks/useDocuments";
import { LabResultsView } from "../documents/components/LabResultsView";

export const LabResultDetailPage: React.FC = () => {
  const { documentId } = useParams<{ documentId: string }>();
  const navigate = useNavigate();
  const { documents, isLoading, error, downloadDocument } = useDocuments();

  const document = documents.find(
    (doc) => doc.id === documentId && doc.type === "Lab_Results"
  );

  const handleDownload = () => {
    if (document) {
      downloadDocument(document);
    }
  };

  const handleGoBack = () => {
    navigate(-1);
  };

  if (isLoading) {
    return (
      <LoadingOverlay isLoading={true}>
        <div className="min-h-screen bg-gray-100">
          <Header />
          <div className="pt-20 max-w-7xl mx-auto px-4 py-8">
            <div className="text-center">Loading lab result details...</div>
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
            <p className="text-red-600">Error loading lab result: {error}</p>
            <Button onClick={handleGoBack} className="mt-4">
              Go Back
            </Button>
          </div>
        </div>
      </div>
    );
  }

  if (!document) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <div className="pt-20 max-w-7xl mx-auto px-4 py-8">
          <div className="text-center">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">
              Lab Result Not Found
            </h2>
            <p className="text-gray-600 mb-4">
              The requested lab result could not be found.
            </p>
            <Button onClick={handleGoBack}>Go Back</Button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      <Header />
      <div className="pt-20 max-w-7xl mx-auto px-4 py-8">
        {/* Header with back button */}
        <div className="mb-8">
          <div className="flex items-center justify-between mb-6">
            <button
              onClick={handleGoBack}
              className="flex items-center space-x-2 text-blue-600 hover:text-blue-800 transition-colors"
            >
              <ArrowLeft size={20} />
              <span>Back to Lab Results</span>
            </button>

            <Button
              onClick={handleDownload}
              variant="primary"
              className="flex items-center space-x-2"
            >
              <Download size={16} />
              <span>Download PDF</span>
            </Button>
          </div>

          <div className="bg-white rounded-lg border border-gray-200 p-6 mb-6">
            <div className="flex items-start justify-between mb-4">
              <div>
                <h1 className="text-2xl font-bold text-gray-900 mb-2">
                  {document.data.testType || "Laboratory Results"}
                </h1>
                <div className="flex flex-wrap gap-4 text-sm text-gray-600">
                  <div className="flex items-center space-x-2">
                    <Calendar size={16} />
                    <span>
                      Test Date: {document.data.testDate || document.createdAt}
                    </span>
                  </div>
                  {document.data.laboratory && (
                    <div className="flex items-center space-x-2">
                      <MapPin size={16} />
                      <span>Laboratory: {document.data.laboratory}</span>
                    </div>
                  )}
                  <div className="flex items-center space-x-2">
                    <Calendar size={16} />
                    <span>
                      Issued:{" "}
                      {new Date(document.createdAt).toLocaleDateString()}
                    </span>
                  </div>
                </div>
              </div>

              {document.data.status && (
                <div className="text-right">
                  <div className="text-sm text-gray-600 mb-1">
                    Overall Status
                  </div>
                  <Badge
                    variant={
                      document.data.status === "Normal"
                        ? "success"
                        : document.data.status === "Critical"
                          ? "error"
                          : document.data.status === "Abnormal"
                            ? "warning"
                            : "default"
                    }
                    size="lg"
                  >
                    {document.data.status}
                  </Badge>
                </div>
              )}
            </div>

            {document.notes && (
              <div className="bg-blue-50 border-l-4 border-blue-400 p-4 rounded">
                <h3 className="font-medium text-blue-900 mb-1">
                  Clinical Notes
                </h3>
                <p className="text-blue-800 text-sm">{document.notes}</p>
              </div>
            )}
          </div>
        </div>

        {/* Lab Results Content */}
        <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
          <LabResultsView document={document} />
        </div>
      </div>
    </div>
  );
};
