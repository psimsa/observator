# Observator: Zero-Effort .NET Observability

[![NuGet version](https://img.shields.io/nuget/v/Observator.svg)](https://www.nuget.org/packages/Observator/)
[![Build Status](https://img.shields.io/github/actions/workflow/status/psimsa/observator/dotnet.yml?branch=main)](https://github.com/psimsa/observator/actions/workflows/dotnet.yml)
[![License](https://img.shields.io/github/license/psimsa/observator)](https://github.com/psimsa/observator/blob/main/LICENSE)

Observator is an AOT-compatible source generator that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities. It uses compile-time code generation and interceptors to provide zero-configuration, high-performance observability instrumentation that works seamlessly with modern .NET applications, including those using Native AOT compilation.

## Features

- **AOT-First Design**: Full compatibility with Native AOT compilation by eliminating runtime reflection.
- **Zero Dependencies**: Only references `System.Diagnostics.DiagnosticSource`, which is built into .NET.
- **Zero Configuration**: Works out-of-the-box with sensible defaults.
- **Attribute-Driven Instrumentation**: Enable selective instrumentation by decorating methods, classes, or interfaces with a simple `[ObservatorTrace]` attribute.
- **Cross-Assembly Compatibility**: Supports instrumentation across project boundaries within a solution.
- **High Performance**: Generates optimized code that adds minimal overhead to instrumented methods.
- **Standards Compliant**: Generates code compatible with OpenTelemetry standards and .NET diagnostic conventions.

## Getting Started

### Installation

Add the Observator NuGet package to your project:

```bash
dotnet add package Observator
```

### Usage

1.  **Add the `[ObservatorTrace]` attribute** to any method, class, or interface you want to instrument. For classes and interfaces, all methods will be traced.

    ```csharp
    using Observator;

    namespace MyAwesomeApp;

    [ObservatorTrace]
    public partial class MyService // Make sure the class is partial
    {
        public virtual string Greet(string name)
        {
            return $"Hello, {name}!";
        }
    }
    ```

    **Important**: The class containing the method must be marked as `partial`.

2.  **Configure your OpenTelemetry pipeline** to listen to the `ActivitySource` created by Observator.

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

            var myService = new MyAwesomeApp.MyService();
            myService.Greet("World");
        }
    }
    ```

## Configuration

The `[ObservatorTrace]` attribute provides properties to control the generated trace data:

- `IncludeParameters` (default: `true`): Set to `false` to exclude method parameters from the trace.
- `IncludeReturnValue` (default: `true`): Set to `false` to exclude the method's return value from the trace.

```csharp
[ObservatorTrace(IncludeParameters = false, IncludeReturnValue = false)]
public partial class MyService
{
    // ...
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

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
