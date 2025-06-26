namespace TestLib;

public partial class Class1
{

    [Observator.Generated.TestLib.ObservatorTraceAttribute]
    public void Foo()
    {
        Console.WriteLine("Hello from Class1.Foo()");
    }

    [Observator.Generated.TestLib.ObservatorTraceAttribute]
    public void Bar()
    {
        Foo();
        Console.WriteLine("Hello from Class1.Bar()");
    }
}
