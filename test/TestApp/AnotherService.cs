using Observator.Generated.TestApp;

namespace TestApp;

public class AnotherService
{
    [ObservatorTrace]
    public string AnotherMethod(string input)
    {
        Console.WriteLine($"AnotherMethod called with: {input}");
        return $"Processed: {input}";
    }

    [ObservatorTrace]
    public int Calculate(int a, int b)
    {
        Console.WriteLine($"Calculate called with: {a}, {b}");
        return a + b;
    }
}