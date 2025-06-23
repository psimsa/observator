using Observator.Abstractions;

namespace TestApp;

[ObservatorTraceAttribute]
public interface IMyService
{
    string DoSomething(string input);
    
    int Calculate(int a, int b);
}