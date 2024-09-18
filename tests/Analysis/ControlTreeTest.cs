using CSynth.Analysis;
using CSynth.AST;
using CSynth.Transformation;

namespace CSynth.Test;

public class ControlTreeTest
{
    [Fact]
    public void CreateCorrectControlTree() {
        var graph = """
        0[1]
        1[3]
        2[7]
        3[2,8]
        4[5]
        5[]
        6[3,4]
        7[6]
        8[6]
        """;

        var cfg = CFG.FromEquality(graph);

        Restructure.RestructureCFG(cfg);

        Assert.Equal(graph, Normalize(cfg.ToEquality()));
        
        var controlTree = ControlTree.From(cfg);

        var expected = """
        LinearStructure(5)
          CSynth.AST.NoopBlock
          CSynth.AST.NoopBlock
          LoopStructure(2)
            BranchStructure(2)
              LinearStructure(2)
                CSynth.AST.NoopBlock
                CSynth.AST.NoopBlock
              LinearStructure(1)
                CSynth.AST.NoopBlock
            CSynth.AST.BranchBlock
          CSynth.AST.NoopBlock
          CSynth.AST.NoopBlock
        """;

        var result = Normalize(controlTree.ToString());
        Assert.Equal(expected, result);
    }

    private string Normalize(string s) {
        return s.Replace("\r\n", "\n");
    }
}
