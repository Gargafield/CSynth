using System.Runtime.CompilerServices;
using CSynth.AST;

namespace CSynth.AST;

public abstract class Block {
    public int Id { get; set; }

    public HashSet<int> Predecessors { get; set; } = new();
    public List<int> Successors { get; set; } = new();

    protected Block() { }

    public void AddTarget(Block target) {
        Successors.Add(target.Id);
        target.Predecessors.Add(this.Id);
    }

    public void RemoveTarget(Block target) {
        Successors.Remove(target.Id);
        target.Predecessors.Remove(this.Id);
    }

    public virtual void ReplaceTarget(Block oldTarget, Block newTarget) {
        oldTarget.Predecessors.Remove(this.Id);
        newTarget.Predecessors.Add(this.Id);

        var index = Successors.IndexOf(oldTarget.Id);
        Successors[index] = newTarget.Id;
    }

    public override bool Equals(object? obj) {
        return obj is Block block && block.Id == Id;
    }

    public override int GetHashCode() => Id;
}

public enum BranchVariable {
    HeaderExit,
    LoopControl,
    BranchControl
}

public class BlockCollection : List<Block>, IEnumerable<int> {
    public int First() => base[0].Id;

    public BlockCollection() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public List<int> Successors(int id) => this[id].Successors;
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public HashSet<int> Predecessors(int id) => this[id].Predecessors;

    public int AddAssignment(BranchVariable variable, int value) {
        var assignment = AssignmentBlock.Create(this);
        assignment.AddVariable(variable.ToString(), value);
        return assignment.Id;
    }
    public int AddBranch(BranchVariable variable) {
        return BranchBlock.Create(this, variable.ToString()).Id;
    }
    public int AddNoop() {
        return NoopBlock.Create(this).Id;
    }

    public void AddBranch(int from, int to, int value) {
        var block = this[from] as BranchBlock;
        block!.AddBranch(value, this[to]);
    }
    public void AddEdge(int from, int to) {
        this[from].AddTarget(this[to]);
    }
    public void RemoveEdge(int from, int to) {
        this[from].RemoveTarget(this[to]);
    }
    public void ReplaceEdge(int from, int oldTo, int newTo) {
        this[from].ReplaceTarget(this[oldTo], this[newTo]);
    }

    public T Add<T>(T block)
    where T: Block
    {
        block.Id = Count;
        base.Add(block);
        return block;
    }

    public IEnumerable<Block> GetEnumerable() => this;
    public IEnumerable<int> GetEnumerableIds() => Enumerable.Range(0, Count);

    IEnumerator<int> IEnumerable<int>.GetEnumerator() => Enumerable.Range(0, Count).GetEnumerator();
}

public class BasicBlock : Block {
    public List<Statement> Statements { get; set; } = new();

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

    public static AssignmentBlock Create(BlockCollection blocks) {
        return blocks.Add(new AssignmentBlock());
    }

    public void AddVariable(string name, int value) {
        Statements.Add(new AssignmentStatement(name, new NumberExpression(value)));
    }

    public void AddVariable(string name, bool value) {
        Statements.Add(new AssignmentStatement(name, value ? BoolExpression.True : BoolExpression.False));
    }
}

public class BranchBlock : Block {
    public string Variable { get; set; }

    protected BranchBlock(string Variable) {
        this.Variable = Variable;
    }

    public static BranchBlock Create(BlockCollection blocks, string variable) {
        return blocks.Add(new BranchBlock(variable));
    }

    public void AddBranch(int value, Block target) {
        Successors.Insert(value, target.Id);
        target.Predecessors.Add(this.Id);
    }
}

public class NoopBlock : SyntheticBlock {
    protected NoopBlock() { }

    public static NoopBlock Create(BlockCollection blocks) {
        return blocks.Add(new NoopBlock());
    }
}