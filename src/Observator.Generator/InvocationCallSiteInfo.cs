using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Observator.Generator
{
    public class InvocationCallSiteInfo
    {
        public InvocationExpressionSyntax Invocation { get; }
        public IMethodSymbol TargetMethod { get; }
        public InterceptableLocation Location { get; }

        public InvocationCallSiteInfo(InvocationExpressionSyntax invocation, IMethodSymbol targetMethod, InterceptableLocation location)
        {
            Invocation = invocation;
            TargetMethod = targetMethod;
            Location = location;
        }
    }
}