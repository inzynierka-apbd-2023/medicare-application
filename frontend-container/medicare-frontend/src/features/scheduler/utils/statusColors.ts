export type StatusColor = { bg: string; border: string; text?: string };

// Centralized color mapping for appointment statuses
const STATUS_COLORS: Record<string, StatusColor> = {
  // canonical
  scheduled: { bg: "#3b82f6", border: "#2563eb", text: "#ffffff" }, // blue
  confirmed: { bg: "#10b981", border: "#059669", text: "#ffffff" }, // green
  inprogress: { bg: "#06b6d4", border: "#0891b2", text: "#ffffff" }, // cyan
  completed: { bg: "#6b7280", border: "#4b5563", text: "#ffffff" }, // gray
  cancelled: { bg: "#ef4444", border: "#dc2626", text: "#ffffff" }, // red
  overdue: { bg: "#f97316", border: "#ea580c", text: "#ffffff" }, // orange
  noshow: { bg: "#9ca3af", border: "#6b7280", text: "#ffffff" }, // gray (no-show)

  // aliases and legacy names
  pending: { bg: "#f59e0b", border: "#d97706", text: "#111827" }, // amber
  "no-show": { bg: "#9ca3af", border: "#6b7280", text: "#ffffff" },
  unknown: { bg: "#8b5cf6", border: "#7c3aed", text: "#ffffff" }, // purple
  default: { bg: "#8b5cf6", border: "#7c3aed", text: "#ffffff" },
};

export function getStatusColors(statusName?: string): StatusColor {
  if (!statusName) return STATUS_COLORS.default;
  const key = statusName.replace(/\s+/g, "").toLowerCase();
  return STATUS_COLORS[key] || STATUS_COLORS.default;
}
