# AGENTS.md — agent rules for CO.FAST

Every AI agent working in this repository (Claude Code, Antigravity, Cursor, etc.) follows these rules.

## Source of truth
- The single source of truth is `docs/MASTER_PLAN.md`. If a change falls outside the plan, update the plan first, then the code.
- Do not leave the current phase. Current phase: **MVP**. The limit system (v1) and the settings window / TR-EN UI (v2) are not written in this phase.

## Technology
- C# / .NET 10, `net10.0-windows10.0.19041.0`, WinForms (tray only; no visible form).
- Win32 access only through `src/CoFast/Native/NativeMethods.cs` (`DllImport`). No `unsafe` code.
- No third-party NuGet packages in the MVP. FlaUI is added in v1.
- User-facing text lives in `Localization/Strings.cs`; never hard-code UI text elsewhere.
- All code, comments, docs, and UI text are in English.

## Mandatory security rules
1. The keyboard hook looks only at `PrtScn` (VK_SNAPSHOT) and `Esc`. No other key is stored, logged, or forwarded.
2. The hook never swallows a key; `CallNextHookEx` is always called.
3. The app never connects to the network.
4. Clipboard contents (image or text) are never written to disk or logged.
5. The log contains only event types, process names, and error messages.

## UI thread rules
- The hook and the clipboard listener run on the UI thread. Never block it with `Thread.Sleep`; use `await Task.Delay` where waiting is needed. (Long blocking makes Windows silently remove the low-level hook.)

## Workflow
- One commit per logical step. Commit message format: `type(scope): description` (e.g. `feat(capture): ...`).
- Pure logic (parsing, matching, math) stays in classes separate from Win32 and is tested in `tests/CoFast.Tests`.
- After every step: `dotnet build`, `dotnet test`, and the manual checklist in the README.
