using CSynth.Analysis;
using CSynth.Analysis.Transformation;
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

        var flowInfo = FlowInfo.From(assembly.MainModule.EntryPoint.Body.Instructions);
        var cfg = flowInfo.CFG;
        cfg.Print();
        cfg.PrintMermaid();
        RestructureLoop.Restructure(cfg);
        foreach (var loop in cfg.Regions.OfType<LoopRegion>())
            loop.RemoveBackedge();
        
        RestructureBranch.Restructure(cfg);
        
        foreach (var loop in cfg.Regions.OfType<LoopRegion>())
            loop.AddBackedge();

        cfg.PrintMermaid();
        var compiler = new Compiler.Compiler(cfg);
        var statements = compiler.Compile();

        foreach (var statement in statements)
            Console.WriteLine(statement);
    }
}
