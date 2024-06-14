using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CSynth.Analysis;

public class CFG
{
    public Node Entry { get; set; } = new EntryNode();
    public Node Exit { get; set; } = new ExitNode();
    public List<Node> Nodes = new();

    private Dictionary<int, Node> _branchTargets = new();

    internal CFG() {
        Nodes.Add(Entry);
        Nodes.Add(Exit);
    }

    public static CFG From(MethodDefinition method)
    {
        var cfg = new CFG();
        cfg.Build(method);
        return cfg;
    }

    private T NewNode<T>()
        where T : Node, new()
    {
        var node = new T();
        Nodes.Add(node);
        return node;
    }

    private BlockNode NewNode<T>(List<Instruction> instructions)
        where T : BlockNode, new()
    {
        var copy = new List<Instruction>(instructions);
        instructions.Clear();

        var node = new T() {
            Instructions = copy
        };
        Nodes.Add(node);
        return node;
    }

    private Node AddEdge(Node? from, Node to, Edge? edge = null)
    {
        if (from == null) {
            return to;
        }

        edge ??= new FallthroughEdge();
        edge.From = from;
        edge.To = to;

        from.Edges.Add(to, edge);
        return to;
    }

    public void Build(MethodDefinition method)
    {
        var node = Entry;
        var instructions = new List<Instruction>();

        foreach (var instruction in method.Body.Instructions)
        {
            if (_branchTargets.TryGetValue(instruction.Offset, out var target)) {
                if (instructions.Count > 0) {
                    node = AddEdge(node, NewNode<BlockNode>(instructions));
                }

                node = AddEdge(node, NewNode<TargetNode>());
                node = AddEdge(target, node, new ConditionalEdge());
            }

            instructions.Add(instruction);

            if (instruction.OpCode.FlowControl == FlowControl.Branch && instruction.Operand is Instruction branchTarget) {
                _branchTargets[branchTarget.Offset] = AddEdge(node, NewNode<BranchNode>(instructions));
                node = null;
            }
            else if (instruction.OpCode.FlowControl == FlowControl.Cond_Branch && instruction.Operand is Instruction condBranchTarget) {
                node = AddEdge(node, NewNode<BranchNode>(instructions));
                _branchTargets[condBranchTarget.Offset] = node;
            }
            else if (instruction.OpCode == OpCodes.Ret) {
                AddEdge(AddEdge(node, NewNode<BlockNode>(instructions)), Exit);
                node = null;
            }
        }
    }

    public void Print()
    {
        foreach (var node in Nodes)
        {
            Console.WriteLine($"Node {Nodes.IndexOf(node)}: ");

            // Edges
            foreach (var edge in node.Edges.Keys) {
                Console.WriteLine($"  Edge to {Nodes.IndexOf(edge)}");
            }

            switch (node)
            {
                case BlockNode blockNode:
                    PrintBlockNode(blockNode);
                    break;
            }
        }
    }

    private void PrintBlockNode(BlockNode node)
    {
        foreach (var instruction in node.Instructions)
        {
            Console.WriteLine($"  {instruction}");
        }
    }   

    public string ToMermaid() {
        var sb = new StringBuilder();
        sb.AppendLine("graph TD;");

        foreach (var node in Nodes)
        {
            sb.AppendLine($"  {Nodes.IndexOf(node)}[{node}]");

            foreach (var edge in node.Edges.Values) {
                sb.AppendLine($"  {Nodes.IndexOf(edge.From)} --> {Nodes.IndexOf(edge.To)}");
            }
        }

        return sb.ToString();
    }
}
