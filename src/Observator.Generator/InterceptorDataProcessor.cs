using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Observator.Generator
{
    public static class InterceptorDataProcessor
    {
        public static Dictionary<string, Dictionary<string, List<MethodInterceptorInfo>>> Process(ImmutableArray<MethodToInterceptInfo> attributedMethods, ImmutableArray<InvocationCallSiteInfo> callSites)
        {
            var validMethods = attributedMethods.Where(x => x.Diagnostic == null && x.MethodSymbol != null && x.MethodDeclaration != null).ToList();

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

                    if (SymbolEqualityComparer.Default.Equals(targetMethod.OriginalDefinition, methodSymbol.OriginalDefinition) &&
                        SymbolEqualityComparer.Default.Equals(targetMethod.ContainingType, methodSymbol.ContainingType))
                    {
                        callSiteInfos.Add(new InterceptorCandidateInfo(methodSymbol, methodDecl, invocation, location));
                        break;
                    }
                }
            }

            var interceptorsByNamespace = new Dictionary<string, Dictionary<string, List<MethodInterceptorInfo>>>();
            foreach (var call in callSiteInfos)
            {
                var method = call.MethodSymbol;
                var location = call.Location;

                var ns = (call.Invocation.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString())
                          ?? method.ContainingType.ContainingNamespace?.ToDisplayString() ?? "";

                var methodSig = method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                if (!interceptorsByNamespace.TryGetValue(ns, out var methodDict))
                {
                    methodDict = new Dictionary<string, List<MethodInterceptorInfo>>();
                    interceptorsByNamespace[ns] = methodDict;
                }

                if (!methodDict.TryGetValue(methodSig, out var callList))
                {
                    callList = new List<MethodInterceptorInfo>();
                    methodDict[methodSig] = callList;
                }

                callList.Add(new MethodInterceptorInfo(method, location));
            }

            return interceptorsByNamespace;
        }
    }
}