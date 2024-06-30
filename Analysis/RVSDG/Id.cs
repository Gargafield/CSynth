namespace CSynth.Analysis.RVSDG;

public enum NodeId : uint {}
public enum EdgeId : uint {}
public enum LinkId : uint {}
public record PortId(NodeId NodeId, uint PortIndex) { }

