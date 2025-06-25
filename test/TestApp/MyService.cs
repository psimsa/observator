namespace TestApp
{
    public class MyService : IMyService
    {
        public string DoSomething(string input)
        {
            return input;
        }

        public int Calculate(int a, int b)
        {
            return a + b;
        }
    }
}