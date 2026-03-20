# AGENTS.md

## Project Overview

- Repository: `scoresheet_gui`
- Product: ScoreSheet, a Windows desktop application for managing the Belgian table tennis competition.
- Stack: C#, WPF, mostly SDK-style .NET Framework 4.8 projects, plus a small MSTest project targeting `netcoreapp3.0`.
- Main user-facing executable: `scoresheet.exe` from the GUI project.

## Repository Layout

- `README.md`
  - High-level project description and license.
- `src/gui`
  - Main GUI solution: `PieterP.ScoreSheet.sln`
  - Primary app project: `PieterP.ScoreSheet.GUI`
  - Supporting projects:
    - `PieterP.ScoreSheet.Model`
    - `PieterP.ScoreSheet.ViewModels`
    - `PieterP.ScoreSheet.Localization`
    - `PdfSharp`
    - `PieterP.ScoreSheet.Tests`
    - `DebugProject`
- `src/shared`
  - Shared solution: `PieterP.Shared.sln`
  - Shared libraries:
    - `PieterP.ScoreSheet.Connector`
    - `PieterP.Shared.Cells`
    - `PieterP.Shared.Services`
- `src/launcher`
  - Separate launcher solution: `PieterP.ScoreSheet.Launcher.sln`
  - Legacy-style .csproj with `packages.config`
- `src/installer`
  - Separate installer solution: `PieterP.ScoreSheet.Installer.sln`
  - Legacy-style .csproj
- `src/publish`
  - Checked-in publish/update payloads, including:
    - `launcher.exe`
    - `versions/<season-version>/scoresheet.exe`
    - `www/` static web assets

## Main Solution Structure

### `src/gui/PieterP.ScoreSheet.sln`

Contains:

- `PieterP.ScoreSheet.GUI`
- `PieterP.ScoreSheet.Localization`
- `PieterP.ScoreSheet.Model`
- `PieterP.ScoreSheet.ViewModels`
- `PdfSharp`
- shared projects from `src/shared`
- `DebugProject`

## Target Frameworks and Build Style

- Most active SDK-style projects target `net48`.
- `src/gui/global.json` pins SDK selection to `3.1`, but local `dotnet` on this machine resolved to SDK `10.0.201` during inspection.
- Test project:
  - `src/gui/PieterP.ScoreSheet.Tests/PieterP.ScoreSheet.Tests.csproj`
  - targets `netcoreapp3.0`
  - uses MSTest
- Older non-SDK projects still exist:
  - `src/launcher/PieterP.ScoreSheet.Launcher/PieterP.ScoreSheet.Launcher.csproj`
  - `src/installer/PieterP.ScoreSheet.Installer/PieterP.ScoreSheet.Installer.csproj`
  - `src/gui/DebugProject/DebugProject.csproj`

## Build and Test Commands

Run from the repository root unless noted otherwise.

### Main GUI solution

```powershell
dotnet build src\gui\PieterP.ScoreSheet.sln
```

### Shared libraries only

```powershell
dotnet build src\shared\PieterP.Shared.sln
```

### Launcher

```powershell
dotnet build src\launcher\PieterP.ScoreSheet.Launcher.sln
```

### Installer

```powershell
dotnet build src\installer\PieterP.ScoreSheet.Installer.sln
```

## Build Notes and Environment Gotchas

- This is a Windows-only codebase in practice because it depends on WPF and .NET Framework 4.8.
- Builds may require Visual Studio Build Tools / Windows SDK components beyond the `dotnet` CLI alone.
- In this Codex environment, `dotnet build` and `dotnet test` were not fully verifiable because MSBuild tried to access:
  - `C:\Users\Pieter\AppData\Local\Microsoft SDKs`
- In this Codex environment, the first `dotnet` invocation also needed a writable CLI home. If needed:

```powershell
$env:DOTNET_CLI_HOME='C:\Users\Pieter\source\repos\scoresheet_gui\.dotnet'
```

- `git` commands may fail in this environment with a dubious ownership warning until the repo is marked safe:

```powershell
git config --global --add safe.directory C:/Users/Pieter/source/repos/scoresheet_gui
```

## Testing

- Test framework: do not run the unit tests after making code changes

## Publishing and Release Artifacts

- `src/publish` appears to contain shipped or deployable artifacts.
- `src/gui/PieterP.ScoreSheet.GUI/Properties/PublishProfiles` contains publish profiles:
  - `CoreProfile.pubxml`
  - `NetFXProfile.pubxml`
- `src/gui/PieterP.ScoreSheet.GUI/VersionHistory.txt` likely matters for release/version tracking.

## Dependencies and Libraries

Common dependencies visible in project files:

- `Unity`
- `Newtonsoft.Json`
- `System.Text.Json`
- `System.Security.Cryptography.ProtectedData`
- `PInvoke.*`
- ASP.NET Core 2.2 packages inside `PieterP.ScoreSheet.Model`
- WCF-related `System.ServiceModel.*` packages in `PieterP.ScoreSheet.Connector`

## Files and Folders Worth Treating Carefully

- `src/publish`
  - Contains release-like binaries and web assets; do not casually delete or regenerate.
- `src/gui/PieterP.ScoreSheet.GUI/PieterP.ScoreSheet.GUI_fty544cm_wpftmp.csproj`
  - Temporary/generated WPF project file; usually not the right file to edit.
- `bin/`, `obj/`, `packages/`
  - Many such directories are present locally, and some outputs are checked in or already exist in the worktree.
  - Prefer editing source files, not build output.

## Practical Editing Guidance for Future Sessions

- Start with `src/gui/PieterP.ScoreSheet.sln` unless the task clearly targets the launcher, installer, or a shared library in isolation.
- For UI behavior, check:
  - `src/gui/PieterP.ScoreSheet.GUI`
  - `src/gui/PieterP.ScoreSheet.ViewModels`
- For business logic or persistence/web-serving behavior, check:
  - `src/gui/PieterP.ScoreSheet.Model`
- For shared reactive/state/service utilities, check:
  - `src/shared/PieterP.Shared.Cells`
  - `src/shared/PieterP.Shared.Services`
  - `src/shared/PieterP.ScoreSheet.Connector`
- Prefer not to touch temporary files, publish outputs, or checked binaries unless the task is explicitly about packaging/release output.
