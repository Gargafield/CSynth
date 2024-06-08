using CSynth.Analysis;

namespace CSynth.CLI;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: CSynth <assembly>");
            return;
        }

        var path = args[0];
        var analyzer = AssemblyAnalyzer.Create(path);
        Console.WriteLine(string.Join('\n', analyzer.GetMethods()));
    }
}
