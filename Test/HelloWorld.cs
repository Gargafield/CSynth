using System.Diagnostics;

namespace CSynth.Test;

public class HelloWorldTest
{
    [Fact]
    public void HelloWorld()
    {
        while (true)
        {
            Console.WriteLine("Hello, World!");
        }
    }
}