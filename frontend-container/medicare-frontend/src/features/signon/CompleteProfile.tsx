import React, { useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { ArrowLeft } from "lucide-react";

import { useAuth } from "../../shared/auth/AuthContext";
import { getDefaultDashboard } from "../../shared/constants/routes";
import { RegisterRequest } from "../../shared/services/authService";

interface CompleteProfileForm {
  phone?: string;
  dateOfBirth?: string;
  gender?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
  avatarUrl?: string | null;
}

const CompleteProfile: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { register, updateProfile, loading } = useAuth();
  const registerData = useMemo(
    () =>
      (location.state as { registerData?: Partial<RegisterRequest> })
        ?.registerData,
    [location.state]
  );
  const [form, setForm] = useState<CompleteProfileForm>({
    phone: registerData?.phoneNumber || "",
    dateOfBirth: registerData?.dateOfBirth || "",
    gender: "",
    addressLine1: "",
    addressLine2: "",
    city: "",
    state: "",
    zipCode: "",
    country: "",
    avatarUrl: "",
  });
  const [error, setError] = useState<string | null>(null);

  const onChange: React.ChangeEventHandler<
    HTMLInputElement | HTMLSelectElement
  > = (e) => {
    const { name, value } = e.target as HTMLInputElement;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const onSubmit: React.FormEventHandler<HTMLFormElement> = async (e) => {
    e.preventDefault();
    setError(null);
    try {
      if (!registerData) {
        navigate("/register");
        return;
      }
      // Create the account now with all collected info
      const fullRegisterData = {
        ...registerData,
        phoneNumber: form.phone || registerData.phoneNumber,
        dateOfBirth: form.dateOfBirth || registerData.dateOfBirth,
        addressLine1: form.addressLine1,
        addressLine2: form.addressLine2,
        city: form.city,
        state: form.state,
        zipCode: form.zipCode,
        country: form.country,
        avatarUrl: form.avatarUrl,
      };

      const reg = await register(
        fullRegisterData as unknown as RegisterRequest
      );

      // Optionally set avatar immediately
      if (form.avatarUrl !== undefined) {
        await updateProfile(
          {
            avatarUrl:
              form.avatarUrl && form.avatarUrl.trim().length > 0
                ? form.avatarUrl
                : "",
          },
          reg.id
        );
      }

      // Redirect: Admins go straight to their dashboard, others see success page
      if (reg.role === "Admin") {
        navigate(getDefaultDashboard("Admin"));
      } else {
        navigate("/registration-success");
      }
    } catch (err) {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      setError((err as any)?.message || "Failed to save profile");
    }
  };

  return (
    <div className="page-container-with-scroll">
      <div className="auth-card">
        <button onClick={() => navigate(-1)} className="btn-back">
          <ArrowLeft className="icon-small" /> Back
        </button>
        <h1 className="auth-header">Complete Your Profile</h1>
        <p className="auth-subtitle">Tell us a bit more about you</p>

        <form onSubmit={onSubmit} className="auth-form-small">
          <div className="form-group-small">
            <label className="form-label" htmlFor="phone">
              Phone
            </label>
            <input
              id="phone"
              name="phone"
              value={form.phone}
              onChange={onChange}
              className="form-input text-sm"
              placeholder="+48 123 456 789"
            />
          </div>

          <div className="form-group-small">
            <label className="form-label" htmlFor="dateOfBirth">
              Date of Birth
            </label>
            <input
              id="dateOfBirth"
              type="date"
              name="dateOfBirth"
              value={form.dateOfBirth}
              onChange={onChange}
              className="form-input text-sm"
            />
          </div>

          <div className="form-group-small">
            <label className="form-label" htmlFor="avatarUrl">
              Avatar URL (optional)
            </label>
            <input
              id="avatarUrl"
              name="avatarUrl"
              value={form.avatarUrl || ""}
              onChange={onChange}
              className="form-input text-sm"
              placeholder="https://.../avatar.png"
            />
          </div>

          <div className="grid-2">
            <div className="form-group-small">
              <label className="form-label" htmlFor="addressLine1">
                Address line 1
              </label>
              <input
                id="addressLine1"
                name="addressLine1"
                value={form.addressLine1}
                onChange={onChange}
                className="form-input text-sm"
              />
            </div>
            <div className="form-group-small">
              <label className="form-label" htmlFor="addressLine2">
                Address line 2
              </label>
              <input
                id="addressLine2"
                name="addressLine2"
                value={form.addressLine2}
                onChange={onChange}
                className="form-input text-sm"
              />
            </div>
          </div>

          <div className="grid-3">
            <div className="form-group-small">
              <label className="form-label" htmlFor="city">
                City
              </label>
              <input
                id="city"
                name="city"
                value={form.city}
                onChange={onChange}
                className="form-input text-sm"
              />
            </div>
            <div className="form-group-small">
              <label className="form-label" htmlFor="state">
                State
              </label>
              <input
                id="state"
                name="state"
                value={form.state}
                onChange={onChange}
                className="form-input text-sm"
              />
            </div>
            <div className="form-group-small">
              <label className="form-label" htmlFor="zipCode">
                Zip Code
              </label>
              <input
                id="zipCode"
                name="zipCode"
                value={form.zipCode}
                onChange={onChange}
                className="form-input text-sm"
              />
            </div>
          </div>

          <div className="form-group-small">
            <label className="form-label" htmlFor="country">
              Country
            </label>
            <input
              id="country"
              name="country"
              value={form.country}
              onChange={onChange}
              className="form-input text-sm"
            />
          </div>

          {error && <div className="text-red-600 text-sm mb-2">{error}</div>}
          <button type="submit" className="btn-primary" disabled={loading}>
            {loading ? "Creating..." : "Create Account"}
          </button>
        </form>
      </div>
    </div>
  );
};

export default CompleteProfile;
