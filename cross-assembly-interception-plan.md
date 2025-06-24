# Plan for Cross-Assembly Interception

## Problem Statement
The `Observator.Generator` currently only discovers `[ObservatorTrace]` annotated methods within the *current compilation unit*, not in *referenced assemblies*. This prevents interception code from being generated for calls to methods in external libraries, such as `TestLib.Class1.Foo()` when called from `TestApp`.

## Proposed Solution
Modify the `InterceptorGenerator` to discover attributed methods from both the current compilation and referenced assemblies.

## Detailed Plan:

### Phase 1: Modify `MethodToInterceptInfo`

1.  **Modify `src/Observator.Generator/MethodToInterceptInfo.cs`:**
    *   Add a new constructor that accepts only an `IMethodSymbol` and an optional `Diagnostic`.
    *   Make the `MethodDeclarationSyntax MethodDeclaration` property nullable, as methods from referenced assemblies will not have an associated `SyntaxNode`.

### Phase 2: Enhance `InterceptorGenerator` to discover methods from all assemblies

1.  **Modify `src/Observator.Generator/InterceptorGenerator.cs`:**
    *   **Keep existing `attributedMethods` pipeline:** This pipeline correctly identifies methods declared within the current compilation.
    *   **Add a new `IncrementalValueProvider` for external attributed methods:**
        *   This new provider will use `context.CompilationProvider` to get the `Compilation` object.
        *   It will then traverse the `Compilation`'s `GlobalNamespace` and its `ReferencedAssemblySymbols` to find all `IMethodSymbol`s that have the `[ObservatorTrace]` attribute.
        *   For each such method, it will create a `MethodToInterceptInfo` instance using the new constructor (without a `MethodDeclarationSyntax`).
    *   **Combine the two sets of attributed methods:** Merge the results from the existing `attributedMethods` pipeline and the new external attributed methods pipeline into a single collection. This combined collection will then be passed to `InterceptorDataProcessor.Process`.

### Phase 3: Ensure `InterceptorDataProcessor` handles external methods

1.  **Verify `src/Observator.Generator/InterceptorDataProcessor.cs`:**
    *   Confirm that the `Process` method correctly handles `MethodToInterceptInfo` instances where `MethodDeclaration` is `null`. The `SymbolEqualityComparer.Default.Equals(targetMethod.OriginalDefinition, methodSymbol.OriginalDefinition)` comparison should still work correctly as it operates on `IMethodSymbol`s, not `SyntaxNode`s. No changes are expected here, but it's a crucial verification step.

## Verification:
After implementing these changes, I will run `dotnet build` on `test/TestApp` and verify that `test/TestApp/obj/Debug/net9.0/generated/Observator.Generator/Observator.Generator.InterceptorGenerator/ObservatorInterceptors.g.cs` now contains interception code for `TestLib.Class1.Foo()`.

## Mermaid Diagram: Proposed Data Flow in InterceptorGenerator

```mermaid
graph TD
    A[IncrementalGeneratorInitializationContext] --> B{context.SyntaxProvider for MethodDeclarationSyntax};
    B --> C[MethodAnalyzer.AnalyzeMethodDeclaration];
    C --> D[Attributed Methods (Current Compilation)];

    A --> E{context.CompilationProvider};
    E --> F[Compilation];
    F --> G[Discover Attributed Methods in Compilation & Referenced Assemblies];
    G --> H[Attributed Methods (All Assemblies)];

    D & H --> I[Combine Attributed Methods];
    I --> J[InterceptorDataProcessor.Process];

    A --> K{context.SyntaxProvider for InvocationExpressionSyntax};
    K --> L[CallSiteAnalyzer.AnalyzeInvocationExpression];
    L --> M[Invocation Call Sites];

    J & M --> N[RegisterSourceOutput];
    N --> O[Generate ObservatorInterceptors.g.cs];