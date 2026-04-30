@echo off
setlocal

REM =========================================================
REM YTDownloader 發布腳本
REM 用法: build-installer.bat [版本號]
REM 範例: build-installer.bat 1.0.0
REM =========================================================

SET VERSION=%1
IF "%VERSION%"=="" SET VERSION=1.0.0

SET PROJECT_ROOT=%~dp0..
SET PUBLISH_DIR=%PROJECT_ROOT%\publish
SET INSTALLER_DIR=%~dp0

echo [1/3] 清理並發布 .NET 專案 (版本: %VERSION%)
dotnet publish "%PROJECT_ROOT%\YTDownloader\YTDownloader.csproj" ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=false ^
    -o "%PUBLISH_DIR%"

IF ERRORLEVEL 1 (
    echo [錯誤] dotnet publish 失敗
    exit /b 1
)

echo [2/3] 執行 Inno Setup 打包...

REM 嘗試找到 Inno Setup 編譯器
SET ISCC="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
IF NOT EXIST %ISCC% SET ISCC="C:\Program Files\Inno Setup 6\ISCC.exe"
IF NOT EXIST %ISCC% (
    echo [錯誤] 找不到 Inno Setup，請確認已安裝 Inno Setup 6
    echo 下載網址: https://jrsoftware.org/isdl.php
    exit /b 1
)

%ISCC% /DAppVersion=%VERSION% "%INSTALLER_DIR%setup.iss"

IF ERRORLEVEL 1 (
    echo [錯誤] Inno Setup 打包失敗
    exit /b 1
)

echo [3/3] 完成！
echo 安裝檔位置: %INSTALLER_DIR%Output\YTDownloader_Setup_v%VERSION%.exe

endlocal
