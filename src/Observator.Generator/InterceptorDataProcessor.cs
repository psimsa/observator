using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Observator.Generator;

public static class InterceptorDataProcessor
{
    public static Dictionary<string, List<MethodInterceptorInfo>> Process(
        ImmutableArray<MethodToInterceptInfo> attributedMethods,
        ImmutableArray<InvocationCallSiteInfo> callSites)
    {
        // Filter valid methods
        var validMethods = attributedMethods
            .Where(x => x.Diagnostic == null && x.MethodSymbol != null)
            .ToList();

        // Match call sites to valid methods using LINQ for clarity
        // Match call sites to valid methods in a step-by-step, readable way
        var callSiteInfos = new List<InterceptorCandidateInfo>();
        foreach (var callEntry in callSites)
        {
            var invocation = callEntry.Invocation;
            var targetMethod = callEntry.TargetMethod;
            var location = callEntry.Location;

            foreach (var validEntry in validMethods)
            {
                var methodSymbol = validEntry.MethodSymbol;
                var methodDecl = validEntry.MethodDeclaration;

                // Match by method definition and containing type
                bool isSameMethod = SymbolEqualityComparer.Default.Equals(targetMethod.OriginalDefinition, methodSymbol.OriginalDefinition)
                    && SymbolEqualityComparer.Default.Equals(targetMethod.ContainingType, methodSymbol.ContainingType);

                if (isSameMethod)
                {
                    callSiteInfos.Add(new InterceptorCandidateInfo(methodSymbol, methodDecl, invocation, location));
                }
            }
        }

        // Group by namespace
        var interceptorsByNamespace = new Dictionary<string, List<MethodInterceptorInfo>>();

        foreach (var call in callSiteInfos)
        {
            var method = call.MethodSymbol;
            var location = call.Location;
            var ns = (call.Invocation.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString())
                     ?? (call.Invocation.Ancestors().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString())
                     ?? method.ContainingType.ContainingNamespace?.ToDisplayString() ?? "";

            if (!interceptorsByNamespace.TryGetValue(ns, out var callList))
                interceptorsByNamespace[ns] = callList = new List<MethodInterceptorInfo>();

            // Find the original MethodToInterceptInfo to get IsInterfaceMethod
            var methodInfo = validMethods.FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.MethodSymbol, method));
            bool isInterfaceMethod = false;
            if (methodInfo != null)
            {
                isInterfaceMethod = methodInfo.IsInterfaceMethod;
            }
            callList.Add(new MethodInterceptorInfo(method, location, isInterfaceMethod));
        }

        return interceptorsByNamespace;
    }
}