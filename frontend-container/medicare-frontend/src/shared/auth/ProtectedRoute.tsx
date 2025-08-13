import React from "react";
import { Navigate } from "react-router-dom";

import { useAuth } from "./AuthContext";

export const ProtectedRoute = ({
  children,
}: {
  children: React.ReactElement;
}) => {
  const { token, loading } = useAuth();
  if (loading) return <div className="p-4 text-center">Loading...</div>;
  if (!token) return <Navigate to="/login" replace />;
  return children;
};
