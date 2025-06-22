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
**Goal**: Generate per-assembly ActivitySource, Meter, and attributes

#### 1.1 Project Setup
- [ ] Configure `Observator.Generator.csproj` for source generator
- [ ] Add required NuGet packages:
  - `Microsoft.CodeAnalysis.CSharp` (4.8.0+)
  - `Microsoft.CodeAnalysis.Analyzers` (3.3.4+)
- [ ] Configure `<IncludeBuildOutput>false</IncludeBuildOutput>`
- [ ] Add analyzer references in `.targets` file

#### 1.2 Assembly Metadata Extraction
```csharp
// AssemblyInfoExtractor.cs
public static class AssemblyInfoExtractor
{
    public static (string Name, string Version) GetAssemblyInfo(GeneratorExecutionContext context)
    {
        var assemblyName = context.Compilation.AssemblyName ?? "Unknown";
        var version = context.Compilation.Assembly
            .GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "AssemblyVersionAttribute")
            ?.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? "1.0.0";
        
        return (assemblyName, version);
    }
}
```

#### 1.3 Infrastructure Code Generation
```csharp
// Generated per assembly: ObservatorInfrastructure.g.cs
namespace Observator.Generated
{
    public static class ObservatorInfrastructure
    {
        public static readonly ActivitySource ActivitySource = 
            new("{AssemblyName}", "{Version}");
            
        public static readonly Meter Meter = 
            new("{AssemblyName}", "{Version}");
            
        public static string ActivitySourceName => "{AssemblyName}";
        public static string Version => "{Version}";
    }
    
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public sealed class ObservatorTraceAttribute : System.Attribute
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Debug;
        public bool IncludeParameters { get; set; } = false;
        public bool IncludeReturnValue { get; set; } = false;
    }
    
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5
    }
}
```

#### 1.4 TestApp Setup & Integration
**Goal**: Configure TestApp to consume the generator and verify generated code compiles and works

##### 1.4.1 Configure TestApp Project Reference
Update `test/TestApp/TestApp.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Observator.Generator\Observator.Generator.csproj" 
                      OutputItemType="Analyzer" 
                      ReferenceOutputAssembly="false" />
  </ItemGroup>

  <!-- Enable interceptors (experimental feature) -->
  <PropertyGroup>
    <InterceptorsPreviewNamespaces>$(InterceptorsPreviewNamespaces);Observator.Generated</InterceptorsPreviewNamespaces>
  </PropertyGroup>
</Project>
```

##### 1.4.2 Create Test Service Classes
Update `test/TestApp/Program.cs`:
```csharp
using System.Diagnostics;
using Observator.Generated;

Console.WriteLine("=== Observator Test App ===");

// Test infrastructure generation
Console.WriteLine($"ActivitySource Name: {ObservatorInfrastructure.ActivitySourceName}");
Console.WriteLine($"ActivitySource Version: {ObservatorInfrastructure.Version}");
Console.WriteLine($"ActivitySource Instance: {ObservatorInfrastructure.ActivitySource}");
Console.WriteLine($"Meter Instance: {ObservatorInfrastructure.Meter}");

// Test service with attributed methods
var service = new TestService();
var result = service.DoWork("test parameter");
Console.WriteLine($"Service result: {result}");

var asyncResult = await service.DoWorkAsync("async parameter");
Console.WriteLine($"Async service result: {asyncResult}");

Console.WriteLine("Test completed successfully!");

public class TestService 
{
    [ObservatorTrace]
    public string DoWork(string input)
    {
        Console.WriteLine($"DoWork called with: {input}");
        Thread.Sleep(100); // Simulate work
        return $"Processed: {input}";
    }

    [ObservatorTrace(LogLevel = LogLevel.Information)]
    public async Task<string> DoWorkAsync(string input)
    {
        Console.WriteLine($"DoWorkAsync called with: {input}");
        await Task.Delay(50); // Simulate async work
        return $"Async processed: {input}";
    }

    // Test method without attribute (should not be intercepted)
    public string DoUnmonitoredWork(string input)
    {
        Console.WriteLine($"DoUnmonitoredWork called with: {input}");
        return $"Unmonitored: {input}";
    }
}
```

##### 1.4.3 Add Additional Test Classes
Create `test/TestApp/ComplexService.cs`:
```csharp
using Observator.Generated;

namespace TestApp;

public class ComplexService
{
    [ObservatorTrace(IncludeParameters = true)]
    public int Calculate(int a, int b)
    {
        return a + b;
    }

    [ObservatorTrace]
    public void VoidMethod()
    {
        Console.WriteLine("VoidMethod executed");
    }

    // Test Task-returning method without async (should generate warning)
    [ObservatorTrace]
    public Task<string> TaskWithoutAsync()
    {
        return Task.FromResult("Task result");
    }
}
```

##### 1.4.4 Verification Steps
- [ ] Build TestApp and verify no compilation errors
- [ ] Run TestApp and verify console output shows:
  ```
  === Observator Test App ===
  ActivitySource Name: TestApp
  ActivitySource Version: 1.0.0.0
  ActivitySource Instance: System.Diagnostics.ActivitySource
  Meter Instance: System.Diagnostics.Metrics.Meter
  DoWork called with: test parameter
  Service result: Processed: test parameter
  DoWorkAsync called with: async parameter
  Async service result: Async processed: async parameter
  Test completed successfully!
  ```
- [ ] Verify generated files appear in `obj/Debug/net9.0/generated/` folder
- [ ] Check that only attributed methods get intercepted (unmonitored method works normally)
- [ ] Validate diagnostic warnings for Task-without-async methods

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

**Prerequisites**: Phase 1 complete, TestApp successfully running with generated infrastructure

#### 2.1 Update TestApp for Interceptor Testing
Before implementing interceptors, enhance TestApp to help verify interceptor functionality:

##### 2.1.1 Add Activity Listening
Update `test/TestApp/Program.cs` to listen for activities:
```csharp
using System.Diagnostics;
using Observator.Generated;

// Set up activity listener to capture generated traces
using var activityListener = new ActivityListener
{
    ShouldListenTo = _ => true,
    Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
    SampleUsingParentId = (ref ActivityCreationOptions<string> options) => ActivitySamplingResult.AllData,
    ActivityStarted = activity => Console.WriteLine($"🟢 Activity Started: {activity.DisplayName}"),
    ActivityStopped = activity => Console.WriteLine($"🔴 Activity Stopped: {activity.DisplayName} (Duration: {activity.Duration.TotalMilliseconds}ms)")
};
ActivitySource.AddActivityListener(activityListener);

Console.WriteLine("=== Observator Test App ===");
Console.WriteLine("Activity listener configured");

// ...existing code...

// Add more complex call scenarios
Console.WriteLine("\n=== Testing Call Interception ===");
var complexService = new ComplexService();

// Direct method calls (should be intercepted)
var calcResult = complexService.Calculate(5, 3);
Console.WriteLine($"Calculate result: {calcResult}");

complexService.VoidMethod();

// Method calls through variables (should be intercepted)
var service2 = new TestService();
CallServiceMethod(service2);

// Nested calls (should be intercepted)
var nestedResult = service.DoWork(complexService.Calculate(2, 3).ToString());
Console.WriteLine($"Nested result: {nestedResult}");

static void CallServiceMethod(TestService service)
{
    var result = service.DoWork("called from static method");
    Console.WriteLine($"Static method call result: {result}");
}
```

##### 2.1.2 Expected Console Output After Interceptors
```
=== Observator Test App ===
Activity listener configured
ActivitySource Name: TestApp
...
🟢 Activity Started: TestService.DoWork
DoWork called with: test parameter
🔴 Activity Stopped: TestService.DoWork (Duration: 102ms)
Service result: Processed: test parameter

🟢 Activity Started: TestService.DoWorkAsync  
DoWorkAsync called with: async parameter
🔴 Activity Stopped: TestService.DoWorkAsync (Duration: 53ms)
Async service result: Async processed: async parameter

=== Testing Call Interception ===
🟢 Activity Started: ComplexService.Calculate
🔴 Activity Stopped: ComplexService.Calculate (Duration: 0ms)
Calculate result: 8
🟢 Activity Started: ComplexService.VoidMethod
VoidMethod executed
🔴 Activity Stopped: ComplexService.VoidMethod (Duration: 0ms)
...
```

#### 2.1 Method Discovery
```csharp
// InterceptorGenerator.cs - Find attributed methods
private static List<AttributedMethod> FindAttributedMethods(Compilation compilation)
{
    var methods = new List<AttributedMethod>();
    
    foreach (var syntaxTree in compilation.SyntaxTrees)
    {
        var root = syntaxTree.GetRoot();
        var methodDeclarations = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => HasObservatorTraceAttribute(m));
            
        foreach (var method in methodDeclarations)
        {
            methods.Add(new AttributedMethod
            {
                Declaration = method,
                SemanticModel = compilation.GetSemanticModel(syntaxTree),
                SyntaxTree = syntaxTree
            });
        }
    }
    
    return methods;
}
```

#### 2.2 Call Site Discovery
```csharp
// Find all invocations of attributed methods
private static List<CallSite> FindCallSites(
    List<AttributedMethod> attributedMethods, 
    Compilation compilation)
{
    var callSites = new List<CallSite>();
    
    foreach (var syntaxTree in compilation.SyntaxTrees)
    {
        var root = syntaxTree.GetRoot();
        var invocations = root.DescendantNodes()
            .OfType<InvocationExpressionSyntax>();
            
        foreach (var invocation in invocations)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var symbolInfo = semanticModel.GetSymbolInfo(invocation);
            
            if (IsTargetMethod(symbolInfo.Symbol, attributedMethods))
            {
                callSites.Add(new CallSite
                {
                    Invocation = invocation,
                    Location = invocation.GetLocation(),
                    SyntaxTree = syntaxTree,
                    TargetMethod = GetTargetMethod(symbolInfo.Symbol, attributedMethods)
                });
            }
        }
    }
    
    return callSites;
}
```

#### 2.3 Basic Interceptor Generation
```csharp
// Generated interceptor example
[InterceptsLocation(@"C:\dev\psimsa\observator\test\TestApp\Program.cs", 15, 8)]
public static T InterceptMyMethod<T>(this MyService service, string parameter)
{
    using var activity = ObservatorInfrastructure.ActivitySource
        .StartActivity("MyService.MyMethod");
        
    activity?.SetTag("method", "MyMethod");
    activity?.SetTag("class", "MyService");
    
    try
    {
        return service.MyMethodOriginal(parameter);
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        throw;
    }
}
```

### Phase 3: Async/Task Handling (Week 5)
**Goal**: Properly handle async methods and Task-returning methods

#### 3.1 Async Method Detection
```csharp
private static bool IsAsyncMethod(MethodDeclarationSyntax method)
{
    return method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));
}

private static bool IsTaskReturning(IMethodSymbol method)
{
    var returnType = method.ReturnType;
    return returnType.Name == "Task" && 
           returnType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks";
}
```

#### 3.2 Task Method Warning Diagnostic
```csharp
// DiagnosticDescriptors.cs
public static readonly DiagnosticDescriptor TaskWithoutAsync = new(
    "OBS001",
    "Task-returning method should use async keyword",
    "Method '{0}' returns Task but is not marked as async. Consider using async/await pattern.",
    "Design",
    DiagnosticSeverity.Warning,
    isEnabledByDefault: true);
```

#### 3.3 Async Interceptor Generation
```csharp
// Generated async interceptor
[InterceptsLocation(@"path", line, col)]
public static async Task<T> InterceptMyMethodAsync<T>(
    this MyService service, 
    string parameter)
{
    using var activity = ObservatorInfrastructure.ActivitySource
        .StartActivity("MyService.MyMethodAsync");
        
    try
    {
        var result = await service.MyMethodAsyncOriginal(parameter);
        activity?.SetStatus(ActivityStatusCode.Ok);
        return result;
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        throw;
    }
}
```

### Phase 4: Logging Integration (Week 6)
**Goal**: Add structured logging capabilities

#### 4.1 Logger Field Detection
```csharp
// LoggerFieldAnalyzer.cs
private static IFieldSymbol? FindLoggerField(INamedTypeSymbol classSymbol)
{
    return classSymbol.GetMembers()
        .OfType<IFieldSymbol>()
        .FirstOrDefault(f => 
            f.Name.Equals("_logger", StringComparison.OrdinalIgnoreCase) ||
            f.Name.Equals("logger", StringComparison.OrdinalIgnoreCase) &&
            IsLoggerType(f.Type));
}

private static bool IsLoggerType(ITypeSymbol type)
{
    return type.Name == "ILogger" && 
           type.ContainingNamespace.ToDisplayString().StartsWith("Microsoft.Extensions.Logging");
}
```

#### 4.2 Logger Diagnostic
```csharp
public static readonly DiagnosticDescriptor LoggerFieldMissing = new(
    "OBS002",
    "Logger field required for logging",
    "Class '{0}' contains ObservatorTrace attributes with logging enabled but no logger field found. Add ILogger _logger or logger field.",
    "Usage",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);
```

#### 4.3 Logging Interceptor Generation
```csharp
[InterceptsLocation(@"path", line, col)]
public static T InterceptWithLogging<T>(this MyService service, string parameter)
{
    const string methodName = "MyMethod";
    var logger = service._logger; // Generated field access
    
    using var activity = ObservatorInfrastructure.ActivitySource.StartActivity(methodName);
    
    logger.LogDebug("Starting {Method} with parameters: {Parameters}", 
        methodName, new { parameter });
    
    try
    {
        var result = service.MyMethodOriginal(parameter);
        logger.LogDebug("Completed {Method} successfully", methodName);
        return result;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error in {Method}", methodName);
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        throw;
    }
}
```

### Phase 5: Advanced Features (Week 7-8)
**Goal**: Parameter capture, return value logging, metrics

#### 5.1 Parameter Capture
```csharp
// When IncludeParameters = true
logger.LogDebug("Starting {Method} with {Parameter1}: {Value1}", 
    methodName, nameof(parameter), parameter);
    
activity?.SetTag("parameter.parameter", parameter?.ToString());
```

#### 5.2 Return Value Capture
```csharp
// When IncludeReturnValue = true
var result = service.MyMethodOriginal(parameter);
logger.LogDebug("Completed {Method} with result: {Result}", methodName, result);
activity?.SetTag("result", result?.ToString());
```

#### 5.3 Metrics Integration
```csharp
// Generated metrics
private static readonly Counter<long> _methodCallCounter = 
    ObservatorInfrastructure.Meter.CreateCounter<long>("method_calls_total");
    
private static readonly Histogram<double> _methodDuration = 
    ObservatorInfrastructure.Meter.CreateHistogram<double>("method_duration_seconds");

// In interceptor
var stopwatch = Stopwatch.StartNew();
_methodCallCounter.Add(1, new("method", methodName), new("class", className));

try
{
    var result = service.MyMethodOriginal(parameter);
    _methodDuration.Record(stopwatch.Elapsed.TotalSeconds, 
        new("method", methodName), new("status", "success"));
    return result;
}
catch (Exception ex)
{
    _methodDuration.Record(stopwatch.Elapsed.TotalSeconds, 
        new("method", methodName), new("status", "error"));
    throw;
}
```

## Testing Strategy

### Unit Tests
- [ ] Assembly metadata extraction tests
- [ ] Code generation template tests
- [ ] Syntax analysis helper tests

### Integration Tests
- [ ] End-to-end source generation tests
- [ ] Cross-assembly reference tests
- [ ] AOT compilation tests

### Manual Testing
- [ ] TestApp with various method signatures
- [ ] Async/await scenarios
- [ ] Exception handling scenarios
- [ ] Performance benchmarks

## Packaging & Distribution

### NuGet Package Structure
```xml
<PackageReference Include="Observator.Generator" Version="1.0.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

### Package Contents
- `analyzers/dotnet/cs/Observator.Generator.dll`
- `build/Observator.Generator.targets`
- Documentation and samples

## Performance Considerations

### Compile-Time Performance
- Incremental generation support
- Efficient syntax tree traversal
- Caching of expensive operations

### Runtime Performance
- Zero reflection overhead
- Minimal memory allocations
- Efficient string interning for activity names

## AOT Compatibility Checklist
- [ ] No runtime reflection usage
- [ ] All generated code is statically analyzable
- [ ] No dynamic code generation at runtime
- [ ] Compatible with trimming
- [ ] Works with ReadyToRun images

## Risks & Mitigations

### Technical Risks
1. **Interceptor API Stability**: Interceptors are experimental in .NET 9
   - *Mitigation*: Provide fallback to method decoration if interceptors change
2. **Cross-Assembly Complexity**: Call sites in different assemblies
   - *Mitigation*: Phase implementation, start with same-assembly scenarios
3. **Performance Impact**: Code generation overhead
   - *Mitigation*: Incremental generation and performance testing

### Architectural Risks
1. **Generated Code Conflicts**: Multiple generators producing conflicting code
   - *Mitigation*: Unique namespaces and careful naming conventions
2. **Debugging Complexity**: Generated interceptors make debugging harder
   - *Mitigation*: Generate readable code with clear naming and comments

## Success Metrics
- [ ] Zero-config installation in new projects
- [ ] Sub-second incremental compilation times
- [ ] Compatible with AOT scenarios
- [ ] Less than 5% runtime performance overhead
- [ ] Works across assembly boundaries
- [ ] Clear diagnostic messages for common issues

## Timeline Summary
- **Week 1-2**: Infrastructure generation (ActivitySource, Meter, Attributes)
- **Week 3-4**: Basic interceptor implementation
- **Week 5**: Async/Task handling and diagnostics
- **Week 6**: Logging integration
- **Week 7-8**: Advanced features (parameters, metrics)
- **Week 9**: Testing, documentation, packaging

This implementation plan provides a clear roadmap for building a production-ready, AOT-compatible observability source generator that meets all the requirements outlined in the project specification.
