using System.Diagnostics;
using Mono.Cecil.Cil;

namespace CSynth.Analysis;

public enum BlockId : int { }

public abstract class Block {
    public BlockId Id { get; set; }

    public HashSet<Block> Predecessors { get; set; } = new();
    public HashSet<Block> Successors { get; set; } = new();

    protected Block(BlockId id) {
        Id = id;
    }

    public void AddTarget(Block target) {
        Successors.Add(target);
        target.Predecessors.Add(this);
    }

    public void RemoveTarget(Block target) {
        Successors.Remove(target);
        target.Predecessors.Remove(this);
    }

    public void ReplaceTarget(Block oldTarget, Block newTarget) {
        if (oldTarget == newTarget) return;
        Successors.Remove(oldTarget);
        Successors.Add(newTarget);
        oldTarget.Predecessors.Remove(this);
        newTarget.Predecessors.Add(this);
    }

    public bool TargetsBlock(Block block) {
        return Successors.Contains(block);
    }

    public override bool Equals(object? obj) {
        return obj is Block block && Equals(block);
    }

    public bool Equals(Block block) {
        return Id == block.Id;
    }

    public override int GetHashCode() => (int)Id;
}

public class BlockCollection : AbstractCollection<Block> {
    public BlockCollection() { }

    public Block this[BlockId id] => _items[(int)id];

    public T Add<T>(Func<BlockId, T> factory)
        where T : Block
    {
        var node = factory((BlockId)getId());
        _items.Add(node);
        return node;
    }
}

public class BasicBlock : Block {
    public List<Instruction> Instructions { get; set; } = new();
    public int Offset { get; set; }

    protected BasicBlock(BlockId id, List<Instruction> instructions) : base(id) {
        Instructions = instructions;
        Offset = instructions.FirstOrDefault()?.Offset ?? -1;
    }

    public static BasicBlock Create(CFG cfg, List<Instruction> instructions) {
        var block = cfg.Blocks.Add(id => new BasicBlock(id, instructions));
        return block;
    }
    
    public static BasicBlock Create(CFG cfg) {
        var block = cfg.Blocks.Add(id => new BasicBlock(id, new()));
        return block;
    }
}


public abstract class SyntheticBlock : Block {
    protected SyntheticBlock(BlockId id) : base(id) { }
}

public class EntryBlock : SyntheticBlock {
    protected EntryBlock(BlockId id) : base(id) {}

    public static EntryBlock Create(CFG cfg) {
        var block = cfg.Blocks.Add(id => new EntryBlock(id));
        return block;
    }
}

public class ExitBlock : SyntheticBlock {
    protected ExitBlock(BlockId id) : base(id) {}

    public static ExitBlock Create(CFG cfg) {
        var block = cfg.Blocks.Add(id => new ExitBlock(id));
        return block;
    }
}

public enum BlockVariable {
    HeaderExit,
    LoopControl,
    BranchControl
}

public class AssignmentBlock : SyntheticBlock {
    public Dictionary<BlockVariable, int> Assignments { get; set; } = new();

    protected AssignmentBlock(BlockId id) : base(id) { }

    public static AssignmentBlock Create(CFG cfg) {
        var block = cfg.Blocks.Add(id => new AssignmentBlock(id));
        return block;
    }

    public void AddVariable(BlockVariable name, int value) {
        Assignments[name] = value;
    }
}

public class BranchBlock : SyntheticBlock {
    public BlockVariable Variable { get; set; } = default!;
    public Dictionary<int, Block> Branches { get; set; } = new();

    protected BranchBlock(BlockId id) : base(id) { }

    public static BranchBlock Create(CFG cfg, BlockVariable variable) {
        var block = cfg.Blocks.Add(id => new BranchBlock(id));
        block.Variable = variable;
        return block;
    }

    public void AddBranch(int value, Block target) {
        Branches[value] = target;
        AddTarget(target);
    }
}

public class NoopBlock : SyntheticBlock {
    protected NoopBlock(BlockId id) : base(id) { }

    public static NoopBlock Create(CFG cfg) {
        var block = cfg.Blocks.Add(id => new NoopBlock(id));
        return block;
    }
}