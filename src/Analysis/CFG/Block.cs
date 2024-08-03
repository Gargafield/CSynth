using System.Diagnostics;
using CSynth.AST;
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

    public virtual void ReplaceTarget(Block oldTarget, Block newTarget) {
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
    public List<Statement> Statements { get; set; } = new();
    public int Offset => Statements.FirstOrDefault()?.Offset ?? -1;

    protected BasicBlock(BlockId id, List<Statement> statement) : base(id) {
        Statements = statement;
    }

    public static BasicBlock Create(CFG cfg, List<Statement> statements) {
        var block = cfg.Blocks.Add(id => new BasicBlock(id, statements));
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

public static class BlockVariable {
    public const string HeaderExit = "HeaderExit";
    public const string LoopControl = "LoopControl";
    public const string BranchControl = "BranchControl";
}

public class AssignmentBlock : BasicBlock {
    public Dictionary<string, int> Assignments { get; set; } = new();

    protected AssignmentBlock(BlockId id) : base(id, new List<Statement>()) { }

    public static new AssignmentBlock Create(CFG cfg) {
        var block = cfg.Blocks.Add(id => new AssignmentBlock(id));
        return block;
    }

    public void AddVariable(string name, object value) {
        Expression expr = value switch {
            int i => new NumberExpression(i),
            bool b => b ? BoolExpression.True : BoolExpression.False,
            _ => throw new NotImplementedException()
        };

        Statements.Add(new AssignmentStatement(-1, name, expr));
    }
}

public class BranchBlock : Block {
    public Dictionary<int, BlockId> Branches { get; set; } = new();
    public string Variable { get; set; }

    protected BranchBlock(BlockId id, string Variable) : base(id) {
        this.Variable = Variable;
    }

    public static BranchBlock Create(CFG cfg, string variable) {
        var block = cfg.Blocks.Add(id => new BranchBlock(id, variable));
        return block;
    }

    public void AddBranch(int value, Block target) {
        Branches.Add(value, target.Id);
        AddTarget(target);
    }

    public override void ReplaceTarget(Block oldTarget, Block newTarget) {
        base.ReplaceTarget(oldTarget, newTarget);
        var index = Branches.FirstOrDefault(x => x.Value == oldTarget.Id).Key;
        Branches[index] = newTarget.Id;
    }
}

public class NoopBlock : SyntheticBlock {
    protected NoopBlock(BlockId id) : base(id) { }

    public static NoopBlock Create(CFG cfg) {
        var block = cfg.Blocks.Add(id => new NoopBlock(id));
        return block;
    }
}