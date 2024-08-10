using CSynth.Analysis;
using CSynth.AST;
using CSynth.Transformation;
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

        var debug = args.Contains("--debug") || args.Contains("-d");

        var path = args[0];
        var assembly = AssemblyDefinition.ReadAssembly(path);

        var context = new TranslationContext() {
            Debug = debug
        };

        var moduleContext = new ModuleContext(assembly.MainModule, context);

        var module = new Module(moduleContext);

        Console.WriteLine(LuauWriter.Write(module.Compile(), moduleContext));
    }
}
