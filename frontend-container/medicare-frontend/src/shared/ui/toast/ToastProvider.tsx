import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";

type ToastType = "info" | "success" | "warning" | "error";

export interface ToastOptions {
  id?: string;
  type?: ToastType;
  durationMs?: number; // auto-dismiss
}

export interface ToastItem extends Required<ToastOptions> {
  message: string;
}

interface ToastContextValue {
  showToast: (message: string, options?: ToastOptions) => void;
  removeToast: (id: string) => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

export const ToastProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [toasts, setToasts] = useState<ToastItem[]>([]);

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const showToast = useCallback((message: string, options?: ToastOptions) => {
    const id = options?.id ?? Math.random().toString(36).slice(2);
    const type: ToastType = options?.type ?? "info";
    const durationMs = options?.durationMs ?? 5000;
    const item: ToastItem = { id, message, type, durationMs };
    setToasts((prev) => [...prev, item]);
    if (durationMs > 0) {
      setTimeout(() => removeToast(id), durationMs);
    }
  }, [removeToast]);

  const value = useMemo(() => ({ showToast, removeToast }), [showToast, removeToast]);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape" && toasts.length > 0) {
        removeToast(toasts[0].id);
      }
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [toasts, removeToast]);

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="fixed inset-0 pointer-events-none z-50 flex flex-col items-end gap-2 p-4">
        {toasts.map((t) => (
          <div
            key={t.id}
            className={[
              "pointer-events-auto w-full max-w-sm shadow-lg rounded-md border p-3 text-sm",
              t.type === "success" && "bg-emerald-50 border-emerald-200 text-emerald-900",
              t.type === "error" && "bg-red-50 border-red-200 text-red-900",
              t.type === "warning" && "bg-amber-50 border-amber-200 text-amber-900",
              t.type === "info" && "bg-blue-50 border-blue-200 text-blue-900",
            ].filter(Boolean).join(" ")}
          >
            <div className="flex items-start gap-2">
              <div className="flex-1 whitespace-pre-line">{t.message}</div>
              <button
                aria-label="Close"
                className="ml-2 text-xs opacity-70 hover:opacity-100"
                onClick={() => removeToast(t.id)}
              >
                ?
              </button>
            </div>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
};

export const useToastContext = (): ToastContextValue => {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error("useToastContext must be used within ToastProvider");
  return ctx;
};
