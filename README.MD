# TimeTracker

Simple desktop time registration app built with C#, Avalonia UI, MVVM, EF Core, and SQLite.

## What this app does

- Manage projects and tasks
- Start and stop a timer for task-based tracking
- Add and edit manual time entries
- View time logs and date-filtered summary data
- Persist data locally with SQLite

## Tech stack

- .NET 10
- Avalonia 12
- CommunityToolkit.Mvvm
- EF Core SQLite

## Prerequisites

- macOS, Windows, or Linux
- .NET SDK 10 installed

Check your SDK:

```bash
dotnet --version
```

## Clone and run

```bash
git clone https://github.com/<your-account>/TimeTracker.git
cd TimeTracker
dotnet restore
dotnet run
```

## Build

```bash
dotnet build -c Debug
dotnet build -c Release
```

## Publish

macOS (Apple Silicon):

```bash
dotnet publish -r osx-arm64 --self-contained -p:PublishSingleFile=true -c Release
```

Windows x64:

```bash
dotnet publish -r win-x64 --self-contained -p:PublishSingleFile=true -c Release
```

## macOS app bundle

This repository includes a helper script to build a .app bundle on macOS:

```bash
bash bundle-macos.sh
```

The script creates TimeTracker.app on your Desktop.

## Using Avalonia with GitHub

This section is a practical workflow for developing Avalonia apps in a GitHub repository.

1. Create a branch for each feature or fix.

```bash
git checkout -b feature/<short-name>
```

2. Make changes and run local checks.

```bash
dotnet restore
dotnet build -c Release
dotnet run
```

3. Commit clear, focused changes.

```bash
git add .
git commit -m "Add <feature-name>"
```

4. Push and open a pull request.

```bash
git push -u origin feature/<short-name>
```

5. In pull requests, include:
- What changed
- Why it changed
- How it was tested
- Screenshots for UI changes

6. Keep Avalonia dependencies consistent by updating package versions together in TimeTracker.csproj.

## Project structure

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
See the LICENSE file for full terms.

## Contributors welcome

We welcome contributors and would love your help improving TimeTracker.

If you want to contribute:

- Open an issue to discuss a bug, idea, or enhancement
- Create a feature branch and keep changes focused
- Submit a pull request with a clear summary and testing notes
- Keep it simple and stupid:-)

Thank you for helping improve the app for everyone.
