
namespace CSynth.AST;

public abstract class Structure {
    public List<object> Children { get; set; } = new();
}

public class LinearStructure : Structure {
    public override string ToString() {
        return $"LinearStructure({Children.Count})";
    }
}

public class BranchStructure : Structure {
    public Block Branch { get; set; }
    public List<(Structure, int)> Conditions { get; set; } = new();

    public BranchStructure(Block block) {
        Branch = block;
    }

    public void AddCondition(Structure structure, int value) {
        Conditions.Add((structure, value));
        Children.Add(structure);
    }

    public override string ToString() {
        return $"BranchStructure({Children.Count})";
    }
}

public class LoopStructure : Structure {
    public override string ToString() {
        return $"LoopStructure({Children.Count})";
    }
}