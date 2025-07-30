import React from "react";

export interface TableColumn<T = Record<string, unknown>> {
  key: string;
  title: string;
  render?: (value: unknown, record: T, index: number) => React.ReactNode;
  width?: string;
  align?: "left" | "center" | "right";
}

export interface TableProps<T = Record<string, unknown>> {
  columns: TableColumn<T>[];
  data: T[];
  loading?: boolean;
  emptyText?: string;
  className?: string;
  rowKey?: string | ((record: T, index: number) => string);
}

const Table = <T,>({
  columns,
  data,
  loading = false,
  emptyText = "No data available",
  className = "",
  rowKey = "id",
}: TableProps<T>) => {
  const getRowKey = (record: T, index: number): string => {
    if (typeof rowKey === "function") {
      return rowKey(record, index);
    }
    return String((record as Record<string, unknown>)[rowKey] || index);
  };

  const getCellValue = (
    record: T,
    column: TableColumn<T>,
    index: number
  ): React.ReactNode => {
    const value = (record as Record<string, unknown>)[column.key];
    if (column.render) {
      return column.render(value, record, index);
    }
    return String(value ?? "");
  };

  if (loading) {
    return (
      <div
        className={`bg-white rounded-2xl shadow-lg overflow-hidden ${className}`}
      >
        <div className="p-8 text-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-500">Loading...</p>
        </div>
      </div>
    );
  }

  return (
    <div
      className={`bg-white rounded-2xl shadow-lg overflow-x-auto ${className}`}
    >
      <table className="w-full">
        <thead>
          <tr className="border-b border-gray-200">
            {columns.map((column) => (
              <th
                key={column.key}
                className={`py-3 px-4 text-blue-600 font-semibold ${
                  column.align === "center"
                    ? "text-center"
                    : column.align === "right"
                      ? "text-right"
                      : "text-left"
                }`}
                style={{ width: column.width }}
              >
                {column.title}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.length === 0 ? (
            <tr>
              <td
                colSpan={columns.length}
                className="py-8 text-center text-gray-800"
              >
                {emptyText}
              </td>
            </tr>
          ) : (
            data.map((record, index) => (
              <tr
                key={getRowKey(record, index)}
                className="border-b border-gray-100 hover:bg-gray-50 transition"
              >
                {columns.map((column) => (
                  <td
                    key={column.key}
                    className={`py-3 px-4 text-gray-800 ${
                      column.align === "center"
                        ? "text-center"
                        : column.align === "right"
                          ? "text-right"
                          : "text-left"
                    }`}
                  >
                    {getCellValue(record, column, index)}
                  </td>
                ))}
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
};

export { Table };
