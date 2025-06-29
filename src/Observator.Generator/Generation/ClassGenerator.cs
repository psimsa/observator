using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Observator.Generator.Helpers;

namespace Observator.Generator.Generation;

internal static class ClassGenerator
{
    public static MemberDeclarationSyntax GenerateObservatorGeneratedClass(List<MethodInterceptorInfo> methodGroup)
    {
        var methods = methodGroup
            .GroupBy(MethodSignature)
            .Select(x => InterceptorMethodGenerator.GenerateMethodCode(x.ToList(), x.Key));

        var interceptorClass = SyntaxFactory.ClassDeclaration("ObservatorGenerated")
            .AddModifiers(
                SyntaxTemplates.InternalKeyword,
                SyntaxTemplates.StaticKeyword,
                SyntaxTemplates.PartialKeyword)
            .AddMembers([.. methods]);

        return interceptorClass;

        static string MethodSignature(MethodInterceptorInfo methodInterceptorInfo)
        {
            var methodSymbol = methodInterceptorInfo.MethodSymbol;
            var parameters = methodSymbol.Parameters.Select(p => p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            var typeParameters = methodSymbol.TypeParameters.Select(t => t.Name);
            string typeParametersString = typeParameters.Any() ? $"<{string.Join(", ", typeParameters)}>" : string.Empty;
            return $"{methodSymbol.Name}{typeParametersString}({string.Join(", ", parameters)})";
        }
    }
}