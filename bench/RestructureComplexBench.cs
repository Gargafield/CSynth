using System.Linq;
using BenchmarkDotNet.Attributes;
using CSynth.Analysis;
using CSynth.Analysis.Transformation;

namespace CSynth.Bench;

public class RestructureComplexBench
{
    public CFG cfg;

    public const string graph = """
    0[1]
    1[1,2,3,4]
    2[4]
    3[]
    4[2]
    """;

    [GlobalSetup]
    public void Setup() {
        cfg = CFG.FromEquality(graph);
    }

    [Benchmark]
    public void RestructureComplex() {
        Restructure.RestructureCFG(cfg);
    }

    [Benchmark]
    public void RestructureLoopComplex() {
        RestructureLoop.Restructure(cfg);
    }
    
    [GlobalSetup(Target = nameof(RestructureBranchComplex))]
    public void SetupBranch() {
        // Branch algorithm won't work with backedges

        cfg = CFG.FromEquality(graph);
        RestructureLoop.Restructure(cfg);
        foreach (var loop in cfg.Regions.OfType<LoopRegion>())
            loop.RemoveBackedge();
    }

    [Benchmark]
    public void RestructureBranchComplex() {
        RestructureBranch.Restructure(cfg);
    }
}
