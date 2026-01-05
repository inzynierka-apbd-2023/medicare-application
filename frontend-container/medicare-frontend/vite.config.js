import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  // Load env file based on mode (development/production)
  const env = loadEnv(mode, process.cwd(), "");

  return {
    plugins: [react()],
    server: {
      port: 5173,
      // API URLs are configurable via environment variables for Aspire compatibility
      // Update .env.development with actual ports from Aspire Dashboard after startup
      proxy: {
        // UserService - Authentication & Users
        "/api/auth": {
          target: env.VITE_USER_SERVICE_URL || "http://localhost:9184",
          changeOrigin: true,
          secure: false,
        },
        "/api/users": {
          target: env.VITE_USER_SERVICE_URL || "http://localhost:9184",
          changeOrigin: true,
          secure: false,
        },

        // NotificationService
        "/api/notifications": {
          target: env.VITE_NOTIFICATION_SERVICE_URL || "http://localhost:8884",
          changeOrigin: true,
          secure: false,
        },

        // AppointmentService
        "/api/appointment": {
          target: env.VITE_APPOINTMENT_SERVICE_URL || "http://localhost:8284",
          changeOrigin: true,
          secure: false,
        },
        "/api/slots": {
          target: env.VITE_APPOINTMENT_SERVICE_URL || "http://localhost:8284",
          changeOrigin: true,
          secure: false,
        },
        "/api/schedules": {
          target: env.VITE_APPOINTMENT_SERVICE_URL || "http://localhost:8284",
          changeOrigin: true,
          secure: false,
        },

        // PatientService
        "/api/patient": {
          target: env.VITE_PATIENT_SERVICE_URL || "http://localhost:9084",
          changeOrigin: true,
          secure: false,
        },

        // PractitionerService
        "/api/practitioner": {
          target: env.VITE_PRACTITIONER_SERVICE_URL || "http://localhost:8384",
          changeOrigin: true,
          secure: false,
        },
        "/api/doctors": {
          target: env.VITE_PRACTITIONER_SERVICE_URL || "http://localhost:8384",
          changeOrigin: true,
          secure: false,
        },
        "/api/staff": {
          target: env.VITE_PRACTITIONER_SERVICE_URL || "http://localhost:8384",
          changeOrigin: true,
          secure: false,
        },

        // DocumentsService
        "/api/documents": {
          target: env.VITE_DOCUMENTS_SERVICE_URL || "http://localhost:8184",
          changeOrigin: true,
          secure: false,
        },
        "/api/prescriptions": {
          target: env.VITE_DOCUMENTS_SERVICE_URL || "http://localhost:8184",
          changeOrigin: true,
          secure: false,
        },

        // BillingService
        "/api/billing": {
          target: env.VITE_BILLING_SERVICE_URL || "http://localhost:8584",
          changeOrigin: true,
          secure: false,
        },
        "/api/wallet": {
          target: env.VITE_BILLING_SERVICE_URL || "http://localhost:8584",
          changeOrigin: true,
          secure: false,
        },

        // MedicalRecordsService
        "/api/medicalrecords": {
          target:
            env.VITE_MEDICAL_RECORDS_SERVICE_URL || "http://localhost:8684",
          changeOrigin: true,
          secure: false,
        },

        // MessagingService
        "/api/messaging": {
          target: env.VITE_MESSAGING_SERVICE_URL || "http://localhost:8984",
          changeOrigin: true,
          secure: false,
        },

        // LabService
        "/api/lab": {
          target: env.VITE_LAB_SERVICE_URL || "http://localhost:8784",
          changeOrigin: true,
          secure: false,
        },

        // MedicalCatalogService
        "/api/catalog": {
          target: env.VITE_CATALOG_SERVICE_URL || "http://localhost:8484",
          changeOrigin: true,
          secure: false,
        },
        "/api/icd10": {
          target: env.VITE_CATALOG_SERVICE_URL || "http://localhost:8484",
          changeOrigin: true,
          secure: false,
        },
        "/api/loinc": {
          target: env.VITE_CATALOG_SERVICE_URL || "http://localhost:8484",
          changeOrigin: true,
          secure: false,
        },

        // Fallback: any other /api goes to UserService (as main gateway)
        "/api": {
          target: env.VITE_USER_SERVICE_URL || "http://localhost:9184",
          changeOrigin: true,
          secure: false,
        },
      },
    },
    resolve: {
      alias: {
        "@": path.resolve(__dirname, "./src"),
        "@features": path.resolve(__dirname, "./src/features"),
        "@shared": path.resolve(__dirname, "./src/shared"),
        "@layout": path.resolve(__dirname, "./src/layout"),
      },
    },
  };
});
