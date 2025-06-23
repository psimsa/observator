# Migration to Incremental Source Generator (`IIncrementalGenerator`)

## Background

With the latest Roslyn and .NET 9 SDKs, classic source generators using `ISourceGenerator` and `GeneratorExecutionContext` are now obsolete and banned for new analyzers and generators. All generators must use the incremental generator API (`IIncrementalGenerator`). This is also required to use the new interceptors APIs, such as `GetInterceptableLocation` and the new `[InterceptsLocation(int, string)]` attribute.

## Why Incremental Generators?
- **Performance:** Incremental generators only recompute outputs when relevant inputs change, improving build performance.
- **Correctness:** The new APIs are designed to work with incremental generators, ensuring compatibility with the latest Roslyn features.
- **Required for Interceptors:** The new interceptors APIs and Roslyn analyzers require incremental generators.

## Migration Analysis

### Key Differences
- **Entry Point:** Implement `IIncrementalGenerator` and its `Initialize` method instead of `ISourceGenerator.Execute/Initialize`.
- **Context:** Use `IncrementalGeneratorInitializationContext` and `SourceProductionContext` instead of `GeneratorExecutionContext`.
- **Pipelines:** Build pipelines using `context.SyntaxProvider`, `context.CompilationProvider`, and other incremental sources.
- **Diagnostics:** Report diagnostics using `SourceProductionContext.ReportDiagnostic`.
- **No direct access to all syntax trees:** Work with filtered and transformed syntax nodes via the pipeline.

### Migration Steps
1. **Change the generator class to implement `IIncrementalGenerator`.**
2. **Replace `Execute` and `Initialize` with a single `Initialize` method.**
3. **Use `context.SyntaxProvider` to find attributed methods and call sites.**
4. **Use `SemanticModel.GetInterceptableLocation` for call site interception.**
5. **Build up the code generation pipeline using `Select`, `Where`, `Combine`, etc.**
6. **Emit source and diagnostics using `SourceProductionContext`.**
7. **Update all usages of `GeneratorExecutionContext` and related types to the new incremental context.**
8. **Update diagnostics infrastructure to work with incremental context.**
9. **Test and validate the new generator.**

## Example Skeleton
```csharp
[Generator]
public class InterceptorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Example: Find all method declarations with [ObservatorTrace]
        var attributedMethods = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, ct) => /* filter and return info */)
            .Where(x => x is not null);

        // Example: Find call sites, get interceptable locations, etc.
        // ...

        // Combine and generate source
        context.RegisterSourceOutput(attributedMethods, (spc, methodInfo) =>
        {
            // Generate code and report diagnostics using spc
        });
    }
}
```

## Current Progress (as of June 23, 2025)

- The generator project (`Observator.Generator`) is fully migrated to use the .NET 9 incremental generator API (`IIncrementalGenerator`).
- Both `InterceptorGenerator` and `InfrastructureGenerator` now use incremental pipelines and the new Roslyn APIs.
- The generator emits interceptors using the `[InterceptsLocation(int, string)]` attribute, compatible with the new Roslyn interceptors API.
- For each `[ObservatorTrace]`-annotated method, the generator emits:
  - A private `_Clone` method intended to contain the original method body.
  - An internal interceptor method with the `[InterceptsLocation]` attribute, which calls the `_Clone` method and adds observability logic.
- Diagnostic reporting is migrated to the incremental pipeline.
- The generator emits correct attribute signatures and supports the new `InterceptableLocation.Data` pattern.
- The build succeeds and the generator emits valid code, but the `_Clone` methods currently contain only a `NotImplementedException` placeholder.
- **Next step:** Implement logic to extract and emit the actual method body (block or expression-bodied) into the `_Clone` method for each intercepted method, ensuring all constructs are handled.

## Planned Diagnostics

- **Struct Parameter Warning:**
  - If a method annotated with `[ObservatorTrace]` has any parameters of a `struct` type (value type), emit a warning diagnostic.
  - Rationale: Value-based parameters may cause unnecessary copying or overallocation when intercepted, which can impact performance or correctness.
  - This diagnostic is not yet implemented, but should be considered for future releases.

## Next Steps
- Refactor `InterceptorGenerator` and `InfrastructureGenerator` to use the incremental generator pattern.
- Update diagnostics and code generation logic to use the new context and APIs.
- Test with the latest SDK to ensure compatibility and correct interceptor emission.

---

**References:**
- [Andrew Lock: Creating a source generator - Implementing an interceptor](https://andrewlock.net/creating-a-source-generator-part-11-implementing-an-interceptor-with-a-source-generator/)
- [Roslyn Interceptors Design](https://github.com/dotnet/roslyn/issues/72133)
- [.NET 9 SDK Release Notes](https://github.com/dotnet/core)
