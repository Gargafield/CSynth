using CSynth.Analysis;
using CSynth.Analysis.Transformation;

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

        Assert.Equal(graph, cfg.ToEquality());
        
        var controlTree = ControlTree.From(cfg);

        var expected = """
        CSynth.Analysis.LinearStructure
          CSynth.Analysis.BlockStructure
          CSynth.Analysis.BlockStructure
          CSynth.Analysis.LoopStructure
            CSynth.Analysis.BranchStructure
              CSynth.Analysis.LinearStructure
                CSynth.Analysis.BlockStructure
                CSynth.Analysis.BlockStructure
              CSynth.Analysis.LinearStructure
                CSynth.Analysis.BlockStructure
            CSynth.Analysis.BlockStructure
          CSynth.Analysis.BlockStructure
          CSynth.Analysis.BlockStructure
        """;

        var result = controlTree.ToString();
        Assert.Equal(expected, result);
    }
}
