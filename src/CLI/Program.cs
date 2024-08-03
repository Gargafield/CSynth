using CSynth.Analysis;
using CSynth.Analysis.Transformation;
using CSynth.AST;
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

        var instructions = assembly.MainModule.EntryPoint.Body.Instructions;

        if (debug) {
            foreach (var instruction in instructions)
                Console.WriteLine(instruction);
        }

        var statements = ILTranslator.Translate(instructions);

        if (debug) {
            foreach (var statement in statements)
                Console.WriteLine(statement);
        }

        var flowInfo = FlowInfo.From(statements);
        var cfg = flowInfo.CFG;

        if (debug) {
            Console.WriteLine(cfg.ToDot());
        }

        Restructure.RestructureCFG(cfg);

        if (debug) {
            Console.WriteLine(cfg.ToDot());
        }

        var tree = ControlTree.From(cfg);
        if (debug) {
            Console.WriteLine(tree.ToString());
        }

        var output = Compiler.Compile(tree);

        if (debug) {
            foreach (var statement in output)
                Console.WriteLine(statement);
        }
        
        Console.WriteLine(LuauWriter.Write(output));
    }
}
