[CmdletBinding()]
param(
    [string]$RepositoryRoot,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

$models = @(
    @{
        Name = 'YuNet face detector'
        Url = 'https://github.com/opencv/opencv_zoo/raw/main/models/face_detection_yunet/face_detection_yunet_2023mar.onnx'
        RelativePath = 'models\face-recognition\yunet\face_detection_yunet_2023mar.onnx'
        Sha256 = '8f2383e4dd3cfbb4553ea8718107fc0423210dc964f9f4280604804ed2552fa4'
        MinimumBytes = 100000
        License = 'MIT, from OpenCV Zoo face_detection_yunet'
        Source = 'https://github.com/opencv/opencv_zoo/tree/main/models/face_detection_yunet'
    },
    @{
        Name = 'SFace face recognizer'
        Url = 'https://github.com/opencv/opencv_zoo/raw/main/models/face_recognition_sface/face_recognition_sface_2021dec.onnx'
        RelativePath = 'models\face-recognition\sface\face_recognition_sface_2021dec.onnx'
        Sha256 = '0ba9fbfa01b5270c96627c4ef784da859931e02f04419c829e83484087c34e79'
        MinimumBytes = 1000000
        License = 'Apache-2.0, from OpenCV Zoo face_recognition_sface'
        Source = 'https://github.com/opencv/opencv_zoo/tree/main/models/face_recognition_sface'
    }
)

function Test-ModelFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][hashtable]$Model
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "$($Model.Name) was not found at $Path."
    }

    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $prefixLength = [Math]::Min(128, $bytes.Length)
    $prefix = [System.Text.Encoding]::ASCII.GetString($bytes, 0, $prefixLength)
    if ($prefix.StartsWith('version https://git-lfs.github.com/spec', [System.StringComparison]::Ordinal)) {
        throw "$($Model.Name) is a Git LFS pointer file, not an ONNX model."
    }

    $file = Get-Item -LiteralPath $Path
    if ($file.Length -lt [int64]$Model.MinimumBytes) {
        throw "$($Model.Name) is smaller than expected. Actual bytes: $($file.Length)."
    }

    $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash.ToLowerInvariant()
    if ($hash -ne $Model.Sha256) {
        throw "$($Model.Name) SHA-256 mismatch. Expected $($Model.Sha256); actual $hash."
    }

    return $hash
}

$scriptRoot = if ([string]::IsNullOrWhiteSpace($PSScriptRoot)) { Split-Path -Parent $MyInvocation.MyCommand.Path } else { $PSScriptRoot }
if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = (Resolve-Path (Join-Path $scriptRoot '..')).Path
}

$resolvedRepositoryRoot = [System.IO.Path]::GetFullPath($RepositoryRoot)
Write-Host "AttendAI face model setup"
Write-Host "Repository root: $resolvedRepositoryRoot"
Write-Host "Only official OpenCV Zoo model URLs are used. No biometric samples are downloaded."

foreach ($model in $models) {
    $destination = [System.IO.Path]::GetFullPath((Join-Path $resolvedRepositoryRoot $model.RelativePath))
    $destinationDirectory = Split-Path -Parent $destination
    New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null

    Write-Host ""
    Write-Host "$($model.Name)"
    Write-Host "Source: $($model.Source)"
    Write-Host "License: $($model.License)"
    Write-Host "Destination: $destination"

    if (Test-Path -LiteralPath $destination) {
        try {
            $hash = Test-ModelFile -Path $destination -Model $model
            if (-not $Force) {
                Write-Host "Already verified. SHA-256: $hash"
                continue
            }

            Write-Host "Existing model is verified, but -Force was supplied; downloading a fresh copy."
        }
        catch {
            if (-not $Force) {
                throw "Existing $($model.Name) is not verified. Re-run with -Force to replace it. $($_.Exception.Message)"
            }

            Write-Host "Replacing unverified existing model because -Force was supplied."
        }
    }

    $temporaryPath = "$destination.download"
    if (Test-Path -LiteralPath $temporaryPath) {
        Remove-Item -LiteralPath $temporaryPath -Force
    }

    try {
        Invoke-WebRequest -Uri $model.Url -OutFile $temporaryPath -UseBasicParsing
        $hash = Test-ModelFile -Path $temporaryPath -Model $model
        Move-Item -LiteralPath $temporaryPath -Destination $destination -Force
        Write-Host "Downloaded and verified. SHA-256: $hash"
    }
    catch {
        if (Test-Path -LiteralPath $temporaryPath) {
            Remove-Item -LiteralPath $temporaryPath -Force
        }

        throw
    }
}

Write-Host ""
Write-Host "Model setup complete. Keep these ONNX files out of Git."
