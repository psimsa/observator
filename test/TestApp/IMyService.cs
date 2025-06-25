using Observator.Abstractions;

namespace TestApp
{
    [ObservatorTrace]
    public interface IMyService
    {
        string DoSomething(string input);
        
        int Calculate(int a, int b);
    }
}