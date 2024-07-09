using System.Diagnostics;
using CSynth.Analysis;
using CSynth.Analysis.Transformation;

namespace CSynth.Test;

public class RestructureLoopTest
{
    

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
        2[9,14]
        3[11,13]
        4[6]
        5[6]
        6[]
        7[2,3]
        8[7]
        9[15]
        10[7]
        11[15]
        12[4,5]
        13[15]
        14[15]
        15[7,12]
        """;

        var cfg = CFG.FromEquality(graph);

        RestructureLoop.Restructure(cfg);

        var result = cfg.ToEquality();
        Assert.Equal(expected, result);
    }
}