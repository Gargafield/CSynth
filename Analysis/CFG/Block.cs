using System.Diagnostics;
using Mono.Cecil.Cil;

namespace CSynth.Analysis;

public enum BlockId : int { }

public abstract class Block {
    public BlockId Id { get; set; }
    public List<Block> Targets { get; set; } = new();

    protected Block(BlockId id) {
        Id = id;
    }

    public void AddTarget(Block target) {
        Targets.Add(target);
    }

    public void ReplaceTarget(Block oldTarget, Block newTarget) {
        var index = Targets.IndexOf(oldTarget);
        Targets[index] = newTarget;
    }

    public bool TargetsBlock(Block block) {
        return Targets.Contains(block);
    }
    
    public bool TargetsBlock(BlockId id) {
        return Targets.Any(t => t.Id == id);
    }
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

public class AssignmentBlock : SyntheticBlock {
    public Dictionary<Variable, object> Variables { get; set; } = new();

    protected AssignmentBlock(BlockId id) : base(id) { }

    public static AssignmentBlock Create(CFG cfg) {
        var block = cfg.Blocks.Add(id => new AssignmentBlock(id));
        return block;
    }

    public void AddVariable(Variable name, object value) {
        Variables[name] = value;
    }
}

public class BranchBlock : SyntheticBlock {
    public Variable Variable { get; set; } = default!;
    public Dictionary<object, Block> Branches { get; set; } = new();

    protected BranchBlock(BlockId id) : base(id) { }

    public static BranchBlock Create(CFG cfg, Variable variable) {
        var block = cfg.Blocks.Add(id => new BranchBlock(id));
        block.Variable = variable;
        return block;
    }

    public void AddBranch(object value, Block target) {
        Branches[value] = target;
    }
}
