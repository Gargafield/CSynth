using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using CSynth.Analysis;
using CSynth.Analysis.Transformation;

namespace CSynth.Bench;

public class RestructureComplexBench
{
    public CFG cfg;

    public static string graph;

    [GlobalSetup]
    public void Setup() {
        graph = """
        0[1]
        1[1,2,3,4]
        2[4]
        3[]
        4[2]
        """;
    }

    [Benchmark]
    public void RestructureComplex() {
        Restructure.RestructureCFG(CFG.FromEquality(graph));
    }

    [Benchmark]
    public void RestructureLoopComplex() {
        RestructureLoop.Restructure(CFG.FromEquality(graph));
    }
    
    [GlobalSetup(Target = nameof(RestructureBranchComplex))]
    public void SetupBranch() {
        // Branch algorithm won't work with backedges
        Setup();
        cfg = CFG.FromEquality(graph);
        RestructureLoop.Restructure(cfg);
        foreach (var loop in cfg.Regions.OfType<LoopRegion>())
            loop.RemoveBackedge();
        
        graph = cfg.ToEquality();
    }

    [Benchmark]
    public void RestructureBranchComplex() {
        RestructureBranch.Restructure(CFG.FromEquality(graph));
    }
}
