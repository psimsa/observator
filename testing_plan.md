# Observator Project Testing Plan

This document outlines the critical components in `src/Observator.Abstractions/` and `src/Observator.Generator/` that require testing, along with a brief description of their functionality and testing focus.

## `src/Observator.Abstractions/` Components for Testing

*   **`ObservatorTraceAttribute` Class**:
    *   **Functionality**: This attribute is used to mark classes, interfaces, or methods for tracing by Observator. It includes properties to control whether method parameters and return values should be included in the trace.
    *   **Testing Focus**: Ensure the attribute is correctly applied and its properties (`IncludeParameters`, `IncludeReturnValue`) are properly recognized and utilized by the generator.

## `src/Observator.Generator/` Components for Testing

*   **`CallSiteAnalyzer` Class**:
    *   **Functionality**: Analyzes `InvocationExpressionSyntax` nodes to identify method call sites that are candidates for interception. It extracts the target method symbol and the `InterceptableLocation`.
    *   **Testing Focus**: Verify that it correctly identifies invocation expressions, resolves method symbols, and extracts accurate `InterceptableLocation` information for various invocation scenarios (e.g., static calls, instance calls, extension methods).

*   **`InfrastructureGenerator` Class**:
    *   **Functionality**: An `IIncrementalGenerator` responsible for generating the `ObservatorInfrastructure` static class, which provides assembly-specific information like `ActivitySourceName` and `Version`. It also generates an internal `ObservatorTraceAttribute` for use within the generated code.
    *   **Testing Focus**: Confirm that the generated infrastructure source code is correct, includes the proper assembly name and version, and that the internal `ObservatorTraceAttribute` is correctly defined.

*   **`InterceptorDataProcessor` Class**:
    *   **Functionality**: Processes the collected `MethodToInterceptInfo` (attributed methods) and `InvocationCallSiteInfo` (call sites) to determine which methods should be intercepted and where. It groups interceptors by namespace and method signature.
    *   **Testing Focus**: Crucial for ensuring the correct mapping between attributed methods and their call sites. Test various scenarios including methods with and without attributes, interface methods, and methods from referenced assemblies. Verify correct grouping by namespace and method signature.

*   **`InterceptorGenerator` Class**:
    *   **Functionality**: The main `IIncrementalGenerator` that orchestrates the entire interception generation process. It discovers attributed methods (local, interface, and external), identifies call sites, processes the data using `InterceptorDataProcessor`, and generates the final interceptor source code using `SourceCodeGenerator`. It also reports diagnostics.
    *   **Testing Focus**: This is a high-level component. Tests should focus on its integration with other components, ensuring that the entire pipeline from attribute discovery to source code generation works as expected. Verify that diagnostics are reported correctly for invalid scenarios (e.g., non-partial classes).

*   **`MethodAnalyzer` Class**:
    *   **Functionality**: Analyzes `MethodDeclarationSyntax` nodes and `INamedTypeSymbol` (for interfaces) to identify methods annotated with `ObservatorTraceAttribute`. It creates `MethodToInterceptInfo` objects, including information about whether the method is an interface method.
    *   **Testing Focus**: Ensure it correctly identifies methods with `ObservatorTraceAttribute`, handles interface methods, and correctly sets the `IsInterfaceMethod` property. Test edge cases like abstract methods and methods in non-partial classes.

*   **`SourceCodeGenerator` Class**:
    *   **Functionality**: Generates the actual C# source code for the interceptors based on the processed `MethodInterceptorInfo` data. It constructs the `file static class Interceptor` and the `InterceptsLocationAttribute`.
    *   **Testing Focus**: Verify that the generated C# code is syntactically correct, includes the `InterceptsLocationAttribute` with the correct data, and that the interception logic (Activity creation, try-catch-finally, method invocation) is accurately represented for both synchronous and asynchronous methods, and for methods with and without parameters/return values.

*   **`DiagnosticDescriptors` Class**:
    *   **Functionality**: Defines the `DiagnosticDescriptor` objects for various diagnostic messages (e.g., `OBS001_PartialClass`).
    *   **Testing Focus**: Ensure that diagnostic IDs, titles, message formats, and default severities are correctly defined.

*   **`DiagnosticReporter` Class**:
    *   **Functionality**: Provides helper methods for creating `Diagnostic` objects based on the defined `DiagnosticDescriptors`.
    *   **Testing Focus**: Verify that it correctly creates `Diagnostic` objects with the appropriate descriptor, location, and arguments.

### Data Classes (less critical for direct unit testing, but their construction and usage should be covered by tests of the above components):

*   **`InterceptorCandidateInfo` Class**: Holds information about a method invocation that is a candidate for interception.
*   **`InvocationCallSiteInfo` Class**: Represents information about an invocation call site.
*   **`MethodInterceptorInfo` Class**: Contains details about a method that will be intercepted, including its symbol, location, and whether it's an interface method.
*   **`MethodToInterceptInfo` Class**: Stores information about a method that has been identified for interception, including its symbol, declaration, and any associated diagnostics.
*   **`ObservatorConstants` Class**: Provides constant strings used throughout the generator.

## Component Relationships

```mermaid
graph TD
    A[InterceptorGenerator] --> B(MethodAnalyzer);
    A --> C(CallSiteAnalyzer);
    A --> D(InterceptorDataProcessor);
    A --> E(SourceCodeGenerator);
    A --> F(DiagnosticReporter);
    D --> G(MethodToInterceptInfo);
    D --> H(InvocationCallSiteInfo);
    D --> I(MethodInterceptorInfo);
    B --> G;
    C --> H;
    E --> I;
    F --> J(DiagnosticDescriptors);