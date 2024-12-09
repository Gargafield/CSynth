using BenchmarkDotNet.Attributes;
using CSynth.Core;

namespace CSynth.Bench;

public class RestructureLinearBench
{
    [Params(10, 100, 1000)]
    public int N = 1000;

    public CFG cfg;

    [GlobalSetup]
    public void Setup() {
        cfg = new CFG();
        var blocks = new NoopBlock[N];
        for (int i = 0; i < N; i++)
            blocks[i] = NoopBlock.Create(cfg.Blocks);
        
        for (int i = 0; i < N; i++) {
            if (i < N - 1)
                cfg.Blocks.AddEdge(i, i + 1);
        }
    }

    [Benchmark]
    public void RestructureLinear() {
        Restructure.RestructureCFG(cfg);
    }

    [Benchmark]
    public void RestructureLoopLinear() {
        RestructureLoop.Restructure(cfg);
    }

    [Benchmark]
    public void RestructureBranchLinear() {
        RestructureBranch.Restructure(cfg);
    }
}
