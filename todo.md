# Observator Development Todo

This document outlines the planned features and tasks for completing the Observator project. Each task includes a title, description, specification, and an AI prompt for implementation.

## Phase 3: Enhanced Observability Integration

### Task 1: ActivitySource Integration

**Title**: Integrate Generated ActivitySource into Interceptors

**Description**: Replace console logging with proper OpenTelemetry Activity tracing using the generated ActivitySource. This will provide structured tracing that integrates with OpenTelemetry tooling.

**Specification**:
- Update `InterceptorGenerator.GenerateInterceptorBody` to use `ObservatorInfrastructure.ActivitySource`
- Start an Activity at the beginning of each interceptor method
- Set Activity status to Error on exceptions
- Add method name and parameters as Activity tags (when enabled)
- Dispose Activity properly in finally block
- Maintain backward compatibility with existing logging

**AI Prompt**:
```
Update the Observator InterceptorGenerator to use OpenTelemetry Activities instead of Console.WriteLine for tracing. 

Requirements:
1. Modify GenerateInterceptorBody method in InterceptorGenerator.cs
2. Use ObservatorInfrastructure.ActivitySource.StartActivity(methodName) at the start
3. Set activity.SetStatus(ActivityStatusCode.Error, ex.Message) on exceptions
4. Add activity tags for method name and optionally parameters
5. Ensure activity is disposed in finally block
6. Keep the existing try/catch/finally structure
7. Handle both sync and async methods properly
8. Add using statements for System.Diagnostics

The generated code should look like:
```csharp
internal returnType MethodName_Interceptor(parameters)
{
    using var activity = ObservatorInfrastructure.ActivitySource.StartActivity("MethodName");
    try
    {
        return await this.MethodName_Clone(args);
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        throw;
    }
}
```

Test the changes with the existing TestApp to ensure Activities are created and traced correctly.
```

---

### Task 2: Logger Field Detection and Integration

**Title**: Implement Automatic Logger Detection and Usage

**Description**: Detect existing logger fields in classes and use them for structured logging instead of Console.WriteLine. This will integrate with existing logging infrastructure in applications.

**Specification**:
- Scan containing class for logger fields using common patterns: `_logger`, `logger`, `_log`, `log`
- Support common logger types: `ILogger`, `ILogger<T>`, `Microsoft.Extensions.Logging.ILogger`
- Generate different interceptor bodies based on logger availability
- Use appropriate log levels from the `ObservatorTrace` attribute
- Add method name, parameters, and timing information to log entries
- Emit diagnostic when no suitable logger is found but logging is requested

**AI Prompt**:
```
Implement logger detection and integration in the Observator InterceptorGenerator.

Requirements:
1. Add a method to detect logger fields in the containing class of attributed methods
2. Look for fields named: "_logger", "logger", "_log", "log" 
3. Check field types against common logger interfaces: "ILogger", "ILogger<T>", "Microsoft.Extensions.Logging.ILogger"
4. Modify GenerateInterceptorBody to use detected loggers instead of Console.WriteLine
5. Use the LogLevel from the ObservatorTrace attribute (default Debug)
6. Generate log messages with structured information:
   - Method entry: "Executing {MethodName}"
   - Method success: "Executed {MethodName} in {ElapsedMs}ms"  
   - Method exception: "Exception in {MethodName}: {ExceptionMessage}"
7. Add timing measurement using Stopwatch
8. Fallback to Console.WriteLine when no logger is detected
9. Add the logger field detection logic to the incremental pipeline
10. Pass logger information to the code generation phase

The generated code should use logger methods like:
- _logger.LogDebug("Executing {MethodName}", "methodName")
- _logger.LogError(ex, "Exception in {MethodName}", "methodName")

Update the existing test classes to include logger fields and verify the generated code uses them correctly.
```

---

### Task 3: Parameter and Return Value Logging

**Title**: Implement Parameter and Return Value Capture

**Description**: Add support for logging method parameters and return values when enabled via the `ObservatorTrace` attribute properties.

**Specification**:
- Read `IncludeParameters` and `IncludeReturnValue` properties from `ObservatorTrace` attribute
- Generate code to capture and log parameter values (with ToString() or JSON serialization)
- Capture return values for non-void methods when enabled
- Add parameter and return value information to both Activity tags and log entries
- Handle sensitive data by providing opt-out mechanisms
- Support async methods and handle Task<T> return types properly

**AI Prompt**:
```
Implement parameter and return value logging in the Observator InterceptorGenerator.

Requirements:
1. Update the attribute discovery pipeline to extract IncludeParameters and IncludeReturnValue properties
2. Modify GenerateInterceptorBody to accept these flags and method signature information
3. When IncludeParameters is true:
   - Add parameter names and values to Activity tags: activity.SetTag("param.{paramName}", paramValue)
   - Include parameters in log messages: "Executing {MethodName} with {Parameters}"
   - Handle complex types by using ToString() method
4. When IncludeReturnValue is true:
   - Capture the return value before returning
   - Add return value to Activity tags: activity.SetTag("return.value", returnValue)
   - Log return value: "Executed {MethodName} returned {ReturnValue}"
   - Handle Task<T> return types by extracting T
5. Handle edge cases:
   - Void methods (no return value)
   - Methods with ref/out parameters
   - Methods returning Task or ValueTask
   - Async methods with complex return types
6. Generate safe parameter serialization (handle null values, exceptions in ToString())
7. Update the incremental pipeline to pass parameter metadata and attribute properties
8. Test with TestApp methods that have parameters and return values

Generate code like:
```csharp
var parameters = new Dictionary<string, object> { {"param1", param1}, {"param2", param2} };
activity?.SetTag("parameters", string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}")));
var result = await this.Method_Clone(param1, param2);
activity?.SetTag("return.value", result?.ToString());
return result;
```
```

---

## Phase 4: Comprehensive Diagnostics

### Task 4: Complete Diagnostic System Implementation

**Title**: Implement All Planned Diagnostic Descriptors (OBS002-OBS009)

**Description**: Complete the diagnostic system by implementing all remaining diagnostic descriptors to provide comprehensive guidance to developers.

**Specification**:
- Implement diagnostic descriptors OBS002 through OBS009 as defined in implementation.md
- Add detection logic for each diagnostic condition in the incremental pipeline
- Emit appropriate diagnostics with helpful messages and suggested fixes
- Test each diagnostic with corresponding test cases
- Document diagnostic conditions and resolutions

**AI Prompt**:
```
Complete the Observator diagnostic system by implementing diagnostic descriptors OBS002 through OBS009.

Requirements:
1. Add all diagnostic descriptors to DiagnosticDescriptors.cs based on the specification in implementation.md:
   - OBS002: Abstract method annotation warning
   - OBS003: Interface method annotation guidance  
   - OBS004: Duplicate method signature detection
   - OBS005: Async method support warnings
   - OBS006: Unsupported signature detection (ref/out parameters)
   - OBS007: DI registration guidance for interface methods
   - OBS008: Missing method body detection
   - OBS009: Struct parameter allocation warning

2. Add detection logic in InterceptorGenerator.Initialize method:
   - Check for abstract methods (OBS002)
   - Detect interface method annotations (OBS003)
   - Find duplicate signatures in same class (OBS004)
   - Identify async patterns that need warnings (OBS005)
   - Detect ref/out parameters (OBS006)
   - Scan for DI registration patterns (OBS007)
   - Check for methods without bodies (OBS008)
   - Identify struct parameters (OBS009)

3. Report diagnostics through the SourceProductionContext.ReportDiagnostic method
4. Include location information and helpful messages for each diagnostic
5. Add test cases in TestApp or create new test files to trigger each diagnostic
6. Ensure diagnostics don't prevent code generation when appropriate
7. Document each diagnostic with examples and resolutions

Each diagnostic should provide actionable guidance to help developers resolve issues.
```

---

### Task 5: Iterator and Async Iterator Support

**Title**: Complete Iterator Method Body Extraction

**Description**: Implement proper handling of iterator methods (yield return/yield break) and async iterator methods in the method body cloning process.

**Specification**:
- Detect iterator methods using presence of yield statements
- Detect async iterator methods (IAsyncEnumerable return types)
- Preserve iterator semantics in clone methods
- Handle complex iterator patterns including nested loops and conditional yields
- Support async iterator methods with proper async/await handling
- Test with various iterator patterns including LINQ-style methods

**AI Prompt**:
```
Complete iterator method support in Observator's InterceptorGenerator method body cloning.

Requirements:
1. Enhance the method body extraction logic in InterceptorGenerator.cs to properly handle:
   - Iterator methods with yield return and yield break
   - Async iterator methods returning IAsyncEnumerable<T>
   - Mixed iterator patterns with complex control flow
   
2. Update the method signature detection to:
   - Identify iterator methods by return type (IEnumerable, IEnumerable<T>)
   - Identify async iterators by return type (IAsyncEnumerable<T>)
   - Check for yield statements in method body syntax tree
   
3. Modify the clone method generation to:
   - Preserve exact iterator semantics in _Clone methods
   - Handle both block-bodied and expression-bodied iterator methods
   - Maintain async context for async iterators
   - Ensure proper disposal semantics for iterators
   
4. Update the interceptor wrapper to:
   - Handle IEnumerable and IAsyncEnumerable return types
   - Trace enumeration start but not individual yield operations
   - Properly await async enumerable results when needed
   
5. Test with TestApp examples:
   - Simple iterator: yield return items from collection
   - Async iterator: yield return items from async operations  
   - Iterator with exception handling
   - Iterator with complex control flow (loops, conditions)
   
6. Handle edge cases:
   - Empty iterators
   - Iterators that throw exceptions
   - Iterators with finally blocks
   - Generic iterator methods

Current implementation placeholder in InterceptorGenerator line ~151 needs to be replaced with proper iterator body extraction.
```

---

## Phase 5: Production Readiness

### Task 6: Metrics Collection Integration

**Title**: Add Metrics Collection Using Generated Meter

**Description**: Integrate metrics collection into interceptors using the generated Meter infrastructure for method execution tracking.

**Specification**:
- Use generated `ObservatorInfrastructure.Meter` to create counters and histograms
- Track method execution count, duration, and error rates
- Add method-level tags for metrics segmentation
- Support custom metrics via attribute properties
- Integrate with OpenTelemetry metrics pipeline
- Provide configuration options for metrics collection

**AI Prompt**:
```
Add comprehensive metrics collection to Observator interceptors using the generated Meter infrastructure.

Requirements:
1. Update GenerateInterceptorBody to include metrics collection alongside Activities
2. Create standard metrics for each intercepted method:
   - Counter: "observator.method.calls" (total method invocations)
   - Histogram: "observator.method.duration" (method execution time in milliseconds)
   - Counter: "observator.method.errors" (failed method invocations)
   
3. Add method-level tags to metrics:
   - method.name: the intercepted method name
   - method.class: the containing class name
   - method.assembly: the assembly name
   - success: true/false for error tracking
   
4. Integrate metrics with the existing Activity and logging code:
   - Use Stopwatch for duration measurement (same instance for logs and metrics)
   - Increment error counter in catch blocks
   - Record all metrics in finally blocks
   
5. Handle async methods properly:
   - Measure total async method duration (not just synchronous portion)
   - Use Task.ContinueWith or async/await patterns as appropriate
   
6. Add configuration support through ObservatorTrace attribute:
   - Add EnableMetrics property (default: true)
   - Add CustomTags property for additional metric tags
   
7. Generate efficient metrics code:
   - Cache metric instruments at class level where possible
   - Minimize allocations in hot paths
   - Use spans for tag arrays when available
   
8. Update InfrastructureGenerator to include metric instrument declarations
9. Test metrics collection with TestApp and verify integration with OpenTelemetry exporters

Generated code should include patterns like:
```csharp
private static readonly Counter<long> MethodCallsCounter = ObservatorInfrastructure.Meter.CreateCounter<long>("observator.method.calls");
private static readonly Histogram<double> MethodDurationHistogram = ObservatorInfrastructure.Meter.CreateHistogram<double>("observator.method.duration");
```
```

---

### Task 7: NuGet Package Preparation

**Title**: Prepare Project for NuGet Distribution

**Description**: Configure the project for NuGet packaging including proper metadata, build configurations, and distribution setup.

**Specification**:
- Add comprehensive package metadata (description, tags, license, etc.)
- Configure build for multi-targeting if needed
- Set up proper analyzer/generator packaging
- Create package documentation and samples
- Configure continuous integration for package building
- Set up automated testing pipeline
- Prepare package versioning strategy

**AI Prompt**:
```
Prepare the Observator project for NuGet package distribution.

Requirements:
1. Update Observator.Generator.csproj with comprehensive package metadata:
   - PackageId: "Observator.Generator"
   - Title, Description, Summary for the package
   - Authors, Company information
   - PackageTags: "sourcegeneration", "opentelemetry", "observability", "tracing", "aot"
   - License expression or file
   - Project URL, Repository URL
   - Release notes and version information
   
2. Configure proper analyzer packaging:
   - Set IncludeBuildOutput="false"
   - Add analyzer items to include the generator DLL
   - Ensure proper target framework support
   - Configure dependencies correctly (Microsoft.CodeAnalysis as analyzer dependency)
   
3. Add package build configuration:
   - Configure for Release builds
   - Set up proper version numbering (semantic versioning)
   - Add package validation
   - Configure symbol packages if needed
   
4. Create package documentation:
   - README.md with installation and usage instructions
   - CHANGELOG.md for release notes
   - API documentation examples
   - Migration guides if applicable
   
5. Set up build verification:
   - Ensure package builds correctly
   - Verify generator works when installed as NuGet package
   - Test in clean environment (new project)
   - Validate metadata and content
   
6. Create sample project structure:
   - samples/ directory with example usage
   - Quick start guide
   - Common scenarios and patterns
   
7. Add MSBuild integration files if needed:
   - .props/.targets files for consumer projects
   - Default configuration options
   - Build-time validation

The project should be ready for `dotnet pack` and NuGet publishing after these changes.
```

---

## Phase 6: Advanced Features

### Task 8: Interface Interception via Proxy Generation

**Title**: Implement Proxy Generation for Interface Method Interception

**Description**: Add support for intercepting interface method calls in dependency injection scenarios by generating proxy classes.

**Specification**:
- Detect interfaces with attributed methods
- Generate proxy classes implementing these interfaces
- Create dependency injection integration helpers
- Support generic interfaces and complex inheritance
- Handle async interface methods
- Provide clear guidance for DI container registration

**AI Prompt**:
```
Implement interface method interception through proxy generation for dependency injection scenarios.

Requirements:
1. Create a new ProxyGenerator class implementing IIncrementalGenerator
2. Detect interfaces with methods annotated with [ObservatorTrace]:
   - Scan for interface declarations
   - Find methods with ObservatorTrace attribute
   - Collect interface hierarchy information
   
3. Generate proxy classes for annotated interfaces:
   - Create class implementing the target interface
   - Accept the real implementation via constructor injection
   - Wrap each attributed method with observability code
   - Delegate to real implementation after instrumentation
   - Handle async methods, generic methods, and complex signatures
   
4. Generate proxy code pattern:
```csharp
public class IMyServiceObservatorProxy : IMyService
{
    private readonly IMyService _inner;
    public IMyServiceObservatorProxy(IMyService inner) => _inner = inner;
    
    public async Task<string> TracedMethodAsync(string param)
    {
        using var activity = ObservatorInfrastructure.ActivitySource.StartActivity("TracedMethodAsync");
        // logging and metrics code
        try
        {
            return await _inner.TracedMethodAsync(param);
        }
        catch (Exception ex)
        {
            // error handling
            throw;
        }
    }
}
```

5. Generate DI registration helpers:
   - Extension methods for IServiceCollection
   - Automatic proxy registration for attributed interfaces
   - Support for different service lifetimes
   
6. Add diagnostics:
   - OBS010: Interface method requires proxy registration
   - OBS011: Multiple implementations of proxied interface detected
   
7. Test with dependency injection scenarios in TestApp
8. Document proxy usage patterns and DI integration

This enables interception of interface calls through DI containers, solving the limitation of direct interface method interception.
```

---

### Task 9: Custom Instrumentation Templates

**Title**: Implement Extensible Code Generation Templates

**Description**: Create a template system allowing customization of generated instrumentation code for different observability scenarios.

**Specification**:
- Design template architecture for different instrumentation patterns
- Support custom Activity properties and tags
- Allow custom logging formats and levels
- Enable custom metrics definitions
- Provide template selection mechanisms
- Support user-defined templates

**AI Prompt**:
```
Design and implement an extensible template system for Observator code generation.

Requirements:
1. Create template architecture:
   - Abstract base template class
   - Built-in templates for common patterns (basic tracing, detailed logging, metrics-focused)
   - Interface for custom template implementations
   
2. Template capabilities:
   - Customizable Activity creation and tagging
   - Configurable logging messages and levels
   - Custom metrics collection patterns
   - Flexible exception handling strategies
   - Parameterizable code generation
   
3. Template selection mechanism:
   - Extend ObservatorTrace attribute with Template property
   - Support template selection by method signature patterns
   - Allow assembly-level template defaults
   - Provide fallback to default template
   
4. Built-in template variations:
   - MinimalTemplate: Basic Activity tracing only
   - DetailedTemplate: Full logging, metrics, and tracing
   - PerformanceTemplate: Optimized for high-throughput scenarios
   - DebuggingTemplate: Extensive parameter and state logging
   
5. Custom template support:
   - Allow templates to be defined in user code
   - Template discovery mechanism via attributes or interfaces
   - Compile-time template validation
   - Runtime template caching
   
6. Template API design:
```csharp
public abstract class InstrumentationTemplate
{
    public abstract string GenerateInterceptorBody(MethodInfo method, TemplateContext context);
    public virtual string GeneratePreCall(MethodInfo method) => "";
    public virtual string GeneratePostCall(MethodInfo method) => "";
    public virtual string GenerateExceptionHandler(MethodInfo method) => "";
}
```

7. Integration with existing generators:
   - Update InterceptorGenerator to use template system
   - Maintain backward compatibility with current generation
   - Add template resolution to incremental pipeline
   
8. Documentation and examples:
   - Template creation guide
   - Built-in template showcase
   - Performance comparison between templates
   - Migration guide for custom templates

This enables advanced scenarios like custom observability patterns, third-party integration, and specialized instrumentation requirements.
```

---

## Maintenance and Quality Tasks

### Task 10: Comprehensive Testing Suite

**Title**: Expand Test Coverage and Add Integration Tests

**Description**: Create comprehensive test suite covering all features, edge cases, and integration scenarios.

**Specification**:
- Unit tests for all generator components
- Integration tests with real projects
- Performance benchmarks
- Cross-platform testing
- AOT compilation testing
- Error scenario testing

**AI Prompt**:
```
Create a comprehensive testing suite for the Observator project covering all aspects of functionality and integration.

Requirements:
1. Create test project structure:
   - test/Observator.Generator.Tests/ - Unit tests for generators
   - test/Observator.Integration.Tests/ - Integration tests with real projects
   - test/Observator.Performance.Tests/ - Performance and benchmark tests
   - test/Observator.AOT.Tests/ - Native AOT compilation tests
   
2. Unit test coverage:
   - InfrastructureGenerator: Assembly metadata extraction, code generation
   - InterceptorGenerator: Method discovery, call site detection, code generation
   - Diagnostic system: All diagnostic conditions and reporting
   - Template system: Template selection and code generation
   - Proxy generation: Interface detection and proxy creation
   
3. Integration test scenarios:
   - Real project with complex method patterns
   - Multi-assembly solutions with cross-references
   - Dependency injection scenarios with interfaces
   - OpenTelemetry integration with exporters
   - Various .NET project types (console, web, library)
   
4. Performance testing:
   - Build time impact measurement
   - Runtime overhead benchmarks
   - Memory allocation analysis
   - Large project scalability tests
   - Comparison with baseline (non-instrumented) performance
   
5. AOT compilation testing:
   - Verify generated code is AOT-compatible
   - Test with PublishAot=true
   - Validate no reflection usage
   - Ensure trimming compatibility
   
6. Error scenario testing:
   - Invalid attribute usage patterns
   - Unsupported method signatures
   - Build error conditions
   - Runtime exception scenarios
   - Malformed generated code detection
   
7. Test automation:
   - CI/CD pipeline integration
   - Automated test execution on multiple platforms
   - Performance regression detection
   - Test result reporting and analysis
   
8. Test utilities and helpers:
   - Compilation test helpers for generator testing
   - Mock project creation utilities
   - Performance measurement infrastructure
   - Test data generators for complex scenarios

Use MSTest, xUnit, or NUnit framework and ensure tests run on Windows, Linux, and macOS platforms.
```

This comprehensive todo list provides a clear roadmap for completing the Observator project, with each task including detailed specifications and AI prompts for implementation. The tasks are organized by priority phases, ensuring the most critical functionality is delivered first while building toward advanced features and production readiness.