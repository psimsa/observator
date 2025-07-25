﻿using Observator;

namespace TestLib;

// [ObservatorTraceAttribute] - This is a placeholder for potential future use of the ObservatorTraceAttribute on the class itself.
public partial class MyTestClass<T>
{

    [ObservatorTraceAttribute]
    public void Foo()
    {
        Console.WriteLine("Hello from Class1.Foo()");
    }

    [ObservatorTrace]
    public void Foo(string name)
    {
        Console.WriteLine($"Hello from Class1.Foo({name})");
    }

    [ObservatorTraceAttribute]
    public void Foo<TB>(TB hello, string name)
    {
        Console.WriteLine($"Hello from Class1.Foo({name})");
    }

    [ObservatorTraceAttribute]
    public void Bar()
    {
        Foo();
        // Foo("sadf");
        // Foo(int.Max, "asdf");
        Console.WriteLine("Hello from Class1.Bar()");
    }
}
