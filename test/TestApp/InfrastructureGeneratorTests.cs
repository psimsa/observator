/*
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Observator.Generator;
using Xunit;
 
namespace TestApp
{
    public class InfrastructureGeneratorTests
    {
        [Fact]
        public async Task GeneratesInfrastructureSource_WithCorrectAssemblyNameAndVersion()
        {
            var test = new CSharpIncrementalGeneratorTest<InfrastructureGenerator, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { @"namespace Dummy { public class Foo {} }" },
                    AdditionalReferences = { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                    // Optionally, add AssemblyVersionAttribute if needed
                },
            };
 
            test.ExpectedGeneratedSources.Add(
                typeof(InfrastructureGenerator),
                "ObservatorInfrastructure.g.cs",
                // Only check for key lines to avoid brittle tests
                AssertGeneratedSourceContains("internal static class ObservatorInfrastructure"));
            await test.RunAsync();
        }
 
        private static Action<string> AssertGeneratedSourceContains(string expected)
        {
            return generated =>
            {
                Assert.Contains(expected, generated);
            };
        }
    }
}
*/