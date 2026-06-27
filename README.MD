# TimeTracker

Desktop time registration app built with C#, Avalonia UI, MVVM, EF Core, and SQLite.

## What The App Does

- Project and task management (create, edit, delete)
- Start/stop timer tracking per task
- Manual time entry with edit/delete support
- Summary screen with date filters
- Excel export for filtered summary results
- AI time saved tracking per entry

## Value Based Billing Support

TimeTracker includes a field called AI Time Saved (hours).

Purpose:
- AI may reduce effort for a task while token/API usage introduces cost.
- Tracking AI-saved time helps quantify added value for value-based billing.
- Summary view includes a filtered total: Timed saved by AI.

This gives teams two perspectives:
- Delivery effort (Total hours)
- AI-enabled value gain (Timed saved by AI)

## Tech Stack

- .NET 10
- Avalonia 12
- CommunityToolkit.Mvvm
- EF Core SQLite
- ClosedXML (Excel export)

## Prerequisites

- macOS, Windows, or Linux
- .NET SDK 10.0.300 (pinned via global.json)

Check your SDK:

```bash
dotnet --version
```

## Contributor Setup (Dependencies)

.NET dependencies are managed by project files and restored automatically. There is no separate requirements.txt like Python.

Run this once after cloning:

```bash
dotnet restore
dotnet tool restore
```

What these do:
- dotnet restore: restores NuGet packages from TimeTracker.csproj
- dotnet tool restore: restores local CLI tools from dotnet-tools.json (for example dotnet-ef)

Reproducibility setup in this repo:
- global.json pins the SDK version
- packages.lock.json pins resolved NuGet dependency graph

## Clone And Run

```bash
git clone https://github.com/<your-account>/TimeTracker.git
cd TimeTracker
dotnet restore
dotnet tool restore
dotnet run
```

## Build

```bash
dotnet build -c Debug
dotnet build -c Release
```

## Database Migrations (EF Core)

Use the local tool so contributors run the same EF version:

```bash
dotnet tool restore
dotnet dotnet-ef migrations add <MigrationName>
dotnet dotnet-ef database update
```

## Summary Screen Features

- Filter by From/To date and quick Today button
- Per-entry AI Time column
- Footer totals:
	- Total hours
	- Timed saved by AI (based on current filter)
- Export to Excel button for filtered rows and totals

## Publish

macOS (Apple Silicon):

```bash
dotnet publish -r osx-arm64 --self-contained -p:PublishSingleFile=true -c Release
```

Windows x64:

```bash
dotnet publish -r win-x64 --self-contained -p:PublishSingleFile=true -c Release
```

## GitHub Releases (Automated)

This repository includes a GitHub Actions workflow at .github/workflows/release.yml.
When you push a tag starting with v (for example v1.0.0), it will:

- Build macOS arm64 publish output
- Build Windows x64 publish output
- Zip both artifacts
- Create a GitHub Release and attach both zip files

Trigger a release with:

```bash
git tag v1.0.0
git push origin v1.0.0
```

## macOS App Bundle

This repository includes a helper script to build a .app bundle on macOS:

```bash
bash bundle-macos.sh
```

The script creates TimeTracker.app on your Desktop.

## Project Structure

```text
TimeTracker/
	Models/
	ViewModels/
	Views/
	Repositories/
	Data/
	Helpers/
	Styles/
```

## License

This project is licensed under the GNU General Public License v3.0.
See LICENSE for details.

## Contributors Welcome

- Open an issue for bugs, ideas, or enhancements
- Use a focused feature branch
- Submit a pull request with testing notes and UI screenshots where relevant
- Keep changes small and clear
