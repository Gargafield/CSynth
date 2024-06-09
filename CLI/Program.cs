using CSynth.Analysis;
using Mono.Cecil;

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
        var assembly = AssemblyDefinition.ReadAssembly(path);
        var cfg = CFG.From(assembly.MainModule.EntryPoint);
        cfg.Print();

        Console.WriteLine(cfg.ToMermaid());
    }
}
