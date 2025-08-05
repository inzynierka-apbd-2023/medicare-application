import React from "react";
import {
  FilePlus2,
  FileSignature,
  FileText,
  Stethoscope,
  TestTube,
} from "lucide-react";

import { Badge, Card } from "../../../shared/components";
import type { DocumentCardProps, DocumentType } from "../types";

const docTypeInfo: Record<
  DocumentType,
  {
    icon: React.ReactNode;
    color: string;
    badgeVariant: "default" | "success" | "warning" | "info" | "error";
  }
> = {
  Prescription: {
    icon: <FileText size={16} />,
    color: "text-emerald-600",
    badgeVariant: "success",
  },
  Referral: {
    icon: <FileSignature size={16} />,
    color: "text-indigo-600",
    badgeVariant: "info",
  },
  Sick_Leave: {
    icon: <FilePlus2 size={16} />,
    color: "text-yellow-600",
    badgeVariant: "warning",
  },
  VisitCard: {
    icon: <Stethoscope size={16} />,
    color: "text-blue-600",
    badgeVariant: "default",
  },
  Lab_Results: {
    icon: <TestTube size={16} />,
    color: "text-purple-600",
    badgeVariant: "info",
  },
  Other: {
    icon: <FileText size={16} />,
    color: "text-gray-500",
    badgeVariant: "default",
  },
};

export const DocumentCard: React.FC<DocumentCardProps> = ({
  document,
  onClick,
}) => {
  const typeInfo = docTypeInfo[document.type];

  const handleClick = () => {
    onClick(document);
  };

  const renderPreview = () => {
    switch (document.type) {
      case "Lab_Results":
        return (
          <div className="text-gray-700 text-sm space-y-1">
            {document.data.testType && (
              <div>
                <span className="font-medium">Test:</span>{" "}
                {document.data.testType}
              </div>
            )}
            {document.data.laboratory && (
              <div>
                <span className="font-medium">Lab:</span>{" "}
                {document.data.laboratory}
              </div>
            )}
            {document.data.status && (
              <div>
                <span className="font-medium">Status:</span>
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
                  size="sm"
                  className="ml-1"
                >
                  {document.data.status}
                </Badge>
              </div>
            )}
          </div>
        );
      default:
        return (
          document.notes && (
            <div className="text-gray-700 text-sm">
              <span className="font-medium">Notes:</span> {document.notes}
            </div>
          )
        );
    }
  };

  return (
    <Card
      variant="default"
      padding="md"
      className="cursor-pointer hover:shadow-lg transition-shadow duration-200"
    >
      <div className="flex flex-col gap-3">
        <div className="flex items-center justify-between">
          <Badge variant={typeInfo.badgeVariant} icon={typeInfo.icon} size="md">
            {document.type.replace("_", " ")}
          </Badge>
          <span className="text-gray-400 text-xs">
            {new Date(document.createdAt).toLocaleDateString()}
          </span>
        </div>

        {renderPreview()}

        <button
          onClick={handleClick}
          className="mt-2 bg-blue-100 text-blue-700 px-4 py-2 rounded-lg hover:bg-blue-200 transition duration-150 w-fit text-sm font-medium"
        >
          View Details
        </button>
      </div>
    </Card>
  );
};
