import React from "react";
import { Link, useLocation } from "react-router-dom";
import { useAuth } from "@shared/auth/AuthContext";
import { generateBreadcrumbs } from "@shared/utils/routeUtils";
import { ChevronRight, Home } from "lucide-react";

interface BreadcrumbProps {
  className?: string;
}

export const Breadcrumb: React.FC<BreadcrumbProps> = ({ className = "" }) => {
  const { user } = useAuth();
  const location = useLocation();

  if (!user) return null;

  const breadcrumbs = generateBreadcrumbs(location.pathname, user.role);

  if (breadcrumbs.length <= 1) return null;

  return (
    <nav
      className={`flex items-center space-x-2 text-sm text-gray-600 ${className}`}
    >
      <Home size={16} />
      {breadcrumbs.map((crumb, index) => (
        <React.Fragment key={crumb.path}>
          {index > 0 && <ChevronRight size={14} className="text-gray-400" />}
          {crumb.isActive ? (
            <span className="font-medium text-gray-900">{crumb.label}</span>
          ) : (
            <Link
              to={crumb.path}
              className="hover:text-blue-600 transition-colors"
            >
              {crumb.label}
            </Link>
          )}
        </React.Fragment>
      ))}
    </nav>
  );
};
