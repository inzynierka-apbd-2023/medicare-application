import React from "react";

export interface DefinitionListProps {
  items: Array<{
    label: string;
    value: string | number | React.ReactNode;
    show?: boolean;
  }>;
  className?: string;
  variant?: "default" | "compact" | "bordered";
}

export const DefinitionList: React.FC<DefinitionListProps> = ({
  items,
  className = "",
  variant = "default",
}) => {
  const baseClasses = "space-y-1";
  const variantClasses = {
    default: "space-y-3",
    compact: "space-y-1",
    bordered: "divide-y divide-gray-200",
  };

  const itemClasses = {
    default: "flex flex-col sm:flex-row sm:items-center",
    compact: "flex flex-col sm:flex-row sm:items-center py-1",
    bordered:
      "flex flex-col sm:flex-row sm:items-center py-3 first:pt-0 last:pb-0",
  };

  const filteredItems = items.filter(
    (item) =>
      item.show !== false &&
      item.value !== undefined &&
      item.value !== null &&
      item.value !== ""
  );

  if (filteredItems.length === 0) {
    return null;
  }

  return (
    <dl className={`${baseClasses} ${variantClasses[variant]} ${className}`}>
      {filteredItems.map((item, index) => (
        <div key={index} className={itemClasses[variant]}>
          <dt className="font-medium text-gray-700 w-32 mb-1 sm:mb-0 text-sm">
            {item.label}:
          </dt>
          <dd className="text-gray-900 text-sm">
            {typeof item.value === "string" || typeof item.value === "number"
              ? item.value
              : item.value}
          </dd>
        </div>
      ))}
    </dl>
  );
};
