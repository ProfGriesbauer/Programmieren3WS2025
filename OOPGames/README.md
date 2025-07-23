# OOPGames

This directory contains a WPF project that can be built with the `dotnet` CLI and
used with Visual Studio Code. The project file has been converted to the modern
SDK style so that it works with recent .NET versions.

## Requirements

- .NET SDK 6.0 or newer installed
- Windows machine (WPF apps require Windows)

## Building and running

1. Open the repository in VS Code.
2. Ensure the C# extension is installed.
3. Use the integrated terminal or the provided tasks (`Ctrl+Shift+B`) to build
   the project.
4. Start debugging with `F5` to launch the WPF application.

The configuration files in `.vscode` contain tasks and a launch profile that use
`dotnet` to build and run `OOPGames`.
