using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Observator.Generator;

public record MethodInterceptorInfo(
    IMethodSymbol methodSymbol,
    InterceptableLocation location,
    bool isInterfaceMethod
)
{
    public IMethodSymbol MethodSymbol { get; set; } = methodSymbol;
    public InterceptableLocation Location { get; set; } = location;
    public bool IsInterfaceMethod { get; set; } = isInterfaceMethod;
}