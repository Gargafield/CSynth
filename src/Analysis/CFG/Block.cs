using CSynth.AST;

namespace CSynth.Analysis;

public abstract class Block {
    public int Id { get; set; }

    public List<Block> Predecessors { get; set; } = new();
    public List<Block> Successors { get; set; } = new();

    protected Block() { }

    public void AddTarget(Block target) {
        Successors.Add(target);
        target.Predecessors.Add(this);
    }

    public void RemoveTarget(Block target) {
        Successors.Remove(target);
        target.Predecessors.Remove(this);
    }

    public virtual void ReplaceTarget(Block oldTarget, Block newTarget) {
        oldTarget.Predecessors.Remove(this);
        newTarget.Predecessors.Add(this);

        var index = Successors.IndexOf(oldTarget);
        Successors[index] = newTarget;
    }

    public override bool Equals(object? obj) {
        return obj is Block block && block.Id == Id;
    }

    public override int GetHashCode() => Id;
}

public class BlockCollection : List<Block> {
    public BlockCollection() { }

    public T Add<T>(T block)
    where T: Block
    {
        block.Id = Count;
        base.Add(block);
        return block;
    }
}

public class BasicBlock : Block {
    public List<Statement> Statements { get; set; } = new();
    public int Offset => Statements.FirstOrDefault()?.Offset ?? -1;

    protected BasicBlock(List<Statement> statement) {
        Statements = statement;
    }

    public static BasicBlock Create(CFG cfg, List<Statement> statements) {
        return cfg.Blocks.Add(new BasicBlock(statements));
    }
}


public abstract class SyntheticBlock : Block {
    protected SyntheticBlock() { }
}

public class EntryBlock : SyntheticBlock {
    protected EntryBlock() {}

    public static EntryBlock Create(CFG cfg) {
        
        return cfg.Blocks.Add(new EntryBlock());
    }
}

public class ExitBlock : SyntheticBlock {
    protected ExitBlock() {}

    public static ExitBlock Create(CFG cfg) {
        return cfg.Blocks.Add(new ExitBlock());
    }
}

public static class BlockVariable {
    public const string HeaderExit = "HeaderExit";
    public const string LoopControl = "LoopControl";
    public const string BranchControl = "BranchControl";
}

public class AssignmentBlock : BasicBlock {
    protected AssignmentBlock() : base(new List<Statement>()) { }

    public static AssignmentBlock Create(CFG cfg) {
        return cfg.Blocks.Add(new AssignmentBlock());
    }

    public void AddVariable(string name, int value) {
        Statements.Add(new AssignmentStatement(-1, name, new NumberExpression(value)));
    }

    public void AddVariable(string name, bool value) {
        Statements.Add(new AssignmentStatement(-1, name, value ? BoolExpression.True : BoolExpression.False));
    }
}

public class BranchBlock : Block {
    public List<(Block, int)> Branches { get; set; } = new();
    public string Variable { get; set; }

    protected BranchBlock(string Variable) {
        this.Variable = Variable;
    }

    public static BranchBlock Create(CFG cfg, string variable) {
        return cfg.Blocks.Add(new BranchBlock(variable));
    }

    public void AddBranch(int value, Block target) {
        Branches.Add((target, value));
        AddTarget(target);
    }

    public override void ReplaceTarget(Block oldTarget, Block newTarget) {
        base.ReplaceTarget(oldTarget, newTarget);
        var index = Branches.FindIndex(b => b.Item1 == oldTarget);
        Branches[index] = (newTarget, Branches[index].Item2);
    }
}

public class NoopBlock : SyntheticBlock {
    protected NoopBlock() { }

    public static NoopBlock Create(CFG cfg) {
        return cfg.Blocks.Add(new NoopBlock());
    }
}