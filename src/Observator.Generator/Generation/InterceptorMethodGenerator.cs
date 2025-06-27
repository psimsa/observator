using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
            SyntaxFactory.Token(SyntaxKind.StaticKeyword)
        };

        if (isAsync)
        {
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword));
        }
            
        var attributes = callList.Select(call =>
            SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.ParseName("InterceptsLocation"))
                        .AddArgumentListArguments(
                            SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(call.Location.Version))),
                            SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(call.Location.Data)))
                        )
                )
            )
        ).ToList();

        string thisType = callList[0].IsInterfaceMethod
            ? callList[0].MethodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            : method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var thisParameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("@source"))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.ThisKeyword))
            .WithType(SyntaxFactory.ParseTypeName(thisType));

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
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("@source"),
                SyntaxFactory.IdentifierName(method.Name)
            )
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
                    SyntaxFactory.CatchClause()
                        .WithDeclaration(SyntaxFactory.CatchDeclaration(SyntaxFactory.ParseTypeName("Exception"), SyntaxFactory.Identifier("exception")))
                        .WithBlock(SyntaxFactory.Block(
                            SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression(ObservatorConstants.ActivitySetStatusError)),
                            SyntaxFactory.ThrowStatement()
                        ))
                )
            )
            .WithFinally(
                SyntaxFactory.FinallyClause(
                    SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression(ObservatorConstants.ActivitySetStatusOk))
                    )
                )
            );

        var activityCurrent = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("Activity"),
            SyntaxFactory.IdentifierName("Current")
        );

        var conditionalSource = SyntaxFactory.ConditionalAccessExpression(
            activityCurrent,
            SyntaxFactory.MemberBindingExpression(SyntaxFactory.IdentifierName("Source"))
        );

        var startActivityAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            conditionalSource,
            SyntaxFactory.IdentifierName("StartActivity")
        );

        var argumentList = SyntaxFactory.ArgumentList(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Argument(
                    SyntaxFactory.ParseExpression($"$\"{method.ContainingType.Name}.{method.Name}\"")
                )
            )
        );

        var activityInvocation = SyntaxFactory.InvocationExpression(startActivityAccess, argumentList);

        var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("var"))
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