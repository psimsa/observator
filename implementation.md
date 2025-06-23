# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a non-static interceptor method as a member of the same (partial) class, with `[InterceptsLocation]`:
  - The class must be `partial` to allow code injection.
  - Emit a diagnostic if the annotated method is not in a partial class.
- The interceptor method:
  - Outputs a message for now (later: starts an Activity, calls original method, etc.)
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error (future step)
- Emit all interceptors as a `.g.cs` file in the same namespace as the target class.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

### Phase X: Interface Interception Support (Future)

This phase explores and documents options for supporting interception of interface methods, especially in dependency injection (DI) scenarios.

#### 1. Direct Interception of Interface Methods (Current .NET 9 Interceptors)
- Interceptors generated for interface methods only work when the call is statically dispatched to the concrete implementation.
- Calls made via an interface reference (e.g., through DI) are **not** intercepted by default.
- This is a limitation of the .NET 9 source generator interceptors model, which rewrites call sites to concrete types, not interface dispatch.

#### 2. Proxy Generation for Interface Interception
- Annotate the interface method (not the implementation).
- The generator emits a proxy class implementing the interface, wrapping the real implementation and adding interception logic.
- The proxy is registered in the DI container in place of the real implementation.
- All interface calls (including those via DI) are intercepted by the proxy.
- The generator can:
  - Generate proxy types for annotated interfaces.
  - Generate DI registration helpers (e.g., `services.AddObservatorProxies()`).
  - Emit diagnostics to guide users to update their DI registration.
- **Limitation:** Source generators cannot rewrite user DI registration code automatically; user must opt-in to use the proxy.

#### 3. Intercepting DI Registration Calls Directly
- It is technically possible to generate interceptors for DI registration calls (e.g., `services.AddTransient<IService, Service>()`) using `[InterceptsLocation]`.
- The generator can emit a unique interceptor for each registration call site, matching the method signature and location.
- The interceptor can register a proxy or perform custom logic.
- **Limitations:**
  - Each registration call site requires a unique interceptor.
  - This approach can be brittle and complex for large codebases.
  - The intent behind the registration is not always clear to the generator.

#### 4. AOP Frameworks and Other Approaches
- Use third-party frameworks (e.g., Castle DynamicProxy, DispatchProxy, Fody, PostSharp) to intercept interface calls.
- These frameworks can intercept interface calls transparently, but may not be AOT-friendly or source generator-based.

#### Summary Table
| Approach                | Intercepts Interface Calls | DI Friendly | Pure Source Gen | Complexity |
|-------------------------|:-------------------------:|:-----------:|:---------------:|:----------:|
| Direct Interceptor      | No                        | Yes         | Yes             | Low        |
| Proxy Generation       | Yes                       | Yes         | Yes             | Medium     |
| DI Call Interception   | Yes (per call site)       | Yes         | Yes             | Medium/High|
| AOP Framework          | Yes                       | Yes         | No              | Medium/High|

#### Recommendation
- For robust interface interception in DI scenarios, proxy generation is the most maintainable and AOT-compatible approach.
- Direct interception of DI registration calls is possible but less maintainable for large projects.
- The generator should provide diagnostics and helpers to guide users toward best practices.

---

### Phase X+1: Diagnostics Emission (Planned)

This phase will add Roslyn diagnostics to improve developer experience and guide correct usage of Observator. Below is a list of planned diagnostics, their descriptions, and when they are emitted:

| Diagnostic ID         | Description                                                                 | When Emitted                                                      |
|----------------------|-----------------------------------------------------------------------------|-------------------------------------------------------------------|
| OBS001               | Method annotated with [ObservatorTrace] is not in a partial class            | When an attributed method is found in a non-partial class          |
| OBS002               | [ObservatorTrace] attribute applied to an abstract method                    | When an abstract method is annotated (no interception possible)    |
| OBS003               | [ObservatorTrace] attribute applied to an interface method (proxy required)  | When an interface method is annotated (suggest proxy generation or moving logic to a private method)   |
| OBS004               | Multiple methods with the same signature and [ObservatorTrace] in one class  | When duplicate attributed methods are found in a class             |
| OBS005               | [ObservatorTrace] on async/Task-returning method (async support warning)     | When an attributed method is async or returns Task/ValueTask       |
| OBS006               | [ObservatorTrace] on method with unsupported signature (e.g., ref/out)       | When an attributed method uses unsupported parameter types         |
| OBS007               | DI registration detected for interface with [ObservatorTrace]                | When DI registration is found for an interface with annotation     |
| OBS008               | [ObservatorTrace] on method with no body (e.g., extern, partial, abstract)   | When an attributed method has no body to copy                      |
| OBS009               | [ObservatorTrace] on method with struct parameter (allocation warning)       | When an attributed method has any struct (value type) parameter    |

These diagnostics help ensure correct usage, avoid unsupported scenarios, and guide users toward best practices for AOT-friendly interception and instrumentation.

---

### Planned Features

- **Logger Detection and Usage:**
  - When generating observability code for an annotated method, the generator should attempt to detect if the containing class has a logger field or property using common patterns (e.g., `log`, `_log`, `_logger`, `logger`).
  - If a logger is found, use it for logging instead of `Console`.
  - If no logger is found, fall back to using `Console` as currently implemented.
  - This feature is not yet implemented, but is planned to improve integration with existing logging infrastructure and developer experience.

## Current State

### Generator
- The generator is implemented as a .NET 9 incremental source generator (`IIncrementalGenerator`) and uses the new Roslyn interceptors API.
- For each `[ObservatorTrace]`-annotated method, the generator emits:
  - A private `_Clone` method containing the original method body (block or expression-bodied, with async/iterator support in progress).
  - An internal interceptor method with `[InterceptsLocation]` that wraps the clone call in a try/catch/finally block, logging method start, exception, and end.
  - The interceptor body structure is externalized for future customization.
- The generator supports most C# method constructs and is ready for further extension.

### Test Application
- The test app (`TestApp`) includes coverage for:
  - Block-bodied and expression-bodied methods
  - Iterator methods
  - Async iterator methods
  - Async Task methods
  - Task-returning methods (non-async)
  - Methods with `ref`/`out` parameters
  - Methods with `struct` parameters (for future diagnostics)
- The build is successful and generated code is valid for all supported patterns.

---

### Diagnostics (Planned and Implemented)

| Diagnostic ID         | Description                                                                 | When Emitted                                                      |
|----------------------|-----------------------------------------------------------------------------|-------------------------------------------------------------------|
| OBS001               | Method annotated with [ObservatorTrace] is not in a partial class            | When an attributed method is found in a non-partial class          |
| OBS002               | [ObservatorTrace] attribute applied to an abstract method                    | When an abstract method is annotated (no interception possible)    |
| OBS003               | [ObservatorTrace] attribute applied to an interface method (proxy required)  | When an interface method is annotated (suggest proxy generation or moving logic to a private method)   |
| OBS004               | Multiple methods with the same signature and [ObservatorTrace] in one class  | When duplicate attributed methods are found in a class             |
| OBS005               | [ObservatorTrace] on async/Task-returning method (async support warning)     | When an attributed method is async or returns Task/ValueTask       |
| OBS006               | [ObservatorTrace] on method with unsupported signature (e.g., ref/out)       | When an attributed method uses unsupported parameter types         |
| OBS007               | DI registration detected for interface with [ObservatorTrace]                | When DI registration is found for an interface with annotation     |
| OBS008               | [ObservatorTrace] on method with no body (e.g., extern, partial, abstract)   | When an attributed method has no body to copy                      |
| OBS009               | [ObservatorTrace] on method with struct parameter (allocation warning)       | When an attributed method has any struct (value type) parameter    |

These diagnostics help ensure correct usage, avoid unsupported scenarios, and guide users toward best practices for AOT-friendly interception and instrumentation.