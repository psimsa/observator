using Observator.Generated;
using System;

namespace TestApp
{
    public partial class SampleService
    {
        [ObservatorTrace]
        public string Greet(string name)
        {
            Console.WriteLine($"Greet called with: {name}");
            return $"Hello, {name}!";
        }

        [ObservatorTrace]
        public string GreetWithException(string name)
        {
            throw new NotImplementedException($"GreetWithException not implemented for: {name}");
        }
    }
}
