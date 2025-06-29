using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Observator.Generator;

public record MethodInterceptorInfo(
    IMethodSymbol MethodSymbol,
    InterceptableLocation Location,
    bool IsInterfaceMethod
);