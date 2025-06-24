# Plan for Cross-Assembly Interception Refactoring

## Objective
Refactor the interceptor generation logic in `src/Observator.Generator/InterceptorGenerator.cs` to align with a new model that supports cross-assembly calls. The key changes involve generating interceptors as extension methods within a `file static class` in the same assembly as the caller, removing the requirement for intercepted methods to be in partial classes, and defining the `InterceptsLocationAttribute` within the generated file.

## Current Implementation Analysis
The current `InterceptorGenerator.cs` does the following:
*   Identifies methods with `[ObservatorTrace]` attributes.
*   Enforces that these methods must be in `partial class` definitions.
*   Generates a private clone of the original method (`_Clone`).
*   Generates an `internal` interceptor method (`_Interceptor`) within the same `partial class`.
*   Applies `[System.Runtime.CompilerServices.InterceptsLocation]` attributes to the generated interceptor method, using `location.Data` (a base64 encoded file path) and a hardcoded `1` for the line number.

## New Model Requirements
The new model requires:
1.  **Interceptors as Extension Methods:** The generated interceptor method should be an extension method on the intercepted service interface/class.
2.  **`file static class Interceptor`:** Interceptor methods should reside within a `file static class Interceptor`.
3.  **Same Assembly as Caller:** The generated code (containing the `file static class Interceptor`) should be placed in the same assembly as the *caller* of the intercepted method, not the intercepted method's assembly.
4.  **No Partial Class Requirement:** The intercepted method's class should not need to be a `partial class` and should not be modified.
5.  **`InterceptsLocationAttribute` Definition:** The `System.Runtime.CompilerServices.InterceptsLocationAttribute` itself should be defined as a `file sealed class` within the generated source file, with the constructor `(int version, string data)`.
6.  **`InterceptsLocationAttribute` Parameters:** The attribute usage should be `[System.Runtime.CompilerServices.InterceptsLocationAttribute(1, "{location.Data}")]`.
7.  **Activity Source:** The activity source reference should be `GitHubStatsWebApi.DefaultActivitySource.ActivitySource.StartActivity`.

## Detailed Plan

```mermaid
graph TD
    A[Start] --> B{Find Attributed Methods};
    B --> C{Find Call Sites with InterceptableLocation};
    C --> D{Combine Attributed Methods and Call Sites};
    D --> E{Filter Valid Attributed Methods};
    E --> F{Match Call Sites to Attributed Methods};
    F --> G{Group Matched Call Sites by Caller FilePath and Namespace};
    G --> H{Generate Source File for Each Group};
    H --> I{Add Namespace Declaration (if any)};
    H --> J{Add `file static class Interceptor`};
    J --> K{For Each Intercepted Method in Group};
    K --> L{Generate `InterceptsLocationAttribute` for each call site};
    L --> M{Generate Extension Method Interceptor};
    M --> N{Generate Interceptor Body (calling original method via @source)};
    N --> O{End Method Generation Loop};
    O --> P{End Class Generation Loop};
    P --> Q{Add `InterceptsLocationAttribute` Definition};
    Q --> R{Add Generated Source to Context};
    R --> S[End];
```

### Goal 1: Remove Partial Class Requirement
*   **Action:** Modify `src/Observator.Generator/InterceptorGenerator.cs` to remove the `partial class` check and diagnostic reporting (lines 35-45 in the provided file content).

### Goal 2: Generate `InterceptsLocationAttribute` Definition
*   **Action:** Modify `src/Observator.Generator/InterceptorGenerator.cs`.
    *   Append the following `InterceptsLocationAttribute` definition as a `file sealed class` to the `StringBuilder` content, ensuring it's within the `System.Runtime.CompilerServices` namespace:
        ```csharp
        namespace System.Runtime.CompilerServices
        {
            [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
            file sealed class InterceptsLocationAttribute(int version, string data) : Attribute { }
        }
        ```

### Goal 3: Refactor Interceptor Generation Logic
*   **Action:** Modify `src/Observator.Generator/InterceptorGenerator.cs`.
    *   **Change Grouping:** The current grouping is by the intercepted method's containing class. This needs to change to group `callSiteInfos` by the *call site's* file path and its containing namespace. This will ensure the generated interceptor is in the same assembly as the caller.
        *   Create a new dictionary, e.g., `interceptorsByCallSiteLocation`, to group by `location.FilePath` and the namespace of the `invocation`'s containing type.
    *   **Remove Clone Method Generation:** Delete lines 170-185, which generate the `_Clone` method. This method is no longer needed as the original class will not be modified.
    *   **Modify Class and Method Generation:**
        *   Change the class declaration from `partial class {firstMethod.ContainingType.Name}` to `file static class Interceptor`.
        *   Change the interceptor method signature to be an extension method:
            *   From `internal {asyncModifier}{returnType} {interceptorName}({parameters})`
            *   To `public static {asyncModifier}{returnType} Intercepts{method.Name}(this {method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} @source, {parameters})`.
            *   The `interceptorName` should be `Intercepts` followed by the original method name.
            *   The `parameters` for the extension method will include the `this` parameter.
        *   Update the call within the interceptor body:
            *   From `this.{cloneName}({args});`
            *   To `@source.{method.Name}({args});`
    *   **Update Activity Source:** Change `Observator.Generated.ObservatorInfrastructure.ActivitySource.StartActivity` to `GitHubStatsWebApi.DefaultActivitySource.ActivitySource.StartActivity` in the `GenerateInterceptorBody` method.

### Goal 4: Update `InterceptsLocationAttribute` Usage
*   **Action:** Modify `src/Observator.Generator/InterceptorGenerator.cs`.
    *   In the loop where `InterceptsLocation` attributes are added (lines 187-190), update the attribute arguments to match the `(version, data)` format.
        *   The attribute usage should be `[System.Runtime.CompilerServices.InterceptsLocationAttribute(1, "{location.Data}")]`.