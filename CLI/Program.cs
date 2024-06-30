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

        // foreach (var instruction in assembly.MainModule.EntryPoint.Body.Instructions) {
        //     Console.WriteLine(instruction);
        // }

        var flowInfo = FlowInfo.From(assembly.MainModule.EntryPoint.Body.Instructions);
        var cfg = flowInfo.CFG;
        cfg.Print();
        RestructureLoop.Restructure(cfg);
        cfg.PrintMermaid();

        // var cfg = new CFG();
        // var block0 = BasicBlock.Create(cfg);
        // var block1 = BasicBlock.Create(cfg);
        // var block2 = BasicBlock.Create(cfg);
        // var block3 = BasicBlock.Create(cfg);
        // // var block4 = BasicBlock.Create(cfg);

        // block0.AddTarget(block1);
        // block1.AddTarget(block2);
        // block2.AddTarget(block3);
        // block2.AddTarget(block1);

        // cfg.PrintMermaid();



        // var cfg = CFG.From(assembly.MainModule.EntryPoint);
        // cfg.Print();
        // Console.WriteLine(cfg.ToMermaid());

        // var generator = new Generator();
        // generator.Generate(cfg);
        // generator.Print();
    }
}
