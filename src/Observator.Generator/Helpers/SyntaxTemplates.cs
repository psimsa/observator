using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Observator.Generator.Helpers
{
    internal static class SyntaxTemplates
    {
        // Using Directives
        public static readonly UsingDirectiveSyntax SystemUsing =
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"));

        public static readonly UsingDirectiveSyntax SystemDiagnosticsUsing =
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Diagnostics"));

        public static readonly UsingDirectiveSyntax SystemRuntimeCompilerServicesUsing =
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Runtime.CompilerServices"));

        // Common SyntaxTokens
        public static SyntaxToken PublicKeyword = SyntaxFactory.Token(SyntaxKind.PublicKeyword);
        public static SyntaxToken StaticKeyword = SyntaxFactory.Token(SyntaxKind.StaticKeyword);
        public static SyntaxToken FileKeyword = SyntaxFactory.Token(SyntaxKind.FileKeyword);
        public static SyntaxToken SealedKeyword = SyntaxFactory.Token(SyntaxKind.SealedKeyword);
        public static SyntaxToken InternalKeyword = SyntaxFactory.Token(SyntaxKind.InternalKeyword);
        public static SyntaxToken PartialKeyword = SyntaxFactory.Token(SyntaxKind.PartialKeyword);
        public static SyntaxToken AsyncKeyword = SyntaxFactory.Token(SyntaxKind.AsyncKeyword);
        public static SyntaxToken ThisKeyword = SyntaxFactory.Token(SyntaxKind.ThisKeyword);

        // Type Names
        public static readonly TypeSyntax AttributeTypeName = SyntaxFactory.ParseTypeName("Attribute");
        public static readonly TypeSyntax ExceptionTypeName = SyntaxFactory.ParseTypeName("Exception");
        public static readonly TypeSyntax VarTypeName = SyntaxFactory.ParseTypeName("var");

        // Attributes
        public static readonly AttributeSyntax AttributeUsageAttribute =
            SyntaxFactory.Attribute(SyntaxFactory.ParseName("AttributeUsage"));

        public static readonly AttributeArgumentSyntax AttributeTargetsMethodArgument =
            SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("AttributeTargets.Method"));

        public static readonly AttributeArgumentSyntax AllowMultipleTrueArgument = SyntaxFactory
            .AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression))
            .WithNameEquals(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("AllowMultiple")));

        public static readonly AttributeSyntax InterceptsLocationAttribute =
            SyntaxFactory.Attribute(SyntaxFactory.ParseName("InterceptsLocation"));

        // Expressions
        public static LiteralExpressionSyntax StringLiteralExpression(string value) =>
            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));

        public static LiteralExpressionSyntax NumericLiteralExpression(int value) =>
            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));

        public static readonly IdentifierNameSyntax ActivityIdentifierName = SyntaxFactory.IdentifierName("Activity");
        public static readonly IdentifierNameSyntax CurrentIdentifierName = SyntaxFactory.IdentifierName("Current");
        public static readonly IdentifierNameSyntax SourceIdentifierName = SyntaxFactory.IdentifierName("Source");

        public static readonly IdentifierNameSyntax StartActivityIdentifierName =
            SyntaxFactory.IdentifierName("StartActivity");

        public static readonly IdentifierNameSyntax ThisSourceIdentifierName = SyntaxFactory.IdentifierName("@source");

        // Member Access
        public static MemberAccessExpressionSyntax
            SimpleMemberAccessExpression(ExpressionSyntax expression, SimpleNameSyntax name) =>
            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, name);

        public static readonly MemberAccessExpressionSyntax ActivityCurrentMemberAccess =
            SimpleMemberAccessExpression(ActivityIdentifierName, CurrentIdentifierName);

        public static readonly ConditionalAccessExpressionSyntax ActivitySourceConditionalAccess =
            SyntaxFactory.ConditionalAccessExpression(ActivityCurrentMemberAccess,
                SyntaxFactory.MemberBindingExpression(SourceIdentifierName));

        public static readonly MemberAccessExpressionSyntax ActivityStartActivityMemberAccess =
            SimpleMemberAccessExpression(ActivitySourceConditionalAccess, StartActivityIdentifierName);

        // Statements and Clauses
        public static readonly StatementSyntax ThrowStatement = SyntaxFactory.ThrowStatement();

        public static readonly CatchDeclarationSyntax ExceptionCatchDeclaration =
            SyntaxFactory.CatchDeclaration(ExceptionTypeName, SyntaxFactory.Identifier("exception"));

        public static CatchClauseSyntax ExceptionCatchClause(BlockSyntax block) => SyntaxFactory.CatchClause()
            .WithDeclaration(ExceptionCatchDeclaration).WithBlock(block);

        public static readonly ExpressionStatementSyntax ActivitySetStatusErrorStatement =
            SyntaxFactory.ExpressionStatement(
                SyntaxFactory.ParseExpression(ObservatorConstants.ActivitySetStatusError));

        public static readonly ExpressionStatementSyntax ActivitySetStatusOkStatement =
            SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression(ObservatorConstants.ActivitySetStatusOk));

        private static ParameterSyntax ThisSourceParameterBase =
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("@source")).AddModifiers(ThisKeyword);

        public static ParameterSyntax ThisSourceParameter(string typeName) =>
            ThisSourceParameterBase.WithType(SyntaxFactory.ParseTypeName(typeName));
    }
}