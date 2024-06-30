namespace CSynth.Analysis.RVSDG;

public abstract class Port {
    public PortId Id { get; private set; }

    protected Port(PortId id) {
        Id = id;
    }

    public abstract Port Clone(PortId id);
}

public class PortCollection : AbstractCollection<Port> {
    private NodeId _nodeId;
    
    public PortCollection(NodeId nodeId) {
        _nodeId = nodeId;
    }

    public Port this[PortId id] => _items[(int)id.PortIndex];

    public Port Add(Func<PortId, Port> factory) {
        var port = factory(new PortId(_nodeId, getId()));
        _items.Add(port);
        return port;
    }
}

public class IntPort : Port {
    public IntPort(PortId id) : base(id) { }

    public override Port Clone(PortId id) {
        return new IntPort(id);
    }
}

public class PredicatePort : Port {
    public PredicatePort(PortId id) : base(id) { }

    public override Port Clone(PortId id) {
        return new PredicatePort(id);
    }
}

public abstract class ContextPort : Port {
    public ContextPort(PortId id) : base(id) { }
}

public class LambdaPort : ContextPort {
    public LambdaPort(PortId id) : base(id) { }

    public override Port Clone(PortId id) {
        return new LambdaPort(id);
    }
}

public class OmegaPort : ContextPort {
    public OmegaPort(PortId id) : base(id) { }

    public override Port Clone(PortId id) {
        return new OmegaPort(id);
    }
}