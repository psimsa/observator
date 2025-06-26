using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Observator.Generator;

[Generator]
public class InfrastructureGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Use the compilation provider to get assembly info
        var assemblyInfo = context.CompilationProvider.Select((compilation, _) =>
        {
            var assemblyName = compilation.AssemblyName ?? "Unknown";
            var version = "1.0.0.0";
            var versionAttr = compilation.Assembly
                .GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "AssemblyVersionAttribute");
            if (versionAttr != null && versionAttr.ConstructorArguments.Length > 0)
            {
                version = versionAttr.ConstructorArguments[0].Value?.ToString() ?? version;
            }
            return (assemblyName, version);
        });

        context.RegisterSourceOutput(assemblyInfo, (spc, tuple) =>
        {
            var (assemblyName, version) = tuple;
            var source = GenerateInfrastructureSource(assemblyName, version);
            spc.AddSource("ObservatorInfrastructure.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }

    private string GenerateInfrastructureSource(string assemblyName, string version)
    {
        // Use SyntaxFactory to generate the code instead of string interpolation
        var nsName = $"Observator.Generated.{assemblyName}";
        var className = "ObservatorInfrastructure";
        var traceAttrName = "ObservatorTraceAttribute";

        // Attribute class
        var traceAttribute = SyntaxFactory.ClassDeclaration(traceAttrName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword))
            .AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(
                    SyntaxFactory.ParseTypeName("System.Attribute")))
            .AddAttributeLists(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(
                            SyntaxFactory.ParseName("System.AttributeUsage"))
                        .AddArgumentListArguments(
                            SyntaxFactory.AttributeArgument(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.ParseName("System.AttributeTargets"),
                                    SyntaxFactory.IdentifierName("Method")))))))
            .AddMembers(
                SyntaxFactory.PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), "IncludeParameters")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))
                    .WithInitializer(
                        SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), "IncludeReturnValue")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))
                    .WithInitializer(
                        SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            );

        // Infrastructure class
        var infraClass = SyntaxFactory.ClassDeclaration(className)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .AddMembers(
                SyntaxFactory.PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), "ActivitySourceName")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                    .WithExpressionBody(
                        SyntaxFactory.ArrowExpressionClause(
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(assemblyName))))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), "Version")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                    .WithExpressionBody(
                        SyntaxFactory.ArrowExpressionClause(
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(version))))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            );

        // Namespace
        var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(nsName))
            .AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Diagnostics")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Diagnostics.Metrics")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
            .AddMembers(infraClass, traceAttribute);

        // Compilation unit
        var cu = SyntaxFactory.CompilationUnit()
            .WithLeadingTrivia(SyntaxFactory.Comment("// <auto-generated />"))
            .AddMembers(ns)
            .NormalizeWhitespace();

        return cu.ToFullString();
    }
}