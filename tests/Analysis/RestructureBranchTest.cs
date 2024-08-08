using CSynth.Analysis;
using CSynth.AST;
using CSynth.Transformation;

namespace CSynth.Test;

public class RestructureBranchTest
{
    [Fact]
    public void RestructurePerfectCaseNoop() {
        // Branch doesn't require any restructuring

        var graph = """
        0[1,2]
        1[3]
        2[3]
        3[]
        """;

        var cfg = CFG.FromEquality(graph);

        RestructureBranch.Restructure(cfg);

        var result = cfg.ToEquality();
        Assert.Equal(graph, result);
    }

    [Fact]
    public void RestructureTest() {
        // Branching requires restructuring

        var graph = """
        0[1,3]
        1[2,3]
        2[4]
        3[4]
        4[5]
        5[]
        """;

        var expected = """
        0[1,6]
        1[2,9]
        2[10]
        3[4]
        4[5]
        5[]
        6[11]
        7[3,4]
        8[7]
        9[8]
        10[8]
        11[7]
        """;

        var cfg = CFG.FromEquality(graph);

        RestructureBranch.Restructure(cfg);

        var result = cfg.ToEquality();
        Assert.Equal(expected, result);
    }
}