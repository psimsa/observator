using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Observator.Generator.Generation;
using Observator.Generator.Helpers;

using System.Collections.Generic;

namespace Observator.Generator;

public static class SourceCodeGenerator
{
    public static string Generate(Dictionary<string, List<MethodInterceptorInfo>> interceptors)
    {
        var namespaceDeclarations = new List<MemberDeclarationSyntax>();

        foreach (var nsGroup in interceptors)
        {
            var ns = nsGroup.Key;

            var interceptorClass = ClassGenerator.GenerateObservatorGeneratedClass(nsGroup.Value);

            if (!string.IsNullOrEmpty(ns))
            {
                var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(ns))
                    .AddMembers(interceptorClass);
                namespaceDeclarations.Add(namespaceDeclaration);
            }
            else
            {
                namespaceDeclarations.Add(interceptorClass);
            }
        }

        var compilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(SyntaxTemplates.SystemUsing,
                        SyntaxTemplates.SystemDiagnosticsUsing,
                        SyntaxTemplates.SystemRuntimeCompilerServicesUsing)
            .AddMembers(namespaceDeclarations.ToArray())
            .AddMembers(AttributeGenerator.GenerateCompilerServicesNamespace())
            .WithLeadingTrivia(SyntaxFactory.Comment(ObservatorConstants.AutoGeneratedComment));

        return compilationUnit.NormalizeWhitespace().ToFullString();
    }
}