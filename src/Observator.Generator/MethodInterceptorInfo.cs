using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Observator.Generator
{
    public class MethodInterceptorInfo
    {
        public IMethodSymbol MethodSymbol { get; }
        public InterceptableLocation Location { get; }
        public IFieldSymbol? LoggerField { get; }

        public MethodInterceptorInfo(IMethodSymbol methodSymbol, InterceptableLocation location, IFieldSymbol? loggerField)
        {
            MethodSymbol = methodSymbol;
            Location = location;
            LoggerField = loggerField;
        }
    }
}