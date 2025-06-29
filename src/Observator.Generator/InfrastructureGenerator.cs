using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using sf = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


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
        /*var traceAttribute = sf.ClassDeclaration(traceAttrName)
            .AddModifiers(sf.Token(SyntaxKind.InternalKeyword), sf.Token(SyntaxKind.SealedKeyword))
            .AddBaseListTypes(
                sf.SimpleBaseType(
                    sf.ParseTypeName("System.Attribute")))
            .AddAttributeLists(
                sf.AttributeList(
                    sf.SingletonSeparatedList(
                        sf.Attribute(
                            sf.ParseName("System.AttributeUsage"))
                        .AddArgumentListArguments(
                            sf.AttributeArgument(
                                sf.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    sf.ParseName("System.AttributeTargets"),
                                    sf.IdentifierName("Method")))))))
            .AddMembers(
                sf.PropertyDeclaration(sf.PredefinedType(sf.Token(SyntaxKind.BoolKeyword)), "IncludeParameters")
                    .AddModifiers(sf.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        sf.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(sf.Token(SyntaxKind.SemicolonToken)),
                        sf.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(sf.Token(SyntaxKind.SemicolonToken)))
                    .WithInitializer(
                        sf.EqualsValueClause(sf.LiteralExpression(SyntaxKind.FalseLiteralExpression)))
                    .WithSemicolonToken(sf.Token(SyntaxKind.SemicolonToken)),
                sf.PropertyDeclaration(sf.PredefinedType(sf.Token(SyntaxKind.BoolKeyword)), "IncludeReturnValue")
                    .AddModifiers(sf.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        sf.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(sf.Token(SyntaxKind.SemicolonToken)),
                        sf.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(sf.Token(SyntaxKind.SemicolonToken)))
                    .WithInitializer(
                        sf.EqualsValueClause(sf.LiteralExpression(SyntaxKind.FalseLiteralExpression)))
                    .WithSemicolonToken(sf.Token(SyntaxKind.SemicolonToken))
            );*/

        // Infrastructure class
        var infraClass = sf.ClassDeclaration(className)
            .AddModifiers(sf.Token(SyntaxKind.InternalKeyword), sf.Token(SyntaxKind.StaticKeyword))
            .AddMembers(
                sf.PropertyDeclaration(sf.PredefinedType(sf.Token(SyntaxKind.StringKeyword)), "ActivitySourceName")
                    .AddModifiers(sf.Token(SyntaxKind.PublicKeyword), sf.Token(SyntaxKind.StaticKeyword))
                    .WithExpressionBody(
                        sf.ArrowExpressionClause(
                            sf.LiteralExpression(SyntaxKind.StringLiteralExpression, sf.Literal(assemblyName))))
                    .WithSemicolonToken(sf.Token(SyntaxKind.SemicolonToken)),
                sf.PropertyDeclaration(sf.PredefinedType(sf.Token(SyntaxKind.StringKeyword)), "Version")
                    .AddModifiers(sf.Token(SyntaxKind.PublicKeyword), sf.Token(SyntaxKind.StaticKeyword))
                    .WithExpressionBody(
                        sf.ArrowExpressionClause(
                            sf.LiteralExpression(SyntaxKind.StringLiteralExpression, sf.Literal(version))))
                    .WithSemicolonToken(sf.Token(SyntaxKind.SemicolonToken))
            );

        // Namespace
        var ns = sf.NamespaceDeclaration(sf.ParseName(nsName))
            .AddUsings(
                sf.UsingDirective(sf.ParseName("System.Diagnostics")),
                sf.UsingDirective(sf.ParseName("System.Diagnostics.Metrics")),
                sf.UsingDirective(sf.ParseName("System")))
            .AddMembers(infraClass);

        // Compilation unit
        var cu = sf.CompilationUnit()
            .WithLeadingTrivia(sf.Comment("// <auto-generated />"))
            .AddMembers(ns)
            .NormalizeWhitespace();

        return cu.ToFullString();
    }
}