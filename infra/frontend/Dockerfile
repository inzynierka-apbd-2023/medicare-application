# Build Stage
FROM node:20-slim AS build

WORKDIR /app
COPY package.json package-lock.json ./
# Force install to ignore Windows lockfile and get Linux binaries
RUN rm -rf package-lock.json node_modules
RUN npm install

COPY . .
# Fix esbuild binary mismatch by forcing rebuild
# Force align esbuild and rollup versions
RUN npm install -D esbuild rollup

RUN npm run build

# Serve Stage
FROM nginx:alpine

# Copy built assets
COPY --from=build /app/dist /usr/share/nginx/html

# Copy template
COPY nginx.conf.template /etc/nginx/conf.d/default.conf.template

EXPOSE 80

# Map Aspire environment variables to Nginx variables and substitute
# Note: Aspire provides services__name__http__0. We default to http://name if missing.
CMD ["/bin/sh", "-c", "export DOCUMENTS_SERVICE_URL=${services__documentsservice__https__0:-${services__documentsservice__http__0:-http://documentsservice}} && export DOCUMENTS_SERVICE_URL=${DOCUMENTS_SERVICE_URL%/} && \
export BILLING_SERVICE_URL=${services__billingservice__https__0:-${services__billingservice__http__0:-http://billingservice}} && export BILLING_SERVICE_URL=${BILLING_SERVICE_URL%/} && \
export PRACTITIONER_SERVICE_URL=${services__practitionerservice__https__0:-${services__practitionerservice__http__0:-http://practitionerservice}} && export PRACTITIONER_SERVICE_URL=${PRACTITIONER_SERVICE_URL%/} && \
export PATIENT_SERVICE_URL=${services__patientservice__https__0:-${services__patientservice__http__0:-http://patientservice}} && export PATIENT_SERVICE_URL=${PATIENT_SERVICE_URL%/} && \
export APPOINTMENT_SERVICE_URL=${services__appointmentservice__https__0:-${services__appointmentservice__http__0:-http://appointmentservice}} && export APPOINTMENT_SERVICE_URL=${APPOINTMENT_SERVICE_URL%/} && \
export NOTIFICATION_SERVICE_URL=${services__notificationservice__https__0:-${services__notificationservice__http__0:-http://notificationservice}} && export NOTIFICATION_SERVICE_URL=${NOTIFICATION_SERVICE_URL%/} && \
export CATALOG_SERVICE_URL=${services__medicalcatalogservice__https__0:-${services__medicalcatalogservice__http__0:-http://medicalcatalogservice}} && export CATALOG_SERVICE_URL=${CATALOG_SERVICE_URL%/} && \
export LAB_SERVICE_URL=${services__labservice__https__0:-${services__labservice__http__0:-http://labservice}} && export LAB_SERVICE_URL=${LAB_SERVICE_URL%/} && \
export RECORDS_SERVICE_URL=${services__medicalrecordsservice__https__0:-${services__medicalrecordsservice__http__0:-http://medicalrecordsservice}} && export RECORDS_SERVICE_URL=${RECORDS_SERVICE_URL%/} && \
export MESSAGING_SERVICE_URL=${services__messagingservice__https__0:-${services__messagingservice__http__0:-http://messagingservice}} && export MESSAGING_SERVICE_URL=${MESSAGING_SERVICE_URL%/} && \
export USER_SERVICE_URL=${services__userservice__https__0:-${services__userservice__http__0:-http://userservice}} && export USER_SERVICE_URL=${USER_SERVICE_URL%/} && \
envsubst '${DOCUMENTS_SERVICE_URL} ${BILLING_SERVICE_URL} ${PRACTITIONER_SERVICE_URL} ${PATIENT_SERVICE_URL} ${APPOINTMENT_SERVICE_URL} ${NOTIFICATION_SERVICE_URL} ${CATALOG_SERVICE_URL} ${LAB_SERVICE_URL} ${RECORDS_SERVICE_URL} ${MESSAGING_SERVICE_URL} ${USER_SERVICE_URL}' < /etc/nginx/conf.d/default.conf.template > /etc/nginx/conf.d/default.conf && \
nginx -g 'daemon off;'"]
