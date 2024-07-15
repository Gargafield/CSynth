namespace CSynth.Analysis;

public enum RegionId : uint { }

public abstract class Region {
    public RegionId Id { get; set; }
    public HashSet<Block> Blocks { get; set; } = new();

    protected Region(RegionId id) {
        Id = id;
    }
}

public class RegionCollection : AbstractCollection<Region> {
    public RegionCollection() { }

    public Region this[RegionId id] => _items[(int)id];

    public T Add<T>(Func<RegionId, T> factory)
        where T : Region
    {
        var region = factory((RegionId)getId());
        _items.Add(region);
        return region;
    }
}

public class LoopRegion : Region {
    public Block Header { get; set; }
    public Block Control { get; set; }

    private LoopRegion(RegionId id, HashSet<Block> blocks, Block header, Block control) : base(id) {
        Blocks = blocks;
        Header = header;
        Control = control;
    }

    public static LoopRegion Create(CFG cfg, HashSet<Block> blocks, Block header, Block control) {
        var region = cfg.Regions.Add(id => new LoopRegion(id, blocks, header, control));
        return region;
    }

    public void RemoveBackedge() {
        Control.RemoveTarget(Header);
    }

    public void AddBackedge() {
        Control.AddTarget(Header);
    }
}
