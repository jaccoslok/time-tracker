# TimeTracker — Claude Code instructions

## Before we start

- I want Claude to explain every step of the build process because I am learning c#
- Naming conventions and code: follow Microsoft best practice
- Whether you want unit tests added per phase >> not yet, later stadium, focus on learning first
- Avoid packages or mcp tools that require registration and payment
- If we resume a session we always start from the beginning of that session with a small summarization of the previous sessions

---

## Project brief

Build a desktop time registration app called **TimeTracker** using C# with AvaloniaUI and MVVM
pattern. The app runs on macOS (and Windows). Use .NET 8, AvaloniaUI 11, EF Core with SQLite
for persistence.

---

## Data model

| Entity | Fields |
|---|---|
| `Project` | Id, Name, Description, IsActive, CreatedAt |
| `ProjectTask` | Id, ProjectId (FK), Name, Description, IsActive |
| `TimeEntry` | Id, ProjectTaskId (FK), StartedAt, StoppedAt, Duration (computed), Notes |

---

## Features

1. Main window with two panels — project/task selector on the left, time log on the right
2. Start/stop timer button that records a TimeEntry when stopped
3. Manual time entry (pick task, enter start/end time or duration)
4. CRUD for Projects and ProjectTasks via simple dialogs
5. Weekly summary view — hours per project, hours per task, total hours
6. Optional: export summary to CSV

---

## Technical requirements

- MVVM with `INotifyPropertyChanged`, `ObservableCollection<T>`, and `RelayCommand`
- EF Core `DbContext` with SQLite, database file stored in the user's app data folder
- Repository pattern — `ProjectRepository`, `TaskRepository`, `TimeEntryRepository`
- Live timer using `DispatcherTimer` from `Avalonia.Threading`
- `ResourceDictionary` for consistent styles (colors, button styles, fonts)
- Use `CommunityToolkit.Mvvm` for MVVM boilerplate (`ObservableObject`, `RelayCommand`,
  `[ObservableProperty]` source generator)

---

## Project structure

```
TimeTracker/
  Models/           # EF Core entities
  ViewModels/       # One ViewModel per view, BaseViewModel with INPC
  Views/            # AXAML files + code-behind
  Repositories/     # Data access layer
  Data/             # DbContext + migrations
  Helpers/          # RelayCommand, converters
```

---

## Build phases

Build this in phases. **Complete each phase fully before moving to the next.**
After each phase, stop and summarise what was built and what comes next.

---

### Phase 1 — C# fundamentals in context

Scaffold the project and create the data model. This phase is about getting comfortable with
C# classes, properties, collections, and LINQ before any UI exists.

**Steps:**

- Run `dotnet new avalonia.mvvm -n TimeTracker`
- Add NuGet packages: `Microsoft.EntityFrameworkCore.Sqlite`,
  `Microsoft.EntityFrameworkCore.Tools`, `CommunityToolkit.Mvvm`
- Create all three entity classes in `Models/` with correct types and FK relationships
- Create `AppDbContext` in `Data/` with `DbSet<T>` for each entity
- Run initial EF Core migration and verify the SQLite database is created
- Write a small console test (temporary method in `App.axaml.cs`) that creates a Project,
  adds a Task, adds a TimeEntry, then reads them back and prints totals using LINQ
  `.GroupBy()` and `.Sum()`
- Delete the test code once it works

**Demonstrates:** C# classes, properties, `List<T>`, LINQ `.Where()` / `.Select()` / `.Sum()`
/ `.GroupBy()`, `DateTime` arithmetic, EF Core basics.

---

### Phase 2 — Avalonia layout and MVVM pattern

Build the main window UI bound to a ViewModel, with no database yet — use hardcoded
sample data.

**Steps:**

- Create `BaseViewModel` implementing `INotifyPropertyChanged`
- Create `MainViewModel` with an `ObservableCollection<Project>` and
  `ObservableCollection<ProjectTask>` populated with 2–3 hardcoded items
- Build `MainWindow.axaml` with a two-panel layout: project list on the left (ListBox bound
  to Projects), task list on the right (ListBox bound to Tasks for the selected project)
- Wire `SelectedProject` in the ViewModel so selecting a project filters the task list
- Add a status bar at the bottom showing selected project and task name
- No database calls in this phase

**Demonstrates:** AXAML layout with `Grid` and `StackPanel`, data binding with `{Binding}`,
`ObservableCollection`, `INotifyPropertyChanged`, `[ObservableProperty]` source generator,
`SelectedItem` binding.

---

### Phase 3 — SQLite persistence with EF Core

Replace the hardcoded data with real database reads and writes.

**Steps:**

- Implement `ProjectRepository`, `TaskRepository`, `TimeEntryRepository` in `Repositories/`
  — each with `GetAll()`, `Add()`, `Update()`, `Delete()` methods
- Inject repositories into `MainViewModel` via constructor
- Replace hardcoded collections with data loaded from SQLite on window open
- Add dialogs (simple Avalonia `Window`) for adding and editing Projects and ProjectTasks
- Add delete with confirmation for both
- Verify data persists across app restarts

**Demonstrates:** repository pattern, EF Core CRUD, `async/await`, basic Avalonia dialog
windows.

---

### Phase 4 — Complete app with timer and reporting

Wire up the full feature set.

**Steps:**

- Add timer logic to `MainViewModel`: `StartCommand`, `StopCommand`, a `DispatcherTimer`
  that ticks every second, an `ElapsedTime` string property bound to a label in the UI
- On stop: calculate duration, create a `TimeEntry`, save via repository, add to the
  displayed log
- Add manual time entry: a form with task picker, date, start time, end time, optional notes
- Build a `SummaryViewModel` and `SummaryView` showing a `DataGrid` with hours grouped by
  project and task for the current week
- Add a date range picker to the summary to filter by custom range
- Optional: export summary to CSV using `StreamWriter`

**Demonstrates:** `DispatcherTimer`, `TimeSpan` arithmetic, async repository calls from
ViewModel, LINQ `.GroupBy()` for reporting, `DataGrid` binding, basic Avalonia navigation
between views.

---

### Phase 5 — Polish and packaging

Make it production-ready.

**Steps:**

- Add a `ResourceDictionary` with consistent button styles, colors, and font sizes applied
  across all views
- Persist window size and position across restarts using a simple JSON settings file in the
  app data folder (`System.Text.Json`)
- Publish for macOS:
  ```bash
  dotnet publish -r osx-arm64 --self-contained -p:PublishSingleFile=true -c Release
  ```
- Publish for Windows:
  ```bash
  dotnet publish -r win-x64 --self-contained -p:PublishSingleFile=true -c Release
  ```
- Verify the app runs from the published binary with no .NET runtime installed

**Demonstrates:** `ResourceDictionary` styling, `System.Text.Json` for simple settings,
cross-platform dotnet publish.

---

## General instructions

- After scaffolding, always run `dotnet run` to confirm the app compiles before adding more
  code
- If a NuGet package is needed, add it before writing code that depends on it
- Use `Path.Combine()` for all file paths — never hardcode `/` or `\`
- Use `Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)` for the
  database and settings file location
- Add a brief inline comment on anything non-obvious to someone coming from an AL/C#
  background
- If a phase produces a compiler error that isn't immediately obvious, explain what caused
  it before fixing it — this is a learning project
- Do not move to the next phase until the current phase compiles and runs without errors
