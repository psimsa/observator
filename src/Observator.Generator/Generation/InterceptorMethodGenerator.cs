using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Observator.Generator.Helpers;

namespace Observator.Generator.Generation;

internal static class InterceptorMethodGenerator
{
    public static MethodDeclarationSyntax GenerateMethodCode(List<MethodInterceptorInfo> callList)
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
        ).ToList();

        string thisType = callList[0].IsInterfaceMethod
            ? callList[0].MethodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            : method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var thisParameter = SyntaxTemplates.ThisSourceParameter(thisType);

        var allParameters = new[] { thisParameter }.Concat(parameters);

        var methodDeclaration = SyntaxFactory.MethodDeclaration(returnType, $"Intercepts{methodName}")
            .AddModifiers(modifiers.ToArray())
            .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(allParameters)))
            .WithBody(GenerateInterceptorBody(method))
            .WithAttributeLists(SyntaxFactory.List(attributes));

        return methodDeclaration;
    }

    private static BlockSyntax GenerateInterceptorBody(IMethodSymbol method)
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
                    SyntaxFactory.ParseExpression($"\"{method.ContainingNamespace.Name}.{method.ContainingType.Name}.{method.Name}\"")
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