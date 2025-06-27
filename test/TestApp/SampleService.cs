using Microsoft.Extensions.Logging;
using Observator.Generated.TestApp;
using System;

namespace TestApp;

public partial class SampleService
{
    private readonly ILogger<SampleService> _logger;
    public SampleService(ILogger<SampleService> logger) => _logger = logger;

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
