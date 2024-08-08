using System.Diagnostics;
using CSynth.Analysis;
using CSynth.AST;
using CSynth.Transformation;

namespace CSynth.Test;

public class RestructureLoopTest
{
    [Fact]
    public void RestructurePerfectCaseNoop() {
        // Loop doesn't require any restructuring

        var graph = """
        0[1]
        1[1,2]
        2[]
        """;

        var cfg = CFG.FromEquality(graph);

        RestructureLoop.Restructure(cfg);

        var result = cfg.ToEquality();
        Assert.Equal(graph, result);
    }

    [Fact]
    public void RestructureBigBodyNoop() {
        // Loop doesn't require any restructuring

        var graph = """
        0[1]
        1[2]
        2[3]
        3[4]
        4[5]
        5[1,6]
        6[]
        """;

        var cfg = CFG.FromEquality(graph);

        Debug.WriteLine(cfg.ToMermaid());

        RestructureLoop.Restructure(cfg);

        Debug.WriteLine(cfg.ToMermaid());

        var result = cfg.ToEquality();
        Assert.Equal(graph, result);
    }

    [Fact]
    public void RestructureWithAllSinglePoints() {
        // Restructures a graph that requires a header, exit and control node
        // to be created

        var graph = """
        0[1]
        1[2,3]
        2[3,4]
        3[2,5]
        4[6]
        5[6]
        6[]
        """;

        var expected = """
        0[1]
        1[8,10]
        2[11,13]
        3[9,14]
        4[6]
        5[6]
        6[]
        7[2,3]
        8[7]
        9[16]
        10[7]
        11[17]
        12[4,5]
        13[18]
        14[19]
        15[7,12]
        16[15]
        17[15]
        18[15]
        19[15]
        """;

        var cfg = CFG.FromEquality(graph);

        RestructureLoop.Restructure(cfg);

        var result = cfg.ToEquality();

        var mermaidBefore = CFG.FromEquality(expected).ToMermaid();
        var mermaidAfter = cfg.ToMermaid();
        
        Assert.Equal(expected, result);
    }
}