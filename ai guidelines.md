### Guidelines for AI assistants

- When manipulating projects, prefer using the `dotnet` CLI over direct file manipulation.
- At the end of each task, ensure to run `dotnet clean && dotnet build` to ensure successful compilation. Cleaning the solution first is essential when developing roslyn analyzers or generators to avoid stale build artifacts.
- For code generation tasks, always use `Microsoft.CodeAnalysis.CSharp.SyntaxFactory` to construct syntax trees instead of string interpolation.
- When refactoring code, adhere to the Single Responsibility Principle (SRP) and aim to reduce method complexity.
- New code should be tested with unit tests. Run `dotnet test` to ensure all tests pass after modifications. 