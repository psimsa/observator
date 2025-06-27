// Unit tests for MyService covering all public methods and edge cases
using Xunit;

namespace TestApp;

public class MyServiceTests
{
    [Fact]
    public void DoSomething_ReturnsExpectedResult()
    {
        var service = new MyService();
        Assert.Equal("input", service.DoSomething("input"));
        Assert.Equal(string.Empty, service.DoSomething(string.Empty));
        Assert.Null(service.DoSomething(null));
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(-1, -2, -3)]
    [InlineData(0, 0, 0)]
    [InlineData(int.MaxValue, 0, int.MaxValue)]
    [InlineData(int.MinValue, 1, int.MinValue + 1)]
    public void Calculate_ReturnsSum(int a, int b, int expected)
    {
        var service = new MyService();
        Assert.Equal(expected, service.Calculate(a, b));
    }
}