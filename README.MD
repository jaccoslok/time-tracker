# TimeTracker

Desktop time registration app built with C#, Avalonia UI, MVVM, EF Core, and SQLite.

## What The App Does

- Projects (code, customer, description) and tasks (estimated hours) with hourly rate, billable/productive flags, and "closed" to keep history while hiding them
- Timer with Start, Pause/Resume, Stop, and switching to another task while running
- Manual time entry with project/task picker, edit and delete
- Period filter (Today … All Time) combined with project and task selection
- Summary cards: total, billable %, billable amount, productive %, AI time saved
- Charts: hours per project, amount per project, hours per task, hours per day
- CSV and Excel export of the activities shown
- AI time saved tracking per entry

## Value Based Billing Support

TimeTracker includes a field called AI Time Saved (hours).

Purpose:
- AI may reduce effort for a task while token/API usage introduces cost.
- Tracking AI-saved time helps quantify added value for value-based billing.
- The "AI time saved" summary card shows the total for the selected period, project and task.

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

## Main Window

- Timer bar: project and task pickers, remarks, running clock, Start/Pause/Stop
- Period list, Projects and Tasks tables; "(All …)" rows widen the selection
- Summary cards stay visible; billable and productive percentages turn red below
  their minimum and green at their target
- Activities tab: add/edit/delete, right-click to copy remarks, CSV/Excel export
- Charts tab: bar charts for the current selection

Billing rules: a task's hourly rate overrides its project's rate when it is above 0,
and an activity is billable (or productive) only when both its task and project are.

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
