import React from "react";
import { Navigate } from "react-router-dom";

import { useAuth } from "./AuthContext";

interface RoleBasedRouteProps {
  children: React.ReactElement;
  allowedRoles: string[];
  redirectTo?: string;
}

export const RoleBasedRoute: React.FC<RoleBasedRouteProps> = ({
  children,
  allowedRoles,
  redirectTo = "/login",
}) => {
  const { user, loading } = useAuth();

  if (loading) {
    return <div className="p-4 text-center">Loading...</div>;
  }

  if (!user) {
    return <Navigate to={redirectTo} replace />;
  }

  if (!allowedRoles.includes(user.role)) {
    // Redirect to appropriate dashboard based on user role
    const roleRedirects: Record<string, string> = {
      Patient: "/patient-dashboard",
      Doctor: "/doctor-dashboard",
      Owner: "/owner-dashboard",
      Admin: "/owner-dashboard",
      Receptionist: "/receptionist-dashboard",
    };

    const userDashboard = roleRedirects[user.role] || "/";
    return <Navigate to={userDashboard} replace />;
  }

  return children;
};
