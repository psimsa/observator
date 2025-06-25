// Covers internal classes from Observator.Generator
using System;
using Xunit;
using System.Reflection;

namespace TestLib
{
    public class ObservatorGeneratorInternalsTests
    {
        [Fact]
        public void ObservatorInfrastructure_Type_IsAccessible()
        {
            var type = Type.GetType("Observator.Generator.InfrastructureGenerator+ObservatorInfrastructure, Observator.Generator");
            Assert.NotNull(type);
            Assert.True(type.IsAbstract && type.IsSealed); // static class
        }

        [Fact]
        public void ObservatorTraceAttribute_Type_IsAccessible()
        {
            var type = Type.GetType("Observator.Generator.InfrastructureGenerator+ObservatorTraceAttribute, Observator.Generator");
            Assert.NotNull(type);
            Assert.True(type.IsSubclassOf(typeof(Attribute)));
        }

        [Fact]
        public void DiagnosticDescriptors_Type_IsAccessible()
        {
            var type = Type.GetType("Observator.Generator.Diagnostics.DiagnosticDescriptors, Observator.Generator");
            Assert.NotNull(type);
            Assert.True(type.IsAbstract && type.IsSealed); // static class
        }

        [Fact]
        public void DiagnosticReporter_Type_IsAccessible()
        {
            var type = Type.GetType("Observator.Generator.Diagnostics.DiagnosticReporter, Observator.Generator");
            Assert.NotNull(type);
            Assert.True(type.IsAbstract && type.IsSealed); // static class
        }
    }
}