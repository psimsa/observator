using Observator.Generated;
using System;

namespace TestApp
{
    public interface ISampleService
    {
        string GreetViaInterface(string name);
    }

    public abstract partial class AbstractSampleService
    {
        [ObservatorTrace]
        public abstract string GreetAbstract(string name);
    }

    public partial class SampleService : AbstractSampleService, ISampleService
    {
        [ObservatorTrace]
        public string GreetExpression(string name) => $"Hi, {name} (expression)!";

        [ObservatorTrace]
        public string GreetViaInterface(string name)
        {
            Console.WriteLine($"GreetViaInterface called with: {name}");
            return $"Hello from interface, {name}!";
        }

        public override string GreetAbstract(string name)
        {
            Console.WriteLine($"GreetAbstract OVERRIDE called with: {name}");
            return $"Hello from abstract override, {name}!";
        }
    }
}
