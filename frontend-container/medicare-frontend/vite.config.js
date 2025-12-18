import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // API URLs are configurable via environment variables for Aspire compatibility
    // Create a .env.local file with VITE_API_URL, VITE_NOTIFICATION_URL, VITE_APPOINTMENT_URL
    // to override these defaults after checking service URLs in the Aspire Dashboard
    proxy: {
      // Route notifications API directly to the notification-service when running `npm run dev`
      "/api/notifications": {
        target: process.env.VITE_NOTIFICATION_URL || "http://localhost:8090",
        changeOrigin: true,
        secure: false,
      },
      // Route appointment service endpoints directly
      "/api/appointment": {
        target: process.env.VITE_APPOINTMENT_URL || "http://localhost:8082",
        changeOrigin: true,
        secure: false,
      },
      // Fallback default: other /api requests go to the user-service (acts as API gateway in dev)
      "/api": {
        target: process.env.VITE_API_URL || "http://localhost:8080",
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
});
