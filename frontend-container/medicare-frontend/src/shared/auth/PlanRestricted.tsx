import React from "react";
import { Navigate } from "react-router-dom";
import { LoadingOverlay } from "@shared/components";
import { useSubscription } from "@shared/hooks/useSubscription";

type Feature = "hasMessaging" | "hasPrescriptions" | "hasDocuments";

interface PlanRestrictedRouteProps {
  feature: Feature;
  children: React.ReactNode;
  redirectTo?: string;
}

export const PlanRestrictedRoute: React.FC<PlanRestrictedRouteProps> = ({
  feature,
  children,
  redirectTo = "/choose-plan",
}) => {
  const { features, isLoading } = useSubscription();

  if (isLoading) {
    return (
      <LoadingOverlay isLoading={true} message="Checking subscription...">
        <div className="min-h-screen" />
      </LoadingOverlay>
    );
  }

  if (!features[feature]) {
    return <Navigate to={redirectTo} replace />;
  }

  return <>{children}</>;
};

interface PlanRestrictedContentProps {
  feature: Feature;
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

export const PlanRestrictedContent: React.FC<PlanRestrictedContentProps> = ({
  feature,
  children,
  fallback = null,
}) => {
  const { features, isLoading } = useSubscription();

  if (isLoading) return null; // or a skeleton

  if (!features[feature]) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
};
