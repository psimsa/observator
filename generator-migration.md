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

## Next Steps
- Refactor `InterceptorGenerator` and `InfrastructureGenerator` to use the incremental generator pattern.
- Update diagnostics and code generation logic to use the new context and APIs.
- Test with the latest SDK to ensure compatibility and correct interceptor emission.

---

**References:**
- [Andrew Lock: Creating a source generator - Implementing an interceptor](https://andrewlock.net/creating-a-source-generator-part-11-implementing-an-interceptor-with-a-source-generator/)
- [Roslyn Interceptors Design](https://github.com/dotnet/roslyn/issues/72133)
- [.NET 9 SDK Release Notes](https://github.com/dotnet/core)
