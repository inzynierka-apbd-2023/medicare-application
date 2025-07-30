import React from "react";
import { FilePlus2, FileSignature, FileText, Stethoscope } from "lucide-react";

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

        {document.notes && (
          <div className="text-gray-700 text-sm">
            <span className="font-medium">Notes:</span> {document.notes}
          </div>
        )}

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
