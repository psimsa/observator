using Observator.Abstractions;

namespace TestLib;

public partial class Class1
{

    [ObservatorTraceAttribute]
    public void Foo()
    {
        Console.WriteLine("Hello from Class1.Foo()");
    }

    [ObservatorTraceAttribute]
    public void Bar()
    {
        Foo();
        Console.WriteLine("Hello from Class1.Bar()");
    }
}
