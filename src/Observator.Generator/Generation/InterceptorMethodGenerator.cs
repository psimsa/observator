using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Observator.Generator.Helpers;

namespace Observator.Generator.Generation;

internal static class InterceptorMethodGenerator
{
    public static MethodDeclarationSyntax GenerateMethodCode(List<MethodInterceptorInfo> callList, string signature)
    {
        var method = callList[0].MethodSymbol;
        var returnType = SyntaxFactory.ParseTypeName(method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        var methodName = method.Name;

        var parameters = method.Parameters.Select(p =>
            SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                .WithType(SyntaxFactory.ParseTypeName(p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))
        ).ToArray();

        var isAsync = method.ReturnType.Name == ObservatorConstants.TaskReturnType || method.ReturnType.Name == ObservatorConstants.ValueTaskReturnType;

        var modifiers = new List<SyntaxToken>
        {
            SyntaxTemplates.PublicKeyword,
            SyntaxTemplates.StaticKeyword
        };

        if (isAsync)
        {
            modifiers.Add(SyntaxTemplates.AsyncKeyword);
        }

        var attributes = callList.Select(call =>
            SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxTemplates.InterceptsLocationAttribute
                        .AddArgumentListArguments(
                            SyntaxFactory.AttributeArgument(SyntaxTemplates.NumericLiteralExpression(call.Location.Version)),
                            SyntaxFactory.AttributeArgument(SyntaxTemplates.StringLiteralExpression(call.Location.Data))
                        )
                )
            )
        );

        var thisType = callList[0].IsInterfaceMethod
            ? callList[0].MethodSymbol.ContainingType
            : method.ContainingType;

        var thisParameter = SyntaxTemplates.ThisSourceParameter(thisType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

        var allParameters = new[] { thisParameter }.Concat(parameters);

        var methodDeclaration = SyntaxFactory.MethodDeclaration(returnType, $"Intercepts{methodName}_{callList[0].Id}")
            .AddModifiers(modifiers.ToArray())
            .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(allParameters)))
            .WithBody(GenerateInterceptorBody(method, signature))
            .WithAttributeLists(SyntaxFactory.List(attributes))
            .WithLeadingTrivia(SyntaxFactory.Comment($"// {signature}"));

        var typeParameters = new List<TypeParameterSyntax>();

        if (thisType.TypeArguments.Length > 0)
        {
            typeParameters.AddRange(thisType.TypeArguments.Select(t => SyntaxFactory.TypeParameter(t.Name)));
        }
        if (method.TypeParameters.Length > 0)
        {
            typeParameters.AddRange(method.TypeParameters.Select(tp => SyntaxFactory.TypeParameter(tp.Name)));
        }
        if (typeParameters.Count > 0)
        {
            methodDeclaration = methodDeclaration.WithTypeParameterList(SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(typeParameters)));
        }

        return methodDeclaration;
    }

    private static BlockSyntax GenerateInterceptorBody(IMethodSymbol method, string signature)
    {
        var isAsync = method.ReturnType.Name == ObservatorConstants.TaskReturnType || method.ReturnType.Name == ObservatorConstants.ValueTaskReturnType;

        var invocation = SyntaxFactory.InvocationExpression(
            SyntaxTemplates.SimpleMemberAccessExpression(SyntaxTemplates.ThisSourceIdentifierName, SyntaxFactory.IdentifierName(method.Name))
        ).WithArgumentList(
            SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList(
                    method.Parameters.Select(p => SyntaxFactory.Argument(SyntaxFactory.IdentifierName(p.Name)))
                )
            )
        );

        var awaitedInvocation = isAsync ? SyntaxFactory.AwaitExpression(invocation) : (ExpressionSyntax)invocation;

        StatementSyntax invocationStatement = method.ReturnsVoid
            ? SyntaxFactory.ExpressionStatement(awaitedInvocation)
            : SyntaxFactory.ReturnStatement(awaitedInvocation);

        var tryStatement = SyntaxFactory.TryStatement()
            .WithBlock(SyntaxFactory.Block(invocationStatement))
            .WithCatches(
                SyntaxFactory.SingletonList(
                    SyntaxTemplates.ExceptionCatchClause(
                        SyntaxFactory.Block(
                            SyntaxTemplates.ActivitySetStatusErrorStatement,
                            SyntaxTemplates.ThrowStatement
                        )
                    )
                )
            )
            .WithFinally(
                SyntaxFactory.FinallyClause(
                    SyntaxFactory.Block(
                        SyntaxTemplates.ActivitySetStatusOkStatement
                    )
                )
            );

        var activityCurrent = SyntaxTemplates.ActivityCurrentMemberAccess;

        var conditionalSource = SyntaxTemplates.ActivitySourceConditionalAccess;

        var startActivityAccess = SyntaxTemplates.ActivityStartActivityMemberAccess;

        var argumentList = SyntaxFactory.ArgumentList(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Argument(
                    SyntaxFactory.ParseExpression($"\"{method.ContainingNamespace.Name}.{method.ContainingType.Name}.{signature}\"")
                )
            )
        );

        var activityInvocation = SyntaxFactory.InvocationExpression(startActivityAccess, argumentList);

        var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxTemplates.VarTypeName)
            .AddVariables(
                SyntaxFactory.VariableDeclarator("activity")
                    .WithInitializer(
                        SyntaxFactory.EqualsValueClause(activityInvocation)
                    )
            );

        var usingStatement = SyntaxFactory.UsingStatement(variableDeclaration, null, tryStatement);

        return SyntaxFactory.Block(usingStatement);
    }
}