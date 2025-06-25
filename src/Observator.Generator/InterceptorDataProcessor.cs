using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Observator.Generator
{
    public static class InterceptorDataProcessor
    {
        public static Dictionary<string, Dictionary<string, List<MethodInterceptorInfo>>> Process(
            ImmutableArray<MethodToInterceptInfo> attributedMethods,
            ImmutableArray<InvocationCallSiteInfo> callSites)
        {
            // Filter valid methods
            var validMethods = attributedMethods
                .Where(x => x.Diagnostic == null && x.MethodSymbol != null)
                .ToList();

            // Match call sites to valid methods using LINQ for clarity
            var callSiteInfos = (
                from callEntry in callSites
                let invocation = callEntry.Invocation
                let targetMethod = callEntry.TargetMethod
                let location = callEntry.Location
                from validEntry in validMethods
                let methodSymbol = validEntry.MethodSymbol
                let methodDecl = validEntry.MethodDeclaration
                where SymbolEqualityComparer.Default.Equals(targetMethod.OriginalDefinition, methodSymbol.OriginalDefinition)
                   && SymbolEqualityComparer.Default.Equals(targetMethod.ContainingType, methodSymbol.ContainingType)
                select new InterceptorCandidateInfo(methodSymbol, methodDecl, invocation, location)
            ).ToList();

            // Group by namespace and method signature
            var interceptorsByNamespace = new Dictionary<string, Dictionary<string, List<MethodInterceptorInfo>>>();

            foreach (var call in callSiteInfos)
            {
                var method = call.MethodSymbol;
                var location = call.Location;
                var ns = (call.Invocation.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString())
                          ?? method.ContainingType.ContainingNamespace?.ToDisplayString() ?? "";
                var methodSig = method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                // Helper to get or add dictionary entry
                if (!interceptorsByNamespace.TryGetValue(ns, out var methodDict))
                    interceptorsByNamespace[ns] = methodDict = new Dictionary<string, List<MethodInterceptorInfo>>();
                if (!methodDict.TryGetValue(methodSig, out var callList))
                    methodDict[methodSig] = callList = new List<MethodInterceptorInfo>();

                // Find the original MethodToInterceptInfo to get IsInterfaceMethod
                var methodInfo = validMethods.FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.MethodSymbol, method));
                bool isInterfaceMethod = methodInfo?.IsInterfaceMethod ?? false;
                callList.Add(new MethodInterceptorInfo(method, location, isInterfaceMethod));
            }

            return interceptorsByNamespace;
        }
    }
}