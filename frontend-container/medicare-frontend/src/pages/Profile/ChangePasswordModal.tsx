import React, { useState } from "react";

export default function ChangePasswordModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [form, setForm] = useState({
    current: "",
    new: "",
    confirm: "",
  });
  const [passMsg, setPassMsg] = useState<null | { type: "success" | "error"; text: string }>(null);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (form.new !== form.confirm) {
      setPassMsg({ type: "error", text: "New passwords do not match." });
      return;
    }
    setTimeout(() => {
      setPassMsg({ type: "success", text: "Password changed successfully!" });
      setTimeout(() => {
        setPassMsg(null);
        onClose();
        setForm({ current: "", new: "", confirm: "" });
      }, 1400);
    }, 900);
  };

  const handleForgotPassword = () => {
    console.log("Forgot password clicked");
  };

  if (!open) return null;

  return (
    <>
      <div
        className="fixed inset-0 bg-black bg-opacity-40 z-40 transition-opacity"
        onClick={onClose}
      />
      <div className="fixed inset-0 flex items-center justify-center z-50">
        <div className="bg-white rounded-2xl shadow-lg p-8 w-full max-w-md relative">
          <button
            className="absolute top-3 right-3 text-blue-300 hover:text-blue-500 transition text-base font-bold p-1"
            style={{ lineHeight: 1 }}
            onClick={onClose}
            aria-label="Close modal"
          >
            &times;
          </button>
          <h2 className="text-2xl font-semibold text-blue-600 mb-4 text-center">Change Password</h2>
          <form className="space-y-4" onSubmit={handleSubmit}>
            <div>
              <label className="block text-blue-600 font-semibold mb-1" htmlFor="current">
                Current Password
              </label>
              <input
                id="current"
                name="current"
                type="password"
                value={form.current}
                onChange={handleChange}
                className="w-full px-4 py-2 rounded-lg border border-gray-200 focus:border-blue-400 focus:ring-1 focus:ring-blue-100 transition"
                required
              />
            </div>
            <div>
              <label className="block text-blue-600 font-semibold mb-1" htmlFor="new">
                New Password
              </label>
              <input
                id="new"
                name="new"
                type="password"
                value={form.new}
                onChange={handleChange}
                className="w-full px-4 py-2 rounded-lg border border-gray-200 focus:border-blue-400 focus:ring-1 focus:ring-blue-100 transition"
                required
              />
            </div>
            <div>
              <label className="block text-blue-600 font-semibold mb-1" htmlFor="confirm">
                Confirm New Password
              </label>
              <input
                id="confirm"
                name="confirm"
                type="password"
                value={form.confirm}
                onChange={handleChange}
                className="w-full px-4 py-2 rounded-lg border border-gray-200 focus:border-blue-400 focus:ring-1 focus:ring-blue-100 transition"
                required
              />
            </div>
            {passMsg && (
              <div className={`w-full text-center rounded-lg py-2 ${passMsg.type === "success" ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"}`}>
                {passMsg.text}
              </div>
            )}
            <button
              type="submit"
              className="w-full px-4 py-2 bg-blue-700 text-white rounded-lg hover:bg-blue-800 transition font-semibold"
            >
              Save New Password
            </button>
          </form>
          {/* Forgot password - underlined text, left-aligned */}
          <div className="mt-4 flex items-center">
            <span
              className="text-blue-600 underline text-sm cursor-pointer hover:text-blue-800 transition select-none"
              onClick={handleForgotPassword}
              tabIndex={0}
              role="button"
              aria-label="Forgot your password"
            >
              Forgot your password?
            </span>
          </div>
        </div>
      </div>
    </>
  );
}
