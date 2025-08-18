// Route configuration with role-based access control
export const ROUTES = {
  // ===== PUBLIC ROUTES =====
  PUBLIC: {
    HOME: "/",
    LOGIN: "/login",
    REGISTER: "/register",
    FORGOT_PASSWORD: "/forgot-password",
    FORGOT_CARD: "/forgot-card",
    CHOOSE_PLAN: "/choose-plan",
    SUBSCRIPTION_VIEW: "/subscription-view",
    LOGIN_SUCCESS: "/login-success",
    REGISTRATION_SUCCESS: "/registration-success",
    PASSWORD_RESET_SUCCESS: "/password-reset-success",
  },

  // ===== DASHBOARD ROUTES =====
  DASHBOARDS: {
    PATIENT: "/patient-dashboard",
    DOCTOR: "/doctor-dashboard",
    OWNER: "/owner-dashboard",
    RECEPTIONIST: "/receptionist-dashboard",
  },

  // ===== PATIENT ROUTES =====
  PATIENT: {
    MY_APPOINTMENTS: "/my-appointments",
    APPOINTMENT_SCHEDULER: "/appointment-scheduler",
    MY_DOCUMENTS: "/my-documents",
    LAB_RESULTS: "/lab-results",
    LAB_RESULT_DETAIL: "/lab-results/:documentId",
    MY_PRESCRIPTIONS: "/my-prescriptions",
  },

  // ===== DOCTOR ROUTES =====
  DOCTOR: {
    PATIENT_LIST: "/patient-list",
    TODAYS_APPOINTMENTS: "/todays-appointments",
    DOCTOR_SCHEDULER: "/doctor-scheduler",
    MEDICAL_RECORDS: "/medical-records",
    MEDICAL_RECORDS_PATIENT: "/medical-records/:patientId",
    PRESCRIPTIONS_MANAGEMENT: "/prescriptions-management",
    LAB_RESULTS_REVIEW: "/lab-results-review",
  },

  // ===== OWNER ROUTES =====
  OWNER: {
    APPOINTMENT_ANALYTICS: "/appointment-analytics",
    STAFF_MANAGEMENT: "/staff-management",
  },

  // ===== RECEPTIONIST ROUTES =====
  RECEPTIONIST: {
    RECEPTIONIST_SCHEDULER: "/receptionist-scheduler",
    PATIENT_REGISTRY: "/patient-registry",
  },

  // ===== SHARED ROUTES (Multiple Roles) =====
  SHARED: {
    MESSAGES: "/messages",
  },

  // ===== USER ROUTES (Profile & Account Management) =====
  USER: {
    MY_PROFILE: "/user/myprofile",
    MY_WALLET: "/user/wallet",
    SUBSCRIPTION_MANAGEMENT: "/user/wallet/subscription",
  },

  // ===== LEGACY ROUTES (for backward compatibility) =====
  LEGACY: {
    DASHBOARD: "/dashboard",
    DCT_DASH: "/dctdash",
    OWNER_DASH: "/ownerdash",
  },
};

// Role-based route access configuration
export const ROUTE_ACCESS = {
  [ROUTES.DASHBOARDS.PATIENT]: ["Patient"],
  [ROUTES.DASHBOARDS.DOCTOR]: ["Doctor"],
  [ROUTES.DASHBOARDS.OWNER]: ["Owner"],
  [ROUTES.DASHBOARDS.RECEPTIONIST]: ["Receptionist"],

  // Patient routes
  [ROUTES.PATIENT.MY_APPOINTMENTS]: ["Patient"],
  [ROUTES.PATIENT.APPOINTMENT_SCHEDULER]: ["Patient"],
  [ROUTES.PATIENT.MY_DOCUMENTS]: ["Patient"],
  [ROUTES.PATIENT.LAB_RESULTS]: ["Patient"],
  [ROUTES.PATIENT.LAB_RESULT_DETAIL]: ["Patient"],
  [ROUTES.PATIENT.MY_PRESCRIPTIONS]: ["Patient"],

  // Doctor routes
  [ROUTES.DOCTOR.PATIENT_LIST]: ["Doctor"],
  [ROUTES.DOCTOR.TODAYS_APPOINTMENTS]: ["Doctor"],
  [ROUTES.DOCTOR.DOCTOR_SCHEDULER]: ["Doctor"],
  [ROUTES.DOCTOR.MEDICAL_RECORDS]: ["Doctor"],
  [ROUTES.DOCTOR.MEDICAL_RECORDS_PATIENT]: ["Doctor"],
  [ROUTES.DOCTOR.PRESCRIPTIONS_MANAGEMENT]: ["Doctor"],
  [ROUTES.DOCTOR.LAB_RESULTS_REVIEW]: ["Doctor"],

  // Owner routes
  [ROUTES.OWNER.APPOINTMENT_ANALYTICS]: ["Owner"],
  [ROUTES.OWNER.STAFF_MANAGEMENT]: ["Owner"],

  // Receptionist routes
  [ROUTES.RECEPTIONIST.RECEPTIONIST_SCHEDULER]: ["Receptionist"],
  [ROUTES.RECEPTIONIST.PATIENT_REGISTRY]: ["Receptionist"],

  // Shared routes
  [ROUTES.SHARED.MESSAGES]: ["Patient", "Doctor", "Receptionist"],

  // User routes
  [ROUTES.USER.MY_PROFILE]: ["Patient", "Doctor", "Owner", "Receptionist"],
  [ROUTES.USER.MY_WALLET]: ["Patient"],
  [ROUTES.USER.SUBSCRIPTION_MANAGEMENT]: ["Patient"],
};

// Navigation menu structure for different roles
export const NAVIGATION_MENUS = {
  Patient: [
    {
      label: "My Appointments",
      path: ROUTES.PATIENT.MY_APPOINTMENTS,
      icon: "calendar",
    },
    {
      label: "Schedule Appointment",
      path: ROUTES.PATIENT.APPOINTMENT_SCHEDULER,
      icon: "schedule",
    },
    {
      label: "My Documents",
      path: ROUTES.PATIENT.MY_DOCUMENTS,
      icon: "document",
    },
    {
      label: "Lab Results",
      path: ROUTES.PATIENT.LAB_RESULTS,
      icon: "lab",
    },
    {
      label: "My Prescriptions",
      path: ROUTES.PATIENT.MY_PRESCRIPTIONS,
      icon: "prescription",
    },
    {
      label: "Messages",
      path: ROUTES.SHARED.MESSAGES,
      icon: "message",
    },
  ],

  Doctor: [
    {
      label: "Patient List",
      path: ROUTES.DOCTOR.PATIENT_LIST,
      icon: "patients",
    },
    {
      label: "Today's Appointments",
      path: ROUTES.DOCTOR.TODAYS_APPOINTMENTS,
      icon: "today",
    },
    {
      label: "My Schedule",
      path: ROUTES.DOCTOR.DOCTOR_SCHEDULER,
      icon: "schedule",
    },
    {
      label: "Medical Records",
      path: ROUTES.DOCTOR.MEDICAL_RECORDS,
      icon: "records",
    },
    {
      label: "Prescriptions",
      path: ROUTES.DOCTOR.PRESCRIPTIONS_MANAGEMENT,
      icon: "prescription",
    },
    {
      label: "Lab Results Review",
      path: ROUTES.DOCTOR.LAB_RESULTS_REVIEW,
      icon: "lab-review",
    },
    {
      label: "Messages",
      path: ROUTES.SHARED.MESSAGES,
      icon: "message",
    },
  ],

  Owner: [
    {
      label: "Appointment Analytics",
      path: ROUTES.OWNER.APPOINTMENT_ANALYTICS,
      icon: "analytics",
    },
    {
      label: "Staff Management",
      path: ROUTES.OWNER.STAFF_MANAGEMENT,
      icon: "staff",
    },
  ],

  Receptionist: [
    {
      label: "Schedule Management",
      path: ROUTES.RECEPTIONIST.RECEPTIONIST_SCHEDULER,
      icon: "schedule",
    },
    {
      label: "Patient Registry",
      path: ROUTES.RECEPTIONIST.PATIENT_REGISTRY,
      icon: "registry",
    },
    {
      label: "Messages",
      path: ROUTES.SHARED.MESSAGES,
      icon: "message",
    },
  ],
};

// Helper function to get navigation menu for a role
export const getNavigationForRole = (role: string) => {
  return NAVIGATION_MENUS[role as keyof typeof NAVIGATION_MENUS] || [];
};

// Helper function to check if user has access to a route
export const hasRouteAccess = (route: string, userRole: string): boolean => {
  const allowedRoles = ROUTE_ACCESS[route];
  return allowedRoles ? allowedRoles.includes(userRole) : false;
};

// Helper function to get default dashboard for a role
export const getDefaultDashboard = (role: string): string => {
  const dashboards: Record<string, string> = {
    Patient: ROUTES.DASHBOARDS.PATIENT,
    Doctor: ROUTES.DASHBOARDS.DOCTOR,
    Owner: ROUTES.DASHBOARDS.OWNER,
  // Map Admins to the Owner dashboard for now
  Admin: ROUTES.DASHBOARDS.OWNER,
    Receptionist: ROUTES.DASHBOARDS.RECEPTIONIST,
  };

  return dashboards[role] || ROUTES.PUBLIC.LOGIN;
};
