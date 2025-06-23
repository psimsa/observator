# Observator Project

## Mission Statement

Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors. The project aims to provide zero-configuration, high-performance observability instrumentation that works seamlessly with modern .NET applications, including those using Native AOT compilation.

## Goals

### Primary Goals
- **AOT-First Design**: Provide complete compatibility with Native AOT compilation by eliminating runtime reflection and performing all code generation at compile-time
- **Zero Dependencies**: Minimize external dependencies by only referencing `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero Configuration**: Work out-of-the-box with sensible defaults, requiring minimal developer intervention
- **Attribute-Driven Instrumentation**: Enable selective instrumentation through simple method decoration with `[ObservatorTrace]`
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
1. **InfrastructureGenerator**: Generates per-assembly `ActivitySource`, `Meter`, and attribute definitions
2. **InterceptorGenerator**: Creates method interceptors for attributed methods with observability instrumentation
3. **Diagnostics System**: Provides compile-time warnings and errors to guide correct usage
4. **Template System**: (Planned) Extensible code generation templates for different instrumentation patterns

### Key Design Principles
- **Compile-Time Only**: All code generation happens during compilation, no runtime dependencies
- **Partial Class Requirement**: Intercepted methods must be in partial classes to enable code injection
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
