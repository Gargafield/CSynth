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
    public BranchStructure(Block block) : base(block) {}
}

public class LoopStructure : Structure {
    // Tail looped
}