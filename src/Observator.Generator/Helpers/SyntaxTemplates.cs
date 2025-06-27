using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Observator.Generator.Helpers
{
    internal static class SyntaxTemplates
    {
        // Using Directives
        public static UsingDirectiveSyntax SystemUsing => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"));
        public static UsingDirectiveSyntax SystemDiagnosticsUsing => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Diagnostics"));
        public static UsingDirectiveSyntax SystemRuntimeCompilerServicesUsing => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Runtime.CompilerServices"));

        // Common SyntaxTokens
        public static SyntaxToken PublicKeyword => SyntaxFactory.Token(SyntaxKind.PublicKeyword);
        public static SyntaxToken StaticKeyword => SyntaxFactory.Token(SyntaxKind.StaticKeyword);
        public static SyntaxToken FileKeyword => SyntaxFactory.Token(SyntaxKind.FileKeyword);
        public static SyntaxToken SealedKeyword => SyntaxFactory.Token(SyntaxKind.SealedKeyword);
        public static SyntaxToken InternalKeyword => SyntaxFactory.Token(SyntaxKind.InternalKeyword);
        public static SyntaxToken PartialKeyword => SyntaxFactory.Token(SyntaxKind.PartialKeyword);
        public static SyntaxToken AsyncKeyword => SyntaxFactory.Token(SyntaxKind.AsyncKeyword);
        public static SyntaxToken ThisKeyword => SyntaxFactory.Token(SyntaxKind.ThisKeyword);

        // Type Names
        public static TypeSyntax AttributeTypeName => SyntaxFactory.ParseTypeName("Attribute");
        public static TypeSyntax ExceptionTypeName => SyntaxFactory.ParseTypeName("Exception");
        public static TypeSyntax VarTypeName => SyntaxFactory.ParseTypeName("var");

        // Attributes
        public static AttributeSyntax AttributeUsageAttribute => SyntaxFactory.Attribute(SyntaxFactory.ParseName("AttributeUsage"));
        public static AttributeArgumentSyntax AttributeTargetsMethodArgument => SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("AttributeTargets.Method"));
        public static AttributeArgumentSyntax AllowMultipleTrueArgument => SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)).WithNameEquals(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("AllowMultiple")));
        public static AttributeSyntax InterceptsLocationAttribute => SyntaxFactory.Attribute(SyntaxFactory.ParseName("InterceptsLocation"));

        // Expressions
        public static LiteralExpressionSyntax TrueLiteralExpression => SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
        public static LiteralExpressionSyntax StringLiteralExpression(string value) => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
        public static LiteralExpressionSyntax NumericLiteralExpression(int value) => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        public static IdentifierNameSyntax ActivityIdentifierName => SyntaxFactory.IdentifierName("Activity");
        public static IdentifierNameSyntax CurrentIdentifierName => SyntaxFactory.IdentifierName("Current");
        public static IdentifierNameSyntax SourceIdentifierName => SyntaxFactory.IdentifierName("Source");
        public static IdentifierNameSyntax StartActivityIdentifierName => SyntaxFactory.IdentifierName("StartActivity");
        public static IdentifierNameSyntax ExceptionIdentifierName => SyntaxFactory.IdentifierName("exception");
        public static IdentifierNameSyntax ThisSourceIdentifierName => SyntaxFactory.IdentifierName("@source");
        public static IdentifierNameSyntax VariableDeclaratorName(string name) => SyntaxFactory.IdentifierName(name);

        // Member Access
        public static MemberAccessExpressionSyntax SimpleMemberAccessExpression(ExpressionSyntax expression, SimpleNameSyntax name) => SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, name);
        public static MemberAccessExpressionSyntax ActivityCurrentMemberAccess => SimpleMemberAccessExpression(ActivityIdentifierName, CurrentIdentifierName);
        public static ConditionalAccessExpressionSyntax ActivitySourceConditionalAccess => SyntaxFactory.ConditionalAccessExpression(ActivityCurrentMemberAccess, SyntaxFactory.MemberBindingExpression(SourceIdentifierName));
        public static MemberAccessExpressionSyntax ActivityStartActivityMemberAccess => SimpleMemberAccessExpression(ActivitySourceConditionalAccess, StartActivityIdentifierName);
        
        // Statements and Clauses
        public static BlockSyntax EmptyBlock => SyntaxFactory.Block();
        public static StatementSyntax ThrowStatement => SyntaxFactory.ThrowStatement();
        public static CatchDeclarationSyntax ExceptionCatchDeclaration => SyntaxFactory.CatchDeclaration(ExceptionTypeName, SyntaxFactory.Identifier("exception"));
        public static CatchClauseSyntax ExceptionCatchClause(BlockSyntax block) => SyntaxFactory.CatchClause().WithDeclaration(ExceptionCatchDeclaration).WithBlock(block);
        public static ExpressionStatementSyntax ActivitySetStatusErrorStatement => SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression(ObservatorConstants.ActivitySetStatusError));
        public static ExpressionStatementSyntax ActivitySetStatusOkStatement => SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression(ObservatorConstants.ActivitySetStatusOk));
        public static ParameterSyntax ThisSourceParameter(string typeName) => SyntaxFactory.Parameter(SyntaxFactory.Identifier("@source")).AddModifiers(ThisKeyword).WithType(SyntaxFactory.ParseTypeName(typeName));
    }
}