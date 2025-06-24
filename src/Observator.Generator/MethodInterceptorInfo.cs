using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Observator.Generator
{
    public class MethodInterceptorInfo
    {
        public IMethodSymbol MethodSymbol { get; }
        public InterceptableLocation Location { get; }

        public MethodInterceptorInfo(IMethodSymbol methodSymbol, InterceptableLocation location)
        {
            MethodSymbol = methodSymbol;
            Location = location;
        }
    }
}