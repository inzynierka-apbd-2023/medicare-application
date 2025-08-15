import { getDefaultDashboard, ROUTE_ACCESS, ROUTES } from "../constants/routes";

/**
 * Utility functions for route management and validation
 */

/**
 * Get the appropriate redirect URL based on user role
 */
export const getRoleBasedRedirect = (userRole: string): string => {
  return getDefaultDashboard(userRole);
};

/**
 * Check if a user has access to a specific route
 */
export const canAccessRoute = (route: string, userRole: string): boolean => {
  const allowedRoles = ROUTE_ACCESS[route];
  return allowedRoles ? allowedRoles.includes(userRole) : false;
};

/**
 * Get all routes accessible to a specific role
 */
export const getAccessibleRoutes = (userRole: string): string[] => {
  return Object.entries(ROUTE_ACCESS)
    .filter(([, roles]) => roles.includes(userRole))
    .map(([route]) => route);
};

/**
 * Convert legacy route names to new standardized format
 */
export const convertLegacyRoute = (legacyRoute: string): string => {
  const legacyToNewMapping: Record<string, string> = {
    "/dashboard": ROUTES.DASHBOARDS.PATIENT,
    "/dctdash": ROUTES.DASHBOARDS.DOCTOR,
    "/ownerdash": ROUTES.DASHBOARDS.OWNER,
    "/appointments": ROUTES.PATIENT.MY_APPOINTMENTS,
    "/scheduler": ROUTES.PATIENT.APPOINTMENT_SCHEDULER,
    "/documents": ROUTES.PATIENT.MY_DOCUMENTS,
    "/prescriptions": ROUTES.PATIENT.MY_PRESCRIPTIONS,
    "/user/wallet": ROUTES.USER.MY_WALLET,
    "/user/myprofile": ROUTES.USER.MY_PROFILE,
    "/user/wallet/subscription": ROUTES.USER.SUBSCRIPTION_MANAGEMENT,
    "/doctor/scheduler": ROUTES.DOCTOR.DOCTOR_SCHEDULER,
    "/patientlist": ROUTES.DOCTOR.PATIENT_LIST,
    "/analytics": ROUTES.OWNER.APPOINTMENT_ANALYTICS,
    "/my-wallet": ROUTES.USER.MY_WALLET,
    "/my-profile": ROUTES.USER.MY_PROFILE,
    "/subscription-management": ROUTES.USER.SUBSCRIPTION_MANAGEMENT,
  };

  return legacyToNewMapping[legacyRoute] || legacyRoute;
};

/**
 * Generate breadcrumb data for a route
 */
export const generateBreadcrumbs = (currentRoute: string, userRole: string) => {
  const breadcrumbs = [
    {
      label: "Dashboard",
      path: getDefaultDashboard(userRole),
      isActive: currentRoute === getDefaultDashboard(userRole),
    },
  ];

  // Add route-specific breadcrumbs
  const routeBreadcrumbMap: Record<string, { label: string; parent?: string }> =
    {
      [ROUTES.PATIENT.MY_APPOINTMENTS]: { label: "My Appointments" },
      [ROUTES.PATIENT.APPOINTMENT_SCHEDULER]: { label: "Schedule Appointment" },
      [ROUTES.PATIENT.MY_DOCUMENTS]: { label: "My Documents" },
      [ROUTES.PATIENT.LAB_RESULTS]: { label: "Lab Results" },
      [ROUTES.PATIENT.MY_PRESCRIPTIONS]: { label: "My Prescriptions" },
      [ROUTES.USER.MY_WALLET]: { label: "My Wallet" },
      [ROUTES.USER.SUBSCRIPTION_MANAGEMENT]: {
        label: "Subscription Management",
        parent: ROUTES.USER.MY_WALLET,
      },
      [ROUTES.DOCTOR.PATIENT_LIST]: { label: "Patient List" },
      [ROUTES.DOCTOR.TODAYS_APPOINTMENTS]: { label: "Today's Appointments" },
      [ROUTES.DOCTOR.DOCTOR_SCHEDULER]: { label: "My Schedule" },
      [ROUTES.DOCTOR.MEDICAL_RECORDS]: { label: "Medical Records" },
      [ROUTES.DOCTOR.PRESCRIPTIONS_MANAGEMENT]: { label: "Prescriptions" },
      [ROUTES.DOCTOR.LAB_RESULTS_REVIEW]: { label: "Lab Results Review" },
      [ROUTES.OWNER.APPOINTMENT_ANALYTICS]: { label: "Appointment Analytics" },
      [ROUTES.OWNER.STAFF_MANAGEMENT]: { label: "Staff Management" },
      [ROUTES.RECEPTIONIST.RECEPTIONIST_SCHEDULER]: {
        label: "Schedule Management",
      },
      [ROUTES.RECEPTIONIST.PATIENT_REGISTRY]: { label: "Patient Registry" },
      [ROUTES.SHARED.MESSAGES]: { label: "Messages" },
      [ROUTES.USER.MY_PROFILE]: { label: "My Profile" },
    };

  const routeInfo = routeBreadcrumbMap[currentRoute];
  if (routeInfo) {
    // Add parent breadcrumb if exists
    if (routeInfo.parent) {
      const parentInfo = routeBreadcrumbMap[routeInfo.parent];
      if (parentInfo) {
        breadcrumbs.push({
          label: parentInfo.label,
          path: routeInfo.parent,
          isActive: false,
        });
      }
    }

    // Add current route breadcrumb
    breadcrumbs.push({
      label: routeInfo.label,
      path: currentRoute,
      isActive: true,
    });
  }

  return breadcrumbs;
};

/**
 * Check if route requires authentication
 */
export const isProtectedRoute = (route: string): boolean => {
  const publicRoutes = Object.values(ROUTES.PUBLIC);
  return !publicRoutes.includes(route);
};

/**
 * Get the appropriate error page route for a user role
 */
export const getErrorPageRoute = (
  userRole: string,
  _errorType: "403" | "404" | "500" = "404"
): string => {
  // For now, redirect to dashboard on errors
  // Could be extended to have role-specific error pages
  return getDefaultDashboard(userRole);
};
