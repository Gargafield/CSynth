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
            cfg.PrintMermaid();
        }

        Restructure.RestructureCFG(cfg);

        if (debug) {
            cfg.PrintMermaid();
        }

        var tree = ControlTree.From(cfg);
        Console.WriteLine(tree.ToString());

        var compiler = new Compiler.Compiler(tree);
        compiler.Compile();

        var writer = new LuauWriter(compiler.Statements);
        writer.Write();
    }
}
