import React from "react";
import type { Document, LabTestResult } from "@features/documents/types";
import { Badge } from "@shared/components";
import { AlertTriangle, CheckCircle, XCircle } from "lucide-react";

interface LabResultsViewProps {
  document: Document;
}

export const LabResultsView: React.FC<LabResultsViewProps> = ({ document }) => {
  if (document.type !== "Lab_Results" || !document.data.results) {
    return null;
  }

  const { data } = document;

  const getStatusColor = (status?: string) => {
    switch (status) {
      case "Normal":
        return "success";
      case "High":
        return "warning";
      case "Low":
        return "warning";
      case "Critical":
        return "error";
      default:
        return "default";
    }
  };

  const getStatusIcon = (status?: string) => {
    switch (status) {
      case "Normal":
        return <CheckCircle size={16} className="text-green-500" />;
      case "High":
      case "Low":
        return <AlertTriangle size={16} className="text-yellow-500" />;
      case "Critical":
        return <XCircle size={16} className="text-red-500" />;
      default:
        return null;
    }
  };

  return (
    <div className="p-6 space-y-6">
      {/* Test Results Table */}
      <div>
        <div className="mb-4">
          <h2 className="text-xl font-semibold text-gray-900">Test Results</h2>
          {data.interpretation && (
            <p className="text-gray-600 mt-2">{data.interpretation}</p>
          )}
        </div>

        <div className="overflow-x-auto border border-gray-200 rounded-lg">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-200 bg-gray-50">
                <th className="text-left py-3 px-4 font-medium text-gray-700">
                  Parameter
                </th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">
                  Value
                </th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">
                  Reference Range
                </th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">
                  Status
                </th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">
                  Notes
                </th>
              </tr>
            </thead>
            <tbody>
              {data.results?.map((result: LabTestResult, index: number) => (
                <tr
                  key={index}
                  className={`border-b border-gray-100 hover:bg-gray-50 ${
                    result.status === "Critical"
                      ? "bg-red-50"
                      : result.status === "High" || result.status === "Low"
                        ? "bg-yellow-50"
                        : ""
                  }`}
                >
                  <td className="py-3 px-4">
                    <div className="font-medium text-gray-900">
                      {result.parameter}
                    </div>
                  </td>
                  <td className="py-3 px-4">
                    <div className="flex items-center space-x-1">
                      <span className="font-mono text-base">
                        {result.value}
                      </span>
                      {result.unit && (
                        <span className="text-gray-500">{result.unit}</span>
                      )}
                    </div>
                  </td>
                  <td className="py-3 px-4 text-gray-600 font-mono text-sm">
                    {result.referenceRange || "N/A"}
                  </td>
                  <td className="py-3 px-4">
                    <div className="flex items-center space-x-2">
                      {getStatusIcon(result.status)}
                      <Badge variant={getStatusColor(result.status)} size="sm">
                        {result.status || "Unknown"}
                      </Badge>
                    </div>
                  </td>
                  <td className="py-3 px-4 text-sm text-gray-600">
                    {result.notes || "-"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Additional Notes */}
      {document.notes && (
        <div className="bg-gray-50 rounded-lg p-4">
          <h4 className="text-base font-semibold text-gray-900 mb-2">
            Additional Notes
          </h4>
          <p className="text-gray-700 text-sm whitespace-pre-wrap">
            {document.notes}
          </p>
        </div>
      )}

      {/* Reference Ranges Info */}
      {data.referenceRanges && (
        <div className="bg-blue-50 rounded-lg p-3 border-l-4 border-blue-400">
          <h4 className="text-sm font-medium text-blue-900 mb-1">
            Reference Ranges
          </h4>
          <p className="text-xs text-blue-800">{data.referenceRanges}</p>
        </div>
      )}
    </div>
  );
};
