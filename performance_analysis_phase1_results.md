# Performance Analysis - Phase 1: Research and Design Results

This document outlines the research and design considerations for optimizing the `GetAttributedMethodsFromReferences` method in `src/Observator.Generator/InterceptorGenerator.cs`, focusing on leveraging Roslyn's `IncrementalGenerator` capabilities.

## Current Bottleneck

The `GetAttributedMethodsFromReferences` method currently performs a full scan of all types and methods across all referenced assemblies in the `Compilation`. This exhaustive iteration, particularly for large projects with numerous references, leads to significant performance degradation and acts as a bottleneck in the source code generation process.

## Leveraging Roslyn's Incremental Features

Roslyn's `IncrementalGenerator` API is designed to handle such scenarios efficiently by allowing generators to declare their inputs as `IncrementalValueProvider` or `IncrementalValuesProvider`. Roslyn then optimizes recompilations by re-running parts of the generator only when the corresponding inputs change.

### 1. Leveraging `IncrementalValueProvider` for `Compilation` and `ReferencedAssemblies`

The `IncrementalGeneratorInitializationContext` provides access to `context.CompilationProvider` and the `context.MetadataReferences` can be accessed from the `Compilation`. These are `IncrementalValueProvider`s, meaning Roslyn will only re-execute downstream computations if the compilation or its references change.

```csharp
// Example conceptual usage:
var compilationProvider = context.CompilationProvider;
```

### 2. Targeted Assembly Scanning (within Incremental Pipeline)

To reduce the scope of the search for `ObservatorTraceAttribute`, we can implement a targeted assembly scanning approach within the incremental pipeline. Instead of iterating all types in all assemblies, we can pre-filter assemblies that are likely to contain our attribute.

**Proposed Approach:**
*   **Identify relevant assemblies:** An assembly is relevant if it directly or indirectly references `Observator.Abstractions`. This can be determined by checking the `ReferencedAssemblies` of each `IAssemblySymbol` for a reference to `Observator.Abstractions`.
*   **Incremental Filtering:** This filtering logic can be part of an `IncrementalValueProvider` chain that processes `compilation.References`.

```csharp
// Conceptual: Filter references to only those that might contain ObservatorTraceAttribute
var relevantAssemblySymbols = compilationProvider
    .SelectMany((compilation, _) =>
    {
        var relevantAssemblies = new List<IAssemblySymbol>();
        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol asmSymbol)
            {
                // Check if this assembly or any of its direct references depend on Observator.Abstractions
                // This is a simplified check, a more robust solution might involve deeper dependency analysis.
                if (asmSymbol.ReferencedAssemblyNames.Any(an => an.Name == ObservatorConstants.ObservatorAbstractionsAssemblyName))
                {
                    relevantAssemblies.Add(asmSymbol);
                }
            }
        }
        return relevantAssemblies;
    });
```
This would create an `IncrementalValuesProvider<IAssemblySymbol>` containing only the assemblies that are likely to contain the attribute, significantly reducing the scope for the next step.

### 3. Incremental Analysis of Types and Methods

Once we have the `relevantAssemblySymbols`, we can then incrementally analyze the types and methods within these assemblies.

**Proposed Approach:**
*   **Flatten Types:** For each `IAssemblySymbol` in `relevantAssemblySymbols`, flatten its types into an `IncrementalValuesProvider<INamedTypeSymbol>`.
*   **Filter Attributed Methods:** From the `INamedTypeSymbol` stream, filter for `IMethodSymbol`s that have `ObservatorTraceAttribute`.

```csharp
// Conceptual: Extract methods with the attribute from relevant assemblies
var attributedMethodsProvider = relevantAssemblySymbols
    .SelectMany((asmSymbol, _) => GetAllTypes(asmSymbol.GlobalNamespace)) // Reuse existing GetAllTypes
    .SelectMany((typeSymbol, _) => typeSymbol.GetMembers().OfType<IMethodSymbol>())
    .Where((methodSymbol, _) =>
    {
        return methodSymbol.GetAttributes().Any(attr =>
        {
            var attrClass = attr.AttributeClass;
            if (attrClass == null) return false;
            // Use the same robust name comparison as current implementation
            return attrClass.ToDisplayString() == ObservatorConstants.ObservatorTraceAttributeFullName ||
                   attrClass.Name == ObservatorConstants.ObservatorTraceAttributeName ||
                   attrClass.Name == ObservatorConstants.ObservatorTraceShortName;
        });
    })
    .Select((methodSymbol, _) => new MethodToInterceptInfo(methodSymbol)); // Transform to MethodToInterceptInfo
```
This chain of `SelectMany` and `Where` operations, powered by `IncrementalValuesProvider`, ensures that Roslyn only re-evaluates the necessary parts of the graph when changes occur (e.g., a new reference is added, a method attribute changes in a relevant assembly).

### 4. Caching Strategy within `IncrementalGenerator`

Roslyn's `IncrementalGenerator` inherently provides a powerful caching mechanism. When you define your generator's pipeline using `IncrementalValueProvider` and `IncrementalValuesProvider`, Roslyn automatically caches the results of each step. If the inputs to a step haven't changed, Roslyn reuses the previously computed output.

Therefore, explicit, manual caching (e.g., using a `Dictionary` outside the `IncrementalGenerator` pipeline) is generally not required and can often be detrimental as it bypasses Roslyn's optimized caching and invalidation logic.

Any complex computations performed on the `IMethodSymbol`s (e.g., mapping them to `MethodToInterceptInfo`) should be done within the `Select` or `SelectMany` transforms, allowing Roslyn to cache these intermediate results.

### Proposed Refactored Structure (Conceptual)

The `RegisterExternalAttributedMethodProvider` method in `InterceptorGenerator.cs` would be refactored to something conceptually similar to this:

```csharp
private static IncrementalValueProvider<ImmutableArray<MethodToInterceptInfo>> RegisterExternalAttributedMethodProvider(IncrementalGeneratorInitializationContext context)
{
    var relevantAssemblySymbols = context.CompilationProvider
        .SelectMany((compilation, _) =>
        {
            var relevantAssemblies = new List<IAssemblySymbol>();
            foreach (var reference in compilation.References)
            {
                if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol asmSymbol)
                {
                    // Check if this assembly or any of its direct references depend on Observator.Abstractions
                    // A more robust check might involve deeper dependency analysis if needed.
                    if (asmSymbol.ReferencedAssemblyNames.Any(an => an.Name == ObservatorConstants.ObservatorAbstractionsAssemblyName))
                    {
                        relevantAssemblies.Add(asmSymbol);
                    }
                }
            }
            return relevantAssemblies;
        });

    var attributedMethods = relevantAssemblySymbols
        .SelectMany((asmSymbol, _) => GetAllTypes(asmSymbol.GlobalNamespace))
        .SelectMany((typeSymbol, _) => typeSymbol.GetMembers().OfType<IMethodSymbol>())
        .Where((methodSymbol, _) =>
        {
            return methodSymbol.GetAttributes().Any(attr =>
            {
                var attrClass = attr.AttributeClass;
                if (attrClass == null) return false;
                return attrClass.ToDisplayString() == ObservatorConstants.ObservatorTraceAttributeFullName ||
                       attrClass.Name == ObservatorConstants.ObservatorTraceAttributeName ||
                       attrClass.Name == ObservatorConstants.ObservatorTraceShortName;
            });
        })
        .Select((methodSymbol, _) => new MethodToInterceptInfo(methodSymbol));

    return attributedMethods.Collect(); // Collect all results into an ImmutableArray
}

// GetAllTypes helper method (from current InterceptorGenerator.cs)
private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
{
    foreach (var member in ns.GetTypeMembers())
        yield return member;
    foreach (var nested in ns.GetNamespaceMembers())
    foreach (var t in GetAllTypes(nested))
        yield return t;
}
```

This conceptual design leverages Roslyn's incremental pipeline to:
1.  Identify only relevant assemblies.
2.  Incrementally analyze types and methods within those assemblies.
3.  Benefit from Roslyn's built-in caching and invalidation, reducing the need for explicit caching logic.

This completes Phase 1's research and design, providing a clear path for a Proof of Concept (Phase 2).