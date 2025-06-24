using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Observator.Generator
{
    public class MethodInterceptorInfo
    {
        public IMethodSymbol MethodSymbol { get; }
        public InterceptableLocation Location { get; }
        public bool IsInterfaceMethod { get; }

        public MethodInterceptorInfo(IMethodSymbol methodSymbol, InterceptableLocation location, bool isInterfaceMethod)
        {
            MethodSymbol = methodSymbol;
            Location = location;
            IsInterfaceMethod = isInterfaceMethod;
        }
    }
}