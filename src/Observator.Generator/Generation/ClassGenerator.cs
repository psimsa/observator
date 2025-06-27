using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Observator.Generator.Helpers;

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
                SyntaxTemplates.InternalKeyword,
                SyntaxTemplates.StaticKeyword,
                SyntaxTemplates.PartialKeyword)
            .AddMembers(methods.ToArray());

        return interceptorClass;
    }
}