using Observator.Generated;
using System;

namespace TestApp
{
    public class SampleService
    {
        [ObservatorTrace]
        public string Greet(string name)
        {
            Console.WriteLine($"Greet called with: {name}");
            return $"Hello, {name}!";
        }
    }
}
