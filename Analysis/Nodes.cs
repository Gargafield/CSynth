using Mono.Cecil.Cil;

namespace CSynth.Analysis;

public class Node
{
    public Dictionary<Node, Edge> Edges { get; set; } = new ();
    public List<Node> Outgoing => Edges.Values.Select(e => e.To).ToList();
}

public class EntryNode : Node
{
    public override string ToString() => "Entry";
}

public class ExitNode : Node
{
    public override string ToString() => "Exit";
}

public class TargetNode : Node
{
    public override string ToString() => "Target";
}

public class BlockNode : Node {
    public List<Instruction> Instructions { get; set; } = new ();

    public override string ToString() => "Block";
}

public class BranchNode : BlockNode {
    public Node Target => Outgoing.OfType<TargetNode>().Single();

    public override string ToString() => "Branch";
}
