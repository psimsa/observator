using Observator.Abstractions;

namespace TestApp
{
    [ObservatorTrace]
    public interface IMyService
    {
        string DoSomething(string input);
        
        [ObservatorTrace(IncludeParameters = false, IncludeReturnValue = true)]
        int Calculate(int a, int b);
    }
}