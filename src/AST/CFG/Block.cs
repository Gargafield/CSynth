using System.Runtime.CompilerServices;
using CSynth.AST;
using Mono.Cecil.Cil;

namespace CSynth.AST;

public abstract class Block {
    public int Id { get; set; }

    protected Block() { }

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

public class BlockCollection {
    public List<Block> Blocks { get; } = new();
    public List<List<int>> _successors { get; } = new();
    public List<HashSet<int>> _predecessors { get; } = new();

    public Block this[int id] => Blocks[id];

    public int First() => Blocks[0].Id;

    public BlockCollection() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public List<int> Successors(int id) => _successors[id];
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public HashSet<int> Predecessors(int id) => _predecessors[id];

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
    public void AddEdge(int from, int to) {
        _successors[from].Add(to);
        _predecessors[to].Add(from);
    }
    public void RemoveEdge(int from, int to) {
        _successors[from].Remove(to);
        _predecessors[to].Remove(from);
    }
    public void ReplaceEdge(int from, int oldTo, int newTo) {
        var successors = _successors[from];
        var index = successors.IndexOf(oldTo);
        successors[index] = newTo;

        _predecessors[oldTo].Remove(from);
        _predecessors[newTo].Add(from);
    }

    public T Add<T>(T block)
    where T: Block
    {
        block.Id = Blocks.Count;
        Blocks.Add(block);
        _successors.Add(new List<int>());
        _predecessors.Add(new HashSet<int>());
        return block;
    }

    public IEnumerable<int> GetEnumerableIds() => Enumerable.Range(0, Blocks.Count);
}

public class BasicBlock : Block {
    public List<Instruction> Instructions { get; set; } = new();

    protected BasicBlock(List<Instruction> instructions) {
        Instructions = instructions;
    }

    public static BasicBlock Create(CFG cfg, List<Instruction> instructions) {
        return cfg.Blocks.Add(new BasicBlock(instructions));
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

public class AssignmentBlock : SyntheticBlock {
    public List<KeyValuePair<string, Expression>> Instructions { get; set; } = new();

    protected AssignmentBlock() : base() { }

    public static AssignmentBlock Create(BlockCollection blocks) {
        return blocks.Add(new AssignmentBlock());
    }

    public void AddVariable(string name, int value) {
        Instructions.Add(new KeyValuePair<string, Expression>(name, new NumberExpression(value, TypeResolver.Int32Type)));
    }

    public void AddVariable(string name, bool value) {
        var expr = value ? BoolExpression.True : BoolExpression.False;
        Instructions.Add(new KeyValuePair<string, Expression>(name, expr));
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
}

public class NoopBlock : SyntheticBlock {
    protected NoopBlock() { }

    public static NoopBlock Create(BlockCollection blocks) {
        return blocks.Add(new NoopBlock());
    }
}