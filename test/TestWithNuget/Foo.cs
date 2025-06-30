using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Observator;

namespace TestWithNuget;

internal class Foo
{
    public Foo()
    {
        TestMethod();
    }

    [ObservatorTrace]

    public void TestMethod()
    {
        Console.WriteLine("TestMethod executed.");
    }
}
