param(
    [string]$PatientId,
    [string]$DoctorId,
    [string]$AppointmentId
)

$ErrorActionPreference = 'Stop'

$base = "http://localhost"

function Resolve-Ids {
    if (-not $PatientId -or -not $DoctorId) {
        Write-Host "Resolving PatientId/DoctorId from existing documents..."
        $docs = Invoke-RestMethod -Method GET -Uri "$base/api/documents"
        if (-not $docs) { throw "No documents found. Provide -PatientId and -DoctorId explicitly." }
        if (-not $PatientId) { $script:PatientId = $docs[0].patientId }
        if (-not $DoctorId) { $script:DoctorId = $docs[0].doctorId }
        Write-Host "Using PatientId=$PatientId DoctorId=$DoctorId"
    }
}

function New-LabDocument {
    $body = @{ 
        patientId = $PatientId
        doctorId = $DoctorId
        documentTypeCode = "LAB_RESULTS"
        notes = "Abnormal panel - metabolic and inflammatory"
    } | ConvertTo-Json

    $resp = Invoke-RestMethod -Method POST -Uri "$base/api/documents" -ContentType 'application/json' -Body $body
    if (-not $resp) { throw "Failed to create document" }
    # Handle case/shape differences
    if ($resp -is [System.Array]) {
        # Fallback: query back the most recent matching doc for this patient
        $list = Invoke-RestMethod -Method GET -Uri ("$base/api/documents?patientId={0}" -f $PatientId)
        $match = $list | Where-Object { $_.type -eq 5 -and $_.notes -eq "Abnormal panel - metabolic and inflammatory" } | Sort-Object createdAt -Descending | Select-Object -First 1
        if ($match) { return $match.id }
        $latest = $list | Where-Object { $_.type -eq 5 } | Sort-Object createdAt -Descending | Select-Object -First 1
        if ($latest) { return $latest.id }
        throw "Could not determine created document Id from list response."
    }
    $props = $resp.PSObject.Properties.Name
    if ($props -contains 'Id') { return $resp.Id }
    if ($props -contains 'id') { return $resp.id }
    throw "Response did not include an Id: $($resp | ConvertTo-Json -Depth 5)"
}

function Add-LabResults($docId) {
    # Build an abnormal panel; units chosen to match common LOINC example units
    $results = @(
        @{ LoincCode = "4548-4"; ParameterName = "Hemoglobin A1c"; NumericValue = 8.1; Unit = "%"; ReferenceRange = "<5.7"; Status = "Final"; IsAbnormal = $true },
        @{ LoincCode = "2160-0"; ParameterName = "Creatinine"; NumericValue = 1.8; Unit = "mg/dL"; ReferenceRange = "0.6-1.3"; Status = "Final"; IsAbnormal = $true },
        @{ LoincCode = "2823-3"; ParameterName = "Potassium"; NumericValue = 5.9; Unit = "mmol/L"; ReferenceRange = "3.5-5.1"; Status = "Final"; IsAbnormal = $true },
        @{ LoincCode = "1742-6"; ParameterName = "ALT (SGPT)"; NumericValue = 80; Unit = "U/L"; ReferenceRange = "7-56"; Status = "Final"; IsAbnormal = $true },
        @{ LoincCode = "1988-5"; ParameterName = "C-Reactive protein"; NumericValue = 15; Unit = "mg/L"; ReferenceRange = "<5.0"; Status = "Final"; IsAbnormal = $true }
    )

    $payload = @{
        TestType = "Abnormal panel"
        TestDate = (Get-Date).ToUniversalTime().ToString("o")
        Laboratory = "IMUP Medical Laboratory"
        OverallStatus = "Final"
        Interpretation = "Abnormal findings in HbA1c, CRP, liver enzyme, and electrolytes"
        Results = $results
    } | ConvertTo-Json -Depth 6

    try {
        Invoke-RestMethod -Method POST -Uri "$base/api/documents/$docId/lab-results" -ContentType 'application/json' -Body $payload | Out-Null
    }
    catch {
        Write-Warning "Attach with units failed (likely unit mismatch). Retrying without units..."
        foreach ($r in $results) { $r.Unit = $null }
        $payload2 = @{
            TestType = "Abnormal panel"
            TestDate = (Get-Date).ToUniversalTime().ToString("o")
            Laboratory = "IMUP Medical Laboratory"
            OverallStatus = "Final"
            Interpretation = "Abnormal findings in HbA1c, CRP, liver enzyme, and electrolytes"
            Results = $results
        } | ConvertTo-Json -Depth 6
        Invoke-RestMethod -Method POST -Uri "$base/api/documents/$docId/lab-results" -ContentType 'application/json' -Body $payload2 | Out-Null
    }
}

function Set-AssignmentIfNeeded($docId) {
    if ($AppointmentId) {
        $assign = @{ appointmentId = $AppointmentId } | ConvertTo-Json
        Invoke-RestMethod -Method POST -Uri "$base/api/documents/$docId/assign" -ContentType 'application/json' -Body $assign | Out-Null
    }
}

Resolve-Ids
$docId = New-LabDocument
Add-LabResults -docId $docId
Set-AssignmentIfNeeded -docId $docId

# Verify
$verify = Invoke-RestMethod -Method GET -Uri "$base/api/documents?patientId=$PatientId&type=5"
Write-Host "Seeded abnormal lab results document: $docId"
Write-Host ("Patient {0}: now has {1} lab result document(s)." -f $PatientId, ($verify | Measure-Object).Count)
