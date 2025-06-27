using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Observator.Generator.Generation;

internal static class ClassGenerator
{
    public static MemberDeclarationSyntax GenerateObservatorGeneratedClass(List<MethodInterceptorInfo> methodGroup)
    {
        var methods = methodGroup
            .GroupBy(g => g.MethodSymbol.Name)
            .Select(x => InterceptorMethodGenerator.GenerateMethodCode(x.ToList()))
            .ToList();

        var interceptorClass = SyntaxFactory.ClassDeclaration("ObservatorGenerated")
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.InternalKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddMembers(methods.ToArray());

        return interceptorClass;
    }
}