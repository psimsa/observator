# Observator Repository Overview

## Purpose
Observator is an AOT-compatible source generator for .NET that automatically instruments assemblies with OpenTelemetry tracing, logging, and metrics capabilities. It provides zero-configuration, high-performance observability instrumentation using compile-time code generation and interceptors, making it ideal for modern .NET applications including those using Native AOT compilation.

## General Setup
- **Target Framework**: .NET 9
- **Generator Type**: Incremental Source Generator (`IIncrementalGenerator`)
- **Dependencies**: Only references `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Installation**: Add the Observator NuGet package to your project
- **Usage**: Apply `[ObservatorTrace]` attribute to methods, classes, or interfaces

## Repository Structure
- **src/**: Contains the main source code
  - `Observator/`: Core library
  - `Observator.Generator/`: Source generator implementation
- **test/**: Test projects and applications
  - `RoslynTests/`: Unit tests for the generator
  - `TestApp/`: Integration test application
  - `TestLib/`: Test library for various scenarios
  - `TestShared/`: Shared test utilities
  - `TestWithNuget/`: Tests with NuGet package references
- **.github/**: GitHub workflows and configurations
  - **workflows/**: CI/CD pipelines
    - `dotnet.yml`: Main .NET build and test workflow
    - `nuget.yml`: NuGet package publishing workflow
    - `github-nuget.yml`: GitHub NuGet package publishing workflow
- **Documentation**: README.md, project.md, and other documentation files

## CI/CD Pipelines
The repository includes several GitHub Actions workflows:

1. **.NET Build and Test (dotnet.yml)**:
   - Triggers on pushes to main branch and pull requests
   - Runs on Ubuntu latest
   - Steps: checkout, setup .NET 9, restore dependencies, build, test

2. **NuGet Publishing (nuget.yml)**:
   - Triggers on version tags (v*)
   - Builds and publishes packages to nuget.org
   - Uses Akeyless for secret management

3. **GitHub NuGet Publishing (github-nuget.yml)**:
   - Manual trigger (workflow_dispatch)
   - Builds and publishes packages to GitHub Packages
   - Creates versioned packages with preview suffix for non-main branches

## Code Quality
- **EditorConfig**: Comprehensive .editorconfig with .NET coding conventions
- **No Pre-commit Hooks**: Currently no pre-commit configuration found
- **Build System**: MSBuild integration with modern .NET SDK
- **IDE Support**: Compatible with Visual Studio, VS Code, and other Roslyn-based IDEs

