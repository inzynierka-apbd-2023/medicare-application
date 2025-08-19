import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Route notifications API directly to the notification-service when running `npm run dev`
      "/api/notifications": {
        target: "http://localhost:8090",
        changeOrigin: true,
        secure: false,
      },
      // Fallback default: other /api requests go to the user-service (acts as API gateway in dev)
      "/api": {
        target: "http://localhost:8080",
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
