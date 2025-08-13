param(
  [Parameter(Mandatory=$true)][string]$InputPath,
  [Parameter(Mandatory=$true)][string]$OutputPath,
  [string]$EffectiveFrom = '2025-10-01',
  [string]$EffectiveTo = '',
  [string]$Status = 'active'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (!(Test-Path -LiteralPath $InputPath)) { throw "Input file not found: $InputPath" }

# Import CSV with UTF8; tolerate variable headers (code/title or code/desc/description)
$rows = Import-Csv -LiteralPath $InputPath -Encoding UTF8
if ($rows.Count -eq 0) { throw "Empty CSV: $InputPath" }

# Determine column names for code and title
$headers = @($rows[0].PSObject.Properties.Name)
$codeCol = $headers | Where-Object { $_ -match '^(code)$' } | Select-Object -First 1
if (-not $codeCol) { throw "Cannot find 'code' column in input CSV." }
$titleCol = $headers | Where-Object { $_ -match '^(title|desc|description)$' } | Select-Object -First 1
if (-not $titleCol) { throw "Cannot find a title/desc/description column in input CSV." }

# Build enriched rows with desired column order
$enriched = foreach ($r in $rows) {
  $code = ($r.$codeCol).ToString().Trim()
  if ([string]::IsNullOrWhiteSpace($code)) { continue }
  $title = ($r.$titleCol).ToString()
  [PSCustomObject]@{
    code = $code
    title = $title
    effective_from = $EffectiveFrom
    effective_to = $EffectiveTo
    status = $Status
  }
}

# Export using UTF8 without type info
$null = New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputPath)
$enriched | Export-Csv -LiteralPath $OutputPath -NoTypeInformation -Encoding UTF8
Write-Host "Wrote enriched CSV: $OutputPath (`$($enriched.Count) rows)"
