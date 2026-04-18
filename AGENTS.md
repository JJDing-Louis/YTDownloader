# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## Project Overview

YTDownloader is a Windows desktop application built with C# (.NET 8) and Windows Forms (WinForms). It is currently in early scaffolding stage — the default WinForms project template with no download logic yet implemented.

## Build & Run

```bash
# Build the project
dotnet build YTDownloader/YTDownloader.csproj

# Run the application
dotnet run --project YTDownloader/YTDownloader.csproj

# Build in Release mode
dotnet build YTDownloader/YTDownloader.csproj -c Release
```

The project targets `net8.0-windows` and requires Windows to run (WinForms dependency).

## Architecture

- **Single-project solution** — `YTDownloader.sln` contains one project at `YTDownloader/YTDownloader.csproj`
- **Entry point** — `Program.cs` bootstraps the WinForms application and launches `Form1`
- **Main form** — `Form1.cs` (logic) + `Form1.Designer.cs` (auto-generated layout code, do not edit manually)
- `Form1.resx` holds embedded resources for the form

## Key Technical Details

- Target framework: `net8.0-windows`
- Output type: `WinExe` (Windows GUI application, no console)
- Nullable reference types enabled
- Implicit global usings enabled
- No NuGet packages added yet — download functionality (e.g., yt-dlp integration) still needs to be implemented
