# Creates a test appointment for a given user via UserService and AppointmentService
param(
  [string]$Username = "patient_a_20250818",
  [string]$Password = "P@ssw0rd!",
  [string]$DoctorId = "5a576dc0-cf45-4868-9112-9ae245461020",
  [string]$UserServiceBase = "http://localhost:8080/api",
  [string]$AppointmentServiceBase = "http://localhost:8087/api/appointment",
  [string]$AppointmentType = "in-person",
  [string]$Notes = "Test appointment created via script"
)

$ErrorActionPreference = "Stop"

Write-Host "Logging in as $Username..." -ForegroundColor Cyan
$loginBody = @{ username = $Username; password = $Password } | ConvertTo-Json
$login = Invoke-RestMethod -Method Post -Uri "$UserServiceBase/auth/login" -ContentType 'application/json' -Body $loginBody

if (-not $login.token) {
  throw "Login failed; no token returned. Response: $($login | ConvertTo-Json -Depth 5)"
}

$token = $login.token
$patientId = $login.user.id
Write-Host "Login OK. PatientId: $patientId" -ForegroundColor Green

# Compute next half-hour slot UTC
$now = [DateTime]::UtcNow
$minutes = $now.Minute
if ($minutes -lt 30) { $add = 30 - $minutes } else { $add = 60 - $minutes }
$start = $now.AddMinutes($add).AddSeconds(-$now.Second).AddMilliseconds(-$now.Millisecond)
$end = $start.AddMinutes(30)

$payload = @{ 
  patientId = $patientId;
  doctorId = $DoctorId;
  scheduledAt = $start.ToString("o");
  scheduledEndAt = $end.ToString("o");
  appointmentType = $AppointmentType;
  notes = $Notes;
} | ConvertTo-Json

Write-Host "Creating appointment for patient $patientId with doctor $DoctorId at $($start.ToString('u'))..." -ForegroundColor Cyan
$headers = @{ Authorization = "Bearer $token" }
$created = Invoke-RestMethod -Method Post -Uri "$AppointmentServiceBase/appointments" -Headers $headers -ContentType 'application/json' -Body $payload

Write-Host "Created appointment:" -ForegroundColor Green
$created | ConvertTo-Json -Depth 6
