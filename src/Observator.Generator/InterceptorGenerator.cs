using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Observator.Generator.Diagnostics;
using System.Diagnostics;
using System.Collections.Immutable;

namespace Observator.Generator
{
    [Generator]
    public class InterceptorGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var attributedMethods = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is MethodDeclarationSyntax,
                    transform: (ctx, ct) => MethodAnalyzer.AnalyzeMethodDeclaration(ctx.Node, ctx, ct))
                .Where(x => x != null)
                .Select((x, _) => x);

            var callSites = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is InvocationExpressionSyntax,
                    transform: (ctx, ct) => CallSiteAnalyzer.AnalyzeInvocationExpression(ctx.Node, ctx, ct))
                .Where(x => x != null)
                .Select((x, _) => x);

            var combined = attributedMethods.Collect().Combine(callSites.Collect());

            context.RegisterSourceOutput(combined, (spc, tuple) =>
            {
                var attributedMethodsArr = tuple.Left;
                var callSitesArr = tuple.Right;

                foreach (var entry in attributedMethodsArr)
                {
                    if (entry.Diagnostic != null)
                        spc.ReportDiagnostic(entry.Diagnostic);
                }

                var interceptorsByNamespace = InterceptorDataProcessor.Process(attributedMethodsArr, callSitesArr);
                var generatedCode = SourceCodeGenerator.Generate(interceptorsByNamespace);

                spc.AddSource("ObservatorInterceptors.g.cs", SourceText.From(generatedCode, System.Text.Encoding.UTF8));
            });
        }
    }
}
