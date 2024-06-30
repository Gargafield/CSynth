namespace CSynth.Analysis.RVSDG;

public class Link {
    public LinkId Id { get; set; }

    public PortId Source { get; set; }
    public PortId Target { get; set; }

    public Link(PortId source, PortId target) {
        Source = source;
        Target = target;
    }
}

public class LinkCollection : AbstractCollection<Link> {
    public LinkCollection() { }
    
    public Link this[LinkId id] => _items[(int)id];

    public Link Add(PortId source, PortId target) {
        var link = new Link(source, target) { Id = (LinkId)getId() };
        _items.Add(link);
        return link;
    }
}
