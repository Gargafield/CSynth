using CSynth.AST;
using CSynth.Transformation;

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
        2[18]
        3[]
        4[16]
        5[3,15,17]
        6[11]
        7[12]
        8[13]
        9[1,5]
        10[9]
        11[9]
        12[9]
        13[9]
        14[2,4]
        15[14]
        16[21]
        17[14]
        18[22]
        19[]
        20[14,19]
        21[20]
        22[20]
        """;

        var cfg = CFG.FromEquality(graph);

        Restructure.RestructureCFG(cfg);

        var result = cfg.ToEquality();
        Assert.Equal(expected, result);
    }
}
