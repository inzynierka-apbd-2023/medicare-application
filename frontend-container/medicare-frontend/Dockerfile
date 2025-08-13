# Build stage: compile the Vite React app
FROM node:20-alpine AS build
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}
WORKDIR /app

# Install dependencies
COPY package*.json ./
# No lockfile in repo, use npm install
RUN npm install --no-audit --no-fund

# Copy source and build
COPY . .
RUN echo "Using VITE_API_BASE_URL=$VITE_API_BASE_URL" && npm run build

# Runtime stage: serve static files via Nginx
FROM nginx:alpine AS runtime

# Copy custom nginx config for SPA routing
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy build artifacts
COPY --from=build /app/dist /usr/share/nginx/html

# Install curl for healthcheck
RUN apk add --no-cache curl

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --retries=3 CMD curl -fs http://localhost/ || exit 1

# Use the default nginx startup command
