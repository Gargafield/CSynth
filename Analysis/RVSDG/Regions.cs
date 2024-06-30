namespace CSynth.Analysis.RVSDG;

public abstract class Region
{
    public NodeCollection Nodes { get; set; } = new();
    public LinkCollection Links { get; set; } = new();

    public abstract PortCollection Arguments { get; }
    public abstract PortCollection Results { get; }
}

