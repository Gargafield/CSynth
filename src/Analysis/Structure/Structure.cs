using CSynth.AST;

namespace CSynth.Analysis;

public abstract class Structure {
    public List<Structure> Children { get; set; } = new();
}

public class LinearStructure : Structure { }

public class BlockStructure : Structure {
    public Block Block { get; set; }

    public BlockStructure(Block block) {
        Block = block;
    }
}

public class BranchStructure : BlockStructure {
    public List<(Structure, int)> Conditions { get; set; } = new();

    public BranchStructure(Block block) : base(block) {}

    public void AddCondition(Structure structure, int value) {
        Conditions.Add((structure, value));
        Children.Add(structure);
    }
}

public class LoopStructure : Structure {
    // Tail looped
}