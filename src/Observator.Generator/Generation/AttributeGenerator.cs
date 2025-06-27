using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Observator.Generator.Helpers;

namespace Observator.Generator.Generation;

internal static class AttributeGenerator
{
    public static NamespaceDeclarationSyntax GenerateCompilerServicesNamespace()
    {
        var attributeConstructor = SyntaxFactory.ConstructorDeclaration(ObservatorConstants.InterceptsLocationAttributeName)
            .AddModifiers(SyntaxTemplates.PublicKeyword)
            .AddParameterListParameters(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("version")).WithType(SyntaxFactory.ParseTypeName("int")),
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("data")).WithType(SyntaxFactory.ParseTypeName("string"))
            )
            .WithBody(SyntaxFactory.Block());
            
        var attributeClass = SyntaxFactory.ClassDeclaration(ObservatorConstants.InterceptsLocationAttributeName)
            .AddModifiers(SyntaxTemplates.FileKeyword, SyntaxTemplates.SealedKeyword)
            .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxTemplates.AttributeTypeName))
            .AddAttributeLists(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxTemplates.AttributeUsageAttribute
                            .AddArgumentListArguments(
                                SyntaxTemplates.AttributeTargetsMethodArgument,
                                SyntaxTemplates.AllowMultipleTrueArgument
                            )
                    )
                )
            )
            .AddMembers(attributeConstructor);

        return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("System.Runtime.CompilerServices"))
            .AddMembers(attributeClass);
    }
}