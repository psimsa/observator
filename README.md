# Observator: Zero-Effort .NET Observability

[![NuGet version](https://img.shields.io/nuget/v/Observator.svg)](https://www.nuget.org/packages/Observator/)
[![Build Status](https://img.shields.io/github/actions/workflow/status/psimsa/observator/dotnet.yml?branch=main)](https://github.com/psimsa/observator/actions/workflows/dotnet.yml)
[![License](https://img.shields.io/github/license/psimsa/observator)](https://github.com/psimsa/observator/blob/main/LICENSE)

Observator is an AOT-compatible source generator that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities. It uses compile-time code generation and interceptors to provide zero-configuration, high-performance observability instrumentation that works seamlessly with modern .NET applications, including those using Native AOT compilation.

## Features

- **AOT-First Design**: Full compatibility with Native AOT compilation by eliminating runtime reflection.
- **Zero Dependencies**: Only references `System.Diagnostics.DiagnosticSource`, which is built into .NET.
- **Zero Configuration**: Works out-of-the-box with sensible defaults.
- **Attribute-Driven Instrumentation**: Enable selective instrumentation by decorating methods or classes with the `[ObservatorTrace]` attribute.
- **Cross-Assembly Compatibility**: Supports instrumentation across project boundaries within a solution.
- **High Performance**: Generates optimized code that adds minimal overhead to instrumented methods.
- **Standards Compliant**: Generates code compatible with OpenTelemetry standards and .NET diagnostic conventions.
- **Automatic Namespace Generation**: Creates a parallel namespace structure under `Observator.Generated` for the generated infrastructure code.

## Getting Started

### Installation

Add the Observator NuGet package to your project:

```bash
dotnet add package Observator
```

### Usage

1. **Add the `[ObservatorTrace]` attribute** to any method or class you want to instrument. For classes, all public instance methods will be traced.
   - **Note:** You cannot use `[ObservatorTrace]` on both a class and its methods at the same time. If both are present, a compile-time error will be emitted.

   ```csharp
   using Observator;

   namespace MyAwesomeApp;

   // Class-level usage with constructor parameters
   [ObservatorTrace]
   public class MyService
   {
       private readonly ILogger<MyService> _logger;
       
       public MyService(ILogger<MyService> logger)
       {
           _logger = logger;
       }
       
       public virtual string Greet(string name)
       {
           return $"Hello, {name}!";
       }
   }

   // Method-level usage (only this method instrumented)
   public class MyOtherService
   {
       [ObservatorTrace]
       public string OnlyThisIsTraced() => "Traced!";
   }

   // Error: Do not combine class-level and method-level attributes
   [ObservatorTrace]
   public class InvalidService
   {
       [ObservatorTrace] // This will cause a compile-time error
       public void Conflict() { }
   }
   ```

2. **Configure your OpenTelemetry pipeline** to listen to the `ActivitySource` created by Observator.

   ```csharp
   using OpenTelemetry;
   using OpenTelemetry.Trace;
   using Observator.Generated.MyAwesomeApp; // Import the generated namespace

   public class Program
   {
       public static void Main(string[] args)
       {
           using var tracerProvider = Sdk.CreateTracerProviderBuilder()
               .AddSource(ObservatorInfrastructure.ActivitySourceName)
               .AddConsoleExporter() // Or your preferred exporter
               .Build();

           // Create service with dependency injection
           var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
           var myService = new MyAwesomeApp.MyService(loggerFactory.CreateLogger<MyAwesomeApp.MyService>());
           myService.Greet("World");
       }
   }
   ```

## Building from Source

To build the project from source, you will need the .NET 9 SDK.

1.  Clone the repository:
    ```bash
    git clone https://github.com/psimsa/observator.git
    ```
2.  Navigate to the project directory:
    ```bash
    cd observator
    ```
3.  Build the solution:
    ```bash
    dotnet build
    ```

## Examples and Testing

The repository includes several test projects that demonstrate how to use Observator:

- **TestApp**: A complete example application showing Observator in action
- **TestLib**: A test library with various scenarios for the generator
- **RoslynTests**: Unit tests for the source generator

## Contributing

We welcome contributions! Please see the [project documentation](project.md) for detailed information about the project's goals, architecture, and contribution guidelines.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
