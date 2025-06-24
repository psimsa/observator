using Observator.Generated;
namespace TestLib;

public partial class Class1
{

    [ObservatorTrace]
    public void Foo()
    {
        Console.WriteLine("Hello from Class1.Foo()");
    }
}
