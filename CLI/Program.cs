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

        // var path = args[0];
        // var assembly = AssemblyDefinition.ReadAssembly(path);

        // foreach (var instruction in assembly.MainModule.EntryPoint.Body.Instructions) {
        //     Console.WriteLine(instruction);
        // }

        // var flowInfo = FlowInfo.From(assembly.MainModule.EntryPoint.Body.Instructions);
        // var cfg = flowInfo.CFG;
        // cfg.Print();
        // RestructureLoop.Restructure(cfg);
        // cfg.PrintMermaid();

        var graph = """
        0[1,3]
        1[3,2]
        2[4]
        3[4]
        4[5]
        5[]
        """;

        // var graph = """
        // 0[1]
        // 1[2,3,5]
        // 2[4]
        // 3[4]
        // 4[]
        // 5[]
        // """;

        var cfg = CFG.FromEquality(graph);

        cfg.PrintMermaid();
        RestructureBranch.Restructure(cfg);
        cfg.PrintMermaid();
        


        // var cfg = CFG.From(assembly.MainModule.EntryPoint);
        // cfg.Print();
        // Console.WriteLine(cfg.ToMermaid());

        // var generator = new Generator();
        // generator.Generate(cfg);
        // generator.Print();
    }
}
