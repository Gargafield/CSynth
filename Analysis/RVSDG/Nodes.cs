using System.Diagnostics;

namespace CSynth.Analysis.RVSDG;

public abstract class Node {
    public NodeId Id { get; set; }
    public Region Container { get; set; }

    public PortCollection Input { get; }
    public PortCollection Output { get; }

    protected Node(Region container, NodeId id) {
        Container = container;
        Id = id;
        Input = new(id);
        Output = new(id);
    }
}

public class NodeCollection : AbstractCollection<Node> {
    
    public NodeCollection() { }

    public Node this[NodeId id] => _items[(int)id];

    public T Add<T>(Func<NodeId, T> factory)
        where T : Node
    {
        var node = factory((NodeId)getId());
        _items.Add(node);
        return node;
    }
}


public abstract class SimpleNode : Node {
    public string Name { get; private set; } = string.Empty;

    protected SimpleNode(Region container, NodeId id) : base(container, id) { }
}

public class ApplyNode : SimpleNode {
    public LambdaPort Function => (LambdaPort)Input[0];
    public Port Arguments => Input.Skip(1).First();

    internal ApplyNode(Region container, NodeId id) : base(container, id) { }

    public static ApplyNode Create(Region container, LambdaNode function) {
        var node = container.Nodes.Add(id => new ApplyNode(container, id));

        node.Input.Add(id => new LambdaPort(id));

        foreach (var argument in function.Region.Arguments) {
            node.Input.Add(argument.Clone);
        }

        foreach (var result in function.Region.Results) {
            node.Output.Add(result.Clone);
        }

        return node;
    }
}

public abstract class StructuralNode : Node {
    protected StructuralNode(Region container, NodeId id) : base(container, id) { }
}

public class GammaNode : StructuralNode {
    public List<Region> Regions { get; } = new();

    public Port Predicate => Input[0];
    public IEnumerable<Port> Arguments => Input.Skip(1);

    internal GammaNode(Region container, NodeId id) : base(container, id) { }

    public static GammaNode Create(Region container) {
        var node = container.Nodes.Add(id => new GammaNode(container, id));

        node.Input.Add(id => new IntPort(id));

        return node;
    }

    public void AddArgument(Port argument) {
        Input.Add(argument.Clone);
        Output.Add(argument.Clone);

        foreach (var region in Regions) {
            if (!region.Arguments.Contains(argument)) {
                region.Arguments.Add(argument.Clone);
            }
        }
    }
}

public class ThetaNode : StructuralNode {
    public Region Region { get; }

    internal ThetaNode(Region container, NodeId id, Region region) : base(container, id) {
        Region = region;
    }

    public static ThetaNode Create(Region container, Region region) {
        var node = container.Nodes.Add(id => new ThetaNode(container, id, region));

        Trace.Assert(region.Results.First() is PredicatePort);

        return node;
    }

    public void AddArgument(Port input) {
        Input.Add(input.Clone);
        Region.Arguments.Add(input.Clone);
        Output.Add(input.Clone);
    }
}

public class LambdaNode : StructuralNode {
    public Region Region { get; }

    public IEnumerable<ContextPort> ContextVariables => Input.Where(p => p is ContextPort).Cast<ContextPort>();

    internal LambdaNode(Region container, NodeId id, Region region) : base(container, id) {
        Region = region;
    }

    public static LambdaNode Create(Region container, Region region) {
        var node = container.Nodes.Add(id => new LambdaNode(container, id, region));

        node.Output.Add(id => new LambdaPort(id));

        return node;
    }

    public void AddArgument(Port input) {
        Input.Add(input.Clone);
        Region.Arguments.Add(input.Clone);
    }

    public void AddResult(Port output) {
        Region.Results.Add(output.Clone);
    }
}
