param(
    [Parameter(Mandatory=$true)] [string]$InputPath,
    [Parameter(Mandatory=$true)] [string]$OutputPath
)

if (-not (Test-Path -LiteralPath $InputPath)) {
    Write-Error "Input file not found: $InputPath"
    exit 1
}

$lines = Get-Content -LiteralPath $InputPath -Encoding UTF8

# Write header
"code,title" | Out-File -LiteralPath $OutputPath -Encoding utf8

foreach ($line in $lines) {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    if ($line -match '^(\S+)\s+(.+)$') {
        $code = $Matches[1].Trim()
        $title = $Matches[2].Trim()
        $title = $title -replace '"','""'
        '"{0}","{1}"' -f $code, $title | Out-File -LiteralPath $OutputPath -Append -Encoding utf8
    }
}

Write-Host "Wrote CSV: $OutputPath"
