using Microsoft.Extensions.Logging;

using Observator;

namespace TestApp;

public partial class EdgeCases
{
    private readonly ILogger<EdgeCases> _logger;
    public EdgeCases(ILogger<EdgeCases> logger) => _logger = logger;

    // Iterator method
    [ObservatorTrace]
    public IEnumerable<int> GetNumbers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return i;
        }
    }

    // Async iterator method
    [ObservatorTrace]
    public async IAsyncEnumerable<int> GetNumbersAsync(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await Task.Delay(10);
            yield return i;
        }
    }

    // Async Task method
    [ObservatorTrace]
    public async Task<string> GetGreetingAsync(string name)
    {
        await Task.Delay(10);
        return $"Hello async, {name}!";
    }

    // Task method (non-async)
    [ObservatorTrace]
    public Task<string> GetGreetingTask(string name)
    {
        return Task.FromResult($"Hello task, {name}!");
    }

    // Method with ref/out parameters
    [ObservatorTrace]
    public void ProcessValues(ref int a, out int b)
    {
        a = a * 2;
        b = a + 10;
    }

    // Method with struct parameter (should trigger diagnostic in future)
    [ObservatorTrace]
    public void ProcessStruct(MyStruct s)
    {
        Console.WriteLine($"Processing struct: {s.Value}");
    }
}

public struct MyStruct
{
    public int Value { get; set; }
}
