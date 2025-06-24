# Plan for Removing Obsolete Logging Code

## Goal
Remove all obsolete code related to detecting loggers and emitting logging code.

## Affected Files
*   `src/Observator.Generator/ObservatorConstants.cs`
*   `src/Observator.Generator/SourceCodeGenerator.cs`
*   `src/Observator.Generator/MethodToInterceptInfo.cs`
*   `src/Observator.Generator/MethodInterceptorInfo.cs`
*   `src/Observator.Generator/MethodAnalyzer.cs`
*   `src/Observator.Generator/InterceptorDataProcessor.cs`
*   `src/Observator.Generator/InterceptorCandidateInfo.cs`

## Detailed Plan

1.  **Remove Logger-Related Constants:**
    *   **File:** `src/Observator.Generator/ObservatorConstants.cs`
    *   **Action:** Remove the following lines:
        ```csharp
        public const string LoggerFieldName1 = "_logger";
        public const string LoggerFieldName2 = "logger";
        public const string LoggerFieldName3 = "_log";
        public const string LoggerFieldName4 = "log";

        public const string LoggerTypeName = "ILogger";
        public const string LoggerTypePrefix = "Microsoft.Extensions.Logging.ILogger";

        public const string LoggingUsingDirective = "using Microsoft.Extensions.Logging;";
        ```

2.  **Update Data Transfer Objects (DTOs):**
    *   **File:** `src/Observator.Generator/MethodToInterceptInfo.cs`
    *   **Action:**
        *   Remove `public IFieldSymbol? LoggerField { get; }` property.
        *   Remove `IFieldSymbol? loggerField` parameter from the constructor and its assignment.
    *   **File:** `src/Observator.Generator/MethodInterceptorInfo.cs`
    *   **Action:**
        *   Remove `public IFieldSymbol? LoggerField { get; }` property.
        *   Remove `IFieldSymbol? loggerField` parameter from the constructor and its assignment.
    *   **File:** `src/Observator.Generator/InterceptorCandidateInfo.cs`
    *   **Action:**
        *   Remove `public IFieldSymbol? LoggerField { get; }` property.
        *   Remove `IFieldSymbol? loggerField` parameter from the constructor and its assignment.

3.  **Remove Logger Detection Logic:**
    *   **File:** `src/Observator.Generator/MethodAnalyzer.cs`
    *   **Action:**
        *   Remove the code block responsible for finding the logger field:
            ```csharp
            var containingType = methodSymbol.ContainingType;
            var loggerField = containingType.GetMembers()
                .OfType<IFieldSymbol>()
                .FirstOrDefault(f =>
                    (f.Name == ObservatorConstants.LoggerFieldName1 || f.Name == ObservatorConstants.LoggerFieldName2 ||
                     f.Name == ObservatorConstants.LoggerFieldName3 || f.Name == ObservatorConstants.LoggerFieldName4) &&
                    (f.Type.Name == ObservatorConstants.LoggerTypeName || f.Type.ToDisplayString().StartsWith(ObservatorConstants.LoggerTypePrefix)));
            ```
        *   Modify the `MethodToInterceptInfo` constructor call to remove the `loggerField` argument.

4.  **Update Source Code Generation:**
    *   **File:** `src/Observator.Generator/SourceCodeGenerator.cs`
    *   **Action:**
        *   Remove the conditional `using Microsoft.Extensions.Logging;` directive generation:
            ```csharp
            bool anyLogger = interceptorsByNamespace.Values.SelectMany(x => x.Values).SelectMany(x => x).Any(x => x.LoggerField != null);
            if (anyLogger)
            {
                sb.AppendLine(ObservatorConstants.LoggingUsingDirective);
            }
            ```
        *   Remove the `loggerField` parameter from the `GenerateInterceptorBody_Extension` method signature and its usage in the method call.

5.  **Update Interceptor Data Processing:**
    *   **File:** `src/Observator.Generator/InterceptorDataProcessor.cs`
    *   **Action:**
        *   Remove all references to `loggerField` in the `Process` method, including its declaration, assignment, and usage in `InterceptorCandidateInfo` and `MethodInterceptorInfo` constructor calls.

## Flow Diagram

```mermaid
graph TD
    A[Start] --> B{Analyze Logging Code References};
    B --> C[Identify Affected Files];
    C --> D[Remove Logger Constants from ObservatorConstants.cs];
    D --> E[Update DTOs: MethodToInterceptInfo.cs, MethodInterceptorInfo.cs, InterceptorCandidateInfo.cs];
    E --> F[Remove Logger Detection from MethodAnalyzer.cs];
    F --> G[Update SourceCodeGenerator.cs: Remove conditional logging directive and loggerField parameter];
    G --> H[Update InterceptorDataProcessor.cs: Remove loggerField references];
    H --> I[End];