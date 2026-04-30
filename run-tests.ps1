<#
.SYNOPSIS
    在 Windows Docker 容器中執行 YTDownloaderTest。

.DESCRIPTION
    此腳本會：
      1. 確認 Docker Desktop 已切換至 Windows 容器模式
      2. 建立 TestResults 輸出目錄
      3. 以 docker-compose 建置映像並執行測試
      4. 顯示結果摘要並清理容器

.PARAMETER Filter
    NUnit 測試篩選條件（傳入 dotnet test --filter）。
    例如：
      .\run-tests.ps1 -Filter "FullyQualifiedName~DBTool"
      .\run-tests.ps1 -Filter "Category=Unit"
      .\run-tests.ps1 -Filter "TestName=UpsertDownloadHistoryTest"

.PARAMETER NoBuild
    跳過映像重新建置（使用現有映像），加快執行速度。

.PARAMETER KeepContainer
    測試結束後保留容器，方便除錯（預設自動移除）。

.EXAMPLE
    .\run-tests.ps1
    執行所有測試

.EXAMPLE
    .\run-tests.ps1 -Filter "Category!=Integration"
    排除整合測試，只執行單元測試

.EXAMPLE
    .\run-tests.ps1 -NoBuild -Filter "FullyQualifiedName~AdjustableConcurrencyLimiter"
    不重建映像，只執行指定測試類別
#>
param(
    [string] $Filter       = "",
    [switch] $NoBuild,
    [switch] $KeepContainer
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

# ── 工具函式 ──────────────────────────────────────────────────────────────────
function Write-Step([string]$msg) {
    Write-Host ""
    Write-Host ">>> $msg" -ForegroundColor Cyan
}

function Test-DockerWindowsMode {
    try {
        $osType = docker info --format "{{.OSType}}" 2>$null
        return ($osType -eq "windows")
    }
    catch {
        return $false
    }
}

function Test-DockerRunning {
    try {
        docker info *>$null
        return $true
    }
    catch {
        return $false
    }
}

# ── 前置確認 ──────────────────────────────────────────────────────────────────
Write-Step "確認 Docker 環境"

if (-not (Test-DockerRunning)) {
    Write-Host "Docker 未啟動，請先啟動 Docker Desktop。" -ForegroundColor Red
    exit 1
}

if (-not (Test-DockerWindowsMode)) {
    Write-Host ""
    Write-Host "[錯誤] Docker 目前為 Linux 容器模式。" -ForegroundColor Red
    Write-Host ""
    Write-Host "YTDownloaderTest 包含 WinForms 測試（net8.0-windows），" -ForegroundColor Yellow
    Write-Host "必須使用 Windows 容器才能正確執行。" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "切換方式：Docker Desktop 系統匣圖示 > 右鍵 > Switch to Windows containers..." -ForegroundColor Cyan
    Write-Host ""
    exit 1
}

Write-Host "Docker Windows 容器模式：OK" -ForegroundColor Green

# ── 建立 TestResults 目錄 ─────────────────────────────────────────────────────
$testResultsDir = Join-Path $PSScriptRoot "TestResults"
if (-not (Test-Path $testResultsDir)) {
    Write-Step "建立 TestResults 目錄"
    New-Item -ItemType Directory -Path $testResultsDir | Out-Null
    Write-Host "已建立：$testResultsDir" -ForegroundColor Green
}

# ── 設定篩選條件環境變數 ──────────────────────────────────────────────────────
if ($Filter) {
    $env:TEST_FILTER = $Filter
    Write-Step "套用測試篩選：$Filter"
}
else {
    $env:TEST_FILTER = ""
}

# ── 組合 docker-compose 參數 ──────────────────────────────────────────────────
$composeArgs = @(
    "-f", "docker-compose.test.yml",
    "up",
    "--abort-on-container-exit"
)

if (-not $NoBuild) {
    $composeArgs += "--build"
    Write-Step "建置測試映像並執行測試"
}
else {
    Write-Step "執行測試（使用現有映像）"
}

Write-Host "指令：docker-compose $($composeArgs -join ' ')" -ForegroundColor Gray
Write-Host ""

# ── 執行測試 ──────────────────────────────────────────────────────────────────
docker-compose @composeArgs
$exitCode = $LASTEXITCODE

# ── 顯示結果 ──────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host ("=" * 60) -ForegroundColor DarkGray

if ($exitCode -eq 0) {
    Write-Host "  測試結果：通過" -ForegroundColor Green
}
else {
    Write-Host "  測試結果：失敗（退出碼：$exitCode）" -ForegroundColor Red
}

$trxFile = Join-Path $testResultsDir "TestResults.trx"
if (Test-Path $trxFile) {
    Write-Host "  TRX 報告：$trxFile" -ForegroundColor Cyan
}

Write-Host ("=" * 60) -ForegroundColor DarkGray

# ── 清理容器 ──────────────────────────────────────────────────────────────────
if (-not $KeepContainer) {
    Write-Step "清理容器"
    docker-compose -f docker-compose.test.yml down --remove-orphans
}
else {
    Write-Host ""
    Write-Host "已保留容器（-KeepContainer）。完成後請手動執行：" -ForegroundColor Yellow
    Write-Host "  docker-compose -f docker-compose.test.yml down" -ForegroundColor Gray
}

exit $exitCode
