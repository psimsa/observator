using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Observator.Generator.Generation;

internal static class AttributeGenerator
{
    public static NamespaceDeclarationSyntax GenerateCompilerServicesNamespace()
    {
        var attributeConstructor = SyntaxFactory.ConstructorDeclaration(ObservatorConstants.InterceptsLocationAttributeName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("version")).WithType(SyntaxFactory.ParseTypeName("int")),
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("data")).WithType(SyntaxFactory.ParseTypeName("string"))
            )
            .WithBody(SyntaxFactory.Block());
            
        var attributeClass = SyntaxFactory.ClassDeclaration(ObservatorConstants.InterceptsLocationAttributeName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.FileKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword))
            .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("Attribute")))
            .AddAttributeLists(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.ParseName("AttributeUsage"))
                            .AddArgumentListArguments(
                                SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("AttributeTargets.Method")),
                                SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression))
                                    .WithNameEquals(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("AllowMultiple")))
                            )
                    )
                )
            )
            .AddMembers(attributeConstructor);

        return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("System.Runtime.CompilerServices"))
            .AddMembers(attributeClass);
    }
}