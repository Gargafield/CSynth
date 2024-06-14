
namespace CSynth.Analysis;

public abstract class Edge
{
    public Node From { get; set; } = null!;
    public Node To { get; set; } = null!;
}

public class ConditionalEdge : Edge { }
public class FallthroughEdge : Edge { }
