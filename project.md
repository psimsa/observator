# Observator Project

## Mission Statement

Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors. The project aims to provide zero-configuration, high-performance observability instrumentation that works seamlessly with modern .NET applications, including those using Native AOT compilation.

## Goals

### Primary Goals
- **AOT-First Design**: Provide complete compatibility with Native AOT compilation by eliminating runtime reflection and performing all code generation at compile-time
- **Zero Dependencies**: Minimize external dependencies by only referencing `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero Configuration**: Work out-of-the-box with sensible defaults, requiring minimal developer intervention
- **Attribute-Driven Instrumentation**: Enable selective instrumentation through simple decoration of methods, classes, or interfaces with `[ObservatorTrace]`
- **Cross-Assembly Compatibility**: Support instrumentation across project boundaries within a solution

### Secondary Goals
- **High Performance**: Generate optimized code that adds minimal overhead to instrumented methods
- **Developer Experience**: Provide comprehensive diagnostics and clear error messages to guide proper usage
- **Extensibility**: Design modular architecture to support future expansion of observability features
- **Standards Compliance**: Generate code compatible with OpenTelemetry standards and .NET diagnostic conventions

## Basic Parameters

### Technical Architecture
- **Target Framework**: netstandard2.0 for the generator (broad compatibility)
- **Generator Type**: Incremental Source Generator (`IIncrementalGenerator`) using .NET 9 APIs
- **Interception Method**: Compile-time method call interception using `[InterceptsLocation]` attribute
- **Code Generation**: Emit both infrastructure code and method interceptors as separate source files

### Core Components
1. **InterceptorGenerator**: Creates method interceptors for attributed methods with observability instrumentation
2. **Diagnostics System**: Provides compile-time warnings and errors to guide correct usage
3. **Template System**: (Planned) Extensible code generation templates for different instrumentation patterns

### Key Design Principles
- **Compile-Time Only**: All code generation happens during compilation, no runtime dependencies
- **Non-Invasive**: Original method bodies are preserved in private clone methods
- **Exception Safe**: Generated interceptors handle exceptions properly and maintain original behavior
- **Async/Iterator Support**: Full support for async methods, Task-returning methods, and iterator methods

### Supported Scenarios
- Block-bodied and expression-bodied methods
- Async methods (`async Task`, `async ValueTask`)
- Task-returning methods (with diagnostic warnings)
- Iterator methods (`yield return`, `yield break`)
- Methods with various parameter types (with diagnostics for problematic cases)
- Cross-class and cross-assembly method calls

### Quality Standards
- **Build System Integration**: Seamless integration with MSBuild and modern .NET SDK
- **IDE Support**: Full compatibility with Visual Studio, VS Code, and other Roslyn-based IDEs
- **Testing Coverage**: Comprehensive test application covering all supported method patterns
- **Documentation**: Clear documentation of features, limitations, and best practices

### Limitations
- **Class-Level Attributes Supported**: The `[ObservatorTrace]` attribute can be applied to methods, classes, or interfaces. For classes and interfaces, all public instance methods will be traced.
    - **Conflict Rule:** You cannot use `[ObservatorTrace]` on both a class and its methods at the same time. If both are present, a compile-time error will be emitted.
- **No Attribute-Level Configuration**: The `[ObservatorTrace]` attribute does not support configuration properties (e.g., `IncludeParameters`).
- **Limited Parameter Support**: Complex parameter types (e.g., pointers, `ref struct`) are not supported and will result in diagnostic warnings.
- **No Generic Method Support**: Generic methods cannot be intercepted.
- **No Constructor/Property Support**: Constructors and property accessors cannot be intercepted.
- **Internal Methods Only**: Only internal and public methods can be intercepted due to accessibility constraints.

## Roadmap

### Version 1.0 (Current)
- **Core Functionality**: Basic method interception and `ActivitySource` generation
- **Supported Patterns**: Block-bodied, expression-bodied, async, and iterator methods
- **Diagnostics**: Initial set of diagnostic warnings for unsupported scenarios

### Version 1.1 (Planned)
- **Metrics Support**: Add `Meter` and `Counter` generation for basic metrics
- **Logging Support**: Integrate with `ILogger` for structured logging
- **Enhanced Diagnostics**: Improve diagnostic messages and add more analyzers

### Version 2.0 (Future)
- **Extensibility API**: Allow custom instrumentation via a public API
- **Template System**: Implement a template-based code generation system
- **Advanced Scenarios**: Support for generic methods and other complex patterns
- **Performance Tuning**: Optimize generated code for even lower overhead

## Contribution Guidelines

### Reporting Issues
- Use the GitHub issue tracker to report bugs or suggest features
- Provide detailed steps to reproduce the issue, including code snippets and environment details

### Submitting Pull Requests
- Fork the repository and create a new branch for your changes
- Follow the existing code style and conventions
- Ensure all tests pass before submitting a pull request
- Provide a clear description of the changes in the pull request

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
