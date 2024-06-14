namespace CSynth.Analysis;

public class Scope {
    public List<Statement> Statements { get; set; } = new();
}

public class Generator
{
    public Stack<Scope> Scopes { get; set; } = new();
    public List<Statement> Statements => Scopes.Peek().Statements;

    public Builder Builder { get; set; } = new();

    public Queue<Node> Queue { get; set; } = new();
    public HashSet<Node> Visited { get; set; } = new();

    public void Generate(CFG cfg)
    {
        Scopes.Push(new Scope());
        HandleNode(cfg.Entry);
    }

    internal void HandleNode(Node node) {
        if (Visited.Contains(node))
            return;

        Visited.Add(node);
            
        switch (node) {
            case EntryNode:
            case ExitNode:
            case TargetNode:
                break;
            case BranchNode branch:
                Builder.GreedyGenerate(branch.Instructions);
                Statements.AddRange(Builder.Statements);
                Builder.Statements.Clear();

                Scopes.Push(new Scope());
                HandleNode(branch.Target);
                var scope = Scopes.Pop();

                Statements.Add(new IfStatement() {
                    Branch = Builder.Branches.Pop(),
                    Statements = scope.Statements
                });
                break;
            case BlockNode block:
                Builder.GreedyGenerate(block.Instructions);
                Statements.AddRange(Builder.Statements);
                Builder.Statements.Clear();
                break;
        }

        foreach (var edge in node.Edges.Values)
            HandleNode(edge.To);
    }

    public void Print()
    {
        foreach (var statement in Statements)
        {
            Console.WriteLine(statement);
        }
    }
}
