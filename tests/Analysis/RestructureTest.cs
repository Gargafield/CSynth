using System;
using CSynth.Analysis;
using CSynth.Analysis.Transformation;

namespace CSynth.Test;

public class RestructureTest
{
    [Fact]
    public void TestRestructureComplex() {
        var graph = """
        0[1]
        1[1,2,3,4]
        2[4]
        3[]
        4[2]
        """;

        var expected = """
        0[1]
        1[6,7,8,10]
        2[14]
        3[]
        4[12]
        5[3,13,15]
        6[9]
        7[9]
        8[9]
        9[1,5]
        10[9]
        11[2,4]
        12[17]
        13[11]
        14[17]
        15[11]
        16[]
        17[11,16]
        """;

        var cfg = CFG.FromEquality(graph);

        Restructure.RestructureCFG(cfg);

        var result = cfg.ToEquality();
        Assert.Equal(expected, result);
    }
}
