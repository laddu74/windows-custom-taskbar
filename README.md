# windows-custom-taskbar

A custom branding bar/taskbar for Windows 11 written in C# WinForms.

## Features
- Replaces the default Windows 11 Taskbar with a custom dark aesthetic.
- Hooks directly into native `SHAppBarMessage` and `user32.dll` APIs.
- Automatically hides the default Windows taskbar and forces an unlocked state.
- Simulates the Windows key to open the Start menu natively.

## Branching Strategy
- **main**: Stable production code.
- **feature/***: New features are developed here and merged into main.
- **bug-fix/***: Hotfixes and bug resolutions are developed here and merged into main.
