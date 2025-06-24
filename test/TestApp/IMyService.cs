using Observator.Abstractions;

namespace TestApp
{
    // [ObservatorInterfaceTrace]
    public interface IMyService
    {
        string DoSomething(string input);
        
        [ObservatorInterfaceTrace(IncludeParameters = false, IncludeReturnValue = true)]
        int Calculate(int a, int b);
    }
}