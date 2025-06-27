using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using System.Collections.Immutable;
using Observator.Generator;
using Observator.Abstractions;
using System.Threading;
using System.Linq; // Added for .First() and .OfType()

namespace TestLib;

public class MethodAnalyzerUnitTests
{
    private static Compilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        return CSharpCompilation.Create("TestAssembly",
            new[] { syntaxTree },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ObservatorTraceAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Observator.Generator.MethodAnalyzer).Assembly.Location) // Reference Observator.Generator
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }


    [Fact]
    public void AnalyzeMethodDeclaration_ReturnsNull_WhenNoObservatorTraceAttribute()
    {
        var source = @"
            using System;
            namespace Test
            {
                public class MyClass
                {
                    public void MyMethod() { }
                }
            }";

        var compilation = CreateCompilation(source);
        var methodDeclaration = compilation.SyntaxTrees.First().GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var semanticModel = compilation.GetSemanticModel(methodDeclaration.SyntaxTree);

        var result = MethodAnalyzer.AnalyzeMethodDeclaration(methodDeclaration, semanticModel, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public void AnalyzeMethodDeclaration_ReturnsMethodInfo_WhenObservatorTraceAttributePresent()
    {
        var source = @"
            using Observator.Abstractions;
            namespace Test
            {
                public class MyClass
                {
                    [ObservatorTrace]
                    public void MyTracedMethod() { }
                }
            }";

        var compilation = CreateCompilation(source);
        var methodDeclaration = compilation.SyntaxTrees.First().GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var semanticModel = compilation.GetSemanticModel(methodDeclaration.SyntaxTree);

        var result = MethodAnalyzer.AnalyzeMethodDeclaration(methodDeclaration, semanticModel, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("MyTracedMethod", result.MethodSymbol.Name);
        Assert.False(result.IsInterfaceMethod);
    }

    [Fact]
    public void AnalyzeMethodDeclaration_ReturnsMethodInfo_ForInterfaceMethodWithAttribute()
    {
        var source = @"
            using Observator.Abstractions;
            namespace Test
            {
                public interface IMyInterface
                {
                    [ObservatorTrace]
                    void InterfaceMethod();
                }
            }";

        var compilation = CreateCompilation(source);
        var methodDeclaration = compilation.SyntaxTrees.First().GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var semanticModel = compilation.GetSemanticModel(methodDeclaration.SyntaxTree);

        var result = MethodAnalyzer.AnalyzeMethodDeclaration(methodDeclaration, semanticModel, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("InterfaceMethod", result.MethodSymbol.Name);
        Assert.True(result.IsInterfaceMethod);
    }

    [Fact]
    public void AnalyzeMethodDeclaration_ReturnsMethodInfo_ForAbstractMethodWithAttribute()
    {
        var source = @"
            using Observator.Abstractions;
            namespace Test
            {
                public abstract class MyAbstractClass
                {
                    [ObservatorTrace]
                    public abstract void AbstractMethod();
                }
            }";

        var compilation = CreateCompilation(source);
        var methodDeclaration = compilation.SyntaxTrees.First().GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var semanticModel = compilation.GetSemanticModel(methodDeclaration.SyntaxTree);

        var result = MethodAnalyzer.AnalyzeMethodDeclaration(methodDeclaration, semanticModel, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("AbstractMethod", result.MethodSymbol.Name);
        Assert.True(result.IsInterfaceMethod); // Abstract methods are treated similarly to interface methods for interception
    }

    [Fact]
    public void AnalyzeTypeDeclaration_ReturnsNoMethods_WhenNoObservatorTraceAttributeOnType()
    {
        var source = @"
            using System;
            namespace Test
            {
                public interface IMyInterface
                {
                    void MyMethod();
                }
            }";

        var compilation = CreateCompilation(source);
        var typeSymbol = compilation.GetSemanticModel(compilation.SyntaxTrees.First())
                                    .GetDeclaredSymbol(compilation.SyntaxTrees.First().GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>().First()) as INamedTypeSymbol;
        Assert.NotNull(typeSymbol); // Ensure typeSymbol is not null for the test

        var result = MethodAnalyzer.AnalyzeTypeDeclaration(typeSymbol!).ToList(); // Use null-forgiving operator after Assert.NotNull

        Assert.Empty(result);
    }

    [Fact]
    public void AnalyzeTypeDeclaration_ReturnsPublicMethods_WhenObservatorTraceAttributeOnType()
    {
        var source = @"
            using Observator.Abstractions;
            namespace Test
            {
                [ObservatorTrace]
                public interface IMyInterface
                {
                    void PublicMethod();
                    internal void InternalMethod(); // Should not be included
                    private void PrivateMethod(); // Should not be included
                    static void StaticMethod(); // Should not be included
                }
            }";

        var compilation = CreateCompilation(source);
        var typeSymbol = compilation.GetSemanticModel(compilation.SyntaxTrees.First())
                                    .GetDeclaredSymbol(compilation.SyntaxTrees.First().GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>().First()) as INamedTypeSymbol;
        Assert.NotNull(typeSymbol); // Ensure typeSymbol is not null for the test

        var result = MethodAnalyzer.AnalyzeTypeDeclaration(typeSymbol!).ToList(); // Use null-forgiving operator after Assert.NotNull

        Assert.Single(result);
        Assert.Equal("PublicMethod", result.First().MethodSymbol.Name);
        Assert.True(result.First().IsInterfaceMethod);
    }

    [Fact]
    public void AnalyzeTypeDeclaration_ExcludesMethodsWithExplicitObservatorTraceAttribute()
    {
        var source = @"
            using Observator.Abstractions;
            namespace Test
            {
                [ObservatorTrace]
                public interface IMyInterface
                {
                    void MethodFromTypeAttribute();
                    [ObservatorTrace]
                    void MethodWithExplicitAttribute(); // Should still be included by MethodAnalyzer.AnalyzeMethodDeclaration, but not by AnalyzeTypeDeclaration
                }
            }";

        var compilation = CreateCompilation(source);
        var typeSymbol = compilation.GetSemanticModel(compilation.SyntaxTrees.First())
                                    .GetDeclaredSymbol(compilation.SyntaxTrees.First().GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>().First()) as INamedTypeSymbol;
        Assert.NotNull(typeSymbol); // Ensure typeSymbol is not null for the test

        var result = MethodAnalyzer.AnalyzeTypeDeclaration(typeSymbol!).ToList(); // Use null-forgiving operator after Assert.NotNull

        Assert.Equal(2, result.Count); // Both methods should be returned by AnalyzeTypeDeclaration
        Assert.Contains(result, m => m.MethodSymbol.Name == "MethodFromTypeAttribute");
        Assert.Contains(result, m => m.MethodSymbol.Name == "MethodWithExplicitAttribute");
    }
}