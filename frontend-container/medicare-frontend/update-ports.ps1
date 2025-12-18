#!/usr/bin/env pwsh
# update-ports.ps1 - Updates .env.development with current Aspire ports
# Run this after starting Aspire to get the correct ports

Write-Host "=== Medicare Frontend Port Updater ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Open Aspire Dashboard at: http://localhost:18888" -ForegroundColor Yellow
Write-Host ""
Write-Host "Copy the ports from the 'Endpoints' column and update .env.development:"
Write-Host ""
Write-Host "Example mappings:"
Write-Host "  userservice       -> VITE_USER_SERVICE_URL"
Write-Host "  appointmentservice -> VITE_APPOINTMENT_SERVICE_URL"
Write-Host "  patientservice    -> VITE_PATIENT_SERVICE_URL"
Write-Host "  ... etc"
Write-Host ""

# Read current .env.development
$envFile = Join-Path $PSScriptRoot ".env.development"
if (Test-Path $envFile) {
    Write-Host "Current .env.development values:" -ForegroundColor Green
    Get-Content $envFile | Where-Object { $_ -match "^VITE_" } | ForEach-Object {
        Write-Host "  $_"
    }
} else {
    Write-Host ".env.development not found. Run 'npm run dev' first." -ForegroundColor Red
}

Write-Host ""
Write-Host "After updating .env.development, restart vite: npm run dev" -ForegroundColor Yellow
