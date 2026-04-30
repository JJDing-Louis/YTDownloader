<#
.SYNOPSIS
    Docker 容器內的測試入口點腳本。

.DESCRIPTION
    執行 YTDownloaderTest，並將結果輸出至 C:\TestResults。
    支援以下環境變數：
      TEST_FILTER  - NUnit 測試篩選條件（選填），例如 "Category=Unit" 或 "FullyQualifiedName~DBTool"
#>

$testArgs = @(
    "test",
    "YTDownloaderTest/YTDownloaderTest.csproj",
    "--no-restore",
    "--no-build",
    "--configuration", "Debug",
    "--logger", "trx;LogFileName=TestResults.trx",
    "--logger", "console;verbosity=detailed",
    "--results-directory", "C:/TestResults"
)

# 若有設定測試篩選條件，則附加 --filter 參數
if ($env:TEST_FILTER) {
    Write-Host "套用測試篩選條件：$($env:TEST_FILTER)" -ForegroundColor Cyan
    $testArgs += "--filter"
    $testArgs += $env:TEST_FILTER
}

Write-Host "執行指令：dotnet $($testArgs -join ' ')" -ForegroundColor Gray
Write-Host ""

& dotnet @testArgs
$exitCode = $LASTEXITCODE

Write-Host ""
if ($exitCode -eq 0) {
    Write-Host "所有測試通過" -ForegroundColor Green
} else {
    Write-Host "測試失敗（退出碼：$exitCode）" -ForegroundColor Red
}

exit $exitCode
