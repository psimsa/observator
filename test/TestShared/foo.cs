using Observator.Abstractions;

namespace Foo;

public class Bar
{
    [ObservatorTrace]
    public void Baz()
    {
        // This is a test method in the foo.bar class.
        // It does not do anything significant.
        System.Console.WriteLine("Hello from foo.bar.baz()");
    }
}